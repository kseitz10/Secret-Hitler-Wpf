using System;
using System.Collections.Generic;
using System.Linq;

using GalaSoft.MvvmLight;

namespace SecretHitler.UI.ViewModels
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
            get => _item;
            set
            {
                _item = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged();
                IsSelectedChanged?.Invoke(this, _isSelected);
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }

        public EventHandler<bool> IsSelectedChanged;
    }
}
