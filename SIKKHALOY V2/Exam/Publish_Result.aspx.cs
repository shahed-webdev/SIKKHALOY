﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class Publish_Result : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        protected void ShowPanel()
        {
            if (OptionalSubjectRadioButtonList.SelectedIndex == 1)
            {
                SpecificMarkPanel.Visible = true;
                OptionalPercentageRequired.Enabled = true;
                OptionalRegularExpressionValidator.Enabled = true;
            }
            else
            {
                SpecificMarkPanel.Visible = false;
                OptionalPercentageRequired.Enabled = false;
                OptionalRegularExpressionValidator.Enabled = false;
            }

            EMMultiView.ActiveViewIndex = ExamRadioButtonList.SelectedIndex;

            if (SubExamRadioButtonList.SelectedIndex == 1)
                SubExamPanel.Visible = true;
            else
                SubExamPanel.Visible = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                DataView ClassDv = new DataView();
                ClassDv = (DataView)ClassSQL.Select(DataSourceSelectArguments.Empty);
                if (ClassDv.Count < 1)
                {
                    CreateExamLink.Visible = true;
                }
                else
                {
                    DataView ExamNameDv = new DataView();
                    ExamNameDv = (DataView)ExamNameSQl.Select(DataSourceSelectArguments.Empty);
                    if (ExamNameDv.Count < 1 && ClassDropDownList.SelectedValue != "0")
                    {
                        CreateExamLink.Visible = true;
                    }
                    else
                    {
                        CreateExamLink.Visible = false;
                    }
                }

                ShowPanel();
            }
            catch { FormsAuthentication.SignOut(); }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExamDropDownList.DataBind();
            MinPercentageTextBox.Text = string.Empty;
            AlTotalMarksTextBox.Text = string.Empty;
            AlTotalMarksTextBox.Text = string.Empty;
            ExamRadioButtonList.SelectedIndex = 0;
            ShowPanel();

            FailEnableInOptionalCheckBox.Checked = false;
            AddOptionalinTotalMarkCheckBox.Checked = false;
            Grade_AS_ItisCheckBox.Checked = false;
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView ResetDv = new DataView();
            ResetDv = (DataView)ResetOptionalSQL.Select(DataSourceSelectArguments.Empty);

            if (ResetDv.Count == 1)
            {
                if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "0")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 0;
                    ShowPanel();
                }
                else if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "100")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 2;
                    ShowPanel();
                }
                else if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 2;
                    ShowPanel();
                }
                else
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 1;
                    MinPercentageTextBox.Text = ResetDv[0]["Optional_Percentage_Deduction"].ToString();
                    ShowPanel();
                }

                //----------------------Fail Enable In Optional CheckBox
                FailEnableInOptionalCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Fail_Enable_Optional_Subject"]);


                //----------------------Add Optional Mark in Total Mark CheckBox
                AddOptionalinTotalMarkCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Add_Optional_Mark_In_FullMarks"]);

                //----------------------Grade Setting
                GradeSetting_RBList.SelectedValue = ResetDv[0]["IS_Grade_BasePoint"].ToString();

                //----------------------Grade AS It is CheckBox
                Grade_AS_ItisCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Enable_Grade_as_it_is_if_Fail"]);

                //----------------------SubExamFailCheckBox
                SubExamFailCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Enable_Fail_if_fail_in_sub_Exam"]);

                //----------------------SectionPositionCheckBox
                SectionPositionCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_Sec_Position"]);

                //----------------------H_FullMarkCheckBox
                H_FullMarkCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_FullMark"]);

                //----------------------H_PassMarkCheckBox
                H_PassMarkCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_PassMark"]);



                if (ResetDv[0]["Attendance_FromDate"].ToString() != "")
                {
                    FromDateTextBox.Text = Convert.ToDateTime(ResetDv[0]["Attendance_FromDate"]).ToString("d MMM yyyy");
                }
                else
                {
                    FromDateTextBox.Text = string.Empty;
                }

                if (ResetDv[0]["Attendance_ToDate"].ToString() != "")
                {
                    ToDateTextBox.Text = Convert.ToDateTime(ResetDv[0]["Attendance_ToDate"]).ToString("d MMM yyyy");
                }
                else
                {
                    ToDateTextBox.Text = string.Empty;
                }
            }
            else
            {
                MinPercentageTextBox.Text = string.Empty;
                FromDateTextBox.Text = string.Empty;
                ToDateTextBox.Text = string.Empty;
            }

            DataView ResetExamDV = new DataView();
            ResetExamDV = (DataView)ResetExamMarkSQL.Select(DataSourceSelectArguments.Empty);
            if (ResetExamDV.Count != 0)
            {
                if (ResetExamDV.Count > 1)
                {
                    ExamRadioButtonList.SelectedIndex = 1;
                    AlTotalMarksTextBox.Text = string.Empty;
                }
                else
                {

                    ExamRadioButtonList.SelectedIndex = 0;
                    AlTotalMarksTextBox.Text = ResetExamDV[0]["Countable_Mark"].ToString();

                }
            }
            DataView ResetSubExamDV = new DataView();
            ResetSubExamDV = (DataView)ResetSubExamSQL.Select(DataSourceSelectArguments.Empty);
            if (ResetSubExamDV.Count > 1)
            {
                SubExamRadioButtonList.SelectedIndex = 1;
            }
            else
            {
                SubExamRadioButtonList.SelectedIndex = 0;
            }

            DataView ResetPositionDv = new DataView();
            ResetPositionDv = (DataView)Exam_Publish_SettingSQL.Select(DataSourceSelectArguments.Empty);
            if (ResetPositionDv.Count == 1)
            {
                Position_RadioButtonList.SelectedValue = ResetPositionDv[0]["Exam_Position_Format"].ToString();
            }

            SubExam_Mark_GridView.DataBind();
            DifferntMarksGridView.DataBind();
            ShowPanel();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            Exam_Publish_SettingSQL.Delete();//delete unwanted data from mark input 

            if (SubExamRadioButtonList.SelectedIndex == 0)
            {
                // -------------for Distribute Equally Sub-Exam Full Marks-----------------Update ---------------
                SubExam_Mark_SQL.Insert();
            }

            if (SubExamRadioButtonList.SelectedIndex == 1)
            {
                foreach (GridViewRow row in SubExam_Mark_GridView.Rows)
                {
                    TextBox AddPercentageTextBox = (TextBox)row.FindControl("SpecificSubExammarksTextBox");

                    if (AddPercentageTextBox.Text.Trim() != string.Empty)
                    {
                        SubExam_Mark_SQL.UpdateParameters["AddPercentage"].DefaultValue = AddPercentageTextBox.Text.Trim();
                        SubExam_Mark_SQL.UpdateParameters["SubExamID"].DefaultValue = SubExam_Mark_GridView.DataKeys[row.RowIndex]["SubExamID"].ToString();
                        SubExam_Mark_SQL.UpdateParameters["SubjectID"].DefaultValue = SubExam_Mark_GridView.DataKeys[row.RowIndex]["SubjectID"].ToString();
                        SubExam_Mark_SQL.Update();
                    }
                }
            }




            if (ExamRadioButtonList.SelectedIndex == 0)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    DifferentMarksSQL.InsertParameters["SubjectID"].DefaultValue = DifferntMarksGridView.DataKeys[row.RowIndex]["SubjectID"].ToString();
                    DifferentMarksSQL.InsertParameters["Countable_Mark"].DefaultValue = AlTotalMarksTextBox.Text;
                    DifferentMarksSQL.Insert();
                }
            }



            if (ExamRadioButtonList.SelectedIndex == 1)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    TextBox HMforExamTextBox = (TextBox)row.FindControl("HMforExamTextBox");
                    DifferentMarksSQL.InsertParameters["SubjectID"].DefaultValue = DifferntMarksGridView.DataKeys[row.RowIndex]["SubjectID"].ToString();
                    DifferentMarksSQL.InsertParameters["Countable_Mark"].DefaultValue = HMforExamTextBox.Text;
                    DifferentMarksSQL.Insert();

                    CheckBox IS_Add_ExamCheckBox = (CheckBox)row.FindControl("IS_Add_ExamCheckBox");

                    DifferentMarksSQL.UpdateParameters["SubjectID"].DefaultValue = DifferntMarksGridView.DataKeys[row.RowIndex]["SubjectID"].ToString();
                    DifferentMarksSQL.UpdateParameters["IS_Add_InExam"].DefaultValue = IS_Add_ExamCheckBox.Checked ? "1" : "0";
                    DifferentMarksSQL.Update();

                }
            }



            if (OptionalSubjectRadioButtonList.SelectedIndex == 0)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = "0";
            }

            if (OptionalSubjectRadioButtonList.SelectedIndex == 1)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = MinPercentageTextBox.Text;
            }
            if (OptionalSubjectRadioButtonList.SelectedIndex == 2)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = "100";
            }

            Exam_Publish_SettingSQL.Insert();
            Student_ResultSQL.Insert();

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Result has been published successfully!!')", true);
        }

        protected void SubjectList()
        {
            con.Open();
            SqlCommand SubjectCommand = new SqlCommand("SELECT  SubjectID, SUM(FullMarks) AS FullMarks FROM Exam_Full_Marks WHERE (SchoolID = @SchoolID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) GROUP BY SubjectID", con);

            SubjectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            SubjectCommand.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            SubjectCommand.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
            SubjectCommand.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);


            SqlDataReader SubjectDR;
            SubjectDR = SubjectCommand.ExecuteReader();

            Dictionary<string, string> SubjectList = new Dictionary<string, string>();

            while (SubjectDR.Read())
            {
                SubjectList.Add(SubjectDR["SubjectID"].ToString(), SubjectDR["FullMarks"].ToString());
            }
            con.Close();

            if (SubjectList.Count > 0)
            {
                foreach (GridViewRow Row in DifferntMarksGridView.Rows)
                {
                    TextBox HMforExamTextBox = (TextBox)Row.FindControl("HMforExamTextBox");
                    string value = "";
                    if (SubjectList.TryGetValue(DifferntMarksGridView.DataKeys[Row.RowIndex]["SubjectID"].ToString(), out value))
                    {
                        HMforExamTextBox.Text = value;
                    }

                }
            }
        }

        protected void FullMarkButton_Click(object sender, EventArgs e)
        {
            SubjectList();

        }

        protected void Student_ResultSQL_Inserting(object sender, SqlDataSourceCommandEventArgs e)
        {
            e.Command.CommandTimeout = 0;
        }
    }
}