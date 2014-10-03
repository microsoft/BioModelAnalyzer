namespace BioCheck.Controls
{
    // Time edit
    public enum TimeCommandType
    {
        None = 0,
        LessThan,
        MoreThan,
        Value,
        AlwaysEventually,
        Variable
    }

    public class TimebarCommand
    {
        public TimeCommandType TimeCommandType { get; set; }
    }
}
