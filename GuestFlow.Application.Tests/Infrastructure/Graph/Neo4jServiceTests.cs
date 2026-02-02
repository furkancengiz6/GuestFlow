using FluentAssertions;
using GuestFlow.Application.Infrastructure.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace GuestFlow.Application.Tests.Infrastructure.Graph
{
    public class Neo4jServiceTests
    {
        private readonly Mock<IOptions<Neo4jSettings>> _settingsMock;
        private readonly Mock<ILogger<Neo4jService>> _loggerMock;

        public Neo4jServiceTests()
        {
            _settingsMock = new Mock<IOptions<Neo4jSettings>>();
            _loggerMock = new Mock<ILogger<Neo4jService>>();
        }

        [Fact]
        public async Task CheckConnectionAsync_Should_Return_False_When_Driver_Fails()
        {
            // Arrange
            _settingsMock.Setup(x => x.Value).Returns(new Neo4jSettings
            {
                Uri = "bolt://localhost:7687",
                Username = "neo4j",
                Password = "invalid_password",
                IsEnabled = true
            });

            // Note: Since we cannot easily mock the static GraphDatabase.Driver method without a wrapper,
            // we are testing the actual connection failure or the handling of it.
            // However, this test expects that a real connection to localhost might fail if not running,
            // or if we want to unit test purely logic, we might need a wrapper.
            // For this phase, we will assume we are testing the "Service" logic which handles exceptions.
            
            // To properly Unit Test this without a real DB, we rely on the fact that
            // checking connection against a non-existent DB will throw/return false.
            
            var service = new Neo4jService(_settingsMock.Object, _loggerMock.Object);

            // Act
            var result = await service.CheckConnectionAsync();

            // Assert
            // If DB is down, it returns false. If DB is up but auth fails, it might return false or throw inside CheckConnectionAsync 
            // depending on implementation. My implementation catches exceptions and returns false.
            result.Should().BeFalse();
        }
        
        [Fact]
        public void Constructor_Should_Not_Throw_When_Settings_Valid()
        {
             // Arrange
            _settingsMock.Setup(x => x.Value).Returns(new Neo4jSettings
            {
                Uri = "bolt://localhost:7687",
                Username = "neo4j",
                Password = "password",
                IsEnabled = true
            });

            // Act
            var action = () => new Neo4jService(_settingsMock.Object, _loggerMock.Object);

            // Assert
            action.Should().NotThrow();
        }
    }
}
