using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Technewlogic.WpfDialogManagement {
    public class DialogClosedEventArgs : EventArgs {
        public DialogClosedEventArgs(DialogResultState result) {
            DialogResultState = result;
        }
        public DialogResultState DialogResultState { get; private set; }
    }
}
