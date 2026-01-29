using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MarvinDateMcp.Api.Security;

/// <summary>
/// Authentication handler for API Key-based authentication
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key is provided in header
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            Logger.LogWarning("Missing API Key header from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key header"));
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            Logger.LogWarning("Empty API Key header from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Validate API key
        if (string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            Logger.LogError("API Key not configured in application settings");
            return Task.FromResult(AuthenticateResult.Fail("API Key not configured"));
        }

        if (!string.Equals(providedApiKey, Options.ApiKey, StringComparison.Ordinal))
        {
            Logger.LogWarning("Invalid API Key attempt from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Create authenticated user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser"),
            new Claim(ClaimTypes.Role, "ApiUser")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation("Successful API Key authentication from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.Append("WWW-Authenticate", $"{Scheme.Name} realm=\"API\"");
        Logger.LogWarning("Authentication challenge issued to {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        Logger.LogWarning("Forbidden access attempt from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Options for API Key authentication
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
