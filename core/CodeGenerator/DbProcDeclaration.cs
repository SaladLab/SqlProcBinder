using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CodeGenerator
{
    public class DbProcDeclaration
    {
        public string ClassName;
        public string ProcName;
        public string FilePath;
        public bool Return;
        public string Rowset;
        public bool RowsetFetch;
        public List<DbHelper.Field> Params;
    }
}
