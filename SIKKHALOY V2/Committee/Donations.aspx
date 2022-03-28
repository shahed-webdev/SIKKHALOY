<%@ Page Title="Donations" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Donations.aspx.cs" Inherits="EDUCATION.COM.Committee.Donations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="d-flex justify-content-between align-items-center py-0">Donations
        <a class="btn btn-dark" href="DonationAdd.aspx">Add Donation</a>
    </h3>

    <div class="d-flex align-items-center">
        <div class="form-group">
            <label>Committee Member</label>
            <asp:DropDownList ID="CommitteeMemberDropDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CommitteeMemberSQL" DataTextField="MemberName" DataValueField="CommitteeMemberId">
                <asp:ListItem Value="">[ All ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CommitteeMemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberId, MemberName FROM CommitteeMember WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group ml-4">
            <label>Donation Category</label>
            <asp:DropDownList ID="DonationCategoryDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                <asp:ListItem Value="">[ All ]</asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>

    <div class="table-embed-responsive mt-2">
        <asp:GridView ID="DonationGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" DataSourceID="AddDonationSQL">
            <Columns>
                <asp:TemplateField HeaderText="Donation Category" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditCategoryDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId" SelectedValue='<%# Bind("CommitteeDonationCategoryId") %>'>
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("DonationCategory") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Donation Amount" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Amount") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("Amount") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid Amount" SortExpression="PaidAmount">
                    <ItemTemplate>
                        <%# Eval("PaidAmount") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <%# Eval("Due") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description" SortExpression="Description">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("Description") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Add Date" SortExpression="InsertDate">
                    <ItemTemplate>
                        <%# Eval("InsertDate", "{0:d MMM yyyy}") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Edit">
                    <EditItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                        &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:CommandField HeaderText="Delete" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:SqlDataSource ID="AddDonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeDonation.CommitteeDonationId, CommitteeDonation.CommitteeDonationCategoryId, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.IsPaid, CommitteeDonation.Description, CommitteeDonation.InsertDate, CommitteeDonation.PromiseDate FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.CommitteeMemberId LIKE @CommitteeMemberId) AND (CommitteeDonation.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId)"
        DeleteCommand="DELETE FROM CommitteeDonation WHERE (CommitteeDonationId = @CommitteeDonationId) AND (PaidAmount = 0)"
        UpdateCommand="UPDATE CommitteeDonation SET CommitteeDonationCategoryId = @CommitteeDonationCategoryId, Amount = CASE WHEN PaidAmount &gt; @Amount THEN Amount ELSE @Amount END, Description = @Description WHERE (CommitteeDonationId = @CommitteeDonationId)">
        <DeleteParameters>
            <asp:Parameter Name="CommitteeDonationId" />
        </DeleteParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" DefaultValue="%" Name="CommitteeMemberId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="DonationCategoryDownList" DefaultValue="%" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="CommitteeDonationCategoryId" />
            <asp:Parameter Name="Amount" />
            <asp:Parameter Name="Description" />
            <asp:Parameter Name="CommitteeDonationId" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
