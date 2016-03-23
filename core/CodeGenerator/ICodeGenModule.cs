using Newtonsoft.Json.Linq;

namespace CodeGenerator
{
    public interface ICodeGenModule
    {
        void Generate(JObject input, string inputPath, ICodeGenWriter writer);
    }
}
