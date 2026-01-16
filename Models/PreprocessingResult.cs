namespace PhoneAgent.Models;

public class PreprocessingResult
{
    public string ProcessedText { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public List<PiiDetection> DetectedPii { get; set; } = new();
    public Dictionary<string, string> AnonymizationMap { get; set; } = new();
    public bool WasModified { get; set; }
}

public class PiiDetection
{
    public string Type { get; set; } = string.Empty;
    public string OriginalValue { get; set; } = string.Empty;
    public string ReplacementValue { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int Length { get; set; }
}
