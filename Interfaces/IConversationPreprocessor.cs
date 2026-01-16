using PhoneAgent.Models;

namespace PhoneAgent.Interfaces;

/// <summary>
/// Service for preprocessing conversation text before sending to LLM.
/// Handles PII detection, anonymization, and content filtering.
/// </summary>
public interface IConversationPreprocessor
{
    /// <summary>
    /// Preprocesses user input before sending to LLM.
    /// Detects and anonymizes PII, applies content filtering.
    /// </summary>
    /// <param name="text">The raw transcribed text from speech-to-text</param>
    /// <param name="sessionId">The conversation session ID for maintaining anonymization context</param>
    /// <returns>Preprocessing result with processed text and detection metadata</returns>
    Task<PreprocessingResult> PreprocessAsync(string text, string sessionId);
    
    /// <summary>
    /// Reverses anonymization in LLM response if needed.
    /// Maps anonymized tokens back to original values for natural responses.
    /// </summary>
    /// <param name="text">The LLM response text</param>
    /// <param name="sessionId">The conversation session ID to retrieve anonymization map</param>
    /// <returns>Text with anonymized tokens replaced with original values</returns>
    Task<string> PostprocessAsync(string text, string sessionId);
}
