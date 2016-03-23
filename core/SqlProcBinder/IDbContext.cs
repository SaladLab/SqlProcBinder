using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SqlProcBinder
{
    public interface IDbContext
    {
        IDbCommandContext CreateCommand();
    }
}
