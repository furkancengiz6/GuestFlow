// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Configuration;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    /// <summary>
    /// Neo4j graph database service implementation
    /// </summary>
    public class Neo4jService : INeo4jService, IDisposable
    {
        private readonly Neo4jSettings _settings;
        private readonly ILogger<Neo4jService> _logger;
        private IDriver? _driver;

        public Neo4jService(IOptions<Neo4jSettings> settings, ILogger<Neo4jService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            if (_settings.Enabled)
            {
                InitializeDriver();
            }
        }

        public IDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    throw new InvalidOperationException("Neo4j driver is not initialized. Check configuration.");
                }
                return _driver;
            }
        }

        private void InitializeDriver()
        {
            try
            {
                _driver = GraphDatabase.Driver(
                    _settings.Uri,
                    AuthTokens.Basic(_settings.Username, _settings.Password),
                    config => config
                        .WithConnectionTimeout(TimeSpan.FromSeconds(_settings.ConnectionTimeoutSeconds))
                        .WithMaxConnectionPoolSize(_settings.MaxConnectionPoolSize)
                );

                _logger.LogInformation("Neo4j driver initialized successfully. URI: {Uri}", _settings.Uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Neo4j driver");
                throw;
            }
        }

        public async Task<T?> ExecuteReadAsync<T>(Func<IAsyncQueryRunner, Task<T>> work)
        {
            if (!_settings.Enabled || _driver == null)
            {
                _logger.LogWarning("Neo4j is disabled or not initialized. Skipping read operation.");
                return default(T);
            }

            var session = _driver.AsyncSession(config =>
            {
                if (!string.IsNullOrEmpty(_settings.Database))
                {
                    config.WithDatabase(_settings.Database);
                }
            });

            try
            {
                return await session.ExecuteReadAsync(work);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Neo4j read operation");
                throw;
            }
            finally
            {
                await session.DisposeAsync();
            }
        }

        public async Task<T?> ExecuteWriteAsync<T>(Func<IAsyncQueryRunner, Task<T>> work)
        {
            if (!_settings.Enabled || _driver == null)
            {
                _logger.LogWarning("Neo4j is disabled or not initialized. Skipping write operation.");
                return default(T);
            }

            var session = _driver.AsyncSession(config =>
            {
                if (!string.IsNullOrEmpty(_settings.Database))
                {
                    config.WithDatabase(_settings.Database);
                }
            });

            try
            {
                return await session.ExecuteWriteAsync(work);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Neo4j write operation");
                throw;
            }
            finally
            {
                await session.DisposeAsync();
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (!_settings.Enabled || _driver == null)
            {
                return false;
            }

            try
            {
                var session = _driver.AsyncSession();
                try
                {
                    var result = await session.RunAsync("RETURN 1 as test");
                    var record = await result.SingleAsync();
                    return record["test"].As<int>() == 1;
                }
                finally
                {
                    await session.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neo4j connection test failed");
                return false;
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
