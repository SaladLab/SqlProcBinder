using System.Data.Common;

namespace SqlProcBinder
{
    public class SimpleDbCommandContext : IDbCommandContext
    {
        public DbCommand _command;

        public DbCommand Command
        {
            get { return _command; }
        }

        public SimpleDbCommandContext(DbCommand command)
        {
            _command = command;
        }

        public void Dispose()
        {
            _command.Dispose();
        }

        public void OnExecuted()
        {
        }

        public void OnExecuting()
        {
        }
    }
}
