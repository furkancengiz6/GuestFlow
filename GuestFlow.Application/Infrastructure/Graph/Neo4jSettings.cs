namespace GuestFlow.Application.Infrastructure.Graph
{
    public class Neo4jSettings
    {
        public string Uri { get; set; } = "bolt://localhost:7687";
        public string Username { get; set; } = "neo4j";
        public string Password { get; set; } = "password";
        public bool IsEnabled { get; set; } = true;
    }
}
