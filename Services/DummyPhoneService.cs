using PhoneAgent.Interfaces;

namespace PhoneAgent.Services;

public class DummyPhoneService : IPhoneService
{
    private readonly ILogger<DummyPhoneService> _logger;
    private readonly Dictionary<string, string> _callStatuses = new();

    public DummyPhoneService(ILogger<DummyPhoneService> logger)
    {
        _logger = logger;
    }

    public Task<bool> InitiateCallAsync(string phoneNumber, string sessionId)
    {
        _logger.LogInformation("Dummy: Initiating call to {PhoneNumber} with session {SessionId}", phoneNumber, sessionId);
        _callStatuses[sessionId] = "active";
        return Task.FromResult(true);
    }

    public Task<bool> EndCallAsync(string sessionId)
    {
        _logger.LogInformation("Dummy: Ending call for session {SessionId}", sessionId);
        _callStatuses[sessionId] = "ended";
        return Task.FromResult(true);
    }

    public Task<bool> SendAudioAsync(string sessionId, byte[] audioData)
    {
        _logger.LogInformation("Dummy: Sending {AudioLength} bytes of audio to session {SessionId}", audioData.Length, sessionId);
        return Task.FromResult(true);
    }

    public Task<string> GetCallStatusAsync(string sessionId)
    {
        var status = _callStatuses.TryGetValue(sessionId, out var callStatus) ? callStatus : "not_found";
        _logger.LogInformation("Dummy: Call status for session {SessionId}: {Status}", sessionId, status);
        return Task.FromResult(status);
    }
}
