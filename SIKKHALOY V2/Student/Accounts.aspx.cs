using EDUCATION.COM.PaymentDataSetTableAdapters;
using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Accounts : System.Web.UI.Page
    {
        private string paymentGatewayBase = "https://sandbox.aamarpay.com";
        private string confirmationBase = "http://localhost:3326";
        private string Baseurl = "https://sandbox.aamarpay.com" + "/request.php";

        protected void Page_Load(object sender, EventArgs e)
        {
            //PaymentFactory<PaymentResponse> paymentFactory = new PaymentFactory<PaymentResponse>();
            //var paymentInfo = paymentFactory.GetPaymentInfoFromQueryString(Request);
        }
        protected void MreceiptLinkButton_Command(object sender, CommandEventArgs e)
        {
            AllPayRecordSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void PayButton_Click(object sender, EventArgs e)
        {
            //MakeOnlinePayment("", "");
           // return;

            var Payment_DataSet = new OrdersTableAdapter();

            double TotalPaid = 0;
            int MoneyReceiptID = 0;
            int StudentClassID = Convert.ToInt32(Session["StudentClassID"].ToString());
            int StudentID = Convert.ToInt32(Session["StudentID"].ToString());

            int Crrent_EduYearID = Convert.ToInt32(Session["Edu_Year"].ToString());
            int SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            int RegistrationID = Convert.ToInt32(Session["RegistrationID"].ToString());

            MoneyReceiptID = Convert.ToInt32(Payment_DataSet.Insert_MoneyReceipt(StudentID, RegistrationID, StudentClassID, Crrent_EduYearID, "Institution", DateTime.Now, SchoolID));

            //Current Session GV
            foreach (GridViewRow row in DueGridView.Rows)
            {
                CheckBox DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                int PayOrderID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["PayOrderID"]);
                int RoleID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["RoleID"]);
                int P_Order_EduYearID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["EducationYearID"]);

                double PaidAmount = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                if (DueCheckBox.Checked)
                {
                    //Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), DateTime.Now, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, Convert.ToInt32(AccountDropDownList.SelectedValue));
                    Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), DateTime.Now, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, 2);
                    Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);

                    TotalPaid += PaidAmount;
                    DueCheckBox.Checked = false;
                }
            }

            Payment_DataSet.Update_MoneyReceipt(TotalPaid, MoneyReceiptID);

       
            string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(MoneyReceiptID)));
            string Sid = HttpUtility.UrlEncode(Encrypt(GetStudentId()));
            //Response.Redirect(string.Format("Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));

            //Response.Redirect(string.Format("Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
            MakeOnlinePayment(MRid, Sid);
        }

        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public void MakeOnlinePayment(string moneyReceipt, string sid)
        {
            string messgae = "";
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                var request = new PaymentRequest();
                request.store_id = "aamarpaytest";
                request.signature_key = "dbb74894e82415a2f7ff0ec3a97e4183";
                request.tran_id = RandomString(10);
                request.amount = "100";
                request.currency = "BDT";
                request.desc = "Pay Fee";
                request.cus_name = "Mr Customer";
                request.cus_email = "customer@gmail.com";
                request.cus_phone = "012266584457";
                request.type = "json";
                //request.success_url = confirmationBase + "/Student/OnlinePayment/Success.aspx";
                request.success_url = "http://localhost:3326/Default.aspx";
                request.fail_url = confirmationBase + "/Student/OnlinePayment/Failed.aspx";
                request.cancel_url = confirmationBase + "/Student/OnlinePayment/Cancelled.aspx";
                request.opt_a = GetSessionInfo();
                request.opt_b = moneyReceipt;
                request.opt_c = sid;

                string postData = "";
                var infos = request.GetType().GetProperties();
                foreach (PropertyInfo pair in infos)
                {
                    string name = pair.Name;
                    var value = pair.GetValue(request, null);
                    value = value != null ? value.ToString() : "";
                    //parameters.Add(pair.Name, value.ToString());
                    postData += HttpUtility.UrlEncode(name) + "="
                         + HttpUtility.UrlEncode(value.ToString()) + "&";

                }

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11  | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                
                WebRequest wRequest = WebRequest.Create(Baseurl);
                wRequest.Method = "POST";
                byte[] bArray = Encoding.UTF8.GetBytes(postData);
                wRequest.ContentType = "application/x-www-form-urlencoded";
                wRequest.ContentLength = bArray.Length;
                Stream webData = wRequest.GetRequestStream();
                webData.Write(bArray, 0, bArray.Length);
                webData.Close();
                WebResponse webResponse = wRequest.GetResponse();
                Console.WriteLine(((HttpWebResponse)webResponse).StatusDescription);
                webData = webResponse.GetResponseStream();
                StreamReader reader = new StreamReader(webData);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                webData.Close();
                webResponse.Close();

                //\/ paynow.php ? track = AAM1581391191203665
                var trans = responseFromServer.Remove(0, 2).Split('"')[0];
                string url = paymentGatewayBase + trans;

                Response.Redirect(url);
            }
            catch (Exception e)
            {
                string message = e.Message;
                messgae = message;
            }

            //return "";
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private string GetSessionInfo()
        {
            string schoolID = Session["SchoolID"] != null ? Session["SchoolID"].ToString() : "";
            string schoolName = Session["School_Name"] !=null ? Session["School_Name"].ToString() : "";
            string registrationID = Session["RegistrationID"] != null ? Session["RegistrationID"].ToString() : "";
            string educationYearID = Session["Edu_Year"] != null ? Session["Edu_Year"].ToString() : "";
            string studentID =  Session["StudentID"] != null ? Session["StudentID"].ToString() : "";
            string classID = Session["ClassID"] != null ? Session["ClassID"].ToString() : "";
            string studentClassID = Session["StudentClassID"] != null ? Session["StudentClassID"].ToString() : "";
            string teacherID = Session["TeacherID"] != null ? Session["TeacherID"].ToString() : "";

            var dictionary = new Dictionary<string, string>
            {
                {"SchoolID", schoolID},
                {"SchoolName", schoolName},
                {"RegistrationID", registrationID},
                {"EducationYearID", educationYearID},
                {"StudentID", studentID},
                {"ClassID", classID},
                {"StudentClassID", studentClassID},
                {"TeacherID", teacherID},
            };

            var items = from kvp in dictionary
                        select kvp.Key + "=" + kvp.Value;
            return "{" + string.Join(",", items) + "}";

        }

        private string GetStudentId()
        {
            string id = "";
            string StudentID = Session["StudentID"].ToString();
            string RegistrationID = Session["RegistrationID"].ToString();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT ID FROM Student WHERE StudentID = @StudentID AND StudentRegistrationID = @RegistrationID";
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Parameters.AddWithValue("@RegistrationID", RegistrationID);

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            id = sdr["ID"].ToString();
                        }
                    }
                    conn.Close();

                }
            }
            return id;
        }
    }
}