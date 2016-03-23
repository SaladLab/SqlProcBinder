using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeGenerator.Tests
{
    public class DbProcTest : IClassFixture<Database>
    {
        private Database _db;

        public DbProcTest(Database db)
        {
            _db = db;
        }

        [Fact]
        async Task Execute_StoredProcedure_WithOutput_Succeeded()
        {
            var ret = await CodeGen.SumInt.ExecuteAsync(_db.DbContext, 1, 2);
            Assert.Equal(3, ret.answer);
        }

        [Fact]
        async Task Execute_StoredProcedure_ReturnRowset_Succeeded()
        {
            var ret = await CodeGen.GenerateInt.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10), values);
        }
    }
}
