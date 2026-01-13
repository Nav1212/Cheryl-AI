namespace PhoneAgent.Models;

public class PhoneCallRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string AudioData { get; set; } = string.Empty; // Base64 encoded audio
    public string AudioFormat { get; set; } = "wav"; // wav, mp3, etc.
}
