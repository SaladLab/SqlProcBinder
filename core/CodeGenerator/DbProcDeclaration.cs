using System.Collections.Generic;

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
        public List<DbField> Params;
    }
}
