using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee.Payorder
{
    public partial class Payorder_Monthly : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void AddPayorderButton_Click(object sender, EventArgs e)
        {
            PayorderNameSQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            SqlCommand EmployeeID_Cmd = new SqlCommand("SELECT EmployeeID FROM VW_Emp_Info WHERE (Job_Status = N'Active') AND (Time_Basis_Type = N'Monthly') AND (SchoolID = @SchoolID) AND ( EmployeeID NOT IN(SELECT Employee_Payorder_Monthly.EmployeeID FROM  Employee_Payorder_Monthly INNER JOIN Employee_Payorder ON Employee_Payorder_Monthly.Employee_PayorderID = Employee_Payorder.Employee_PayorderID WHERE (Employee_Payorder_Monthly.SchoolID = @SchoolID) AND (Employee_Payorder_Monthly.MonthName = @MonthName) AND (Employee_Payorder.Employee_Payorder_NameID = @Employee_Payorder_NameID)))", con);
            EmployeeID_Cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            EmployeeID_Cmd.Parameters.AddWithValue("@MonthName", MonthNameDropDownList.SelectedItem.Text);
            EmployeeID_Cmd.Parameters.AddWithValue("@Employee_Payorder_NameID", PayorderNameDropDownList.SelectedValue);
            con.Open();
            SqlDataReader EmployeeID_DR;
            EmployeeID_DR = EmployeeID_Cmd.ExecuteReader();

            while (EmployeeID_DR.Read())
            {
                PayorderSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeID_DR["EmployeeID"].ToString();
                PayorderSQL.Insert();
            }
            con.Close();

            PayOrderGridView.DataBind();
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in PayOrderGridView.Rows)
            {
                Repeater BonusRepeater = (Repeater)row.FindControl("BonusRepeater");
                HiddenField Employee_PayorderID = (HiddenField)row.FindControl("Employee_PayorderID");
                var AttendanceFineTextBox = (TextBox)row.FindControl("AttendanceFineTextBox");

                foreach (RepeaterItem item in BonusRepeater.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var BonusID = (HiddenField)item.FindControl("BonusID");
                        var BonusTextBox = (TextBox)item.FindControl("BonusTextBox");

                        EmplyoeePayOrderSQL.InsertParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                        EmplyoeePayOrderSQL.InsertParameters["Bonus_Amount"].DefaultValue = BonusTextBox.Text.Trim();
                        EmplyoeePayOrderSQL.InsertParameters["BonusID"].DefaultValue = BonusID.Value;
                        EmplyoeePayOrderSQL.InsertParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                        EmplyoeePayOrderSQL.Insert();
                    }
                }


                Repeater FineRepeater = (Repeater)row.FindControl("FineRepeater");
                foreach (RepeaterItem item in FineRepeater.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var FineID = (HiddenField)item.FindControl("FineID");
                        var FineTextBox = (TextBox)item.FindControl("FineTextBox");

                        EmplyoeePayOrderSQL.UpdateParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                        EmplyoeePayOrderSQL.UpdateParameters["Fine_Amount"].DefaultValue = FineTextBox.Text.Trim();
                        EmplyoeePayOrderSQL.UpdateParameters["FineID"].DefaultValue = FineID.Value;
                        EmplyoeePayOrderSQL.UpdateParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                        EmplyoeePayOrderSQL.Update();
                    }
                }

                AttendanceFineUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                AttendanceFineUpdateSQL.UpdateParameters["FineAmount"].DefaultValue = AttendanceFineTextBox.Text.Trim();
                AttendanceFineUpdateSQL.UpdateParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                AttendanceFineUpdateSQL.Update();
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfull !!')", true);
        }
    }
}