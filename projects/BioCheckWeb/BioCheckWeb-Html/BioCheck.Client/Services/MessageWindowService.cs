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
    public interface IMessageWindowService
    {
        void Show(string message);

        void Show(string message, MessageType type, Action<MessageResult> closeAction);
    }

    public enum MessageType
    {
        OKOnly = 0,
        OKCancel,
        YesCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageResult
    {
        OK,
        Cancel, 
        Yes, 
        No
    }

    public class MessageWindowService : IMessageWindowService
    {
        public MessageWindowService()
        {
        }

        public void Show(string message)
        {
            var window = new MessageWindow(message);
            window.Show();
        }

        public void Show(string message, MessageType type, Action<MessageResult> closeAction)
        {
            var window = new MessageWindow(message, type);
            window.Closed += (o, args) => closeAction.Invoke(window.MessageResult);
            window.Show();
        }
    }
}
