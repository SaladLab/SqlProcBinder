namespace SqlProcBinder
{
    public interface IDbContext
    {
        IDbCommandContext CreateCommand();
    }
}
