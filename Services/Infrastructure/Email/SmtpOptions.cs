namespace ALOud.Services;

public class SmtpOptions
{
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;

}
