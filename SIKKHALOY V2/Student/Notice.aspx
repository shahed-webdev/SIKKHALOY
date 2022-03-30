<%@ Page Title="Notice" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Notice.aspx.cs" Inherits="EDUCATION.COM.Student.Notice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Notice Board</h3>


    <asp:Repeater ID="NoticeRepeater" runat="server" DataSourceID="NoticeSQL">
        <ItemTemplate>
            <div class="card card-body mb-3">
                <h4 class="font-weight-bold"><%# Eval("NoticeTitle") %></h4>
                <p><%# Eval("Notice") %></p>
                <small>Post Date: <%# Eval("InsertDate","{0:d MMM yyyy hh:mm tt}") %></small>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="NoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT StudentNotice.NoticeTitle, StudentNotice.Notice, StudentNotice.InsertDate FROM StudentNoticeClass INNER JOIN StudentNotice ON StudentNoticeClass.StudentNoticeId = StudentNotice.StudentNoticeId INNER JOIN StudentsClass ON StudentNoticeClass.ClassId = StudentsClass.ClassID AND StudentNotice.EducationYearId = StudentsClass.EducationYearID WHERE (StudentNotice.EducationYearId = @EducationYearId) AND (StudentNotice.SchoolId = @SchoolId) AND (StudentsClass.StudentClassID = @StudentClassID) ORDER BY StudentNotice.InsertDate DESC">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("#_10").addClass("active");
        });
    </script>
</asp:Content>
