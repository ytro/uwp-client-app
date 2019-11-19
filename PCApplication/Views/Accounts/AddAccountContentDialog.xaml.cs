﻿using PCApplication.Common;
using PCApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PCApplication.Views {
    public sealed partial class AddAccountContentDialog : ContentDialog {

        AddAccountViewModel ViewModel => DataContext as AddAccountViewModel;


        public AddAccountContentDialog(object dataContext) {
            this.InitializeComponent();

            // Set the DataContext
            DataContext = dataContext;
        }

    }
}
