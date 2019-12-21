using System;
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
using System.ComponentModel;

namespace BridgeServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		#region field
		private MainWindowsVM _vm;
		#endregion

		#region ctor
		public MainWindow()
		{
			InitializeComponent();

			_vm = new MainWindowsVM();
			this.DataContext = _vm;
		} 
		#endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BridgeServer.Properties.Settings.Default.Save();
			_vm.StopAll();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
			_vm.ChangeStatus();
        }

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			if (_vm == null) return;

			//quando si riapre l'axpander si chiede se si vuole ripristinare i messaggi stoccati in maniera silenziosa
			if (MessageBox.Show("Restore verbose?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

			_vm.LocalMsg = "RESTORED VERBOSE";
			_vm.RemoteMsg  = "RESTORED VERBOSE";
			_vm.MainMsg = "RESTORED VERBOSE";
		}
    }
}
