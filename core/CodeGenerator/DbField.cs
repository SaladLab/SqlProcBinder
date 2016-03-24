namespace CodeGenerator
{
    public class DbField
    {
        public string Name;
        public string Type;
        public int Len;
        public string Dir;

        public bool IsInput
        {
            get { return string.IsNullOrEmpty(Dir) || Dir == "in" || Dir == "ref"; }
        }

        public bool IsOutput
        {
            get { return Dir == "ref" || Dir == "out"; }
        }
    }
}
