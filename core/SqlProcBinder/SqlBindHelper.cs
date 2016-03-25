using System;
using System.Data;
using System.Data.Common;

namespace SqlProcBinder
{
    public static class SqlBindHelper
    {
        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T value)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameterName;
            if (value != null)
            {
                p.Value = value;
            }
            else
            {
                p.Value = DBNull.Value;
                p.DbType = GetDbType<T>();
            }
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T value,
                                                  ParameterDirection direction)
        {
            var p = AddParameter(command, parameterName, value);
            p.Direction = direction;
            return p;
        }

        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T value,
                                                  ParameterDirection direction, int size)
        {
            var p = AddParameter(command, parameterName, value, direction);
            p.Size = size;
            return p;
        }

        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T? value)
            where T : struct
        {
            var p = command.CreateParameter();
            p.ParameterName = parameterName;
            if (value.HasValue)
            {
                p.Value = value.Value;
            }
            else
            {
                p.Value = DBNull.Value;
                p.DbType = GetDbType<T>();
            }
            command.Parameters.Add(p);
            return p;
        }

        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T? value,
                                                  ParameterDirection direction)
            where T : struct
        {
            var p = AddParameter(command, parameterName, value);
            p.Direction = direction;
            return p;
        }

        public static DbParameter AddParameter<T>(this DbCommand command,
                                                  string parameterName, T? value,
                                                  ParameterDirection direction, int size)
            where T : struct
        {
            var p = AddParameter(command, parameterName, value, direction);
            p.Size = size;
            return p;
        }

        public static DbType GetDbType<T>()
        {
            return GetDbtypeof(typeof(T));
        }

        public static DbType GetDbtypeof(Type type)
        {
            if (type == typeof(bool))
                return DbType.Binary;
            if (type == typeof(byte))
                return DbType.Binary;
            if (type == typeof(short))
                return DbType.Int16;
            if (type == typeof(int))
                return DbType.Int32;
            if (type == typeof(long))
                return DbType.Int64;
            if (type == typeof(float))
                return DbType.Double;
            if (type == typeof(double))
                return DbType.Double;
            if (type == typeof(decimal))
                return DbType.Decimal;
            if (type == typeof(DateTime))
                return DbType.DateTime2;
            if (type == typeof(DateTimeOffset))
                return DbType.DateTimeOffset;
            if (type == typeof(TimeSpan))
                return DbType.Time;
            if (type == typeof(string))
                return DbType.String;
            if (type == typeof(byte[]))
                return DbType.Binary;
            if (type == typeof(Guid))
                return DbType.Guid;

            throw new ArgumentException("Cannot resolve DbType from " + type.Name);
        }
    }
}
