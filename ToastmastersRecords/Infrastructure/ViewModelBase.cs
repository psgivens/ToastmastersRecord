using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.Infrastructure {
    
    public class ViewModelBase : INotifyPropertyChanged, IDisposable {
        
        protected void Notify(string name) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public virtual void Dispose() {
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
