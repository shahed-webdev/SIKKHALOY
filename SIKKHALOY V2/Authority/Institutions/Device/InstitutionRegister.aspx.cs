using System;

namespace EDUCATION.COM.Authority.Institutions.Device
{
    public partial class InstitutionRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Register_Button_Click(object sender, EventArgs e)
        {
            if (School_DropDownList.SelectedIndex > 0)
            {
                UserSQL.Insert();

                Password_TextBox.Text = "";
                School_DropDownList.SelectedIndex = 0;
                ErrorLabel.Text = "User registration Successfully!";
                ErrorLabel.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                ErrorLabel.Text = "server error";
                ErrorLabel.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}