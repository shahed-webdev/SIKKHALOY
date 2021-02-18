<%@ Page Title="My Report" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Student_Report.aspx.cs" Inherits="EDUCATION.COM.Student.Student_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Fault_GV { border: 1px solid #fff; }
            .Fault_GV th { border: 1px solid #fff; }
            .Fault_GV td { border: 1px solid #fff; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>My Report</h3>
    <asp:GridView ID="Fault_Gridview" CssClass="Fault_GV" DataKeyNames="StudentFaultID" runat="server" DataSourceID="FaultSQL" AutoGenerateColumns="False">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <div class="panel well">
                        <div class="panel-heading">
                            <h3><%# Eval("Fault_Title") %></h3>
                            <small><em><%# Eval("Fault_Date","{0:d MMM yyyy}") %></em></small>
                        </div>

                        <div class="panel-body">
                            <p><%# Eval("Fault") %></p>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="FaultSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentFaultID, Fault_Title, Fault, Fault_Date FROM Student_Fault WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("#_9").addClass("active");
        });
    </script>
</asp:Content>
