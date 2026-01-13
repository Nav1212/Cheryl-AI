using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using PhoneAgent.Interfaces;

namespace PhoneAgent.Services;

public class AzureSpeechToTextService : ISpeechToTextService
{
    private readonly ILogger<AzureSpeechToTextService> _logger;
    private readonly IConfiguration _configuration;

    public AzureSpeechToTextService(ILogger<AzureSpeechToTextService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> TranscribeAsync(byte[] audioData, string audioFormat = "wav")
    {
        var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
        var region = _configuration["AzureSpeech:Region"];

        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            _logger.LogWarning("Azure Speech credentials not configured. Returning dummy transcription.");
            return "[Transcribed audio - Azure Speech not configured]";
        }

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var audioStream = new MemoryStream(audioData);
        using var audioInputStream = AudioInputStream.CreatePushStream();

        // Push audio data to the stream
        audioInputStream.Write(audioData);
        audioInputStream.Close();

        using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        _logger.LogInformation("Starting speech recognition...");
        var result = await recognizer.RecognizeOnceAsync();

        switch (result.Reason)
        {
            case ResultReason.RecognizedSpeech:
                _logger.LogInformation("Recognized: {Text}", result.Text);
                return result.Text;
            case ResultReason.NoMatch:
                _logger.LogWarning("No speech could be recognized.");
                return string.Empty;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(result);
                _logger.LogError("Speech recognition canceled: {Reason}. Error: {ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorDetails);
                throw new Exception($"Speech recognition canceled: {cancellation.ErrorDetails}");
            default:
                return string.Empty;
        }
    }

    public async Task<string> TranscribeStreamAsync(Stream audioStream, string audioFormat = "wav")
    {
        var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
        var region = _configuration["AzureSpeech:Region"];

        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            _logger.LogWarning("Azure Speech credentials not configured. Returning dummy transcription.");
            return "[Transcribed audio stream - Azure Speech not configured]";
        }

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var pushStream = AudioInputStream.CreatePushStream();
        using var audioConfig = AudioConfig.FromStreamInput(pushStream);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        // Copy stream data to push stream
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            pushStream.Write(buffer.AsMemory(0, bytesRead).ToArray());
        }
        pushStream.Close();

        _logger.LogInformation("Starting stream speech recognition...");
        var result = await recognizer.RecognizeOnceAsync();

        switch (result.Reason)
        {
            case ResultReason.RecognizedSpeech:
                _logger.LogInformation("Recognized: {Text}", result.Text);
                return result.Text;
            case ResultReason.NoMatch:
                _logger.LogWarning("No speech could be recognized from stream.");
                return string.Empty;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(result);
                _logger.LogError("Stream speech recognition canceled: {Reason}. Error: {ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorDetails);
                throw new Exception($"Stream speech recognition canceled: {cancellation.ErrorDetails}");
            default:
                return string.Empty;
        }
    }
}
