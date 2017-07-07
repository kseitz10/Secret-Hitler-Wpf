using GalaSoft.MvvmLight;

namespace SecretHitler.App.ViewModels
{
    public class SelectableItem<T> : ViewModelBase
    {
        public SelectableItem(T item, bool selected = false, bool editable = true)
        {
            Item = item;
            IsSelected = selected;
            IsEnabled = editable;
        }

        private T _item;
        private bool _isSelected;
        private bool _isEnabled = true;

        public T Item
        {
            get { return _item; }
            set
            {
                _item = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }
    }
}
