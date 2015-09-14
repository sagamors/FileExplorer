using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace FileExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
