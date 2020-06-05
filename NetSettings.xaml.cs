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
using System.Windows.Shapes;

namespace UPanel
{
    /// <summary>
    /// Interaction logic for NetSettings.xaml
    /// </summary>
    public partial class NetSettings : Window
    {
        public NetSettings()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string IP
        {
            get { return textbox_IP_1.Text + "." + textbox_IP_2.Text + "." + textbox_IP_3.Text + "." + textbox_IP_4.Text; }
        }

        public string Port
        {
            get { return textbox_Port.Text; }
        }

        private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.Text, 0);
        }
    }
}
