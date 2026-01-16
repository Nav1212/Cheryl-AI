namespace PhoneAgent.Configuration;

public class PreprocessingOptions
{
    public bool Enabled { get; set; } = true;
    public PiiDetectionOptions PiiDetection { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public Dictionary<string, string> PiiPatterns { get; set; } = new();
}

public class PiiDetectionOptions
{
    public bool Enabled { get; set; } = true;
    public RedactionMode RedactionMode { get; set; } = RedactionMode.Anonymize;
    public List<string> PiiCategories { get; set; } = new();
}

public class LoggingOptions
{
    public bool LogOriginalText { get; set; } = false;
    public bool LogProcessedText { get; set; } = true;
    public bool LogDetections { get; set; } = false;
}

public enum RedactionMode
{
    /// <summary>
    /// Replace PII with labeled tokens like [PHONE], [EMAIL]
    /// </summary>
    Anonymize,
    
    /// <summary>
    /// Remove PII entirely from text
    /// </summary>
    Remove,
    
    /// <summary>
    /// Replace PII with asterisks (e.g., ***)
    /// </summary>
    Mask
}
