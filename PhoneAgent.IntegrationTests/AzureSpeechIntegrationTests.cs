using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace PhoneAgent.IntegrationTests;

public class AzureSpeechIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _configuration;

    public AzureSpeechIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Load appsettings.json to get environment file path
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "appsettings.json");
        var tempConfig = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: true)
            .Build();
        
        var envFilePath = tempConfig["EnvironmentFilePath"];
        
        if (!string.IsNullOrEmpty(envFilePath) && File.Exists(envFilePath))
        {
            foreach (var line in File.ReadAllLines(envFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }

        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
    }

    [Fact]
    public async Task TextToSpeech_ShouldSynthesizeSpeech()
    {
        // Arrange
        var subscriptionKey = _configuration["AZURE_SPEECH_SUBSCRIPTION_KEY"];
        var region = _configuration["AZURE_SPEECH_REGION"];

        Assert.False(string.IsNullOrEmpty(subscriptionKey), "AZURE_SPEECH_SUBSCRIPTION_KEY is not set");
        Assert.False(string.IsNullOrEmpty(region), "AZURE_SPEECH_REGION is not set");

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        // Act
        _output.WriteLine("Synthesizing text: 'Hello, this is a test of Azure Text-to-Speech'");
        var result = await synthesizer.SpeakTextAsync("Hello, this is a test of Azure Text-to-Speech");

        // Assert
        Assert.Equal(ResultReason.SynthesizingAudioCompleted, result.Reason);
        Assert.True(result.AudioData.Length > 0, "Audio data should not be empty");
        _output.WriteLine($"✓ Text-to-Speech succeeded. Audio data length: {result.AudioData.Length} bytes");
    }

    [Fact]
    public async Task SpeechToText_ShouldRecognizeSpeechFromAudioFile()
    {
        // Arrange
        var subscriptionKey = _configuration["AZURE_SPEECH_SUBSCRIPTION_KEY"];
        var region = _configuration["AZURE_SPEECH_REGION"];

        Assert.False(string.IsNullOrEmpty(subscriptionKey), "AZURE_SPEECH_SUBSCRIPTION_KEY is not set");
        Assert.False(string.IsNullOrEmpty(region), "AZURE_SPEECH_REGION is not set");

        // First, synthesize some audio to use for recognition test
        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-GuyNeural";

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);
        var testText = "Hello world, this is a speech recognition test";
        
        _output.WriteLine($"Creating test audio for text: '{testText}'");
        var synthResult = await synthesizer.SpeakTextAsync(testText);
        Assert.Equal(ResultReason.SynthesizingAudioCompleted, synthResult.Reason);

        // Now test speech-to-text
        var recognizerConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        recognizerConfig.SpeechRecognitionLanguage = "en-US";

        using var audioStream = new MemoryStream(synthResult.AudioData);
        using var pushStream = AudioInputStream.CreatePushStream();
        pushStream.Write(synthResult.AudioData);
        pushStream.Close();

        using var audioConfig = AudioConfig.FromStreamInput(pushStream);
        using var recognizer = new SpeechRecognizer(recognizerConfig, audioConfig);

        // Act
        _output.WriteLine("Starting speech recognition...");
        var recognitionResult = await recognizer.RecognizeOnceAsync();

        // Assert
        Assert.Equal(ResultReason.RecognizedSpeech, recognitionResult.Reason);
        Assert.False(string.IsNullOrEmpty(recognitionResult.Text), "Recognized text should not be empty");
        _output.WriteLine($"✓ Speech-to-Text succeeded. Recognized: '{recognitionResult.Text}'");
    }

    [Fact]
    public async Task GetAvailableVoices_ShouldReturnVoiceList()
    {
        // Arrange
        var subscriptionKey = _configuration["AZURE_SPEECH_SUBSCRIPTION_KEY"];
        var region = _configuration["AZURE_SPEECH_REGION"];

        Assert.False(string.IsNullOrEmpty(subscriptionKey), "AZURE_SPEECH_SUBSCRIPTION_KEY is not set");
        Assert.False(string.IsNullOrEmpty(region), "AZURE_SPEECH_REGION is not set");

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        // Act
        _output.WriteLine("Retrieving available voices...");
        var voicesResult = await synthesizer.GetVoicesAsync();

        // Assert
        Assert.Equal(ResultReason.VoicesListRetrieved, voicesResult.Reason);
        Assert.True(voicesResult.Voices.Count > 0, "Voices list should not be empty");
        
        var enUsVoices = voicesResult.Voices.Where(v => v.Locale.StartsWith("en-US")).ToList();
        _output.WriteLine($"✓ Retrieved {voicesResult.Voices.Count} total voices, {enUsVoices.Count} en-US voices");
        
        foreach (var voice in enUsVoices.Take(5))
        {
            _output.WriteLine($"  - {voice.ShortName} ({voice.Gender})");
        }
    }
}
