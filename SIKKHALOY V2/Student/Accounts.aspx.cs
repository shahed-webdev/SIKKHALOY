using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Accounts : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void MreceiptLinkButton_Command(object sender, CommandEventArgs e)
        {
            AllPayRecordSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }
    }
}