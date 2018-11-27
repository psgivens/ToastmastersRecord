using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToastmastersRecords.ViewModels.History;

namespace ToastmastersRecords {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            MemberHistoryDescriptionProvider.Register(new DateTime(2017,08,08), new DateTime(2017, 08, 15), new DateTime(2017, 08, 22));

            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            base.OnStartup(e);
        }
    }

    public class DebugTraceListener : TraceListener {
        public override void Write(string message) {
        }

        public override void WriteLine(string message) {
            Debugger.Break();
        }
    }
}
