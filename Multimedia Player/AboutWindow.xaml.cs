using System.Windows;

namespace Multimedia_Player
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OK_Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
