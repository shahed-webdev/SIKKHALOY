<%@ Page Title="Add Donation" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Donation</h3>

    <div class="row mb-4">
        <div class="col-md-3 col-lg-2">
            <div class="form-btn-group">
                <label>Find Donar</label>
                <asp:TextBox ID="FindDonarTextBox" runat="server" CssClass="form-control" required=""></asp:TextBox>
            </div>
        </div>
        <div class="col-md-3">
            <div class="form-btn-group">
                <label>Donation Category</label>
                <asp:DropDownList ID="CategoryDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                    <asp:ListItem Value="">[ Select Category ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
        <div class="col-md-3">
            <div class="form-btn-group">
                <label>Donation Amount</label>
                <asp:TextBox ID="DonationAmountTextBox" type="number" runat="server" CssClass="form-control" required=""></asp:TextBox>
            </div>
        </div>
        <div class="col-md-3 col-lg-4">
            <div class="form-btn-group">
                <label>Descriptions</label>
                <asp:TextBox ID="DescriptionsTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
    </div>

    <div class="row mb-3">
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Promised Date</label>
                <asp:TextBox ID="PromisedDateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Paid Amount</label>
                <asp:TextBox ID="PaidAmountTextBox" type="number" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Paid Date</label>
                <asp:TextBox ID="PaidDateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Account</label>
                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control">
                    <asp:ListItem Value="">[ Select Account ]</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
    </div>

    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary m-0" Text="Submit" />
</asp:Content>
