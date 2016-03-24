CREATE PROCEDURE [dbo].[GenerateIntWithOutput]
    @count as int,
    @message as nvarchar(100) output
AS
BEGIN
    SELECT TOP (@count) n = CAST(ROW_NUMBER() OVER (ORDER BY number) AS INT)
    FROM [master]..spt_values ORDER BY n

    SET @message = 'COUNT:' + CAST(@count AS NVARCHAR(20))
END
