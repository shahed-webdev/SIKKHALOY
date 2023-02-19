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
    <asp:SqlDataSource ID="SmsSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" SelectCommand="SELECT SmsProvider, SmsProviderMultiple FROM SikkhaloySetting" UpdateCommand="UPDATE SikkhaloySetting SET SmsProvider = @SmsProvider, SmsProviderMultiple = @SmsProviderMultiple">
        <UpdateParameters>
            <asp:ControlParameter ControlID="SmsProviderRadioButtonList" Name="SmsProvider" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SmsProviderMultipleRadioButtonList" Name="SmsProviderMultiple" PropertyName="SelectedValue" />
        </UpdateParameters>
</asp:SqlDataSource>
</asp:Content>
