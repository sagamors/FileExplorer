using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FileExplorer.CustomCollections;

namespace FileExplorer.Helpers
{
    public class ObservableCollectionEx<T> :  ObservableCollection<T>, IObservableCollection<T>
    {
        public ObservableCollectionEx(List<T> list) : base(list)
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableCollectionEx()
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            this.CheckReentrancy();

            //
            // We need the starting index later
            //
            int startingIndex = this.Count;

            //
            // Add the items directly to the inner collection

            //
            foreach (var data in range)
            {
                this.Items.Add(data);
            }


        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
/*            //
            // Now raise the changed events
            //
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            //
            // We have to change our input of new items into an IList since that is what the
            // event args require.
            //
            var changedItems = new List<T>(range);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,changedItems, startingIndex));*/
        }

/*        public void AddRange(IEnumerable<T> range)
        {
            this.CheckReentrancy();

            //
            // We need the starting index later
            //
            int startingIndex = this.Count;

            //
            // Add the items directly to the inner collection

            //
            foreach (var data in range)
            {
                this.Items.Add(data);
            }

            //
            // Now raise the changed events
            //
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));


            //
            // We have to change our input of new items into an IList since that is what the
            // event args require.
            //
            var changedItems = new List<T>(range);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startingIndex));
        }*/
    }
}
