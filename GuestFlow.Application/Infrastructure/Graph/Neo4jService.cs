using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public class Neo4jService : INeo4jService
    {
        private readonly IDriver _driver;
        private readonly ILogger<Neo4jService> _logger;
        private readonly Neo4jSettings _settings;

        public Neo4jService(IOptions<Neo4jSettings> settings, ILogger<Neo4jService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            if (_settings.IsEnabled)
            {
                try
                {
                    _driver = GraphDatabase.Driver(_settings.Uri, AuthTokens.Basic(_settings.Username, _settings.Password));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Neo4j driver. Graph features will be disabled.");
                }
            }
        }

        public async Task<List<T>> RunReadQueryAsync<T>(string query, object? parameters = null, Func<IRecord, T>? mapFunc = null)
        {
            if (_driver == null) return new List<T>();

            try
            {
                await using var session = _driver.AsyncSession();
                return await session.ExecuteReadAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, parameters);
                    var list = new List<T>();
                    while (await result.FetchAsync())
                    {
                        if (mapFunc != null)
                        {
                            list.Add(mapFunc(result.Current));
                        }
                        else
                        {
                            // Default mapping logic if needed, or throw
                             if (typeof(T) == typeof(IRecord))
                             {
                                 list.Add((T)result.Current);
                             }
                        }
                    }
                    return list;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Neo4j read query: {Query}", query);
                throw;
            }
        }

        public async Task<IResultCursor> RunWriteQueryAsync(string query, object? parameters = null)
        {
            if (_driver == null) return null;

            try
            {
                await using var session = _driver.AsyncSession();
                return await session.ExecuteWriteAsync(async tx =>
                {
                    return await tx.RunAsync(query, parameters);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Neo4j write query: {Query}", query);
                throw;
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            if (_driver == null) return false;

            try
            {
                await _driver.VerifyConnectivityAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neo4j connectivity check failed.");
                return false;
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_driver != null)
            {
                await _driver.DisposeAsync();
            }
        }
    }
}
