using System;
using System.Data.Common;

namespace SqlProcBinder
{
    public interface IDbCommandContext : IDisposable
    {
        DbCommand Command { get; }
        void OnExecuting();
        void OnExecuted();
    }
}
