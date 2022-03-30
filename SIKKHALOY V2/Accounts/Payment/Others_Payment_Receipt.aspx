<%@ Page Title="Others Payment Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Others_Payment_Receipt.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Others_Payment_Receipt" %>
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
    <h6 class="font-weight-bold text-center">Payment Receipt</h6>
    <asp:GridView ID="OthersPaymentGridView" runat="server" AutoGenerateColumns="False" DataSourceID="OthersPaymentSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="Extra_IncomeID" HeaderText="Receipt No." SortExpression="Extra_IncomeID" />
            <asp:BoundField DataField="Extra_Income_CategoryName" HeaderText="Category" SortExpression="Extra_Income_CategoryName" />
            <asp:BoundField DataField="Extra_IncomeFor" HeaderText="Description" SortExpression="Extra_IncomeFor" />
            <asp:BoundField DataField="Extra_IncomeDate" HeaderText="Date" SortExpression="Extra_IncomeDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:TemplateField HeaderText="Amount" SortExpression="Extra_IncomeAmount">
                <ItemTemplate>
                    <label class="amount"><%# Eval("Extra_IncomeAmount") %></label>
                </ItemTemplate>
                <FooterTemplate>
                    <strong id="total"></strong>
                </FooterTemplate>
            </asp:TemplateField>

        </Columns>
        <FooterStyle CssClass="grid-footer" />
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="OthersPaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Extra_Income.Extra_IncomeID, Extra_IncomeCategory.Extra_Income_CategoryName, Extra_Income.Extra_IncomeDate, Extra_Income.Extra_IncomeFor, Extra_Income.Extra_IncomeAmount FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID WHERE (Extra_Income.Extra_IncomeID = @Extra_IncomeID) AND (Extra_Income.SchoolID = @SchoolID) AND (Extra_Income.EducationYearID = @EducationYearID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="Extra_IncomeID" QueryStringField="id" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>


    <div class="d-print-none mt-4">
        <input type="button" value="Print" onclick="window.print();" class="btn btn-outline-primary" />
    </div>


    <script>
        $(function () {
            bindPrintOption();

            //show total in gridview footer
            let total = 0;
            $(".amount").each(function () {
                total += parseFloat($(this).text())
            });

            $("#total").text(`Total: ${total}`);
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
