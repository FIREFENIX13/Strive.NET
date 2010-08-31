﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;

using Common.Logging;

using Strive.Client.NeoAxisView;
using Strive.Client.ViewModel;
using Strive.Network.Client;


namespace Strive.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static ILog Log = LogManager.GetCurrentClassLogger();
        public static WorldViewModel _worldViewModel;
        public static ServerConnection _serverConnection;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Log.Info("Starting " + Assembly.GetExecutingAssembly().GetName().FullName);

            _serverConnection = new ServerConnection();
            _worldViewModel = new WorldViewModel(_serverConnection);
            World.Init(_worldViewModel);
        }

        private bool ReportException(Exception ex)
        {
            Log.Fatal("Uncaught Exception", ex);
            using (var d = new System.Windows.Forms.ThreadExceptionDialog(ex))
            {
                return d.ShowDialog() == System.Windows.Forms.DialogResult.Abort;
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (ReportException(e.Exception))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportException(e.ExceptionObject as Exception);
        }
    }
}
