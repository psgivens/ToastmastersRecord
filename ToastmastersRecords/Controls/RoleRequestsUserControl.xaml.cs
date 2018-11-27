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
    /// Interaction logic for RoleRequestsUserControl.xaml
    /// </summary>
    public partial class RoleRequestsUserControl : UserControl {
        protected ViewModels.ApplicationViewModel App { get { return ViewModels.ApplicationViewModel.Instance; } }
        public RoleRequestsUserControl() {
            InitializeComponent();
        }
        public ViewModels.RoleRequestsViewModel ViewModel {
            get { return (ViewModels.RoleRequestsViewModel)DataContext; }
        }
        private void userRoleRequestsDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e) {
            e.NewItem = ViewModel.CreateRoleRequest();
        }

        private void userRoleRequestsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            var req = (RoleRequest)e.Row.Item;
            if (req.RoleType == null)
                return;
            App.Upsert(req);
        }

    }
}
