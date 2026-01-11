using System.Security.Claims;
using System.Text.Encodings.Web;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GuestFlow.Application.Tests.Integration;

/// <summary>
/// Simple always-authenticated handler for integration tests.
/// Returns an Admin principal so role-protected endpoints can be exercised without going through /auth/login.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userType = UserType.Admin.ToString();

        var claims = new[]
        {
            new Claim("id", "1"),
            new Claim("PersonnelId", "1"),
            new Claim("Email", "integration@guestflow.local"),
            new Claim("FullName", "Integration Test User"),
            new Claim("UserType", userType),
            new Claim(ClaimTypes.Role, userType),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

