<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Employee_Entry_Log_Display.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Employee_Entry_Log_Display" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <title></title>
       <meta http-equiv="X-UA-Compatible" content="IE=edge" />
       <link href="CSS/Style.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <marquee behavior="scroll" direction="down" height="800" width="280" scrollamount="3">
            <asp:DataList ID="EmployeeDataList" runat="server" DataSourceID="EmployeeSQL" CssClass="Emp_Main">
                <ItemTemplate>
                    <div class="Emp_Info">
                        <table>
                            <tr>
                                <td>
                                    <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <img src="../../Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="IDLabel" runat="server" Text='<%# Eval("ID") %>' />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="EmployeeTypeLabel" runat="server" Text='<%# Eval("EmployeeType") %>' />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="ExitTimeLabel" runat="server" Text='<%# Eval("EntryTime") %>' />
                                    (<asp:Label ID="AttendanceStatusLabel" runat="server" Text='<%# Eval("AttendanceStatus") %>' />)
                                </td>
                            </tr>
                        </table>
                    </div>
                </ItemTemplate>
            </asp:DataList>
        </marquee>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName+' '+VW_Emp_Info.LastName as Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, 
        CONVERT(varchar(15),Employee_Attendance_Record.EntryTime, 100) AS EntryTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN
        VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID
WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT(date, GETDATE())) AND (Employee_Attendance_Record.SchoolID = @SchoolID)
ORDER BY Employee_Attendance_Record.EntryTime DESC">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
