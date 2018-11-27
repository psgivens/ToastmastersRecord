using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;

namespace ToastmastersRecords.Infrastructure {
    public class DataViewModelBase : ViewModelBase {
        protected TIDbContext Context { get; private set; }
        public DataViewModelBase(TIDbContext context) {
            this.Context = context;
        }
        public override void Dispose() {
            base.Dispose();
        }
    }
}
