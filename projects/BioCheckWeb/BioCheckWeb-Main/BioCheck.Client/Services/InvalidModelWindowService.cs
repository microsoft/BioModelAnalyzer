using System;

namespace BioCheck.Services
{
    public interface IInvalidModelWindowService
    {
        void Show(string modelXml, Action<bool> closeAction);

        void ShowImportError(string modelXml);
    }

    public class InvalidModelWindowService : IInvalidModelWindowService
    {
        public InvalidModelWindowService()
        {
        }

        public void Show(string modelXml, Action<bool> closeAction)
        {
            var window = new InvalidModelWindow(modelXml);
            window.Closed += (o, args) => closeAction.Invoke(window.DialogResult.GetValueOrDefault());
            window.Show();
        }

        public void ShowImportError(string modelXml)
        {
            var window = new ImportErrorWIndow(modelXml);
            window.Show();
        }
    }
}
