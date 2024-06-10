﻿<%@ Page Title="Exam Position" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ExamPosition.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamPosition" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css?v=1.0.1" rel="stylesheet" />
    <style>
        .FthSub { color: #304ffe; font-size: 12px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="ExamPosition_WithSub.aspx" class="NoPrint">Full Tabulation Sheet >>></a>
    <h3>
        <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
    </h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID"
                OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID"
                OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID"
                OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:RadioButtonList ID="Resultstatus_RBList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                <asp:ListItem Selected="True" Value="%">All</asp:ListItem>
                <asp:ListItem Value="P">Passed</asp:ListItem>
                <asp:ListItem Value="F">Failed</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>


    <%if (StudentsGridView.Rows.Count > 0)
        {%>
    <div class="d-print-none text-right">
        <button type="button" class="btn btn-link" data-toggle="modal" data-target="#printOptionModal">
            <i class="fa fa-cog" aria-hidden="true"></i>
            Print Option
        </button>

        <button type="button" class="btn btn-primary" onclick="window.print()">
            <i class="fa fa-print" aria-hidden="true"></i>
            Print
        </button>
    </div>
    <%}%>


    <div id="ExportPanel" runat="server" class="Exam_Position">
        <asp:Label ID="Export_ClassLabel" runat="server" Font-Bold="True" Font-Names="Tahoma" Font-Size="20px"></asp:Label>
        <div class="table-responsive">
            <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PagerStyle-CssClass="pgr" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL"
                DataKeyNames="SMSPhoneNo,StudentID,PassStatus_InSubject,StudentsName,ObtainedMark_ofStudent,Student_Grade,Student_Point,Position_InExam_Class,Position_InExam_Subsection,ID"
                OnRowDataBound="StudentsGridView_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text=" " />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <HeaderStyle CssClass="NoPrint" />
                        <ItemStyle Width="50px" CssClass="NoPrint" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                    <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                    <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                    <asp:TemplateField HeaderText="Subjects - Marks">
                        <ItemTemplate>
                            <asp:DataList ID="SMarksDataList" runat="server" DataSourceID="SUB_markSQL" RepeatDirection="Horizontal" Width="100%">
                                <ItemTemplate>
                                    <div class="Is_Subject">
                                        <input class="PassStatus_Subject" type="hidden" value='<%# Eval("PassStatus_Subject") %>' />
                                        <input class="SubjectType" type="hidden" value='<%# Eval("SubjectType") %>' />

                                        <asp:Label ID="SubjectNameLabel" runat="server" Text='<%# Eval("SubjectName") %>' />
                                        <br />
                                        <asp:Label ID="MarkLabel" runat="server" Text='<%# Eval("Mark","{0:F2}") %>' Font-Bold="True" />
                                    </div>
                                </ItemTemplate>
                            </asp:DataList>
                            <asp:HiddenField ID="StudentResultIDHiddenField" runat="server" Value='<%# Eval("StudentResultID") %>' />
                            <asp:SqlDataSource ID="SUB_markSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectName,Exam_Result_of_Subject.PassStatus_Subject, Exam_Result_of_Subject.SubjectType,Exam_Result_of_Subject.ObtainedMark_ofSubject AS Mark, Subject.SubjectID FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.StudentResultID = @StudentResultID) ORDER BY ISNULL(Subject.SN,999), Subject.SubjectID">
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="StudentResultIDHiddenField" Name="StudentResultID" PropertyName="Value" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ObtainedMark_ofStudent" HeaderText="Total" SortExpression="ObtainedMark_ofStudent" DataFormatString="{0:n2}" />
                    <asp:TemplateField HeaderText="Grade" SortExpression="Student_Grade">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Student_Grade") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Student_Point" HeaderText="Point" SortExpression="Student_Point" DataFormatString="{0:0.00}" />
                    <asp:TemplateField HeaderText="AVG." SortExpression="Average">
                        <ItemTemplate>
                            <span class="avrage-marks">
                                <%# Eval("Average") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Position_InExam_Class" HeaderText="P.C" SortExpression="Position_InExam_Class" />
                    <asp:BoundField DataField="Position_InExam_Subsection" HeaderText="P.S" SortExpression="Position_InExam_Subsection" />
                </Columns>
                <EmptyDataTemplate>
                    Empty
                </EmptyDataTemplate>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
        </div>
        <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID,StudentsClass.RollNo, Student.StudentsName, Exam_Result_of_Student.StudentResultID, Exam_Result_of_Student.ObtainedMark_ofStudent,
                         Exam_Result_of_Student.Student_Grade, Exam_Result_of_Student.Student_Point, CAST(Exam_Result_of_Student.Position_InExam_Class AS int) AS Position_InExam_Class,
                         CAST(Exam_Result_of_Student.Position_InExam_Subsection AS int) AS Position_InExam_Subsection, Student.SMSPhoneNo, Exam_Result_of_Student.ExamID,
                         Exam_Result_of_Student.PassStatus_InSubject, Exam_Result_of_Student.TotalSubject, Exam_Result_of_Student.Average
FROM            StudentsClass INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN
                         Exam_Result_of_Student ON StudentsClass.StudentClassID = Exam_Result_of_Student.StudentClassID
WHERE        (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND
                         (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND
                         (Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND
                         (Exam_Result_of_Student.PassStatus_InSubject LIKE @PassStatus)
ORDER BY Position_InExam_Class , CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END">
            <SelectParameters>
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="Resultstatus_RBList" Name="PassStatus" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
            <InsertParameters>
                <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                <asp:Parameter Name="SchoolID" />
                <asp:Parameter Name="StudentID" />
                <asp:Parameter Name="TeacherID" />
                <asp:Parameter Name="EducationYearID" />
            </InsertParameters>
        </asp:SqlDataSource>
    </div>

    <%if (StudentsGridView.Rows.Count > 0)
        {%>
    <div class="NoPrint">
        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="1"></asp:CustomValidator>
    </div>

    <asp:CheckBox ID="SecPositionCheckBox" CssClass="NoPrint" runat="server" Text="Send Section Position" />

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:Button ID="SMSButton" runat="server" CssClass="btn btn-primary" Text="Send Result By SMS" OnClick="SMSButton_Click" ValidationGroup="1" />
        </div>
        <div class="form-group">
            <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-primary" OnClick="ExportWordButton_Click" Text="Export To Word" />
        </div>
    </div>

    <asp:FormView ID="SMSFormView" runat="server" CssClass="NoPrint" DataKeyNames="SMSID" DataSourceID="SMSSQL" Width="100%">
        <ItemTemplate>
            <div class="alert alert-success">
                (Remaining SMS: <%#Eval("SMS_Balance") %>)
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%}%>

    <!-- modal print option--->
    <div class="modal fade d-print-none" id="printOptionModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Print Option</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <input onchange="printHiddenTableColumn(10,this);" type="checkbox" id="isHiddenPrintClassCol" />
                        <label for="isHiddenPrintClassCol">Hide Class Position Column</label>
                    </div>
                    <div class="form-group">
                        <input onchange="printHiddenTableColumn(11,this);" type="checkbox" id="isHiddenPrintSectionCol" />
                        <label for="isHiddenPrintSectionCol">Hide Section Position Column</label>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" data-dismiss="modal">OK</button>
                </div>
            </div>
        </div>
    </div>




    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        $(function () {
            $(".PassStatus_Subject").each(function () {
                if ($(this).val() === 'F') {
                    $(this).parent('.Is_Subject').css({ "color": "red" });
                }
            });

            $(".SubjectType").each(function () {
                if ($(this).val() === 'Optional') {
                    $(this).parent('.Is_Subject').append('<span class="FthSub">4th</span>');
                }
            });


            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        //Select at least one Checkbox From GridView
        function Validate(d, c) {
            for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };

        // Print Option
        function printHiddenTableColumn(columnNumber, selft) {
            const header = $('#<%=StudentsGridView.ClientID %> thead tr');
            const body = $('#<%=StudentsGridView.ClientID %> tbody tr');

            console.log(selft, columnNumber);

            // if checked then hide the column
            if ($(selft).is(':checked')) {
                header.find('th:nth-child(' + columnNumber + ')').addClass('d-print-none');
                body.find('td:nth-child(' + columnNumber + ')').addClass('d-print-none');
            } else {
                header.find('th:nth-child(' + columnNumber + ')').removeClass('d-print-none');
                body.find('td:nth-child(' + columnNumber + ')').removeClass('d-print-none');
            }
        };
    </script>
</asp:Content>
