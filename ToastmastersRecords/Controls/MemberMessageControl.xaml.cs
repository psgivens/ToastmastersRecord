﻿using System;
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

namespace ToastmastersRecords.Controls {
    /// <summary>
    /// Interaction logic for MemberMessageControl.xaml
    /// </summary>
    public partial class MemberMessageControl : UserControl {
        protected ViewModels.ApplicationViewModel App { get { return ViewModels.ApplicationViewModel.Instance; } }
        public MemberMessageControl() {
            InitializeComponent();
        }
    }
}
