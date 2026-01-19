using Microsoft.AspNetCore.Mvc;
using PhoneAgent.Interfaces;
using PhoneAgent.Models;

namespace PhoneAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhoneController : ControllerBase
{
    private readonly ILogger<PhoneController> _logger;
    private readonly IPhoneService _phoneService;
    private readonly ISpeechToTextService _speechToTextService;
    private readonly ITextToSpeechService _textToSpeechService;

    public PhoneController(
        ILogger<PhoneController> logger,
        IPhoneService phoneService,
        ISpeechToTextService speechToTextService,
        ITextToSpeechService textToSpeechService)
    {
        _logger = logger;
        _phoneService = phoneService;
        _speechToTextService = speechToTextService;
        _textToSpeechService = textToSpeechService;
    }

    /// <summary>
    /// Process incoming phone call audio
    /// This endpoint is designed to be called by the LLM or phone system
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<PhoneCallResponse>> ProcessCall([FromBody] PhoneCallRequest request)
    {
        try
        {
            _logger.LogInformation("Processing call for session {SessionId}", request.SessionId);

            // Decode audio data
            var audioBytes = Convert.FromBase64String(request.AudioData);

            // Transcribe audio to text
            var transcribedText = await _speechToTextService.TranscribeAsync(audioBytes, request.AudioFormat);

            _logger.LogInformation("Transcribed text for session {SessionId}: {Text}", request.SessionId, transcribedText);

            // Return transcribed text - LLM will process this separately
            return Ok(new PhoneCallResponse
            {
                SessionId = request.SessionId,
                TranscribedText = transcribedText,
                ResponseText = string.Empty, // LLM will provide this
                ResponseAudioData = string.Empty, // Will be generated after LLM response
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing call for session {SessionId}", request.SessionId);
            return BadRequest(new PhoneCallResponse
            {
                SessionId = request.SessionId,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Convert text response to speech and send to phone
    /// This endpoint is designed to be called by the LLM with its response
    /// </summary>
    [HttpPost("respond")]
    public async Task<ActionResult> RespondToCall([FromBody] RespondRequest request)
    {
        try
        {
            _logger.LogInformation("Generating response for session {SessionId}", request.SessionId);

            // Convert text to speech
            var audioData = await _textToSpeechService.SynthesizeAsync(request.ResponseText, request.VoiceName);

            // Send audio to phone service
            var sendResult = await _phoneService.SendAudioAsync(request.SessionId, audioData);

            if (!sendResult)
            {
                return BadRequest(new { success = false, message = "Failed to send audio to phone service" });
            }

            _logger.LogInformation("Response sent for session {SessionId}", request.SessionId);

            return Ok(new
            {
                success = true,
                audioLength = audioData.Length,
                sessionId = request.SessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to call for session {SessionId}", request.SessionId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Initiate an outbound call
    /// </summary>
    [HttpPost("initiate")]
    public async Task<ActionResult> InitiateCall([FromBody] InitiateCallRequest request)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            var result = await _phoneService.InitiateCallAsync(request.PhoneNumber, sessionId);

            if (!result)
            {
                return BadRequest(new { success = false, message = "Failed to initiate call" });
            }

            return Ok(new { success = true, sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating call to {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// End an active call
    /// </summary>
    [HttpPost("end")]
    public async Task<ActionResult> EndCall([FromBody] EndCallRequest request)
    {
        try
        {
            var result = await _phoneService.EndCallAsync(request.SessionId);

            if (!result)
            {
                return BadRequest(new { success = false, message = "Failed to end call" });
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending call for session {SessionId}", request.SessionId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get call status
    /// </summary>
    [HttpGet("status/{sessionId}")]
    public async Task<ActionResult> GetCallStatus(string sessionId)
    {
        try
        {
            var status = await _phoneService.GetCallStatusAsync(sessionId);
            return Ok(new { sessionId, status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting call status for session {SessionId}", sessionId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class RespondRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
    public string VoiceName { get; set; } = "en-US-JennyNeural";
}

public class InitiateCallRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class EndCallRequest
{
    public string SessionId { get; set; } = string.Empty;
}
