using PhoneAgent.Interfaces;
using System.Collections.Concurrent;

namespace PhoneAgent.Services;

/// <summary>
/// Testing implementation of phone service that simulates phone calls with audio buffering
/// </summary>
public class TestingPhoneService : IPhoneService
{
    private readonly ILogger<TestingPhoneService> _logger;
    private readonly ConcurrentDictionary<string, CallSession> _activeCalls = new();

    public TestingPhoneService(ILogger<TestingPhoneService> logger)
    {
        _logger = logger;
    }

    public Task<bool> InitiateCallAsync(string phoneNumber, string sessionId)
    {
        _logger.LogInformation("Testing: Initiating call to {PhoneNumber} with session {SessionId}", phoneNumber, sessionId);

        var session = new CallSession
        {
            SessionId = sessionId,
            PhoneNumber = phoneNumber,
            Status = "active",
            StartTime = DateTime.UtcNow
        };

        _activeCalls.TryAdd(sessionId, session);
        return Task.FromResult(true);
    }

    public Task<bool> EndCallAsync(string sessionId)
    {
        _logger.LogInformation("Testing: Ending call for session {SessionId}", sessionId);

        if (_activeCalls.TryRemove(sessionId, out var session))
        {
            session.Status = "ended";
            session.EndTime = DateTime.UtcNow;
            _logger.LogInformation("Testing: Call duration: {Duration} seconds",
                (session.EndTime.Value - session.StartTime).TotalSeconds);
            return Task.FromResult(true);
        }

        _logger.LogWarning("Testing: Session {SessionId} not found", sessionId);
        return Task.FromResult(false);
    }

    public Task<bool> SendAudioAsync(string sessionId, byte[] audioData)
    {
        _logger.LogInformation("Testing: Sending {AudioLength} bytes of audio to session {SessionId}",
            audioData.Length, sessionId);

        if (_activeCalls.TryGetValue(sessionId, out var session))
        {
            session.OutgoingAudioBuffer.Add(audioData);
            session.TotalAudioBytesSent += audioData.Length;
            _logger.LogInformation("Testing: Total audio sent in session {SessionId}: {TotalBytes} bytes",
                sessionId, session.TotalAudioBytesSent);
            return Task.FromResult(true);
        }

        _logger.LogWarning("Testing: Session {SessionId} not found when sending audio", sessionId);
        return Task.FromResult(false);
    }

    public Task<string> GetCallStatusAsync(string sessionId)
    {
        if (_activeCalls.TryGetValue(sessionId, out var session))
        {
            _logger.LogInformation("Testing: Call status for session {SessionId}: {Status}", sessionId, session.Status);
            return Task.FromResult(session.Status);
        }

        _logger.LogInformation("Testing: Session {SessionId} not found", sessionId);
        return Task.FromResult("not_found");
    }

    public CallSession? GetCallSession(string sessionId)
    {
        _activeCalls.TryGetValue(sessionId, out var session);
        return session;
    }

    public IEnumerable<CallSession> GetAllActiveSessions()
    {
        return _activeCalls.Values.Where(s => s.Status == "active");
    }
}

public class CallSession
{
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<byte[]> OutgoingAudioBuffer { get; set; } = new();
    public long TotalAudioBytesSent { get; set; }
}
