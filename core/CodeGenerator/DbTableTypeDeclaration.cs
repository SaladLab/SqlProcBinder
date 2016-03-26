using System.Collections.Generic;

namespace CodeGenerator
{
    public class DbTableTypeDeclaration
    {
        public string ClassName;
        public string TypeName;
        public string FilePath;
        public List<DbField> Fields;
    }
}
