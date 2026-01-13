namespace PhoneAgent.Models;

public class PhoneCallResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string TranscribedText { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
    public string ResponseAudioData { get; set; } = string.Empty; // Base64 encoded audio
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
