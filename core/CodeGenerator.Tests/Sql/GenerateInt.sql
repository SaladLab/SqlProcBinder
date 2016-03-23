CREATE PROCEDURE [dbo].[GenerateInt]
    @count as int
AS
BEGIN
    SELECT TOP (@count) n = CAST(ROW_NUMBER() OVER (ORDER BY number) AS INT)
    FROM [master]..spt_values ORDER BY n 
END
