CREATE PROCEDURE [dbo].[GenerateNullableString]
    @count as int
AS
BEGIN
    SELECT CASE WHEN (N % 2 = 1) THEN CAST(N AS NVARCHAR(10)) ELSE NULL END FROM
        (SELECT TOP (@count) n = CAST(ROW_NUMBER() OVER (ORDER BY number) AS INT)
         FROM [master]..spt_values ORDER BY n) AS N
END
