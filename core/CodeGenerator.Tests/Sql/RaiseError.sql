CREATE PROCEDURE [dbo].[RaiseError]
    @message as nvarchar(100)
AS
BEGIN
    RAISERROR (@message, 16, 1)
END
