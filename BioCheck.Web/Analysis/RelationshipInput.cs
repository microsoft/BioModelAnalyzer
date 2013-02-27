namespace BioCheck.Web.Analysis
{
    public class RelationshipInput
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public int FromVariableId { get; set; }

        public int ToVariableId { get; set; }
    }
}