CREATE PROCEDURE [dbo].[SumInt]
    @value1 as int,
    @value2 as int,
    @answer as int OUTPUT
AS
BEGIN
    SET @answer = @value1 + @value2
END
