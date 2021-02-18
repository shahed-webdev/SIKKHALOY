<%@ Page Title="Employee ID Card" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="ID_Cards.aspx.cs" Inherits="EDUCATION.COM.Employee.ID_Cards" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #wrapper { display: grid; grid-gap: 45px 30px; grid-template-columns: repeat(3, 1fr); }
            #wrapper > div {position: relative; border: 2px solid #000; width: 323.52px; height: 206px; }

        #grid_Header { background-color: #5b0095; border-bottom: 2px solid #4a0184; color: #fff; text-align: center; display: grid; grid-template-columns:40px 1fr; margin-bottom: 5px;}
            #grid_Header img { height: 30px; border-radius: 3px; }
        .Hidden_Ins_Name { position: absolute; visibility: hidden; height: auto; width: auto; white-space: nowrap; }
        .Institution_Dialog { font-size: 11px; letter-spacing: 3.3px; line-height: 14px; text-align: center; }

        .iCard-title { background-color: #5b0095; border-radius: 6px; color: #fff; font-size: 16px; font-weight: bold; margin: auto; text-align: center; width: 100px; }

        #user-info { display: grid; grid-template-columns: 90px 1fr; }
            #user-info img { height: 85px; width: 85px; }
            #user-info ul { margin: 5px 0 0 0; padding-left: 5px; }
                #user-info ul li { list-style: none; font-size: 13px; line-height: 1.5; color: #000; }
        .c-user-name { font-weight: bold; }

        .c-address { background-color: #5b0095; font-size: 12px; text-align: center; color: #fff; position: absolute; bottom: 0; width: 100%; }
        .sign { position: absolute; right: 5px; bottom: 18px; font-weight: normal; margin-bottom: 0; font-size: 8.5pt; }

        @page { margin: 7px 0 0 7px; }
        @media print {
            #header, h3 { display: none; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Employee ID Card</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:RadioButtonList ID="EmpTypeRadioButtonList" CssClass="form-control" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group ml-2">
            <asp:TextBox ID="FindTextBox" runat="server" placeholder="Enter Search Keyword" CssClass="form-control"></asp:TextBox>
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" />
        </div>
        <div class="form-group">
            <label class="btn btn-white" style="font-size: .81rem">
                Signature Browse
                <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <input onclick="window.print()" type="submit" value="Print" class="btn btn-primary" />
        </div>
    </div>

    <div id="wrapper">
        <asp:Repeater ID="IDCard_Repeater" runat="server" DataSourceID="EmployeeSQL">
            <ItemTemplate>
                <div>
                    <div id="grid_Header">
                        <div style="padding-top:6px">
                            <img alt="No Logo" src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" />
                        </div>
                        <div>
                            <div class="Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>
                            <div class="Hidden_Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>
                            <div class="Institution_Dialog">
                                <asp:Label ID="Instit_Dialog" CssClass="Instit_Dialog" runat="server" Text='<%# Bind("Institution_Dialog") %>' />
                            </div>
                        </div>
                    </div>

                    <div class="iCard-title">ID Card</div>

                    <div id="user-info">
                        <div style="text-align: center;">
                            <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="rounded-circle img-thumbnail" /><br />
                        </div>

                        <div>
                            <ul>
                                <li class="c-user-name"><%# Eval("Name") %></li>
                                <li><%# Eval("Designation") %></li>
                                <li>ID: <%# Eval("ID") %></li>
                                <li>Phone: <%# Eval("Phone") %></li>
                            </ul>
                        </div>
                    </div>

                    <div class="sign">authority signature</div>
                    <div class="c-address">
                        <%# Eval("Address") %>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT VW_Emp_Info.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, VW_Emp_Info.Permanent_Temporary, VW_Emp_Info.Salary, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.Phone, VW_Emp_Info.DeviceID, SchoolInfo.SchoolName, SchoolInfo.SchoolLogo, VW_Emp_Info.SchoolID, SchoolInfo.Address, SchoolInfo.Institution_Dialog FROM VW_Emp_Info INNER JOIN SchoolInfo ON VW_Emp_Info.SchoolID = SchoolInfo.SchoolID WHERE (VW_Emp_Info.SchoolID = @SchoolID) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)"
            FilterExpression="ID LIKE '{0}%' or Name LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%'">
            <FilterParameters>
                <asp:ControlParameter ControlID="FindTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>


    <script>
        $(function () {
            var Default_fontSize = 13;
            var Max_fontSize = 20;

            var test = document.getElementsByClassName("Hidden_Ins_Name")[0];
            var Show = document.getElementsByClassName("Ins_Name")[0];

            var New_fontSize = Math.round(((Default_fontSize * parseFloat(Show.clientWidth)) / parseFloat(test.clientWidth)));
            if (New_fontSize > Max_fontSize) {
                New_fontSize = Max_fontSize;
            }
            var width = (test.clientWidth) + "px";

            $('.Ins_Name').css('font-size', New_fontSize);


            if (!$('.Instit_Dialog').text()) {
                $('.Institution_Dialog').hide();
                $('.Hidden_Ins_Name').hide();
                $('.Ins_Name').css("margin-top", "8px");
            }

            // Sign upload
            $("#Hfileupload").change(function () {
                if (typeof (FileReader) != "undefined") {
                    var dvPreview = $(".sign");
                    dvPreview.html("");
                    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
                    $($(this)[0].files).each(function () {
                        var file = $(this);
                        if (regex.test(file[0].name.toLowerCase())) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = $("<img />");
                                img.attr("style", "height:24px;width:75px;");
                                img.attr("src", e.target.result);
                                dvPreview.append(img);
                            }
                            reader.readAsDataURL(file[0]);
                        } else {
                            alert(file[0].name + " is not a valid image file.");
                            dvPreview.html("");
                            return false;
                        }
                    });
                } else {
                    alert("This browser does not support HTML5 FileReader.");
                }
            });
        });
    </script>
</asp:Content>
