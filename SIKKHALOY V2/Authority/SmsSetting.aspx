<%@ Page Title="" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="SmsSetting.aspx.cs" Inherits="EDUCATION.COM.Authority.SmsSetting" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Single SMS Provider</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="SmsProviderRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
            </asp:RadioButtonList>
        </div>
    </div>
    <h3>Multiple SMS Provider</h3> 
    <div class="form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="SmsProviderMultipleRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
            </asp:RadioButtonList>
        </div>
    </div>
     
    <h3>SMS Pending</h3>  
    <div class="form-group">
        <asp:FormView ID="SMSPendingFormView" runat="server" DataSourceID="SMSPendingSQL">
            <ItemTemplate>
                PendingSMS:
                <asp:Label ID="PendingSMSLabel" runat="server" Text='<%# Bind("PendingSMS") %>' />
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="SMSPendingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Attendance_SMSID) AS PendingSMS FROM Attendance_SMS"></asp:SqlDataSource>
    </div>
    <h3>SMS Sender Setting</h3> 
    
    <div class="form-inline">
        <div class="form-group">
            <label>SMS Sending Interval</label>
             <asp:TextBox ID="SMSSendingIntervalTextBox" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>SMS Processing Unit</label>
            <asp:TextBox ID="SMSProcessingUnitTextBox" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
        </div>
        
        <div class="form-group">
            <asp:Button ID="SMSSettingUpdateButton" runat="server"  CssClass="btn btn-primary" Text="Update SMS Setting" OnClick="SMSSettingUpdateButton_Click"/>
            </div>
    </div>
    <asp:SqlDataSource ID="SmsSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" SelectCommand="SELECT SmsProvider, SmsProviderMultiple, SmsSendInterval, SmsProcessingUnit FROM SikkhaloySetting" UpdateCommand="UPDATE SikkhaloySetting SET SmsProvider = @SmsProvider, SmsProviderMultiple = @SmsProviderMultiple" InsertCommand="UPDATE SikkhaloySetting SET SmsSendInterval = @SmsSendInterval, SmsProcessingUnit = @SmsProcessingUnit">
        <InsertParameters>
            <asp:ControlParameter ControlID="SMSSendingIntervalTextBox" Name="SmsSendInterval" PropertyName="Text" />
            <asp:ControlParameter ControlID="SMSProcessingUnitTextBox" Name="SmsProcessingUnit" PropertyName="Text" />
        </InsertParameters>
        <UpdateParameters>
            <asp:ControlParameter ControlID="SmsProviderRadioButtonList" Name="SmsProvider" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SmsProviderMultipleRadioButtonList" Name="SmsProviderMultiple" PropertyName="SelectedValue" />
        </UpdateParameters>
</asp:SqlDataSource>
  
    <h3>SMS Sender Reports</h3>
    <asp:GridView ID="SmsSenderGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="AttendanceSmsSenderId" DataSourceID="SmsSenderSQL" AllowPaging="True" AllowSorting="True" PageSize="50">
        <Columns>
            <asp:BoundField DataField="AppStartTime" HeaderText="App Start Time" SortExpression="AppStartTime" />
            <asp:BoundField DataField="AppCloseTime" HeaderText="App Close Time" SortExpression="AppCloseTime" />
            <asp:BoundField DataField="TotalEventCall" HeaderText="Event Call" SortExpression="TotalEventCall" />
            <asp:BoundField DataField="TotalSmsSend" HeaderText="SMS Send" SortExpression="TotalSmsSend" />
            <asp:BoundField DataField="TotalSmsFailed" HeaderText="SMS Failed" SortExpression="TotalSmsFailed" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="SmsSenderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceSmsSenderId, AppStartTime, AppCloseTime, TotalEventCall, TotalSmsSend, TotalSmsFailed FROM Attendance_SMS_Sender ORDER BY AttendanceSmsSenderId DESC"></asp:SqlDataSource>
    <h3>SMS Fail Records</h3>
    <asp:GridView ID="SmsFailGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="AttendanceSmsFailedId" DataSourceID="SmsFailSQL" AllowPaging="True" AllowSorting="True" PageSize="50">
        <Columns>
            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:BoundField DataField="FailedReson" HeaderText="Failed Reson" SortExpression="FailedReson" />
            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" SortExpression="AttendanceDate" />
            <asp:BoundField DataField="CreateTime" HeaderText="Create" SortExpression="CreateTime" />
            <asp:BoundField DataField="ScheduleTime" HeaderText="Schedule" SortExpression="ScheduleTime" />
            <asp:BoundField DataField="SMS_Text" HeaderText="Text" SortExpression="SMS_Text" />
            <asp:BoundField DataField="MobileNo" HeaderText="Number" SortExpression="MobileNo" />
            <asp:BoundField DataField="AttendanceStatus" HeaderText="Attendance" SortExpression="AttendanceStatus" />
            <asp:BoundField DataField="SMS_TimeOut" HeaderText="TimeOut" SortExpression="SMS_TimeOut" />
            <asp:BoundField DataField="InsertDate" HeaderText="Fail Date" SortExpression="InsertDate" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="SmsFailSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_SMS_Failed.AttendanceSmsFailedId, Attendance_SMS_Failed.SchoolID, Attendance_SMS_Failed.ScheduleTime, Attendance_SMS_Failed.CreateTime, Attendance_SMS_Failed.SentTime, Attendance_SMS_Failed.AttendanceDate, Attendance_SMS_Failed.SMS_Text, Attendance_SMS_Failed.MobileNo, Attendance_SMS_Failed.AttendanceStatus, Attendance_SMS_Failed.SMS_TimeOut, Attendance_SMS_Failed.FailedReson, Attendance_SMS_Failed.InsertDate, SchoolInfo.SchoolName FROM Attendance_SMS_Failed INNER JOIN SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID"></asp:SqlDataSource>
</asp:Content>
