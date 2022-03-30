using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class StudentNotice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void NoticeButton_Click(object sender, EventArgs e)
        {
            //cehck
            var isSelected = false;
            foreach (ListItem item in ClassCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    isSelected = true;
                }
            }

            if (isSelected)
            {
                StudentNoticeSQL.Insert();

                foreach (ListItem item in ClassCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        StudentNoticeClassSQL.InsertParameters["StudentNoticeId"].DefaultValue = ViewState["StudentNoticeId"].ToString();
                        StudentNoticeClassSQL.InsertParameters["ClassId"].DefaultValue = item.Value;
                        StudentNoticeClassSQL.Insert();
                    }
                }
            }
        }

        protected void StudentNoticeSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["StudentNoticeId"] = e.Command.Parameters["@StudentNoticeId"].Value;
        }
    }
}