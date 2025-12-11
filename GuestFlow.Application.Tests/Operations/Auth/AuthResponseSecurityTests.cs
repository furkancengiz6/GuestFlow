using System.Linq;
using FluentAssertions;
using GuestFlow.Api.Models;

namespace GuestFlow.Application.Tests.Operations.Auth;

public class AuthResponseSecurityTests
{
    [Fact]
    public void LoginResponse_ShouldNot_ExposeRefreshToken()
    {
        var propertyNames = typeof(LoginResponse).GetProperties().Select(p => p.Name).ToArray();
        propertyNames.Should().NotContain("RefreshToken");
    }

    [Fact]
    public void RefreshTokenResponse_ShouldNot_ExposeRefreshToken()
    {
        var propertyNames = typeof(RefreshTokenResponse).GetProperties().Select(p => p.Name).ToArray();
        propertyNames.Should().NotContain("RefreshToken");
    }
}

