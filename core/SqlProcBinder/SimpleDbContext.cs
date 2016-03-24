using System.Data.Common;

namespace SqlProcBinder
{
    public class SimpleDbContext : IDbContext
    {
        private DbConnection _connection;

        public SimpleDbContext(DbConnection connection)
        {
            _connection = connection;
        }

        public IDbCommandContext CreateCommand()
        {
            return new SimpleDbCommandContext(_connection.CreateCommand());
        }
    }
}
