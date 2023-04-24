using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Automata.IO;

namespace JsonDiffer.Code
{
    public class Setup : INotifyPropertyChanged
    {
        private IDirectory? _original;
        private IDirectory? _modified;

        public IDirectory? Original
        {
            get => _original;
            set
            {
                if (Equals(value, _original)) return;
                _original = value;
                OnPropertyChanged();
            }
        }

        public IDirectory? Modified
        {
            get => _modified;
            set
            {
                if (Equals(value, _modified)) return;
                _modified = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
