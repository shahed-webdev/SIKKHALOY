using AttendanceDevice.Model;
using System.Windows;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Error_Window.xaml
    /// </summary>
    public partial class Error_Window : Window
    {
        Error error;
        public Error_Window(Error _error)
        {
            error = _error;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ErTitle.Text = error.ErrorTittle;
            ErMessage.Text = error.ErrorMessage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
