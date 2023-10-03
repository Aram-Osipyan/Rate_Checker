namespace RateChecker.Domain;
public class TokenRefreshInput
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string CaptureSolverKey { get; set; }
    public string ImapEmail { get; set; }
    public string ImapPassword { get; set; }
    public string AuthenticatorKey { get; set; }
}
