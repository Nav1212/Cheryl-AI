using Microsoft.CognitiveServices.Speech;
using PhoneAgent.Interfaces;

namespace PhoneAgent.Services;

public class AzureTextToSpeechService : ITextToSpeechService
{
    private readonly ILogger<AzureTextToSpeechService> _logger;
    private readonly IConfiguration _configuration;

    public AzureTextToSpeechService(ILogger<AzureTextToSpeechService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<byte[]> SynthesizeAsync(string text, string voiceName = "en-US-JennyNeural")
    {
        var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
        var region = _configuration["AzureSpeech:Region"];

        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            _logger.LogWarning("Azure Speech credentials not configured. Returning empty audio data.");
            return Array.Empty<byte>();
        }

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        speechConfig.SpeechSynthesisVoiceName = voiceName;

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        _logger.LogInformation("Synthesizing text: {Text} with voice: {Voice}", text, voiceName);
        var result = await synthesizer.SpeakTextAsync(text);

        switch (result.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                _logger.LogInformation("Speech synthesized successfully. Audio data length: {Length} bytes", result.AudioData.Length);
                return result.AudioData;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogError("Speech synthesis canceled: {Reason}. Error: {ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorDetails);
                throw new Exception($"Speech synthesis canceled: {cancellation.ErrorDetails}");
            default:
                _logger.LogWarning("Speech synthesis failed with reason: {Reason}", result.Reason);
                return Array.Empty<byte>();
        }
    }

    public async Task<List<string>> GetAvailableVoicesAsync()
    {
        var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
        var region = _configuration["AzureSpeech:Region"];

        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            _logger.LogWarning("Azure Speech credentials not configured. Returning default voices.");
            return new List<string>
            {
                "en-US-JennyNeural",
                "en-US-GuyNeural",
                "en-US-AriaNeural",
                "en-US-DavisNeural"
            };
        }

        var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);
        var voicesResult = await synthesizer.GetVoicesAsync();

        if (voicesResult.Reason == ResultReason.VoicesListRetrieved)
        {
            _logger.LogInformation("Retrieved {Count} available voices", voicesResult.Voices.Count);
            return voicesResult.Voices
                .Where(v => v.Locale.StartsWith("en-US"))
                .Select(v => v.ShortName)
                .ToList();
        }

        _logger.LogWarning("Failed to retrieve voices list");
        return new List<string> { "en-US-JennyNeural" };
    }
}
