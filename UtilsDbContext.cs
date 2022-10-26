using System.Data;
using System.Data.SqlClient;

namespace Utils
{
    public class UtilsDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UtilsDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
