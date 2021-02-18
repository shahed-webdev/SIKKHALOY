<%@ Page Title="Collect Payment" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Collection.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Payment_Collection" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Payment_Collection.css?v=11.1" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Collect Payment</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="SearchIDTextBox" autocomplete="off" runat="server" CssClass="form-control" placeholder="Enter ID"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="A" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SearchIDTextBox" CssClass="EroorStar" ErrorMessage="Enter Student ID" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
    </div>

    <div class="row">
        <div class="col-lg-8">
            <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="StudentID,StudentClassID,ClassID,EducationYearID,ID" DataSourceID="StudentInfoSQL" Width="100%">
                <ItemTemplate>
                    <div class="z-depth-1 p-3 mb-4">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="p-image">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle img-fluid z-depth-1" />
                                <div class="Student-Status"><%# Eval("Status") %></div>
                            </div>
                            <div class="info">
                                <ul>
                                    <li>
                                        <strong>(<span id="IDLabel"><%# Eval("ID") %></span>) <%# Eval("StudentsName") %></strong>
                                    </li>
                                    <li>
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                    </li>
                                    <li>Roll No:<%# Eval("RollNo") %><%#Eval("Section",", Section: {0}") %><%# Eval("Shift",", Shift: {0}") %></li>
                                    <li><b>Phone: </b><%# Eval("SMSPhoneNo") %></li>
                                    <li><b>Session: </b><%# Eval("EducationYear") %> <i class="fa fa-hand-o-right"></i><a target="_blank" href="/Admission/Student_Report/Report.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">Full Details</a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.StudentImageID, Student.ID, Student.StudentsName, Student.SMSPhoneNo, CreateClass.Class, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, StudentsClass.RollNo, Student.FathersName, Education_Year.EducationYearID, Education_Year.EducationYear, Student.Status FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="col-lg-4">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="mb-4">
                        <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid P-R" DataSourceID="PRecordSQL" AllowPaging="True" PageSize="3">
                            <Columns>
                                <asp:TemplateField HeaderText="Receipt">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="MSNLinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' Text='<%# Eval("MoneyReceipt_SN") %>' ToolTip="Click To Details" OnCommand="MSNLinkButton_Command" />
                                        <small class="d-block"><%# Eval("PaidDate", "{0:d-MMM-yy (hh:mm tt)}") %></small>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Paid">
                                    <ItemTemplate>
                                        <%# Eval("TotalAmount") %> Tk
                                     <small class="d-block">
                                         <asp:LinkButton ID="Print_LinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' ToolTip="Click To Print" OnCommand="Print_LinkButton_Command"><i class="fa fa-print"></i> Print</asp:LinkButton></small>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceiptID FROM Income_MoneyReceipt INNER JOIN Student ON Income_MoneyReceipt.StudentID = Student.StudentID WHERE (Income_MoneyReceipt.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)
ORDER BY Income_MoneyReceipt.PaidDate DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <!--Paid Record Modal -->
                    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                        <div class="modal-dialog modal-lg" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <div class="title">Paid Record Details</div>
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                                </div>
                                <div class="modal-body">
                                    <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidRecordsSQL" CssClass="mGrid" ShowFooter="True" RowStyle-CssClass="Rows">
                                        <Columns>
                                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
                                            <asp:TemplateField HeaderText="Role">
                                                <ItemTemplate>
                                                    <asp:Label ID="RoleLabel" runat="server" Text='<%# Eval("Role") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                                <ItemTemplate>
                                                    <asp:Label ID="PaidAmountLabel" runat="server" Text='<%# Eval("PaidAmount") %>'></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <label id="PGTLabel"></label>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <FooterStyle CssClass="GridFooter" />
                                        <RowStyle CssClass="Rows"></RowStyle>
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="PaidRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:Parameter Name="MoneyReceiptID" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>

                                    <asp:FormView ID="RByFormView" runat="server" CssClass="Hide" DataSourceID="ReceivedBySQL" Width="100%">
                                        <ItemTemplate>
                                            <div class="RecvBy">
                                                Received By: <%# Eval("Name") %> (© Sikkhaloy.com)
                                            </div>
                                        </ItemTemplate>
                                    </asp:FormView>
                                    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:Parameter Name="MoneyReceiptID" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <asp:FormView ID="PDue" runat="server" DataSourceID="TotalDue_ByID_ODS" Width="100%">
        <ItemTemplate>
            <div class="Total_p-due">
                CURRENT DUE: <%# Eval("Due") %> TK
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:ObjectDataSource ID="TotalDue_ByID_ODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.TotalDue_ByIDTableAdapter">
        <SelectParameters>
            <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="0" Name="ID" PropertyName="Text" Type="String" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <div class="table-responsive">
        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid paid-gv" DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="DueSQL" ShowFooter="True" OnRowDataBound="DueGridView_RowDataBound">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="DueCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <asp:Label ID="DueLabel" runat="server" Text='<%# Eval("Due") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pay" SortExpression="Due">
                    <ItemTemplate>
                        <asp:TextBox ID="DueAmountTextBox" CssClass="form-control" Enabled="false" runat="server" Text='<%# Eval("Due") %>' onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" />
                        <asp:RequiredFieldValidator ID="RFV" runat="server" ControlToValidate="DueAmountTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="PaY"></asp:RequiredFieldValidator>
                    </ItemTemplate>
                    <FooterTemplate>
                        <b class="GTotal">Total
                  <label id="GrandTotal">0</label>
                            Tk.</b>
                    </FooterTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT        Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
                         Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
                         Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
                         Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Student.ID = @ID)  AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID)
ORDER BY Income_PayOrder.EndDate">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>

        <%if (OtherSessionGridView.Rows.Count > 0)
            {%>
        <div class="alert alert-success mt-3">OTHERS SESSION DUE</div>
        <asp:GridView ID="OtherSessionGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid paid-gv" DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="OtherSessionSQL" ShowFooter="True" OnRowDataBound="OtherSessionGridView_RowDataBound">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="Other_Session_CheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <asp:Label ID="Other_Session_DueLabel" runat="server" Text='<%# Eval("Due") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pay" SortExpression="Due">
                    <ItemTemplate>
                        <asp:TextBox ID="Other_Session_AmountTextBox" CssClass="form-control" Enabled="false" runat="server" Text='<%# Eval("Due") %>' onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" />
                        <asp:RequiredFieldValidator ID="RFV" runat="server" ControlToValidate="Other_Session_AmountTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="PaY"></asp:RequiredFieldValidator>
                    </ItemTemplate>
                    <FooterTemplate>
                        <b class="GTotal_Others">Total
                  <label id="Others_GrandTotal">0</label>
                            Tk.</b>
                    </FooterTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="OtherSessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT        Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
                         Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
                         Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
                         Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Student.ID = @ID)  AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID &lt;&gt; @EducationYearID)
ORDER BY Income_PayOrder.EndDate">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>
        <%}%>
    </div>

    <div class="All_session_Total Pay_Show text-right">
        Grand Total: <span id="All_Session_Total">0</span> Tk
    </div>


    <label id="Error_Others" class="EroorSummer"></label>
    <label id="Error" class="EroorSummer"></label>
    <div class="form-inline">
        <div class="Pay_Show">
            <div class="form-group">
                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                </asp:DropDownList>
                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [AccountID], [AccountName] FROM [Account] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
        <div class="form-group">
            <button type="button" id="Add_P" data-toggle="modal" data-target="#Others_Modal" class="btn btn-success">Add More Payment</button>
        </div>
        <div class="Pay_Show">
            <div class="form-group">
                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="AccountDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="PaY"></asp:RequiredFieldValidator>
                <asp:Button ID="PayButton" runat="server" Text="Pay" OnClick="PayButton_Click" OnClientClick="return RFV();" ValidationGroup="PaY" CssClass="btn btn-primary" />
            </div>
        </div>
    </div>



    <!--Add More Payment Modal-->
    <div class="modal fade" id="Others_Modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add More Payment</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>
                            Role<a href="Create_Payment_Roles.aspx">Add New Role</a>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="PayRoleDropDownList" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP" InitialValue="0"></asp:RequiredFieldValidator>
                        </label>
                        <asp:DropDownList ID="PayRoleDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="PayRoleSQL" DataTextField="Role" DataValueField="RoleID">
                            <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="PayRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [RoleID], [Role] FROM [Income_Roles] WHERE ([SchoolID] = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <label>Pay For<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="OPayforTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="OPayforTextBox" runat="server" CssClass="form-control" placeholder="Input Pay For"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Amount<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="OAmountTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="OAmountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Input amount"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Concession</label>
                        <asp:TextBox ID="OConcessiontBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Input Concession"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="OthersPaymentButton" runat="server" CssClass="btn btn-primary" Text="Add Payment" ValidationGroup="OP" OnClick="OthersPaymentButton_Click" />
                        <asp:SqlDataSource ID="OthersPaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, Amount, Discount, LateFee, RoleID, PayFor, StartDate, EndDate, CreatedDate, EducationYearID) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @Amount, @Discount, @LateFee, @RoleID, @PayFor, GETDATE(), GETDATE(), GETDATE(), @EducationYearID)" SelectCommand="SELECT * FROM [Income_PayOrder]">
                            <InsertParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                <asp:Parameter Name="EducationYearID" Type="Int32" />
                                <asp:Parameter Name="StudentID" Type="Int32" />
                                <asp:Parameter Name="ClassID" Type="Int32" />
                                <asp:Parameter Name="StudentClassID" Type="Int32" />
                                <asp:Parameter Name="LateFee" />
                                <asp:ControlParameter ControlID="OAmountTextBox" Name="Amount" PropertyName="Text" Type="Double" />
                                <asp:ControlParameter ControlID="OConcessiontBox" Name="Discount" PropertyName="Text" Type="Double" />
                                <asp:ControlParameter ControlID="PayRoleDropDownList" Name="RoleID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:ControlParameter ControlID="OPayforTextBox" Name="PayFor" PropertyName="Text" Type="String" />
                            </InsertParameters>
                        </asp:SqlDataSource>
                        <label id="Msg" style="color: #339933"></label>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        $(function () {
            if ($(".Student-Status").text() == "Active") {
                $(".Student-Status").addClass("Status-on");
            } else {
                $(".Student-Status").addClass("Status-off").removeClass("Status-on");
            }

            $('[id*=SearchIDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            //press enter on ID_textbox
            $("[id*=SearchIDTextBox]").on("keypress", function (e) {
                var key = e.which;
                if (key == 13)  // the enter key code
                {
                    $("[id*=SearchButton]").click();
                    return false;
                }
            });


            //Reset checkbox after Search clicking
            $("[id*=SearchButton]").click(function () {
                $(':checkbox').each(function () {
                    this.checked = false;
                    var td = $("td", $(this).closest("tr"));
                    parseFloat($("[id*=DueAmountTextBox]", td).val($("[id*=DueLabel]", td).text()));
                });
            });

            //Student Info FormView
            if ($('#IDLabel').text() != "") {
                $("#Add_P").show();
            } else {
                $("#Add_P").hide();
            }

            //All GridView is empty
            if ($('[id*=DueGridView] tr').length || $('[id*=OtherSessionGridView] tr').length) {
                $(".Pay_Show").show();
            }

            //others session GridView is empty
            if ($('[id*=OtherSessionGridView] tr').length && $('[id*=DueGridView] tr').length) {
                $(".All_session_Total").show();
            }
            else {
                $(".All_session_Total").hide();
            }


            //Current Session
            $("[id*=DueCheckBox]").prop('checked', false);

            var total = 0;
            var GV_Total1 = 0;
            var GV_Total2 = 0;

            $("[id*=DueCheckBox]").on("click", function () {
                if ($(this).is(":checked")) {
                    $("td", $(this).closest("tr")).addClass("selected"); //Selected Color
                    var td = $("td", $(this).closest("tr"));
                    $("[id*=DueAmountTextBox]", td).removeAttr("disabled");

                    total += parseFloat($("[id*=DueAmountTextBox]", td).val());
                    $("#GrandTotal").text(total);

                    //Final Total
                    GV_Total1 = parseFloat($("#GrandTotal").text());
                    GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                    $("#All_Session_Total").text(GV_Total1 + GV_Total2);

                }
                else {
                    $("td", $(this).closest("tr")).removeClass("selected"); //Selected Color

                    var td = $("td", $(this).closest("tr"));
                    $("[id*=DueAmountTextBox]", td).attr("disabled", "disabled");

                    if (isNaN(parseFloat($("[id*=DueAmountTextBox]", td).val()))) {
                        parseFloat($("[id*=DueAmountTextBox]", td).val(0));
                    }

                    total -= parseFloat($("[id*=DueAmountTextBox]", td).val());
                    $("#GrandTotal").text(total);

                    //Final Total
                    GV_Total1 = parseFloat($("#GrandTotal").text());
                    GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                    $("#All_Session_Total").text(GV_Total1 + GV_Total2);

                    parseFloat($("[id*=DueAmountTextBox]", td).val($("[id*=DueLabel]", td).text()));

                    $("#Error").text("");
                    $("[id*=DueAmountTextBox]", td).removeClass("ErrorTextbox");
                    $(".GTotal").css("color", "#5d6772");
                    $("[id*=DueLabel]", td).css("color", "#333");
                }
            });

            var KeyDown = true;
            var KeyUP = true;
            $("[id*=DueAmountTextBox]").on("keypress", function () {
                if (!KeyDown) return;
                var Focustotal = 0;
                if (!isNaN(parseFloat($(this).val()))) {
                    Focustotal = parseFloat($(this).val());
                }

                var gTotal = (total - Focustotal);
                $("#GrandTotal").text(gTotal);
                total = gTotal;

                KeyUP = true;
                KeyDown = false;

                //Final Total
                GV_Total1 = parseFloat($("#GrandTotal").text());
                GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                $("#All_Session_Total").text(GV_Total1 + GV_Total2);
            });

            $("[id*=DueAmountTextBox]").on("keyup", function () {
                if (!KeyUP) return;

                //rex
                var reg = /^([1-9]\d*)$/;
                if (!reg.test($(this).val())) {
                    $(this).val("");
                    return;
                }


                var Change = 0;
                if (!isNaN(parseFloat($(this).val()))) {
                    Change = parseFloat($(this).val());
                }

                var gTotal = (total + Change);
                $("#GrandTotal").text(gTotal);
                total = gTotal;

                KeyDown = true;
                KeyUP = false;

                var td = $("td", $(this).closest("tr"));
                var DueAmount = parseFloat($("[id*=DueLabel]", td).text());
                var PaidAmount = parseFloat($("[id*=DueAmountTextBox]", td).val());

                if (PaidAmount > DueAmount) {
                    $("#Error").text("Paid Amount Greater than due amount");
                    $("[id*=DueAmountTextBox]", td).addClass("ErrorTextbox");
                    $("[id*=PayButton]").prop("disabled", !0).removeClass("btn btn-primary");
                    $(".GTotal").css("color", "red");
                    $("[id*=DueLabel]", td).css("color", "red");
                }
                else {
                    $("#Error").text("");
                    $("[id*=DueAmountTextBox]", td).removeClass("ErrorTextbox");
                    $("[id*=PayButton]").prop("disabled", !1).addClass("btn btn-primary");
                    $(".GTotal").css("color", "#5d6772");
                    $("[id*=DueLabel]", td).css("color", "#333");
                }

                //Final Total
                GV_Total1 = parseFloat($("#GrandTotal").text());
                GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                $("#All_Session_Total").text(GV_Total1 + GV_Total2);
            });

            //Others Session
            $("[id*=Other_Session_CheckBox]").prop('checked', false);

            var Others_total = 0;
            var GV_Total1 = 0;
            var GV_Total2 = 0;

            $("[id*=Other_Session_CheckBox]").on("click", function () {
                if ($(this).is(":checked")) {
                    $("td", $(this).closest("tr")).addClass("selected"); //Selected Color
                    var td = $("td", $(this).closest("tr"));
                    $("[id*=Other_Session_AmountTextBox]", td).removeAttr("disabled");

                    Others_total += parseFloat($("[id*=Other_Session_AmountTextBox]", td).val());
                    $("#Others_GrandTotal").text(Others_total);

                    //Final Total
                    GV_Total1 = parseFloat($("#GrandTotal").text());
                    GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                    $("#All_Session_Total").text(GV_Total1 + GV_Total2);
                }
                else {
                    $("td", $(this).closest("tr")).removeClass("selected"); //Selected Color

                    var td = $("td", $(this).closest("tr"));
                    $("[id*=Other_Session_AmountTextBox]", td).attr("disabled", "disabled");

                    if (isNaN(parseFloat($("[id*=Other_Session_AmountTextBox]", td).val()))) {
                        parseFloat($("[id*=Other_Session_AmountTextBox]", td).val(0));
                    }

                    Others_total -= parseFloat($("[id*=Other_Session_AmountTextBox]", td).val());
                    $("#Others_GrandTotal").text(Others_total)

                    parseFloat($("[id*=Other_Session_AmountTextBox]", td).val($("[id*=DueLabel]", td).text()));

                    $("#Error_Others").text("");
                    $("[id*=Other_Session_AmountTextBox]", td).removeClass("ErrorTextbox");
                    $(".GTotal_Others").css("color", "#5d6772");
                    $("[id*=DueLabel]", td).css("color", "#333");

                    //Final Total
                    GV_Total1 = parseFloat($("#GrandTotal").text());
                    GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                    $("#All_Session_Total").text(GV_Total1 + GV_Total2);
                }
            });

            var KeyDown = true;
            var KeyUP = true;
            $("[id*=Other_Session_AmountTextBox]").on("keypress", function () {
                if (!KeyDown) return;
                var Focustotal = 0;
                if (!isNaN(parseFloat($(this).val()))) {
                    Focustotal = parseFloat($(this).val());
                }

                var gTotal = (Others_total - Focustotal);
                $("#Others_GrandTotal").text(gTotal);
                Others_total = gTotal;

                KeyUP = true;
                KeyDown = false;

                //Final Total
                GV_Total1 = parseFloat($("#GrandTotal").text());
                GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                $("#All_Session_Total").text(GV_Total1 + GV_Total2);
            });

            $("[id*=Other_Session_AmountTextBox]").on("keyup", function () {
                if (!KeyUP) return;

                //rex
                var reg = /^([1-9]\d*)$/;
                if (!reg.test($(this).val())) {
                    $(this).val("");
                    return;
                }

                var Change = 0;
                if (!isNaN(parseFloat($(this).val()))) {
                    Change = parseFloat($(this).val());
                }

                var gTotal = (Others_total + Change);
                $("#Others_GrandTotal").text(gTotal);
                Others_total = gTotal;

                KeyDown = true;
                KeyUP = false;

                var td = $("td", $(this).closest("tr"));
                var DueAmount = parseFloat($("[id*=Other_Session_DueLabel]", td).text());
                var PaidAmount = parseFloat($("[id*=Other_Session_AmountTextBox]", td).val());

                if (PaidAmount > DueAmount) {
                    $("#Error_Others").text("Paid Amount Greater than due amount");
                    $("[id*=Other_Session_AmountTextBox]", td).addClass("ErrorTextbox");
                    $("[id*=PayButton]").prop("disabled", !0).removeClass("btn btn-primary");
                    $(".GTotal_Others").css("color", "red");
                    $("[id*=Other_Session_DueLabel]", td).css("color", "red");
                }
                else {
                    $("#Error_Others").text("");
                    $("[id*=Other_Session_AmountTextBox]", td).removeClass("ErrorTextbox");
                    $("[id*=PayButton]").prop("disabled", !1).addClass("btn btn-primary");
                    $(".GTotal_Others").css("color", "#5d6772");
                    $("[id*=Other_Session_DueLabel]", td).css("color", "#333");
                }

                //Final Total
                GV_Total1 = parseFloat($("#GrandTotal").text());
                GV_Total2 = parseFloat($("#Others_GrandTotal").text());
                $("#All_Session_Total").text(GV_Total1 + GV_Total2);
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            //Paid Grand Total
            var PaidTotal = 0;
            $("[id*=PaidAmountLabel]").each(function () { PaidTotal = PaidTotal + parseFloat($(this).text()) });
            $("#PGTLabel").text(PaidTotal + " Tk");
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        function DisableButton() { document.getElementById("<%=PayButton.ClientID %>").disabled = true; }

        window.onbeforeunload = DisableButton;

        function openModal() {
            $('#myModal').modal('show');
        }

        function Success() {
            var e = $('#Msg');
            e.text("Payment Added Successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue(); }, 3000); });
            e.fadeOut('slow');
        }

        //Working Reuired validator
        function RFV() {
            if (Page_ClientValidate('PaY')) {
                return true;
            }
            return false;
        }
    </script>
</asp:Content>
