namespace PhoneAgent.Interfaces;

public interface ISpeechToTextService
{
    /// <summary>
    /// Transcribes audio data to text
    /// </summary>
    Task<string> TranscribeAsync(byte[] audioData, string audioFormat = "wav");

    /// <summary>
    /// Transcribes audio stream to text in real-time
    /// </summary>
    Task<string> TranscribeStreamAsync(Stream audioStream, string audioFormat = "wav");
}
