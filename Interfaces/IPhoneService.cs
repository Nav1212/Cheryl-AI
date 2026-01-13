namespace PhoneAgent.Interfaces;

public interface IPhoneService
{
    /// <summary>
    /// Initiates an outbound call to the specified phone number
    /// </summary>
    Task<bool> InitiateCallAsync(string phoneNumber, string sessionId);

    /// <summary>
    /// Ends an active call
    /// </summary>
    Task<bool> EndCallAsync(string sessionId);

    /// <summary>
    /// Sends audio to the active call
    /// </summary>
    Task<bool> SendAudioAsync(string sessionId, byte[] audioData);

    /// <summary>
    /// Gets the current status of a call session
    /// </summary>
    Task<string> GetCallStatusAsync(string sessionId);
}
