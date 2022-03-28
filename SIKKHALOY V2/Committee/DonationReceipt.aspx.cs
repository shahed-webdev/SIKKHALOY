using System;
using Education;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationReceipt : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var id = Request.QueryString["id"];
                if (string.IsNullOrEmpty(id)) Response.Redirect("./DonationAdd.aspx");
            }
        }


        //send sms
        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            var msg = "Congrats! ";
            var isSentSMS = false;

            if (InfoFormView.CurrentMode == FormViewMode.ReadOnly)
            {
                var phoneNo = InfoFormView.DataKey["SMSPhoneNo"].ToString();
                var paid = InfoFormView.DataKey["TotalAmount"].ToString();

                msg += (InfoFormView.Row.FindControl("StudentsNameLabel") as Label)?.Text;
                msg += ". You've Paid: " + paid;
                msg += " Tk. Receipt No: " + (InfoFormView.Row.FindControl("MoneyReceiptIDLabel") as Label)?.Text;

  

                msg += ". Regards, " + Session["School_Name"];

                var sms = new SMS_Class(Session["SchoolID"].ToString());
                var smsBalance = sms.SMSBalance;
                var totalSMS = sms.SMS_Conut(msg);

                if (smsBalance >= totalSMS)
                {
                    if (sms.SMS_GetBalance() >= totalSMS)
                    {
                        var isValid = sms.SMS_Validation(phoneNo, msg);

                        if (isValid.Validation)
                        {
                            var smsSendId = sms.SMS_Send(phoneNo, msg, "Payment Collection");

                            SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                            SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = InfoFormView.DataKey["StudentID"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";
                            SMS_OtherInfoSQL.Insert();
                            isSentSMS = true;
                        }
                        else
                        {
                            ErrorLabel.Text = isValid.Message;
                        }
                    }
                    else
                    {
                        ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                    }
                }
                else
                {
                    ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + smsBalance;
                }
            }

            if (isSentSMS)
            {
                Response.Redirect("DonationAdd.aspx");
            }
        }
    }
}