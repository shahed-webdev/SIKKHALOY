using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Web.Services;

namespace EDUCATION.COM
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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
    }
}