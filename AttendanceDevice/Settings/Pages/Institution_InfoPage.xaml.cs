using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Data.Entity;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Institution_Info.xaml
    /// </summary>

    public partial class Institution_InfoPage : Page
    {
        public Institution_InfoPage()
        {
            InitializeComponent();

            this.DataContext = LocalData.Instance.institution;
        }

        private void Folder_Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderLink.Text = dialog.FileName;
            }
        }
        private void Logo_BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a logo";
            op.Filter = "Supported|*.jpg;*.jpeg;*.png| JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg| Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                LogoSource.Text = op.FileName;
                Logo.ImageSource = new BitmapImage(new Uri(op.FileName));
            }
        }
        public byte[] ImageToByte(BitmapImage image)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(memStream);
            return memStream.ToArray();
        }
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a logo";
            op.Filter = "Supported|*.jpg;*.jpeg;*.png| JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg| Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                LogoSource.Text = op.FileName;
                Logo.ImageSource = new BitmapImage(new Uri(op.FileName));
            }
        }
        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ModelContext())
            {
                var result = LocalData.Instance.institution;

                if (result != null)
                {
                    if (InstitutionName.Text != "")
                    {
                        result.InstitutionName = InstitutionName.Text;
                    }

                    if (FolderLink.Text != "")
                    {
                        if (Directory.Exists(FolderLink.Text))
                        {
                            result.Image_Link = FolderLink.Text;
                        }
                        else
                        {
                            MessageBox.Show("Invalid folder link path");
                            return;
                        }
                    }

                    if (LogoSource.Text != "")
                    {
                        if (File.Exists(LogoSource.Text))
                        {
                            var imgSource = new BitmapImage(new Uri(LogoSource.Text));
                            result.Logo = ImageToByte(imgSource);
                        }
                        else
                        {
                            MessageBox.Show("Invalid logo file path");
                            return;
                        }
                    }

                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();

                    var queue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
                    UpdateSnackbar.MessageQueue = queue;
                    queue.Enqueue("Update Success!!");
                }
            }
        }
    }
}
