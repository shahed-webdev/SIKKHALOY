<%@ Page Title="Add Donation" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #donar-info { margin: 0.5rem 0; display: flex; align-items: center; }
        #donar-info span { border: 1px solid #a5f3b7; margin-right: 8px; padding: 3px 11px; border-radius: 5px; background-color: #e0ffe6; color: #22c147; font-weight: 500; }
        #donar-info span:last-child { cursor: pointer; border: none; border-radius: 5px; background-color: #fff; color: #ff3547; padding: 0 }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Donation</h3>

    <div class="row">
        <div class="col-md-3 col-lg-2">
            <div class="form-btn-group">
                <label>Find Donar</label>
                <asp:TextBox ID="FindDonarTextBox" autocomplete="off" runat="server" CssClass="form-control" required=""></asp:TextBox>
                <asp:HiddenField ID="HiddenCommitteeMemberId" runat="server" />
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
                <asp:TextBox ID="DonationAmountTextBox" onchange="setMaxPaidAmount()" min="0.01" step="0.01" type="number" runat="server" CssClass="form-control" required=""></asp:TextBox>
            </div>
        </div>
        <div class="col-md-3 col-lg-4">
            <div class="form-btn-group">
                <label>Descriptions</label>
                <asp:TextBox ID="DescriptionsTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
    </div>

    <div id="donar-info"></div>

    <div class="row my-4">
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Promised Date</label>
                <asp:TextBox ID="PromisedDateTextBox" autocomplete="off" runat="server" CssClass="form-control date-picker"></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Paid Amount</label>
                <asp:TextBox ID="PaidAmountTextBox" autocomplete="off" min="1" step="0.01" oninput="onChangePaidAmount(this)" type="number" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Paid Date</label>
                <asp:TextBox ID="PaidDateTextBox" autocomplete="off" runat="server" CssClass="form-control date-picker" disabled=""></asp:TextBox>
            </div>
        </div>
        <div class="col-sm-3">
            <div class="form-btn-group">
                <label>Account</label>

                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                </asp:DropDownList>

                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID,AccountName,Default_Status FROM Account WHERE (SchoolID = @SchoolID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>

    <asp:Button ID="SubmitButton" OnClientClick="return isValidForm()" OnClick="SubmitButton_Click" runat="server" CssClass="btn btn-primary m-0" Text="Submit" />

    <asp:SqlDataSource ID="AddDonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeDonation.CommitteeDonationId, CommitteeDonation.CommitteeDonationCategoryId, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.IsPaid, CommitteeDonation.Description, CommitteeDonation.InsertDate, CommitteeDonation.PromiseDate FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.CommitteeMemberId LIKE @CommitteeMemberId) AND (CommitteeDonation.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId)"
        InsertCommand="INSERT INTO CommitteeDonation(SchoolID, RegistrationID, CommitteeMemberId, CommitteeDonationCategoryId, Amount, Description, PromiseDate) 
                       VALUES(@SchoolID,@RegistrationID,@CommitteeMemberId,@CommitteeDonationCategoryId,@Amount,@Description,@PromiseDate); 
                       SELECT @CommitteeDonationId = SCOPE_IDENTITY();"
        OnInserted="AddDonationSQL_Inserted" DeleteCommand="DELETE FROM CommitteeDonation WHERE (CommitteeDonationId = @CommitteeDonationId) AND (PaidAmount = 0)" UpdateCommand="UPDATE CommitteeDonation SET CommitteeDonationCategoryId = @CommitteeDonationCategoryId, Amount = CASE WHEN PaidAmount &gt; @Amount THEN Amount ELSE @Amount END, Description = @Description WHERE (CommitteeDonationId = @CommitteeDonationId)">

        <DeleteParameters>
            <asp:Parameter Name="CommitteeDonationId" />
        </DeleteParameters>

        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
            <asp:ControlParameter ControlID="CategoryDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="DonationAmountTextBox" Name="Amount" PropertyName="Text" />
            <asp:ControlParameter ControlID="DescriptionsTextBox" Name="Description" PropertyName="Text" />
            <asp:ControlParameter ControlID="PromisedDateTextBox" Name="PromiseDate" PropertyName="Text" />
            <asp:Parameter Name="CommitteeDonationId" />
        </InsertParameters>

        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:Parameter Name="CommitteeMemberId" DefaultValue="%" />
            <asp:Parameter DefaultValue="%" Name="CommitteeDonationCategoryId" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="CommitteeDonationCategoryId" />
            <asp:Parameter Name="Amount" />
            <asp:Parameter Name="Description" />
            <asp:Parameter Name="CommitteeDonationId" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="ReceiptSQL" runat="server"
        InsertCommand="INSERT INTO CommitteeMoneyReceipt (RegistrationId, SchoolId, CommitteeMemberId, EducationYearId, AccountId, CommitteeMoneyReceiptSn, PaidDate) 
                       VALUES (@RegistrationID, @SchoolID, @CommitteeMemberId, @EducationYearId, @AccountId, [dbo].[F_CommitteeMoneyReceiptSn](@SchoolID), @PaidDate);
                       SELECT @CommitteeMoneyReceiptId = SCOPE_IDENTITY();"
        OnInserted="ReceiptSQL_Inserted" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMoneyReceiptId FROM CommitteeMoneyReceipt">
        <InsertParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="PaidDateTextBox" Name="PaidDate" PropertyName="Text" />
            <asp:Parameter Direction="Output" Name="CommitteeMoneyReceiptId" Size="50" />
        </InsertParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="PaymentRecordSQL" runat="server"
        InsertCommand="INSERT INTO CommitteePaymentRecord (SchoolId, RegistrationId, CommitteeDonationId, CommitteeMoneyReceiptId, PaidAmount) VALUES (@SchoolID, @RegistrationID, @CommitteeDonationId, @CommitteeMoneyReceiptId, @PaidAmount)" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteePaymentRecordId FROM CommitteePaymentRecord">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter Name="CommitteeDonationId" />
            <asp:Parameter Name="CommitteeMoneyReceiptId" />
            <asp:ControlParameter ControlID="PaidAmountTextBox" Name="PaidAmount" PropertyName="Text" />
        </InsertParameters>
    </asp:SqlDataSource>


    <div class="table-embed-responsive mt-4">
        <asp:GridView ID="DonationGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" DataSourceID="AddDonationSQL">
            <Columns>
                <asp:TemplateField HeaderText="Donation Amount" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditCategoryDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId" SelectedValue='<%# Bind("CommitteeDonationCategoryId") %>'>
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        Category
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
                <asp:TemplateField HeaderText="PaidAmount" SortExpression="PaidAmount">
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
                    localStorage.setItem("_committee_", JSON.stringify(item));
                    location.reload(true);

                    return item;
                }
            });
        });


        //get donar info from local store
        function getDonarInfo() {
            return JSON.parse(localStorage.getItem("_committee_")) || null;
        }


        //show Donar Info
        const infoHolder = document.getElementById("donar-info");

        function showDonarInfo() {
            const info = getDonarInfo();

            const findDonarTextBox = document.getElementById("<%= FindDonarTextBox.ClientID %>");
            const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

            if (info) {
                const { MemberName, SmsNumber, CommitteeMemberId } = info;

                findDonarTextBox.value = MemberName;
                committeeMemberId.value = CommitteeMemberId;
                infoHolder.innerHTML = `<span>${MemberName}</span><span>${SmsNumber}</span></span><span><i id="clearDonar" class="fa fa-trash"></i></span>`;
            } else {
                infoHolder.innerHTML = "";
                findDonarTextBox.value = "";
                committeeMemberId.value = "";
            }
        }

        //clear Donar
        function clearDonarInfo() {
            localStorage.removeItem("_committee_");
            showDonarInfo();
        }

        //click event
        document.addEventListener('click', function (e) {
            if (e.target && e.target.id == 'clearDonar') {
                clearDonarInfo();
            }
        });


        //call on page 
        showDonarInfo();


        //set max paid amount
        function setMaxPaidAmount() {
            const donationAmount = document.getElementById("<%=DonationAmountTextBox.ClientID%>");
            const paidAmount = document.getElementById("<%=PaidAmountTextBox.ClientID%>");

            paidAmount.max = donationAmount.value;
        }

        //on change paid amount
        function onChangePaidAmount(self) {
            const paidDateTextBox = document.getElementById("<%=PaidDateTextBox.ClientID%>");

            paidDateTextBox.required = !!self.value;
            paidDateTextBox.disabled = !self.value;

            if (!self.value)
                paidDateTextBox.value = "";

        }

        //check data is valid
        function isValidForm() {
            const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

            if (committeeMemberId.value) return true;

            infoHolder.innerHTML = `<strong class="red-text">find donar</strong>`;
            return false;
        }
    </script>
</asp:Content>
