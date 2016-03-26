using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private async Task Execute_StoredProcedure_Nullable_WithNull_Succeeded()
        {
            var ret = await Sql.SumNullableInt.ExecuteAsync(_db.DbContext, 1, null);
            Assert.Null(ret.answer);
        }

        [Fact]
        private async Task Execute_StoredProcedure_Nullable_WithoutNull_Succeeded()
        {
            var ret = await Sql.SumNullableInt.ExecuteAsync(_db.DbContext, 1, 2);
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
        private async Task Execute_StoredProcedure_ReturnNullableIntRowset_Succeeded()
        {
            var ret = await Sql.GenerateNullableInt.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10).Select(n => n % 2 == 1 ? (int?)n : null), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnCoalescedIntRowset_Succeeded()
        {
            var ret = await Sql.GenerateCoalescedInt.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10).Select(n => n % 2 == 1 ? n : 0), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnNullableStringRowset_Succeeded()
        {
            var ret = await Sql.GenerateNullableString.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10).Select(n => n % 2 == 1 ? n.ToString() : null), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_ReturnCoalescedStringRowset_Succeeded()
        {
            var ret = await Sql.GenerateCoalescedString.ExecuteAsync(_db.DbContext, 10);
            var values = await ret.Rowset.FetchAllRowsAndDisposeAsync(r => r.Value);
            Assert.Equal(Enumerable.Range(1, 10).Select(n => n % 2 == 1 ? n.ToString() : ""), values);
        }

        [Fact]
        private async Task Execute_StoredProcedure_RaiseError_Failed()
        {
            await
                Assert.ThrowsAsync<SqlException>(
                    async () => { await Sql.RaiseError.ExecuteAsync(_db.DbContext, "TestErrorMessage"); });
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

        [Fact]
        private async Task Execute_InOutNullableTest_Null_Succeeded()
        {
            var ret = await Sql.EchoNullableParameters.ExecuteAsync(
                _db.DbContext,
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null);

            Assert.Null(ret.o_bit);
            Assert.Null(ret.o_tinyint);
            Assert.Null(ret.o_smallint);
            Assert.Null(ret.o_int);
            Assert.Null(ret.o_bigint);
            Assert.Null(ret.o_real);
            Assert.Null(ret.o_float);
            Assert.Null(ret.o_money);
            Assert.Null(ret.o_decimal);
            Assert.Null(ret.o_smalldatetime);
            Assert.Null(ret.o_date);
            Assert.Null(ret.o_datetime);
            Assert.Null(ret.o_datetimeoffset);
            Assert.Null(ret.o_time);
            Assert.Null(ret.o_nchar);
            Assert.Null(ret.o_nvarchar);
            Assert.Null(ret.o_binary);
            Assert.Null(ret.o_varbinary);
            Assert.Null(ret.o_uniqueidentifier);
        }

        [Fact]
        private async Task Execute_InOutNullableTest_NonNull_Succeeded()
        {
            var ret = await Sql.EchoNullableParameters.ExecuteAsync(
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
            Assert.Equal(ret.o_tinyint, (byte)1);
            Assert.Equal(ret.o_smallint, (short)1);
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
        private async Task Execute_InOutNullableTestAsRowset_Null_Succeeded()
        {
            var ret = await Sql.EchoNullableParametersAsRowset.ExecuteAsync(
                _db.DbContext,
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null);

            var rows = await ret.Rowset.FetchAllRowsAndDisposeAsync();
            var row = rows[0];
            Assert.Null(row.o_bit);
            Assert.Null(row.o_tinyint);
            Assert.Null(row.o_smallint);
            Assert.Null(row.o_int);
            Assert.Null(row.o_bigint);
            Assert.Null(row.o_real);
            Assert.Null(row.o_float);
            Assert.Null(row.o_money);
            Assert.Null(row.o_decimal);
            Assert.Null(row.o_smalldatetime);
            Assert.Null(row.o_date);
            Assert.Null(row.o_datetime);
            Assert.Null(row.o_datetimeoffset);
            Assert.Null(row.o_time);
            Assert.Null(row.o_nchar);
            Assert.Null(row.o_nvarchar);
            Assert.Null(row.o_binary);
            Assert.Null(row.o_varbinary);
            Assert.Null(row.o_uniqueidentifier);
        }

        [Fact]
        private async Task Execute_InOutNullableTestAsRowset_NonNull_Succeeded()
        {
            var ret = await Sql.EchoNullableParametersAsRowset.ExecuteAsync(
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
            Assert.Equal(row.o_tinyint, (byte)1);
            Assert.Equal(row.o_smallint, (short)1);
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

        [Fact]
        private async Task Execute_TableType_Succeeded()
        {
            var list = new Sql.Vector3List();
            list.Add(1, 2, 3);
            list.Add(4, 5, 6);

            var ret = await Sql.Vector3ListSum.ExecuteAsync(_db.DbContext, list.Table);
            Assert.Equal(21, ret.answer);
        }
    }
}
