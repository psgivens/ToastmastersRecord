using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToastmastersRecords.Data;

namespace ToastmastersRecords.Controls {
    /// <summary>
    /// Interaction logic for DayOffRequestsUserControl.xaml
    /// </summary>
    public partial class DayOffRequestsUserControl : UserControl {
        protected ViewModels.ApplicationViewModel App { get { return ViewModels.ApplicationViewModel.Instance; } }
        public DayOffRequestsUserControl() {
            InitializeComponent();
        }

        private ViewModels.DayOffRequestsViewModel ViewModel {
            get { return (ViewModels.DayOffRequestsViewModel) DataContext; }
        }

        private void userDaysOffDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e) {
            e.NewItem = ViewModel.CreateDayOffRequest();
        }
        private void userDaysOffDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            var req = (DayOffRequest)e.Row.Item;
                App.Upsert(req);
        }

    }
}
