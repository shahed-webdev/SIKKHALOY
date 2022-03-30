<%@ Page Title="Payment Record" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="PaymentRecord.aspx.cs" Inherits="EDUCATION.COM.Committee.PaymentRecord" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Payment Record</h3>

    <div class="d-flex align-items-center">
        <div class="form-group">
            <label>Committee Member</label>
            <asp:DropDownList ID="CommitteeMemberDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CommitteeMemberSQL" DataTextField="MemberName" DataValueField="CommitteeMemberId">
                <asp:ListItem Value="%">[ All ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CommitteeMemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberId, MemberName FROM CommitteeMember WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mx-3">
            <label>From Date</label>
            <asp:TextBox ID="FormDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>To Date</label>
            <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-outline-primary btn-md" Text="Find" />
        </div>
    </div>

    <asp:FormView ID="TotalFormView" runat="server" DataSourceID="TotalSQL" RenderOuterTable="false">
        <ItemTemplate>
            <div class="alert alert-secondary amount">
                Total: <%#Eval("TOTAL") %> Tk
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT SUM(ISNULL(TotalAmount, 0)) AS TOTAL FROM CommitteeMoneyReceipt WHERE (SchoolId = @SchoolId) AND (EducationYearId LIKE @EducationYearId) AND (CAST(PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND  (CommitteeMemberId LIKE @CommitteeMemberId)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberId" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="table-responsive mt-2">
        <asp:GridView ID="PaymentRecordGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeMoneyReceiptId" DataSourceID="PaymentRecordSQL">
            <Columns>
                <asp:BoundField DataField="CommitteeMoneyReceiptSn" HeaderText="Receipt" SortExpression="CommitteeMoneyReceiptSn" />
                <asp:BoundField DataField="MemberName" HeaderText="Member Name" SortExpression="MemberName" />
                <asp:BoundField DataField="CommitteeMemberType" HeaderText="Member Type" SortExpression="CommitteeMemberType" />
                <asp:BoundField DataField="SmsNumber" HeaderText="Sms Number" SortExpression="SmsNumber" />
                <asp:BoundField DataField="AccountName" HeaderText="Account" SortExpression="AccountName" />
                <asp:TemplateField HeaderText="Details">
                    <ItemTemplate>
                        <asp:Repeater ID="PRRepeater" runat="server" DataSourceID="PRSQL">
                            <ItemTemplate>
                                <p class="mb-0 text-right"><%# Eval("DonationCategory") +" "+Eval("Description",", {0}")+" :  "+Eval("PaidAmount") %>/-</p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:HiddenField ID="MRidHiddenField" runat="server" Value='<%# Bind("CommitteeMoneyReceiptId")%>' />
                        <asp:SqlDataSource ID="PRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT CommitteePaymentRecord.PaidAmount, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description, CommitteePaymentRecord.CommitteeMoneyReceiptId FROM  CommitteePaymentRecord INNER JOIN                            CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN                            CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteePaymentRecord.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="MRidHiddenField" Name="CommitteeMoneyReceiptId" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="TotalAmount" HeaderText="Total" SortExpression="TotalAmount" />
                <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:d MMM yyyy}" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:SqlDataSource ID="PaymentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMoneyReceipt.CommitteeMemberId, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn, CommitteeMoneyReceipt.TotalAmount, CommitteeMoneyReceipt.PaidDate, CommitteeMemberType.CommitteeMemberType, CommitteeMember.MemberName, CommitteeMember.SmsNumber, Account.AccountName FROM CommitteeMoneyReceipt INNER JOIN CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId INNER JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.EducationYearId LIKE @EducationYearId) AND (CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (CommitteeMoneyReceipt.CommitteeMemberId LIKE @CommitteeMemberId)"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberId" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $('.Datetime').datepicker({
            format: 'dd M yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            autoclose: true
        });
    </script>
</asp:Content>
