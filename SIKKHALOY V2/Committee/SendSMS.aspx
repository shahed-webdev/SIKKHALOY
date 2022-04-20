<%@ Page Title="Send SMS to Committee" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SendSMS.aspx.cs" Inherits="EDUCATION.COM.Committee.SendSMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col-md-10 col-lg-8 mx-auto">
            <div class="card card-body">
                <h2 class="font-weight-bold mb-3">Send SMS To Committee</h2>

                <div class="form-group">
                    <label>Member Type</label>

                <asp:GridView ID="AllStudentsGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="CommitteeMemberTypeId" CssClass="mGrid" DataSourceID="MemberTypeSQL">
                    <Columns>
                        <asp:TemplateField HeaderText="Select">
                            <HeaderTemplate>
                                <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text=" " />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CommitteeMemberType" HeaderText="Type" SortExpression="CommitteeMemberType" />
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>

                    <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="form-group">
                    <label>SMS Text</label>
                    <asp:TextBox ID="SMSTextBox" runat="server" CssClass="form-control" TextMode="MultiLine" required=""></asp:TextBox>
                </div>

                <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL" RenderOuterTable="false">
                    <ItemTemplate>
                        Remaining SMS:<%# Eval("SMS_Balance") %>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:Button ID="SMSButton" runat="server" Text="Send SMS" CssClass="btn btn-primary ml-0" OnClick="SMSButton_Click" />
                <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, EducationYearID, CommitteeMemberId) VALUES (@SMS_Send_ID, @SchoolID, @EducationYearID, @CommitteeMemberId)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                        <asp:Parameter Name="SchoolID" />
                        <asp:Parameter Name="StudentID" />
                        <asp:Parameter Name="TeacherID" />
                        <asp:Parameter Name="EducationYearID" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>
</asp:Content>
