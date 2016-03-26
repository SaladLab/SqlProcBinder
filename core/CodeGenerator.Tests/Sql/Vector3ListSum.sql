CREATE PROCEDURE [dbo].[Vector3ListSum]
    @values Vector3List READONLY,
    @answer float OUTPUT
AS
BEGIN
    SELECT @answer = SUM(X) + SUM(Y) + SUM(Z) FROM @values
END
