using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AttendanceDevice
{
    public partial class Window_Startup : Window
    {
        public Window_Startup()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {   //Empty Error
                LocalData.Current_Error = new Setting_Error();


                // Institution not Register in local database
                var ins = LocalData.Instance.institution;

                if (ins == null)
                {
                    LocalData.Current_Error.Message = "No institutional information found in your local Machine";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //check institution is valid
                if (!ins.IsValid)
                {
                    LocalData.Current_Error.Message = $"{ins.InstitutionName} has currently deactivated by the Software Authority";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //No user in local database
                if (!LocalData.Instance.IsUserExist())
                {
                    LocalData.Current_Error.Message = "No User Found on PC!";
                    LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                //check device added or not
                if (!LocalData.Instance.IsDeviceExist())
                {
                    LocalData.Current_Error.Message = "No Device Added In PC!";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                //create all device list
                var deviceList = await LocalData.Instance.DeviceListAsync();
                var deviceConnections = new List<DeviceConnection>();

                foreach (var device in deviceList)
                {
                    var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP);
                    if (checkIp)
                    {
                        deviceConnections.Add(new DeviceConnection(device));
                    }
                }

                //check device ip
                if (deviceConnections.Count > 0)
                {
                    LocalData.Current_Error.Message = "Device IP Not Found";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                //try connection to device successfully
                var isDeviceConnected = false;
                foreach (var item in deviceConnections)
                {
                    var status = await Task.Run(() => item.ConnectDevice());
                    if (status.IsSuccess)
                    {
                        isDeviceConnected = true;
                    }
                }

                if (!isDeviceConnected)
                {
                    LocalData.Current_Error.Message = "Device Unable to Connect";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }






                //show display window
                var initDevice = new DeviceDisplay(deviceConnections);
                var display = new Offline_DisplayWindow(initDevice);
                display.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                var errorObj = new Error("System Error", ex.Message);
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
