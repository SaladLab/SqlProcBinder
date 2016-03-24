using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        private async Task Execute_StoredProcedure_WithOutput_Succeeded()
        {
            var ret = await Sql.SumInt.ExecuteAsync(_db.DbContext, 1, 2);
            Assert.Equal(3, ret.answer);
        }

        [Fact]
        private async Task Execute_StoredProcedure_WithOutputAndReturn_Succeeded()
        {
            var ret = await Sql.SumIntWithReturn.ExecuteAsync(_db.DbContext, 1, 2);
            Assert.Equal(3, ret.answer);
            Assert.Equal(3, ret.Return);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnRowset_Succeeded()
        {
            var ret = await Sql.GenerateInt.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnRowsetByDataReader_Succeeded()
        {
            var ret = await Sql.GenerateIntByRowset.ExecuteAsync(_db.DbContext, 10);
            var values = new List<int>();
            using (ret.Rowset)
            {
                while (await ret.Rowset.ReadAsync())
                    values.Add(ret.Rowset.GetInt32(0));
            }
            Assert.Equal(Enumerable.Range(1, 10), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnRowsetWithOutput_Succeeded()
        {
            var ret = await Sql.GenerateIntWithOutput.ExecuteAsync(_db.DbContext, 10);
            Assert.Equal(Enumerable.Range(1, 10), ret.Rows.Select(r => r.Value));
            Assert.Equal(ret.message, "COUNT:10");
        }


        [Fact]
        private async Task Execute_StoredProcedure_RaiseError_Failed()
        {
            await Assert.ThrowsAsync<SqlException>(async () =>
            {
                await Sql.RaiseError.ExecuteAsync(_db.DbContext, "TestErrorMessage");
            });
        }

        [Fact]
        private async Task Execute_InOutTest_Succeeded()
        {
            var ret = await Sql.EchoParameters.ExecuteAsync(
                _db.DbContext,
                true, 1, 1, 1, 1, 1f, 1.0, 1M, 1M,
                new DateTime(2001, 1, 1, 1, 1, 0),
                new DateTime(2001, 1, 1),
                new DateTime(2001, 1, 1, 1, 1, 1),
                new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)),
                new TimeSpan(1, 1, 1), 
                "TEXT", "TEXT",
                Encoding.UTF8.GetBytes("TEXT"), Encoding.UTF8.GetBytes("TEXT"),
                new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            Assert.Equal(ret.o_bit, true);
            Assert.Equal(ret.o_tinyint, 1);
            Assert.Equal(ret.o_smallint, 1);
            Assert.Equal(ret.o_int, 1);
            Assert.Equal(ret.o_bigint, 1);
            Assert.Equal(ret.o_real, 1f);
            Assert.Equal(ret.o_float, 1.0);
            Assert.Equal(ret.o_money, 1M);
            Assert.Equal(ret.o_decimal, 1M);
            Assert.Equal(ret.o_smalldatetime, new DateTime(2001, 1, 1, 1, 1, 0));
            Assert.Equal(ret.o_date, new DateTime(2001, 1, 1));
            Assert.Equal(ret.o_datetime, new DateTime(2001, 1, 1, 1, 1, 1));
            Assert.Equal(ret.o_datetimeoffset, new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)));
            Assert.Equal(ret.o_time, new TimeSpan(1, 1, 1));
            Assert.Equal(ret.o_nchar, "TEXT");
            Assert.Equal(ret.o_nvarchar, "TEXT");
            Assert.Equal(ret.o_binary, Encoding.UTF8.GetBytes("TEXT"));
            Assert.Equal(ret.o_varbinary, Encoding.UTF8.GetBytes("TEXT"));
            Assert.Equal(ret.o_uniqueidentifier, new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));
        }

        [Fact]
        private async Task Execute_InOutTestAsRowset_Succeeded()
        {
            var ret = await Sql.EchoParametersAsRowset.ExecuteAsync(
                _db.DbContext,
                true, 1, 1, 1, 1, 1f, 1.0, 1M, 1M,
                new DateTime(2001, 1, 1, 1, 1, 0),
                new DateTime(2001, 1, 1),
                new DateTime(2001, 1, 1, 1, 1, 1),
                new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)),
                new TimeSpan(1, 1, 1),
                "TEXT", "TEXT",
                Encoding.UTF8.GetBytes("TEXT"), Encoding.UTF8.GetBytes("TEXT"),
                new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            var rows = await ret.Rowset.FetchAllRowsAndDisposeAsync();
            var row = rows[0];
            Assert.Equal(row.o_bit, true);
            Assert.Equal(row.o_tinyint, 1);
            Assert.Equal(row.o_smallint, 1);
            Assert.Equal(row.o_int, 1);
            Assert.Equal(row.o_bigint, 1);
            Assert.Equal(row.o_real, 1f);
            Assert.Equal(row.o_float, 1.0);
            Assert.Equal(row.o_money, 1M);
            Assert.Equal(row.o_decimal, 1M);
            Assert.Equal(row.o_smalldatetime, new DateTime(2001, 1, 1, 1, 1, 0));
            Assert.Equal(row.o_date, new DateTime(2001, 1, 1));
            Assert.Equal(row.o_datetime, new DateTime(2001, 1, 1, 1, 1, 1));
            Assert.Equal(row.o_datetimeoffset, new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)));
            Assert.Equal(row.o_time, new TimeSpan(1, 1, 1));
            Assert.Equal(row.o_nchar, "TEXT");
            Assert.Equal(row.o_nvarchar, "TEXT");
            Assert.Equal(row.o_binary, Encoding.UTF8.GetBytes("TEXT"));
            Assert.Equal(row.o_varbinary, Encoding.UTF8.GetBytes("TEXT"));
            Assert.Equal(row.o_uniqueidentifier, new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));
        }
    }
}
