<%@ Page Title="Add Member" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="MemberAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.MemberAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .custom-form-row { display: flex; -ms-flex-wrap: wrap; flex-wrap: wrap; gap: 20px }
        .photo{width:50px;border-radius: 5px;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Member</h3>

    <div class="custom-form-row">
        <div class="form-group">
            <label>Name</label>
            <asp:TextBox ID="MemberNameTextBox" runat="server" CssClass="form-control" required=""></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Member Type</label>
            <asp:DropDownList ID="TypeDropDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                <asp:ListItem Value="">[ All Type ]</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Phone</label>
            <asp:TextBox ID="PhoneTextBox" required="" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Address</label>
            <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Photo</label>
            <asp:FileUpload ID="ImageFileUpload" runat="server" CssClass="form-control" />
        </div>
        <div class="form-group" style="padding-top: 1.5rem">
            <asp:Button ID="AddMemberButton" runat="server" CssClass="btn btn-primary btn-md" Text="Submit" ValidationGroup="AS" OnClick="AddMemberButton_Click" />
            <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <div class="table-responsive mt-3">
        <asp:GridView ID="MemberGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId" AllowPaging="True">
            <Columns>
                <asp:TemplateField HeaderText="Photo">
                    <ItemTemplate>
                        <img src="data:image/jpg;base64, <%# Convert.ToBase64String((byte[])Eval("Photo")) %>" onerror="this.src='/Handeler/Default/Male.png'" class="photo" alt="<%#Eval("MemberName") %>" />
                    </ItemTemplate>                  
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <EditItemTemplate>
                        <asp:TextBox ID="MemberNameTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("MemberName") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("MemberName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" SortExpression="CommitteeMemberType">
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditTypeDropDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId" SelectedValue='<%# Bind("CommitteeMemberTypeId") %>'>
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("CommitteeMemberType") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <EditItemTemplate>
                        <asp:TextBox ID="AddressTB" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("Address") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Phone" SortExpression="SmsNumber">
                    <EditItemTemplate>
                        <asp:TextBox ID="SmsNumberTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("SmsNumber") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("SmsNumber") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Total Donation" SortExpression="TotalDonation">
                    <ItemTemplate>
                        <%#Eval("TotalDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid Donation" SortExpression="PaidDonation">
                    <ItemTemplate>
                        <%#Eval("PaidDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due Donation" SortExpression="DueDonation">
                    <ItemTemplate>
                        <%#Eval("DueDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Update">
                    <EditItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                        &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="MemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
            InsertCommand="INSERT INTO CommitteeMember(CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName, SmsNumber, Address, Photo) VALUES (@CommitteeMemberTypeId, @RegistrationID, @SchoolID, @MemberName, @SmsNumber, @Address, @Photo)"
            SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMemberType.CommitteeMemberType,CommitteeMemberType.CommitteeMemberTypeId, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMember.DueDonation, CommitteeMember.InsertDate FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeMember.SchoolID = @SchoolID)"
            UpdateCommand="UPDATE CommitteeMember SET CommitteeMemberTypeId = @CommitteeMemberTypeId, MemberName = @MemberName, SmsNumber = @SmsNumber, Address = @Address WHERE (CommitteeMemberId = @CommitteeMemberId)">
            <InsertParameters>
                <asp:ControlParameter ControlID="TypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="MemberNameTextBox" Name="MemberName" PropertyName="Text" />
                <asp:ControlParameter ControlID="PhoneTextBox" Name="SmsNumber" PropertyName="Text" />
                <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" />
                <asp:ControlParameter ControlID="ImageFileUpload" Name="Photo" PropertyName="FileBytes" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
</asp:Content>
