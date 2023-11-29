using EDUCATION.COM.ACCOUNTS.Payment;
using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Web.Services;

namespace EDUCATION.COM
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaymentFactory<PaymentResponse> paymentFactory = new PaymentFactory<PaymentResponse>();
            var paymentInfo = paymentFactory.GetPaymentInfoFromQueryString(Request);
            if (!string.IsNullOrEmpty(paymentInfo.opt_a))
            {
                SetSessionInfoAfterOnlinePayment(paymentInfo.opt_a);
                Session["OnlinePaymentInfo"] = JsonConvert.SerializeObject(paymentInfo);
                string MRid = paymentInfo.opt_b;
                string Sid = paymentInfo.opt_c;
                Response.Redirect(string.Format("~/Accounts/Payment/Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
                
                //Response.Redirect("~/Student/OnlinePayment/Success.aspx");
            }
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text == "" || MobileTextBox.Text == "") return;

            ContactSQL.Insert();

            NameTextBox.Text = "";
            EmailTextBox.Text = "";
            MobileTextBox.Text = "";
            SubjectTextBox.Text = "";
            MessageTextBox.Text = "";
            MsgLabel.Text = "Thank you for your query, we will respond as soon as possible";
        }

        //Change Session
        [WebMethod]
        public static void Session_Change(string id)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            try
            {
                var cmd = new SqlCommand("UPDATE Education_Year_User SET EducationYearID = Education_Year.EducationYearID FROM Education_Year_User INNER JOIN Education_Year ON Education_Year_User.SchoolID = Education_Year.SchoolID WHERE (Education_Year_User.SchoolID = @SchoolID) AND (Education_Year_User.RegistrationID = @RegistrationID) AND (Education_Year.EducationYearID = @EducationYearID)", con);
                cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@RegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());
                cmd.Parameters.AddWithValue("@EducationYearID", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
                HttpContext.Current.Session["Edu_Year"] = id;
            }
        }


        //Change Session Student
        [WebMethod]
        public static void Student_Session_Change(string id)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            var cmd = new SqlCommand("UPDATE Education_Year_User SET EducationYearID = @EducationYearID WHERE (SchoolID = @SchoolID) AND (RegistrationID = @RegistrationID)", con);

            cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@RegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());
            cmd.Parameters.AddWithValue("@EducationYearID", id);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            var cmd2 = new SqlCommand("SELECT StudentsClass.StudentClassID,StudentsClass.ClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.StudentRegistrationID = @StudentRegistrationID)", con);
            cmd2.Parameters.AddWithValue("@EducationYearID", id);
            cmd2.Parameters.AddWithValue("@StudentRegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());

            con.Open();
            var dr = cmd2.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    HttpContext.Current.Session["ClassID"] = dr["ClassID"].ToString();
                    HttpContext.Current.Session["StudentClassID"] = dr["StudentClassID"].ToString();
                }
            }

            con.Close();

            HttpContext.Current.Session["Edu_Year"] = id;
        }

        //google reCaptcha
        protected static string ReCaptchaKey = "6LdXPY0UAAAAAHg7W3SLGr_MQt7wRvV0HLZ8JTBi";
        protected static string ReCaptchaSecret = "6LdXPY0UAAAAALlL5doPnCfNOmEoeSvGum5CdVTq";

        [WebMethod]
        public static string VerifyCaptcha(string response)
        {
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + ReCaptchaSecret + "&response=" + response;
            return (new WebClient()).DownloadString(url);
        }
        private void SetSessionInfoAfterOnlinePayment(string sessionInfo)
        {
            //sessionInfo = "{SchoolID=1012,SchoolName=SIKKHALOY,RegistrationID=13548,EducationYearID=2464,StudentID=41148,ClassID=130,StudentClassID=189569,TeacherID=null}";
            sessionInfo = sessionInfo.TrimStart('{').TrimEnd('}');
            Dictionary<string, string> dictionary = sessionInfo.Split(',').ToDictionary(item => item.Split('=')[0], item => item.Split('=')[1]);
            if (dictionary != null) {
                foreach (KeyValuePair<string, string> entry in dictionary)
                {
                    Session[entry.Key] = !string.IsNullOrEmpty(entry.Value) ? entry.Value : null;
                }
            }
        }
    }
}