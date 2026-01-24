// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Neo4j.Driver;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    /// <summary>
    /// Neo4j graph database service interface
    /// </summary>
    public interface INeo4jService
    {
        /// <summary>
        /// Get Neo4j driver instance
        /// </summary>
        IDriver Driver { get; }

        /// <summary>
        /// Execute a read query
        /// </summary>
        Task<T?> ExecuteReadAsync<T>(Func<IAsyncQueryRunner, Task<T>> work);

        /// <summary>
        /// Execute a write query
        /// </summary>
        Task<T?> ExecuteWriteAsync<T>(Func<IAsyncQueryRunner, Task<T>> work);

        /// <summary>
        /// Test Neo4j connection
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Dispose resources
        /// </summary>
        void Dispose();
    }
}
