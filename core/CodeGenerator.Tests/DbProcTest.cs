using System.Linq;
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
        private async Task Execute_StoredProcedure_WithOutput_Succeeded()
        {
            var ret = await Sql.SumInt.ExecuteAsync(_db.DbContext, 1, 2);
            Assert.Equal(3, ret.answer);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnRowset_Succeeded()
        {
            var ret = await Sql.GenerateInt.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnRowsetWithOutput_Succeeded()
        {
            var ret = await Sql.GenerateIntWithOutput.ExecuteAsync(_db.DbContext, 10);
            Assert.Equal(Enumerable.Range(1, 10), ret.Rows.Select(r => r.Value));
            Assert.Equal(ret.message, "COUNT:10");
        }
    }
}
