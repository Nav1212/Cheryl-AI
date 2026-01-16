using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PhoneAgent.Configuration;
using PhoneAgent.Interfaces;
using PhoneAgent.Models;

namespace PhoneAgent.Services;

/// <summary>
/// Default implementation of conversation preprocessing using regex-based PII detection.
/// Stores anonymization maps in memory per session.
/// </summary>
public class DefaultConversationPreprocessor : IConversationPreprocessor
{
    private readonly ILogger<DefaultConversationPreprocessor> _logger;
    private readonly PreprocessingOptions _options;
    
    // In-memory storage for anonymization maps per session
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _sessionAnonymizationMaps = new();
    
    // Counter for generating unique replacement tokens
    private readonly ConcurrentDictionary<string, int> _tokenCounters = new();

    public DefaultConversationPreprocessor(
        ILogger<DefaultConversationPreprocessor> logger,
        IOptions<PreprocessingOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<PreprocessingResult> PreprocessAsync(string text, string sessionId)
    {
        if (!_options.Enabled || !_options.PiiDetection.Enabled)
        {
            if (_options.Logging.LogProcessedText)
            {
                _logger.LogInformation("Preprocessing disabled. Returning original text.");
            }
            return new PreprocessingResult
            {
                ProcessedText = text,
                OriginalText = text,
                WasModified = false
            };
        }

        var result = new PreprocessingResult
        {
            OriginalText = text,
            ProcessedText = text
        };

        // Get or create session anonymization map
        var sessionMap = _sessionAnonymizationMaps.GetOrAdd(sessionId, _ => new Dictionary<string, string>());

        // Process each enabled PII category
        foreach (var category in _options.PiiDetection.PiiCategories)
        {
            if (_options.PiiPatterns.TryGetValue(category, out var pattern))
            {
                result = await DetectAndRedactPiiAsync(result, category, pattern, sessionId, sessionMap);
            }
        }

        // Log output if configured
        if (_options.Logging.LogProcessedText)
        {
            _logger.LogInformation("Preprocessed text for session {SessionId}: {ProcessedText}", 
                sessionId, result.ProcessedText);
        }

        return result;
    }

    public Task<string> PostprocessAsync(string text, string sessionId)
    {
        if (!_sessionAnonymizationMaps.TryGetValue(sessionId, out var sessionMap))
        {
            return Task.FromResult(text);
        }

        var processedText = text;
        
        // Reverse the anonymization map (swap keys and values)
        foreach (var kvp in sessionMap)
        {
            var originalValue = kvp.Key;
            var replacementToken = kvp.Value;
            
            // Replace tokens back with original values
            processedText = processedText.Replace(replacementToken, originalValue);
        }

        if (_options.Logging.LogProcessedText)
        {
            _logger.LogInformation("Postprocessed LLM response for session {SessionId}", sessionId);
        }

        return Task.FromResult(processedText);
    }

    private Task<PreprocessingResult> DetectAndRedactPiiAsync(
        PreprocessingResult result,
        string piiType,
        string pattern,
        string sessionId,
        Dictionary<string, string> sessionMap)
    {
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var matches = regex.Matches(result.ProcessedText);

        if (matches.Count == 0)
        {
            return Task.FromResult(result);
        }

        var processedText = result.ProcessedText;
        var detections = new List<PiiDetection>();
        var offset = 0; // Track offset due to text replacements

        foreach (Match match in matches)
        {
            var originalValue = match.Value;
            
            // Check if we've already anonymized this value in this session
            if (!sessionMap.TryGetValue(originalValue, out var replacementValue))
            {
                // Generate new replacement based on redaction mode
                replacementValue = GenerateReplacement(piiType, originalValue, sessionId);
                sessionMap[originalValue] = replacementValue;
                result.AnonymizationMap[originalValue] = replacementValue;
            }

            var detection = new PiiDetection
            {
                Type = piiType,
                OriginalValue = originalValue,
                ReplacementValue = replacementValue,
                StartIndex = match.Index - offset,
                Length = match.Length
            };

            detections.Add(detection);

            // Replace in text
            var adjustedIndex = match.Index - offset;
            processedText = processedText.Remove(adjustedIndex, match.Length)
                                         .Insert(adjustedIndex, replacementValue);
            
            // Update offset for subsequent matches
            offset += match.Length - replacementValue.Length;
            
            result.WasModified = true;
        }

        result.ProcessedText = processedText;
        result.DetectedPii.AddRange(detections);

        return Task.FromResult(result);
    }

    private string GenerateReplacement(string piiType, string originalValue, string sessionId)
    {
        return _options.PiiDetection.RedactionMode switch
        {
            RedactionMode.Anonymize => GenerateAnonymizedToken(piiType, sessionId),
            RedactionMode.Remove => "",
            RedactionMode.Mask => new string('*', Math.Min(originalValue.Length, 8)),
            _ => originalValue
        };
    }

    private string GenerateAnonymizedToken(string piiType, string sessionId)
    {
        var counterKey = $"{sessionId}_{piiType}";
        var counter = _tokenCounters.AddOrUpdate(counterKey, 1, (_, current) => current + 1);
        
        return $"[{piiType.ToUpper()}_{counter}]";
    }
}
