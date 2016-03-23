using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using SqlProcBinder;

namespace CodeGenerator.Tests
{
    public class TestDbContext : IDbContext
    {
        private DbConnection _connection;

        public TestDbContext(DbConnection connection)
        {
            _connection = connection;
        }

        public IDbCommandContext CreateCommand()
        {
            return new TestDbCommandContext(_connection, _connection.CreateCommand());
        }
    }
}
 