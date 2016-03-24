CREATE PROCEDURE [dbo].[EchoParameters]
    @i_bit as bit,
    @i_tinyint as tinyint, 
    @i_smallint as smallint,
    @i_int as int,
    @i_bigint as bigint,
    @i_real as real,
    @i_float as float,
    @i_money as money,
    @i_decimal as decimal,
    @i_smalldatetime as smalldatetime,
    @i_date as date,
    @i_datetime as datetime,
    @i_datetimeoffset as datetimeoffset,
    @i_time as time,
    @i_nchar as nchar(4),
    @i_nvarchar as nvarchar(4),
    @i_binary as binary(4),
    @i_varbinary as varbinary(4),
    @i_uniqueidentifier as uniqueidentifier,
    @o_bit as bit OUTPUT,
    @o_tinyint as tinyint OUTPUT, 
    @o_smallint as smallint OUTPUT,
    @o_int as int OUTPUT,
    @o_bigint as bigint OUTPUT,
    @o_real as real OUTPUT,
    @o_float as float OUTPUT,
    @o_money as money OUTPUT,
    @o_decimal as decimal OUTPUT,
    @o_smalldatetime as smalldatetime OUTPUT,
    @o_date as date OUTPUT,
    @o_datetime as datetime OUTPUT,
    @o_datetimeoffset as datetimeoffset OUTPUT,
    @o_time as time OUTPUT,
    @o_nchar as nchar(4) OUTPUT,
    @o_nvarchar as nvarchar(4) OUTPUT,
    @o_binary as binary(4) OUTPUT,
    @o_varbinary as varbinary(4) OUTPUT,
    @o_uniqueidentifier as uniqueidentifier OUTPUT
AS
BEGIN
    SET @o_bit = @i_bit
    SET @o_tinyint = @i_tinyint
    SET @o_smallint = @i_smallint
    SET @o_int = @i_int
    SET @o_bigint = @i_bigint
    SET @o_real = @i_real
    SET @o_float = @i_float
    SET @o_money = @i_money
    SET @o_decimal = @i_decimal
    SET @o_smalldatetime = @i_smalldatetime
    SET @o_date = @i_date
    SET @o_datetime = @i_datetime
    SET @o_datetimeoffset = @i_datetimeoffset
    SET @o_time = @i_time
    SET @o_nchar = @i_nchar
    SET @o_nvarchar = @i_nvarchar
    SET @o_binary = @i_binary
    SET @o_varbinary = @i_varbinary
    SET @o_uniqueidentifier = @i_uniqueidentifier
END
