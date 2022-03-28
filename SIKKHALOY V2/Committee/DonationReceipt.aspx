<%@ Page Title="Donation Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationReceipt.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationReceipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .info { text-align: center; color: #000; font-weight: 400; margin-bottom: 6px; }
        .mGrid td { padding: 3px; border: 1px solid #a0a0a0; }
        .grid-footer td { font-weight: bold; }

        @page { margin: 0 13.3rem !important; }

        @media print {
            .logo-waper { display: none; }
            #header { margin-bottom: 10px; border-bottom: none !important; }

            /*for black and white page*/
            .bg-main { background-color: #fff; color: #000; box-shadow: none !important }
            #InstitutionName { font-weight: bold; color: #000 !important; }
        }
    </style>
    <!--add dynamic css for printing-->
    <style type="text/css" media="print" id="print-content"></style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a class="d-print-none" href="DonationAdd.aspx"><< Back To</a>

    <asp:FormView ID="InfoFormView" runat="server" DataSourceID="InfoSQL" RenderOuterTable="False" DataKeyNames="CommitteeMoneyReceiptId">
        <ItemTemplate>
            <div class="info">
                <p class="mb-0">Md Satter</p>
                <p class="mb-0">01680525412, uttara, Dhaka 1362</p>
                <p class="mb-2">Payment Method: CASH</p>

                <div class="d-flex justify-content-between align-items-center">
                    <span>Receipt: <%# Eval("CommitteeMoneyReceiptSn") %></span>
                    <span>Date: <%# Eval("PaidDate","{0:d MMM yyyy}") %></span>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="InfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMoneyReceipt.CommitteeMemberId, CommitteeMoneyReceipt.AccountId, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn, CommitteeMoneyReceipt.TotalAmount, CommitteeMoneyReceipt.PaidDate, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMemberType.CommitteeMemberType, Account.AccountName, Education_Year.EducationYear FROM CommitteeMoneyReceipt INNER JOIN CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId INNER JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID INNER JOIN Education_Year ON CommitteeMoneyReceipt.EducationYearId = Education_Year.EducationYearID WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
            <asp:QueryStringParameter Name="CommitteeMoneyReceiptId" QueryStringField="id" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:GridView ID="PaymentGridView" DataKeyNames="CommitteePaymentRecordId" runat="server" AutoGenerateColumns="False" DataSourceID="PaymentSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="CommitteeDonationId" HeaderText="Category" SortExpression="CommitteeDonationId" />
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid Amount" SortExpression="PaidAmount" />
            <asp:BoundField DataField="CommitteeMoneyReceiptId" HeaderText="Description" SortExpression="CommitteeMoneyReceiptId" />

        </Columns>
        <FooterStyle CssClass="grid-footer" />
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="PaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteePaymentRecord.CommitteePaymentRecordId, CommitteePaymentRecord.SchoolId, CommitteePaymentRecord.RegistrationId, CommitteePaymentRecord.CommitteeDonationId, CommitteePaymentRecord.CommitteeMoneyReceiptId, CommitteePaymentRecord.PaidAmount, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description FROM CommitteePaymentRecord INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteePaymentRecord.SchoolId = @SchoolId) AND (CommitteePaymentRecord.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
            <asp:QueryStringParameter Name="CommitteeMoneyReceiptId" QueryStringField="id" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>


    <div class="d-print-none d-flex justify-content-between align-items-center mt-4">
        <div>
            <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMS_OtherInfoSQL" RenderOuterTable="false">
                <ItemTemplate>
                    <div>Remaining SMS: <%# Eval("SMS_Balance") %></div>
                </ItemTemplate>
            </asp:FormView>

            <asp:Button ID="SMSButton" runat="server" Text="Send SMS" CssClass="btn btn-primary" OnClick="SMSButton_Click" />
            <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>

            <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)"
                InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <InsertParameters>
                    <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                    <asp:Parameter Name="SchoolID" />
                    <asp:Parameter Name="StudentID" />
                    <asp:Parameter Name="TeacherID" />
                    <asp:Parameter Name="EducationYearID" />
                </InsertParameters>
            </asp:SqlDataSource>
        </div>
        <input type="button" value="Print" onclick="window.print();" class="btn btn-outline-primary" />
    </div>


    <script>
        $(function () {
            bindPrintOption();
        });


        //print settings
        let printingOptions = {
            isInstitutionName: false,
            topSpace: 0,
            fontSize: 11
        };

        const stores = {
            get: function () {
                const data = localStorage.getItem("receipt-printing");

                if (data) {
                    printingOptions = JSON.parse(data);
                }
            }
        }


        //bind selected options
        function bindPrintOption() {
            const printContent = document.getElementById("print-content");
            stores.get();

            printContent.textContent = `
                 #InstitutionName { font-size: ${printingOptions.fontSize + 4}px}
                 .info {font-size: ${printingOptions.fontSize + 1}px}
                 #header { padding-top: ${printingOptions.topSpace}px}
                .InsInfo p { font-size: ${printingOptions.fontSize + 1}px}
                .mGrid th { font-size: ${printingOptions.fontSize}px}
                .mGrid td { font-size: ${printingOptions.fontSize}px}`;
        }
    </script>
</asp:Content>
