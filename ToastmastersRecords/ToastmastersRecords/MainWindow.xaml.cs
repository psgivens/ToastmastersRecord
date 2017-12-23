using System.Windows;
using System.Windows.Controls;
using Technewlogic.WpfDialogManagement;
using ToastmastersRecords.Data;
using ToastmastersRecords.ViewModels;

namespace ToastmastersRecords {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            var grid = this.mainPanel;
            ApplicationViewModel.Instance = new ApplicationViewModel(new DialogManager(grid, Dispatcher));

            DataContext = App;
        }


        public ApplicationViewModel App { get { return ApplicationViewModel.Instance; } }

        private void usersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            App.MembersViewModel.Member = e.AddedItems.Count > 0 ? (ClubMember)e.AddedItems[0] : null;
        }


        private void userRoleAssignmentsDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e) {
            e.NewItem = App.MembersViewModel.CreateRoleAssignment();

        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "MM/dd/yyyy";
        }

        private void userRoleAssignmentsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            var assignment = (RoleAssignment)e.Row.Item;
            App.Upsert(assignment);
        }
    }
}
