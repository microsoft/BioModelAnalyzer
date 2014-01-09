using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BioCheck.Controls;

namespace BioCheck.Services
{
    public interface ILogWindowService
    {
        void Show(string log);
    }

    public class LogWindowService : ILogWindowService
    {
        public LogWindowService()
        {
        }

        public void Show(string log)
        {
            LogWindow.CreateNew(log);
        }

    }
}
