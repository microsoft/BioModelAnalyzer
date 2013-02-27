namespace BioCheck.ViewModel.Proof
{
    public class VariableCexOutput
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public bool IsStable { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, Value:{1}", Id, Value);
        }
    }

    public class VariableOutput
    {
        public int Id { get; set; }

        public int Low { get; set; }

        public int High { get; set; }

        public bool IsStable { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, Lo:{1}-Hi:{2}", Id, Low, High);
        }
    }
}