namespace BioCheck.Web.Analysis
{
    /// <summary>
    /// Data Transfer Object for the output of validating a formula
    /// </summary>
    public class ValidationOutput
    {
        public bool IsValid { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }

        public override string ToString()
        {
            return string.Format("IsValid={0} : {1}", IsValid, Message);
        }
    }
}