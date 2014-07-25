namespace BioCheck.Web.Analysis
{
    public class VariableInput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RangeFrom { get; set; }

        public int RangeTo { get; set; }

        public string Formula { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1} : {2}-{3}", Id, Name, RangeFrom, RangeTo);
        }
    }
}