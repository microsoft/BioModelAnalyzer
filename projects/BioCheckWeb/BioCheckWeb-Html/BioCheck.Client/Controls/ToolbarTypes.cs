namespace BioCheck.Controls
{
    public enum CommandType
    {
        None = 0,
        Container,
        Variable,
        Constant,
        ActivatorRelationship,
        InhibitorRelationship,
        MembraneReceptor
    }

    public class ToolbarCommand
    {
        public CommandType CommandType { get; set; }    
    }
}
