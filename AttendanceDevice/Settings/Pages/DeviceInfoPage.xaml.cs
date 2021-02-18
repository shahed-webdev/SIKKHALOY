using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DeviceInfoPage.xaml
    /// </summary>
    public partial class DeviceInfoPage : Page
    {
        public DeviceInfoPage()
        {
            InitializeComponent();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (LocalData.Current_Error.Type == Error_Type.DeviceInfoPage)
            {
                ErrorSnackbar.Message.Content = LocalData.Current_Error.Message;
                ErrorSnackbar.IsActive = true;
            }

            LoadingDH.IsOpen = true;
            using (var db = new ModelContext())
            {
                var Devices = db.Devices.ToList();

                if (Devices.Count() > 0)
                {
                    foreach (var Device in Devices)
                    {
                        var checkIP = await Device_PingTest.PingHostAsync(Device.DeviceIP);
                        Device.IsConnected = Convert.ToInt32(checkIP);
                        db.Entry(Device).State = EntityState.Modified;
                    }

                    await db.SaveChangesAsync();

                    DeviceDtagrid.ItemsSource = Devices;
                }
            }

            LoadingDH.IsOpen = false;
        }
        private async void AddDevice_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceNameTextbox.Text.Trim() == "" && DeviceIPTextbox.Text.Trim() == "") return;

            LoadingDH.IsOpen = true;
            ErrorSnackbar.IsActive = false;

            var CheckIP = await Device_PingTest.PingHostAsync(DeviceIPTextbox.Text.Trim());

            if (!CheckIP)
            {
                LoadingDH.IsOpen = false;
                ErrorSnackbar.Message.Content = $"Device is not connected to this {DeviceIPTextbox.Text} IP!";
                ErrorSnackbar.IsActive = true;
                return;
            }

            var device = new Device()
            {
                DeviceName = DeviceNameTextbox.Text,
                DeviceIP = DeviceIPTextbox.Text.Trim(),
                CommKey = 2015,
                Port = 4370
            };

            var D1 = new DeviceConnection(device);

            var status = D1.ConnectDeviceWithoutEvent();

            LoadingDH.IsOpen = false;

            if (status.IsSuccess)
            {
                device.DeviceSN = D1.SN();
                device.IsConnected = Convert.ToInt32(CheckIP);

                using (var db = new ModelContext())
                {
                    db.Devices.Add(device);
                    db.SaveChanges();

                    DeviceDtagrid.ItemsSource = db.Devices.ToList();
                }

                DeviceNameTextbox.Text = "";
                DeviceIPTextbox.Text = "";
            }
        }
        private async void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            var btnConnect = (sender as Button) as Button;
            btnConnect.IsEnabled = false;
            ErrorSnackbar.IsActive = false;
            btnConnect.Content = "Connecting...";

            Device device = (sender as Button).DataContext as Device;

            var checkIP = await Device_PingTest.PingHostAsync(device.DeviceIP);
            if (!checkIP)
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "Connect";
                DeviceDtagrid.UpdateLayout();

                ErrorSnackbar.Message.Content = "Unable to connect device";
                ErrorSnackbar.IsActive = true;
                return;
            }

            var D1 = new DeviceConnection(device);
            var status = await Task.Run(() => D1.ConnectDeviceWithoutEvent());

            if (status.IsSuccess)
            {
                var details = new DeviceDetailsPage(D1);
                NavigationService.Navigate(details);
            }
            else
            {
                btnConnect.Content = "Connect";
            }
        }
        private async void DeviceDtagrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                LoadingDH.IsOpen = true;

                Device deviceContx = e.Row.DataContext as Device;

                using (var db = new ModelContext())
                {
                    if (db.Devices.Any(o => o.DeviceIP == deviceContx.DeviceIP))
                    {
                        DeviceDtagrid.ItemsSource = db.Devices.ToList();
                        LoadingDH.IsOpen = false;
                        return;
                    }

                    var checkIP = await Device_PingTest.PingHostAsync(deviceContx.DeviceIP);

                    deviceContx.IsConnected = Convert.ToInt32(checkIP);
                    db.Entry(deviceContx).State = EntityState.Modified;
                    db.SaveChanges();

                    DeviceDtagrid.ItemsSource = db.Devices.ToList();
                    LoadingDH.IsOpen = false;
                }
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Refresh();
            ErrorSnackbar.IsActive = false;
        }
    }
}
