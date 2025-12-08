using FluentAssertions;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Helpers;

/// <summary>
/// Base class for all unit tests
/// Provides common setup and helper methods
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Creates a mock instance of the specified type
    /// </summary>
    protected Mock<T> CreateMock<T>() where T : class
    {
        return new Mock<T>();
    }

    /// <summary>
    /// Creates a mock instance with strict behavior
    /// </summary>
    protected Mock<T> CreateStrictMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Strict);
    }

    /// <summary>
    /// Creates a mock instance with loose behavior
    /// </summary>
    protected Mock<T> CreateLooseMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Loose);
    }
}

