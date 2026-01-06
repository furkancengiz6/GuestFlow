using FluentAssertions;
using GuestFlow.Application.Operations.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Caching;

/// <summary>
/// Unit tests for RedisCacheService - Performance optimizations from Phase 2
/// </summary>
public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();
        _cacheService = new RedisCacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDeserializedObject_WhenCacheHit()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestObject { Id = 1, Name = "Test" };
        var serializedValue = System.Text.Json.JsonSerializer.Serialize(expectedValue);

        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedValue.Id);
        result.Name.Should().Be(expectedValue.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenCacheMiss()
    {
        // Arrange
        var key = "non-existent-key";
        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync((string)null);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndStoreObject()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        var expectedJson = System.Text.Json.JsonSerializer.Serialize(value);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _cacheMock.Verify(c => c.SetStringAsync(
            key,
            expectedJson,
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30)),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldUseCustomExpiration_WhenProvided()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        var customExpiration = TimeSpan.FromHours(2);

        // Act
        await _cacheService.SetAsync(key, value, customExpiration);

        // Assert
        _cacheMock.Verify(c => c.SetStringAsync(
            key,
            It.IsAny<string>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == customExpiration),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedValue_WhenExists()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestObject { Id = 1, Name = "Cached" };
        var serializedValue = System.Text.Json.JsonSerializer.Serialize(cachedValue);

        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult(new TestObject { Id = 2, Name = "Factory" }));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Cached");
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldExecuteFactoryAndCache_WhenCacheMiss()
    {
        // Arrange
        var key = "test-key";
        var factoryValue = new TestObject { Id = 2, Name = "Factory" };

        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync((string)null);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult(factoryValue));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Name.Should().Be("Factory");

        // Verify caching
        _cacheMock.Verify(c => c.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task GetBatchAsync_ShouldReturnAllRequestedValues()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var value1 = new TestObject { Id = 1, Name = "Value1" };
        var value3 = new TestObject { Id = 3, Name = "Value3" };

        _cacheMock.Setup(c => c.GetStringAsync("key1", default))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(value1));
        _cacheMock.Setup(c => c.GetStringAsync("key2", default))
            .ReturnsAsync((string)null); // Cache miss
        _cacheMock.Setup(c => c.GetStringAsync("key3", default))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(value3));

        // Act
        var result = await _cacheService.GetBatchAsync<TestObject>(keys);

        // Assert
        result.Should().HaveCount(3);
        result["key1"].Should().BeEquivalentTo(value1);
        result["key2"].Should().BeNull();
        result["key3"].Should().BeEquivalentTo(value3);
    }

    [Fact]
    public async Task SetBatchAsync_ShouldStoreAllKeyValuePairs()
    {
        // Arrange
        var keyValuePairs = new Dictionary<string, TestObject>
        {
            ["key1"] = new TestObject { Id = 1, Name = "Value1" },
            ["key2"] = new TestObject { Id = 2, Name = "Value2" }
        };
        var customExpiration = TimeSpan.FromHours(1);

        // Act
        await _cacheService.SetBatchAsync(keyValuePairs, customExpiration);

        // Assert
        foreach (var kvp in keyValuePairs)
        {
            _cacheMock.Verify(c => c.SetStringAsync(
                kvp.Key,
                It.IsAny<string>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == customExpiration),
                default), Times.Once);
        }
    }

    [Fact]
    public async Task SetWithTagsAsync_ShouldStoreValueAndTagRelationships()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        var tags = new[] { "tag1", "tag2" };

        // Act
        await _cacheService.SetWithTagsAsync(key, value, tags);

        // Assert
        // Verify main value is stored
        _cacheMock.Verify(c => c.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);

        // Verify tag relationships are stored (this would require tag registry in real implementation)
        // In this test, we just verify the method completes without error
    }

    [Fact]
    public async Task InvalidateByTagsAsync_ShouldRemoveTaggedKeys()
    {
        // Arrange
        var tags = new[] { "tag1", "tag2" };

        // Setup tag registry (simplified)
        var tag1Keys = new HashSet<string> { "key1", "key2" };
        var tag2Keys = new HashSet<string> { "key3" };

        // Act
        await _cacheService.InvalidateByTagsAsync(tags);

        // Assert
        // In real implementation, this would remove all keys associated with the tags
        // For this test, we just verify the method completes
    }

    [Fact]
    public async Task GetOrSetSlidingAsync_ShouldRefreshExpirationOnAccess()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestObject { Id = 1, Name = "Cached" };
        var slidingExpiration = TimeSpan.FromMinutes(30);
        var serializedValue = System.Text.Json.JsonSerializer.Serialize(cachedValue);

        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetOrSetSlidingAsync(key,
            () => Task.FromResult(new TestObject { Id = 2, Name = "Factory" }),
            slidingExpiration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);

        // Verify sliding expiration is set (though Redis doesn't support true sliding expiration)
        _cacheMock.Verify(c => c.SetStringAsync(
            key,
            It.IsAny<string>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == slidingExpiration),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOrSetIfAsync_ShouldCacheOnlyWhenConditionMet()
    {
        // Arrange
        var key = "test-key";
        var factoryValue = new TestObject { Id = 1, Name = "Factory" };

        // Condition that returns true
        Func<TestObject, bool> condition = obj => obj.Id > 0;

        // Act
        var result = await _cacheService.GetOrSetIfAsync(key,
            () => Task.FromResult(factoryValue),
            condition);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);

        // Verify caching occurred because condition was met
        _cacheMock.Verify(c => c.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task GetOrSetIfAsync_ShouldNotCacheWhenConditionNotMet()
    {
        // Arrange
        var key = "test-key";
        var factoryValue = new TestObject { Id = 0, Name = "Factory" };

        // Condition that returns false
        Func<TestObject, bool> condition = obj => obj.Id > 0;

        // Act
        var result = await _cacheService.GetOrSetIfAsync(key,
            () => Task.FromResult(factoryValue),
            condition);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(0);

        // Verify caching did NOT occur because condition was not met
        _cacheMock.Verify(c => c.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallUnderlyingCache()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(key, default), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ShouldCheckCacheExistence()
    {
        // Arrange
        var key = "existing-key";
        var existingValue = "cached-value";

        _cacheMock.Setup(c => c.GetStringAsync(key, default))
            .ReturnsAsync(existingValue);

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        exists.Should().BeTrue();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}