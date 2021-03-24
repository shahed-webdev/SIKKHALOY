using System;
using System.Web.UI.WebControls;

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

        protected void ActiveCheckBox_OnCheckedChanged(object sender, EventArgs e)
        {
            var activeCheckBox = (CheckBox)sender;
            var row = (GridViewRow)activeCheckBox.Parent.Parent;

            var schoolId = UsersGridView.DataKeys[row.DataItemIndex]?["SchoolID"].ToString();
            DeviceActiveInactiveSQL.UpdateParameters["SchoolID"].DefaultValue = schoolId;
            DeviceActiveInactiveSQL.UpdateParameters["IsActive"].DefaultValue = activeCheckBox.Checked.ToString();
            DeviceActiveInactiveSQL.Update();
        }
    }
}