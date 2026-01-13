namespace PhoneAgent.Models;

public class EmailRequest
{
    public string EmailContent { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
