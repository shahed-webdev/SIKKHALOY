using System;

namespace EDUCATION.COM.Committee
{
    public partial class MemberAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddMemberButton_Click(object sender, EventArgs e)
        {
            MemberSQL.Insert();
            MemberGridView.DataBind();
        }
    }
}