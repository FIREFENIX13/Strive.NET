﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using UpdateControls.XAML;
using AvalonDock;

using Strive.WPF;
using Strive.Client.NeoAxisView;


namespace Strive.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        const string LayoutFileName = "StriveLayout.xml";

        private void SaveLayout(object sender, RoutedEventArgs e)
        {
            dockManager.SaveLayout(LayoutFileName);
        }

        private void RestoreLayout(object sender, RoutedEventArgs e)
        {
            if (File.Exists(LayoutFileName))
                dockManager.RestoreLayout(LayoutFileName);
        }

        private void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BindAndShow(new LogView(), new LogViewModel(App.LogModel));
        }

        private void NewCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var view = new WorldView(World.ViewModel);
            view.ShowAsDocument(dockManager);
            view.Focus();
        }

        private void SearchCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SearchCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BindAndShow(new ResourceList(), App.WorldViewModel);
        }

        private void BrowseHomeCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BrowseHomeCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var connect = new Connect();
            connect.ShowAsDocument(dockManager);
            connect.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var wb = new WebBrowser();
            wb.ShowAsDocument(dockManager);
            wb.Focus();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            BindAndShow(new FactoryView(), new FactoryViewModel());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            BindAndShow(new ServerStatusView(), new ServerStatusViewModel(App.ServerEngine.ServerStatusModel));
            App.ServerEngine.Start();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            BindAndShow(new UnitView(), new UnitViewModel());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            World.Init(this, App.WorldViewModel);
            //NewCmdExecuted(null, null);
        }

        private void BindAndShow(DockableContent view, object viewModel)
        {
            view.DataContext = ForView.Wrap(viewModel);
            view.ShowAsDocument(dockManager);
            view.Focus();
        }
    }
}
