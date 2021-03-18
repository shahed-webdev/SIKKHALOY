<%@ Page Title="Institution Register" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="InstitutionRegister.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Device.InstitutionRegister" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Institution Register</h3>
    <div id="divError" class="alert alert-danger collapse">
        <a id="linkClose" href="#" class="close">&times;</a>
        <div id="divErrorText"></div>
    </div>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="School_DropDownList" CssClass="form-control SearchDDL" runat="server" AppendDataBoundItems="True" DataSourceID="InstitutionSQL" DataTextField="SchoolName" DataValueField="UserName">
                <asp:ListItem Value="0">[ INSTITUTION ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolName + ' (' + UserName + ')' AS SchoolName, CAST(SchoolID AS varchar(10)) + ' - ' + SchoolName AS SchoolName_ID, UserName FROM SchoolInfo WHERE (Validation = N'Valid') AND (SchoolID NOT IN (SELECT SchoolID FROM Attendance_Device_Setting)) ORDER BY SchoolName"></asp:SqlDataSource>
            <asp:RequiredFieldValidator ControlToValidate="School_DropDownList" InitialValue="0" ValidationGroup="C" ID="RequiredFieldValidator2" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Password_TextBox" placeholder="Password" CssClass="form-control" runat="server" TextMode="Password"></asp:TextBox>
            <asp:RequiredFieldValidator ControlToValidate="Password_TextBox" ValidationGroup="C" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="Register_Button" OnClientClick="return CrateUser();" ValidationGroup="C" runat="server" Text="Register" CssClass="btn btn-primary" OnClick="Register_Button_Click" />
            <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
        </div>
        <asp:SqlDataSource ID="UserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Attendance_Device_Setting] WHERE [AttendanceUserID] = @AttendanceUserID" InsertCommand="INSERT INTO Attendance_Device_Setting(SchoolID, UserName, Password,SettingKey) SELECT SchoolID, UserName, @Password,@SettingKey FROM SchoolInfo WHERE (UserName = @UserName)" SelectCommand="SELECT SchoolInfo.SchoolName, Attendance_Device_Setting.UserName, Attendance_Device_Setting.Password, Attendance_Device_Setting.IsActive FROM Attendance_Device_Setting INNER JOIN SchoolInfo ON Attendance_Device_Setting.SchoolID = SchoolInfo.SchoolID" UpdateCommand="UPDATE [Attendance_Device_Setting] SET [SchoolID] = @SchoolID, [UserName] = @UserName, [Password] = @Password, [IsActive] = @IsActive, [InsertDate] = @InsertDate WHERE [AttendanceUserID] = @AttendanceUserID">
            <DeleteParameters>
                <asp:Parameter Name="AttendanceUserID" Type="Int32" />
            </DeleteParameters>
            <InsertParameters>
                <asp:ControlParameter ControlID="Password_TextBox" Name="Password" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="School_DropDownList" Name="UserName" PropertyName="SelectedValue" Type="String" />
                <asp:Parameter DefaultValue="123456" Name="SettingKey" />
            </InsertParameters>
            <UpdateParameters>
                <asp:Parameter Name="SchoolID" Type="Int32" />
                <asp:Parameter Name="UserName" Type="String" />
                <asp:Parameter Name="Password" Type="String" />
                <asp:Parameter Name="IsActive" Type="Boolean" />
                <asp:Parameter Name="InsertDate" Type="DateTime" />
                <asp:Parameter Name="AttendanceUserID" Type="Int32" />
            </UpdateParameters>
        </asp:SqlDataSource>
    </div>

    <asp:GridView ID="UsersGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="UserSQL">
        <Columns>
            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:BoundField DataField="UserName" HeaderText="Username" SortExpression="UserName" />
            <asp:BoundField DataField="Password" HeaderText="Password" SortExpression="Password" />
            <asp:TemplateField HeaderText="Active" SortExpression="IsActive">
                <ItemTemplate>
                    <asp:CheckBox ID="ActiveCheckBox" Text=" " runat="server" Checked='<%# Bind("IsActive") %>' Enabled="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <script>
        $(function () {
            $('#linkClose').click(function () {
                $('#divError').hide('fade');
            });
        });

        function CrateUser() {
            var isSend = false;
            const index = $("[id*=School_DropDownList]").get(0).selectedIndex;
            const password = $('[id*=Password_TextBox]').val();

            if (index > 0 && password !== '') {
                const users = {
                    username: $('[id*=School_DropDownList]').val(),
                    email: $('[id*=School_DropDownList]').val() + "@gmail.com",
                    password: password,
                    confirmPassword: password
                };

                $.ajax({
                    url: `http://localhost:19362/api/account/register`,
                    method: 'POST',
                    async: false,
                    contentType: 'application/json',
                    data: JSON.stringify(users),
                    success: function() {
                        isSend = true;
                    },
                    error: function(err) {
                        var response = null;
                        const errors = [];
                        var errorsString = "";

                        if (err.status === 400) {
                            try {
                                response = JSON.parse(err.responseText);
                            } catch (e) {}
                        }
                        if (response != null) {
                            const modelState = response.ModelState;

                            for (let key in modelState) {
                                if (modelState.hasOwnProperty(key)) {
                                    errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                                    errors.push(modelState[key]); //list of error messages in an array
                                }
                            }
                        }

                        //DISPLAY THE LIST OF ERROR MESSAGES 
                        if (errorsString !== "") {
                            $("#divErrorText").html(errorsString);
                            $('#divError').show('fade');
                        }
                    }
                });
            }
            return isSend;
        }
    </script>
</asp:Content>
