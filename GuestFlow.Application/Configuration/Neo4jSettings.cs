// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Neo4j database configuration settings
    /// </summary>
    public class Neo4jSettings
    {
        public const string SectionName = "Neo4j";

        /// <summary>
        /// Neo4j server URI (e.g., bolt://localhost:7687)
        /// </summary>
        public string Uri { get; set; } = "bolt://localhost:7687";

        /// <summary>
        /// Neo4j username
        /// </summary>
        public string Username { get; set; } = "neo4j";

        /// <summary>
        /// Neo4j password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Database name (Neo4j 4.0+)
        /// </summary>
        public string Database { get; set; } = "neo4j";

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Max connection pool size
        /// </summary>
        public int MaxConnectionPoolSize { get; set; } = 50;

        /// <summary>
        /// Enable Neo4j integration
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
