using FluentAssertions;
using GuestFlow.Application.Operations.Cache;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Caching;

/// <summary>
/// Unit tests for InMemoryCacheService - Production cache implementation
/// </summary>
public class InMemoryCacheServiceTests : IDisposable
{
    private readonly InMemoryCacheService _cacheService;
    private readonly MemoryCache _memoryCache;

    public InMemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new InMemoryCacheService(_memoryCache);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCachedValue_WhenExists()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, expectedValue);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedValue.Id);
        result.Name.Should().Be(expectedValue.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenCacheMiss()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(value.Id);
        result.Name.Should().Be(value.Name);
    }

    [Fact]
    public async Task SetAsync_ShouldUseCustomExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromSeconds(1);

        // Act
        await _cacheService.SetAsync(key, value, expiration);
        await Task.Delay(1100); // Wait for expiration
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull(); // Should be expired
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedValue_WhenExists()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestObject { Id = 1, Name = "Cached" };

        // Pre-populate cache
        await _cacheService.SetAsync(key, cachedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult(new TestObject { Id = 2, Name = "Factory" }));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Cached");
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldExecuteFactory_WhenCacheMiss()
    {
        // Arrange
        var key = "test-key";
        var factoryValue = new TestObject { Id = 2, Name = "Factory" };

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult(factoryValue));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Name.Should().Be("Factory");

        // Verify it was cached
        var cachedResult = await _cacheService.GetAsync<TestObject>(key);
        cachedResult.Should().NotBeNull();
        cachedResult!.Id.Should().Be(2);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var key = "existing-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value);

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        exists.Should().BeFalse();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}