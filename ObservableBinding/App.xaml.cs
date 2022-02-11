﻿using System.Windows;

namespace ObservableBinding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new MainWindow
            {
                DataContext = new MainViewModel()
            }.Show();
        }
    }
}