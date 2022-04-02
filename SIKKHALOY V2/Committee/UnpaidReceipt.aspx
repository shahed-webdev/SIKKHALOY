<%@ Page Title="" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="UnpaidReceipt.aspx.cs" Inherits="EDUCATION.COM.Committee.UnpaidReceipt" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/Admission/CSS/Student_List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Unpaid Money Receipt</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="ReceiptTextBox" placeholder="Enter Receipt No" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" DataKeyNames="CommitteeMoneyReceiptId" Width="100%">
        <ItemTemplate>
            <div class="z-depth-1 p-3 mb-4">
                <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                    <div class="user-photo">
                        <img alt="photo" src="data:image/jpg;base64, <%# Convert.ToBase64String((byte[])Eval("Photo")) %>" onerror="this.src='/Handeler/Default/Male.png'" class="img-thumbnail rounded-circle img-fluid z-depth-1" />
                        <div class="user-activation">
                            <%# Eval("CommitteeMemberType") %>
                        </div>
                    </div>
                    <div class="info">
                        <ul>
                            <li><strong><%# Eval("MemberName") %></strong></li>
                            <li><b>Total Donation: </b><%# Eval("TotalDonation") %></li>
                            <li><b>Total Paid: </b><%# Eval("PaidDonation") %></li>
                            <li><b>Phone: </b><%# Eval("SmsNumber") %></li>
                            <li><b>Address: </b><%# Eval("Address") %></li>
                        </ul>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMemberType.CommitteeMemberType FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId INNER JOIN CommitteeMoneyReceipt ON CommitteeMember.CommitteeMemberId = CommitteeMoneyReceipt.CommitteeMemberId WHERE (CommitteeMoneyReceipt.CommitteeMoneyReceiptSn = @MoneyReceipt_SN) AND (CommitteeMoneyReceipt.SchoolId = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:GridView ID="PaymentGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaymentSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="DonationCategory" HeaderText="Category" SortExpression="DonationCategory" />
            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
            <asp:TemplateField HeaderText="Paid Amount" SortExpression="PaidAmount">
                <ItemTemplate>
                    <label class="paid-amount"><%# Eval("PaidAmount") %></label>
                </ItemTemplate>
                <FooterTemplate>
                    <strong id="total-paid"></strong>
                </FooterTemplate>
            </asp:TemplateField>
        </Columns>
        <FooterStyle CssClass="grid-footer" />
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="PaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                       SelectCommand="SELECT CommitteePaymentRecord.SchoolId, CommitteePaymentRecord.PaidAmount, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description FROM CommitteePaymentRecord INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId WHERE (CommitteePaymentRecord.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptSn = @MoneyReceipt_SN)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%" DataKeyNames="TotalAmount">
        <ItemTemplate>
            <div class="mt-3">
                <h4>
                    <span class="badge badge-light m-2">Receipt No: <%# Eval("CommitteeMoneyReceiptSn") %> </span>
                    <span class="badge badge-light m-2">Payment Method: <%# Eval("AccountName") %> </span>
                    <span class="badge badge-light m-2">Total Amount: <%# Eval("TotalAmount") %> Tk</span>
                    <span class="badge badge-light m-2">Paid Date: <%# Eval("PaidDate","{0:d MMM yyyy (hh:mm tt)}") %></span>
                </h4>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn, CommitteeMoneyReceipt.TotalAmount, CommitteeMoneyReceipt.PaidDate, Account.AccountName FROM CommitteeMoneyReceipt INNER JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID WHERE (CommitteeMoneyReceipt.CommitteeMoneyReceiptSn = @MoneyReceipt_SN) AND (CommitteeMoneyReceipt.SchoolId = @SchoolID)"
        DeleteCommand="BEGIN TRY
    BEGIN TRANSACTION
        DELETE FROM CommitteePaymentRecord FROM CommitteeMoneyReceipt INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId 
        WHERE (CommitteeMoneyReceipt.SchoolID = @SchoolID) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptSn = @MoneyReceipt_SN)

        DELETE FROM CommitteeMoneyReceipt WHERE (SchoolID = @SchoolID) AND (CommitteeMoneyReceiptSn = @MoneyReceipt_SN)
        COMMIT
END TRY
BEGIN CATCH
    ROLLBACK
END CATCH">
        <DeleteParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </DeleteParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="RByFormView" runat="server" DataSourceID="ReceivedBySQL" Width="100%">
        <ItemTemplate>
            <div class="RecvBy">
                Received By:
                <%# Eval("Name") %>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn FROM Admin INNER JOIN CommitteeMoneyReceipt ON Admin.RegistrationID = CommitteeMoneyReceipt.RegistrationId WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptSn = @MoneyReceipt_SN)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <%if (PaymentGridView.Rows.Count > 0)
        { %>
    <div class="mt-2">
        <asp:Button ID="DeleteReceiptButton" runat="server" OnClick="DeleteReceiptButton_Click" Text="Unpaid" CssClass="btn btn-info" />
    </div>
    <%} %>
</asp:Content>
