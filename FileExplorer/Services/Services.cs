namespace FileExplorer.Services
{
    interface IMessageBoxService
    {
        void Show(string caption, string message);
        void ShowError(string message);
    }
}
