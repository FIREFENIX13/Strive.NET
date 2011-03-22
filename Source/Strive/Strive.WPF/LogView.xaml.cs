﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock;


namespace Strive.WPF
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : DockableContent
    {
        public LogView()
        {
            InitializeComponent();
            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            listView1.CommandBindings.Add(cb);
        }

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListView;
            string copyContent = String.Empty;
            foreach (var item in lb.SelectedItems)
                copyContent += item.ToString() + Environment.NewLine;
            Clipboard.SetText(copyContent);
        }

        void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListView;
            e.CanExecute =  (lb.SelectedItems.Count > 0);
        }
    }
}
