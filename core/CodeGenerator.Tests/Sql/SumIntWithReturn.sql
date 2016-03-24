CREATE PROCEDURE [dbo].[SumIntWithReturn]
    @value1 as int,
    @value2 as int,
    @answer as int OUTPUT
AS
BEGIN
    SET @answer = @value1 + @value2
    RETURN @answer
END
