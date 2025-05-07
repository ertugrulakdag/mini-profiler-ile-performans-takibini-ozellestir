using System.Data;
using Microsoft.Data.SqlClient;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace WebAppForMiniProfiler.Context
{
    public class DapperContext : IDisposable
    {
        private readonly ILogger<DapperContext> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private IDbConnection? _connection;
        private SqlConnection? _sqlConnection;

        public DapperContext(IConfiguration configuration, ILogger<DapperContext> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                ?? throw new Exception("DefaultConnection string not found.");
        }

        public IDbConnection CreateConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _sqlConnection = new SqlConnection(_connectionString);

                try
                {
                    using (MiniProfiler.Current.Step("SQL Connection Open"))
                    {
                        _sqlConnection.Open();
                        _logger.LogInformation("Database connection opened successfully.");
                    }

                    // MiniProfiler null değilse sarmalla
                    if (MiniProfiler.Current != null)
                    {
                        using (MiniProfiler.Current.Step("MiniProfiler Wrapping"))
                        {
                            _connection = new ProfiledDbConnection(_sqlConnection, MiniProfiler.Current);
                            _logger.LogInformation("Database connection wrapped with MiniProfiler.");
                        }
                    }
                    else
                    {
                        _connection = _sqlConnection;
                        _logger.LogWarning("MiniProfiler.Current is null. Profiling is disabled for this connection.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open database connection.");
                    throw;
                }
            }
            return _connection;
        }

        public void Dispose()
        {
            if (_sqlConnection?.State == ConnectionState.Open)
            {
                using (MiniProfiler.Current.Step("SQL Connection Close"))
                {
                    _sqlConnection.Close();
                    _logger.LogInformation("Database connection closed successfully.");
                }
            }

            _sqlConnection?.Dispose();
            _connection?.Dispose();
        }
    }
}
