using System.Windows;

namespace FileExplorer.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public static MessageBoxService Instance { get; } = new MessageBoxService();

        private MessageBoxService()
        {

        }

        public void Show(string caption, string message)
        {
            MessageBox.Show(message, caption);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}