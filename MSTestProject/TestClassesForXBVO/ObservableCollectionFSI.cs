using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoundObjectMSTest.TestClassesForXBVO
{
    class ObservableCollectionFSI : ObservableCollection<FilesystemItem>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (
                        INotifyPropertyChanged item in
                        e.NewItems ?? Array.Empty<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged += ListItemChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (
                        INotifyPropertyChanged item in
                        e.OldItems ?? Array.Empty<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged -= ListItemChanged;
                    }
                    break;
            }
        }
        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.PropertyChanged -= ListItemChanged;
            }
            base.ClearItems();
        }

        private void ListItemChanged(object? sender, PropertyChangedEventArgs e)
        {
        }
    }
    class FilesystemItem : XBoundViewObjectImplementer
    { }
}
