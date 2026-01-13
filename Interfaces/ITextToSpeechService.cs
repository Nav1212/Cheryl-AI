namespace PhoneAgent.Interfaces;

public interface ITextToSpeechService
{
    /// <summary>
    /// Converts text to speech audio data
    /// </summary>
    Task<byte[]> SynthesizeAsync(string text, string voiceName = "en-US-JennyNeural");

    /// <summary>
    /// Gets available voice options
    /// </summary>
    Task<List<string>> GetAvailableVoicesAsync();
}
