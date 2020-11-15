namespace DataLayer
{
    public interface IDatabaseConnectionString
    {
    }

    public class DatabaseConnectionString : IDatabaseConnectionString
    {
        public DatabaseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}