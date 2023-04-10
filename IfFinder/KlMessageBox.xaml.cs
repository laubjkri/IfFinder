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

namespace KlUtils {

    public partial class KlMessageBox : Window {
        private KlMessageBox(string caption, string message, Window? parentWindow = null) {
            InitializeComponent();
            Title = caption;
            messageTextBlock.Text = message;

            MinWidth = Width;
            MinHeight = Height;

            // Place in middle of parent window
            if (parentWindow != null) {                
                Left = parentWindow.Left + parentWindow.ActualWidth / 2 - Width / 2;
                Top = parentWindow.Top + parentWindow.ActualHeight / 2 - Height / 2;
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        public static void Show(string caption, string message, Window? parentWindow = null) {

            KlMessageBox mb = new KlMessageBox(caption, message, parentWindow);
            mb.ShowDialog();
        }
    }
}
