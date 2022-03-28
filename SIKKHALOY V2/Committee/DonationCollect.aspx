<%@ Page Title="Donation Collect" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationCollect.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationCollect" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Donation Collect</h3>

    <div class="d-flex align-items-center">
        <div>
            <asp:TextBox ID="FindDonarTextBox" autocomplete="off" runat="server" placeholder="Find Donar" CssClass="form-control"></asp:TextBox>
            <asp:HiddenField ID="HiddenCommitteeMemberId" runat="server" />
        </div>
        <div class="ml-4">
            <asp:Button ID="FindButton" OnClientClick="return checkDonar()" OnClick="FindButton_Click" runat="server" CssClass="btn btn-grey btn-md m-0" Text="Find" />
        </div>
    </div>

    <div class="my-4">
        <asp:GridView ID="DonationGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" DataSourceID="DonationSQL">
            <Columns>
                <asp:TemplateField HeaderText="Donation Category" SortExpression="Amount">
                    <ItemTemplate>
                        <%# Eval("DonationCategory") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Left" />
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description" SortExpression="Description">
                    <ItemTemplate>
                        <%# Eval("Description") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Donation Amount" SortExpression="Amount">
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
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="DonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT CommitteeDonation.CommitteeDonationId, CommitteeDonation.CommitteeDonationCategoryId, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.IsPaid, CommitteeDonation.Description, CommitteeDonation.InsertDate, CommitteeDonation.PromiseDate FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) AND (CommitteeDonation.CommitteeMemberId = @CommitteeMemberId)">

            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="HiddenCommitteeMemberId" DefaultValue="" Name="CommitteeMemberId" PropertyName="Value" />
            </SelectParameters>

        </asp:SqlDataSource>
    </div>


    <script type="text/javascript">
        $(function () {
            //date picker
            $(".date-picker").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });


            //find donar
            $('[id*=FindDonarTextBox]').typeahead({
                displayText: function (item) {
                    return `${item.MemberName}, ${item.SmsNumber}`;
                },
                afterSelect: function (item) {
                    this.$element[0].value = item.MemberName
                },
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/FindDonar.asmx/FindDonarAutocomplete",
                        data: JSON.stringify({ 'prefix': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) { result(JSON.parse(response.d)); },
                        error: function (err) { console.log(err) }
                    });
                },
                updater: function (item) {
                    const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");
                    committeeMemberId.value = item.CommitteeMemberId;

                    return item;
                }
            });
        });

        //check donar id exist
        function checkDonar() {
            const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

            if (committeeMemberId.value) return true;

            return false;
        }
    </script>
</asp:Content>
