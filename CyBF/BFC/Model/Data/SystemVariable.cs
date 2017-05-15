namespace CyBF.BFC.Model.Data
{
    public class SystemVariable : Variable
    {
        private static int _variableAutonum = 1;

        public SystemVariable()
        {
            this.Name = "var" + (_variableAutonum++).ToString();
        }
    }
}
