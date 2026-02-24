using System.Data;
using Npgsql;

namespace server.Data.Persistence;

public class WebSocketDbContext(IConfiguration configuration)
{
    private readonly IDbConnection _dbConnection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
    public IDbConnection DbConnection => _dbConnection;
}