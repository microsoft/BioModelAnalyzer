namespace BioCheck.Services
{
    public interface IErrorWindowService
    {
        void Show(string message);

        void Show(string message, string errorDetails);

        void ShowAnalysisError(string message);
    }

    public class ErrorWindowService : IErrorWindowService
    {
        public ErrorWindowService()
        {
        }

        public void Show(string message)
        {
            ErrorWindow.CreateNew(message, StackTracePolicy.Never);
        }

        public void Show(string message, string errorDetails)
        {
            ErrorWindow.CreateNew(message, errorDetails);
        }

        public void ShowAnalysisError(string message)
        {
            var window = new AnalysisErrorWindow(message);
            window.Show();
        }
    }
}
