namespace PhoneAgent.Models;

public class EmailResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
