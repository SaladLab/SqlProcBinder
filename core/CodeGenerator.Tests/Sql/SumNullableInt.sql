CREATE PROCEDURE [dbo].[SumNullableInt]
    @value1 as int,
    @value2 as int,
    @answer as int OUTPUT
AS
BEGIN
    IF @value1 IS NULL RETURN
    IF @value2 IS NULL RETURN
    SET @answer = @value1 + @value2
END
