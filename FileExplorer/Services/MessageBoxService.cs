using System.Windows;

namespace FileExplorer.Services
{
    class MessageBoxService : IMessageBoxService
    {
        public void Show(string caption,string message)
        {
            MessageBox.Show(message, caption);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}