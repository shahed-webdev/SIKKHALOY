using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using Microsoft.Win32;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for UserInfoPage.xaml
    /// </summary>
    public partial class UserInfoPage : Page
    {
        public UserInfoPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (LocalData.Current_Error.Type == Error_Type.UserInfoPage)
            {
                ErrorSnackbar.Message.Content = LocalData.Current_Error.Message;
                ErrorSnackbar.IsActive = true;
            }

            var U = LocalData.Instance.UserViews;
            if (U.Count > 0)
            {
                UserList.ItemsSource = U;
                TotalRecord.Text = "Total Users: " + U.Count.ToString();
            }

        }
        private void Upload_CSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a .csv file";
            op.Filter = "Supported|*.csv;";

            if (op.ShowDialog() == true)
            {
                FileNameTB.Text = op.FileName;
                if (!Directory.Exists(FileNameTB.Text)) return;


                using (var db = new ModelContext())
                {
                    //For deleting all previous data
                    db.Users.Clear();

                    using (StreamReader sr = new StreamReader(op.FileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            var Line = sr.ReadLine();
                            var valus = Line.Split(',');

                            db.Users.Add(new User
                            {
                                DeviceID = Convert.ToInt32(valus[0]),
                                ScheduleID = Convert.ToInt32(valus[1]),
                                ID = valus[2],
                                RFID = valus[3],
                                Name = valus[4],
                                Designation = valus[5],
                                Is_Student = Convert.ToBoolean(valus[6])
                            });
                        }
                    }

                    db.SaveChanges();
                    LocalData.Instance.Users = db.Users.ToList();
                }


                UserList.ItemsSource = LocalData.Instance.UserViews;
                TotalRecord.Text = "Total Users: " + LocalData.Instance.Users.Count.ToString();
            }
        }

        private void UserList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                UserView Up_user = e.Row.DataContext as UserView;

                var localUser = LocalData.Instance.Users.FirstOrDefault(u => u.DeviceID == Up_user.DeviceID);
                localUser.Name = Up_user.Name;
                localUser.RFID = Up_user.RFID;
                localUser.Designation = Up_user.Designation;

                using (var db = new ModelContext())
                {
                    db.Entry(localUser).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private void UserList_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //User user = UserList.SelectedItem as User;

            // if (e.Command == DataGrid.DeleteCommand)
            //{
            //if (!(MessageBox.Show("want to delete?", "Confirm!", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
            //{
            //    e.Handled = true;
            //}
            //else
            //{
            //    MessageBox.Show("data deleted");
            //}
            // }
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a logo";
            op.Filter = "Supported|*.jpg;*.jpeg;*.png| JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg| Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                //LogoSource.Text = op.FileName;
                //UserImage.ImageSource = new BitmapImage(new Uri(op.FileName));
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingPB.IsIndeterminate = true;
            DownloadButton.IsEnabled = false;

            var NetCheck = await ApiUrl.CheckInterNet();
            if (NetCheck)
            {
                MessageBox.Show("No Internet", "No Internet connection!");

                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;
                return;
            }

            try
            {
                var client = new RestClient(ApiUrl.EndPoint);
                var request = new RestRequest("api/Users/{id}", Method.GET);

                using (var db = new ModelContext())
                {
                    var Ins = LocalData.Instance.institution;

                    request.AddHeader("Authorization", "Bearer " + Ins.Token);
                    request.AddUrlSegment("id", Ins.SchoolID);

                    // Execute the request
                    IRestResponse<List<User>> response = await client.ExecuteTaskAsync<List<User>>(request);

                    if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                    {
                        //For deleting all previous data
                        db.Users.Clear();

                        foreach (var item in response.Data)
                        {
                            db.Users.Add(item);
                        }

                        await db.SaveChangesAsync();
                        LocalData.Instance.Users = response.Data;
                    }
                }


                UserList.ItemsSource = LocalData.Instance.UserViews;
                TotalRecord.Text = "Total Users: " + LocalData.Instance.Users.Count.ToString();


                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;

                //Empty Error
                LocalData.Current_Error = new Setting_Error();
                ErrorSnackbar.IsActive = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (ModelContext db = new ModelContext())
            {
                if (db.Users.Count() == 0) return;

                db.Users.Clear();
                db.SaveChanges();
                LocalData.Instance.UserViews.Clear();

                UserList.ItemsSource = null;
                TotalRecord.Text = "";
            }
        }
    }
}
