<%@ Page Title="" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="MemberAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.MemberAdd" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Member</h3>

    <div class="row">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <div class="form-group">
                        <label>Name<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="MemberNameTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="AS"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="MemberNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="TypeDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                            <asp:ListItem Value="%">[ All Type ]</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Phone</label>
                        <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Address</label>
                        <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Image</label>
                        <asp:FileUpload ID="ImageFileUpload" runat="server" CssClass="form-control" />
                    </div>

                    <asp:Button ID="AddMemberButton" runat="server" CssClass="btn btn-primary" Text="Submit" ValidationGroup="AS" OnClick="AddMemberButton_Click" />
                    <asp:SqlDataSource ID="MemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO CommitteeMember(CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName, SmsNumber, Address, Photo) VALUES (@CommitteeMemberTypeId, @RegistrationID, @SchoolID, @MemberName, @SmsNumber, @Address, @Photo)"
                        SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMemberType.CommitteeMemberType, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMember.DueDonation, CommitteeMember.InsertDate FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeMember.SchoolID = @SchoolID)">
                        <InsertParameters>
                            <asp:ControlParameter ControlID="TypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:ControlParameter ControlID="MemberNameTextBox" Name="MemberName" PropertyName="Text" />
                            <asp:ControlParameter ControlID="PhoneTextBox" Name="SmsNumber" PropertyName="Text" />
                            <asp:ControlParameter ControlID="PhoneTextBox" Name="Address" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ImageFileUpload" Name="Photo" PropertyName="FileBytes" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive mt-5">
        <asp:GridView ID="MemberGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId" AllowPaging="True">
            <Columns>
                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <ItemTemplate>
                        <%#Eval("MemberName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" SortExpression="CommitteeMemberType">
                    <ItemTemplate>
                        <%#Eval("CommitteeMemberType") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <ItemTemplate>
                        <%#Eval("Address") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Phone" SortExpression="SmsNumber">
                    <ItemTemplate>
                        <%#Eval("SmsNumber") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="TotalDonation" SortExpression="TotalDonation">
                    <ItemTemplate>
                        <%#Eval("TotalDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="PaidDonation" SortExpression="PaidDonation">
                    <ItemTemplate>
                        <%#Eval("PaidDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="DueDonation" SortExpression="DueDonation">
                    <ItemTemplate>
                        <%#Eval("DueDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Edit">
                    <ItemTemplate>
                        <a target="_blank" href="Edit_Employee/Staff.aspx?Emp=<%# Eval("CommitteeMemberId") %>">Update Info</a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
    </div>
</asp:Content>
