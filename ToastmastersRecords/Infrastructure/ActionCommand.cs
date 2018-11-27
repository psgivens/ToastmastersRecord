using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ToastmastersRecords.Infrastructure {    
    class ActionCommand : ICommand {
        private readonly Action _del;

        public ActionCommand(Action del) {
            _del = del;
        }

        public void Execute(object parameter) {
            _del();
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
