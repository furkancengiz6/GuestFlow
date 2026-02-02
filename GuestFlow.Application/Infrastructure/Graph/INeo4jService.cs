using Neo4j.Driver;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public interface INeo4jService : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Executes a read query and returns the result.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        Task<List<T>> RunReadQueryAsync<T>(string query, object? parameters = null, Func<IRecord, T>? mapFunc = null);

        /// <summary>
        /// Executes a write query (create, update, delete).
        /// </summary>
        Task<IResultCursor> RunWriteQueryAsync(string query, object? parameters = null);
        
        /// <summary>
        /// Checks if the Neo4j connection is healthy.
        /// </summary>
        Task<bool> CheckConnectionAsync();
    }
}
