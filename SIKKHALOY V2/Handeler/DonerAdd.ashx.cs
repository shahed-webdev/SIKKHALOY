using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Summary description for DonerAdd
    /// </summary>
    public class DonerAdd : WebService
    {

        [WebMethod(EnableSession = true)]
        public string DonerAddApi(DonerAddModel model)
        {
            var committeeMemberModel = new CommitteeMemberViewModel();
            using (SqlConnection con =
                   new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"]
                       .ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText =
                        @"INSERT INTO CommitteeMember(CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName, SmsNumber, Address) VALUES (@CommitteeMemberTypeId, @RegistrationID, @SchoolID, @MemberName, @SmsNumber, @Address) SELECT SCOPE_IDENTITY()";
                    cmd.Parameters.AddWithValue("@CommitteeMemberTypeId", model.CommitteeMemberTypeId);
                    cmd.Parameters.AddWithValue("@RegistrationID",
                        HttpContext.Current.Session["RegistrationID"].ToString());
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@MemberName", model.MemberName);
                    cmd.Parameters.AddWithValue("@SmsNumber", model.SmsNumber);
                    cmd.Parameters.AddWithValue("@Address", model.Address);

                    cmd.Connection = con;

                    con.Open();
                    var committeeMemberIdObj = cmd.ExecuteScalar();
                    con.Close();

                    cmd.CommandText =
                        @"SELECT CommitteeMemberId, MemberName, SmsNumber FROM CommitteeMember WHERE(SchoolID = @SchoolID) AND (CommitteeMemberId = @CommitteeMemberId)";

                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberIdObj.ToString());
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        committeeMemberModel = new CommitteeMemberViewModel
                        {
                            CommitteeMemberId = dr["CommitteeMemberId"].ToString(),
                            MemberName = dr["MemberName"].ToString(),
                            SmsNumber = dr["SmsNumber"].ToString(),
                        };
                    }

                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(committeeMemberModel);
                    return json;
                }
            }
        }

        public class DonerAddModel
        {
            public int CommitteeMemberTypeId { get; set; }
            public int RegistrationID { get; set; }
            public int SchoolID { get; set; }
            public string MemberName { get; set; }
            public string SmsNumber { get; set; }
            public string Address { get; set; }

        }
    }
}