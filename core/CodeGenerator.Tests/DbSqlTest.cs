using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeGenerator.Tests
{
    public class DbSqlTest : IClassFixture<Database>
    {
        private Database _db;

        public DbSqlTest(Database db)
        {
            _db = db;
        }

        [Fact]
        async Task Execute_RunSql_Succeeded()
        {
            var ret = await CodeGen.SumIntBySql.ExecuteAsync(_db.DbContext, 1, 2);
            var result = (await ret.FetchAllRowsAndDisposeAsync(t => t.Value)).First();
            Assert.Equal(3, result);
        }
    }
}
