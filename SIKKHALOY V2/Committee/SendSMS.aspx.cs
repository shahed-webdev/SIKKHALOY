using Education;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace EDUCATION.COM.Committee
{
    public partial class SendSMS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            var msg = SMSTextBox.Text;

            var sms = new SMS_Class(Session["SchoolID"].ToString());

            var totalSms = 0;
            var sentMgsConfirm = false;
            var sentMsgCont = 0;

            var smsBalance = sms.SMSBalance;

            #region Send SMS to All member
            con.Open();
            var smsCommand = new SqlCommand(
                "SELECT CommitteeMemberId, SmsNumber FROM CommitteeMember WHERE (SchoolID = @SchoolID) AND (CommitteeMemberTypeId = @CommitteeMemberTypeId)",
                con);
            smsCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            smsCommand.Parameters.AddWithValue("@CommitteeMemberTypeId", TypeDropDownList.SelectedValue);

            var smsDr = smsCommand.ExecuteReader();

            var phoneNo = "";
            while (smsDr.Read())
            {
                phoneNo = smsDr["SmsNumber"].ToString();

                var isValid = sms.SMS_Validation(phoneNo, msg);
                if (isValid.Validation)
                {
                    totalSms += sms.SMS_Conut(msg);
                }
            }

            con.Close();

            if (smsBalance >= totalSms)
            {
                if (sms.SMS_GetBalance() >= totalSms)
                {
                    con.Open();
                    smsDr = smsCommand.ExecuteReader();

                    while (smsDr.Read())
                    {
                        phoneNo = smsDr["SmsNumber"].ToString();
                        var isValid = sms.SMS_Validation(phoneNo, msg);

                        if (isValid.Validation)
                        {
                            var smsSendId = sms.SMS_Send(phoneNo, msg, "SMS Service");
                            if (smsSendId != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue =
                                    smsSendId.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue =
                                    Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue =
                                    Session["Edu_Year"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["CommitteeMemberId"].DefaultValue =
                                    smsDr["CommitteeMemberId"].ToString();

                                SMS_OtherInfoSQL.Insert();
                                sentMsgCont++;
                                sentMgsConfirm = true;
                            }
                        }
                    }

                    con.Close();

                    if (sentMgsConfirm)
                    {
                        SMSTextBox.Text = string.Empty;
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                            "alert('You Have Successfully Sent " + sentMsgCont.ToString() + " SMS.')", true);
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

            #endregion
        }
    }
}