using System;
using System.Data;
using System.Data.Common;

namespace SqlProcBinder
{
    public static class SqlBindHelper
    {
        public static DbParameter AddParameter(this DbCommand command,
                                               string parameterName, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameterName;
            p.Value = value != null ? value : DBNull.Value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command,
                                               string parameterName, object value,
                                               ParameterDirection direction)
        {
            var p = AddParameter(command, parameterName, value);
            p.Direction = direction;
            return p;
        }

        public static DbParameter AddParameter(this DbCommand command,
                                               string parameterName, object value,
                                               ParameterDirection direction, int size)
        {
            var p = AddParameter(command, parameterName, value, direction);
            p.Size = size;
            return p;
        }
    }
}
