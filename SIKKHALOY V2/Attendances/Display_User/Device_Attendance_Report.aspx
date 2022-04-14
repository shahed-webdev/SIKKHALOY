<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Device_Attendance_Report.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Device_Attendance_Report" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="CSS/Summery.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:FormView ID="FormView1" runat="server" DataSourceID="Stu_Att_Report_SQL">
            <ItemTemplate>
                <div class="info">
                    <h3>Student Attendance</h3>
                    <table>
                        <tr>
                            <td class="TD_Size">Total Student</td>
                            <td>
                                <asp:Label ID="Total_StudentLabel" runat="server" Text='<%# Bind("Total_Student") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Current IN</td>
                            <td>
                                <asp:Label ID="Current_IN_StudentLabel" runat="server" Text='<%# Bind("Current_IN_Student") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total OUT</td>
                            <td>
                                <asp:Label ID="Total_Out_StudentLabel" runat="server" Text='<%# Bind("Total_Out_Student") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Present</td>
                            <td>
                                <asp:Label ID="Total_Student_PrasentLabel" runat="server" Text='<%# Bind("Total_Student_Prasent") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Late</td>
                            <td>
                                <asp:Label ID="Total_Student_LateLabel" runat="server" Text='<%# Bind("Total_Student_Late") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Absent</td>
                            <td>
                                <asp:Label ID="Total_Student_AbsentLabel" runat="server" Text='<%# Bind("Total_Student_Absent") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Late Absent</td>
                            <td>
                                <asp:Label ID="Total_Student_Late_AbsentLabel" runat="server" Text='<%# Bind("Total_Student_Late_Absent") %>' />
                            </td>
                        </tr>
                    </table>
                </div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="Stu_Att_Report_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select (SELECT  COUNT(Student.StudentID) FROM  Student INNER JOIN
           StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
WHERE (Student.Status = N'Active') AND (Education_Year.Status = N'True') AND (Student.SchoolID = @SchoolID))AS Total_Student,

(SELECT COUNT(StudentID)  FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID))AS Current_IN_Student,

(SELECT COUNT(StudentID)  FROM Attendance_Record
WHERE(AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID))AS Total_Out_Student,

(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Pre'))AS Total_Student_Prasent,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late')) AS Total_Student_Late,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE(AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Abs'))AS Total_Student_Absent,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late Abs'))AS Total_Student_Late_Absent">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
        <br />
        <br />
        <asp:FormView ID="FormView2" runat="server" DataSourceID="Emp_Att_Report_SQL">
            <ItemTemplate>
                <div class="info">
                    <h3>Employee Attendance</h3>
                    <table>
                        <tr>
                            <td class="TD_Size">Total Employee</td>
                            <td>
                                <asp:Label ID="Total_EmployeeLabel" runat="server" Text='<%# Bind("Total_Employee") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Current IN</td>
                            <td>
                                <asp:Label ID="Current_IN_EmployeeLabel" runat="server" Text='<%# Bind("Current_IN_Employee") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total OUT</td>
                            <td>
                                <asp:Label ID="Total_Out_EmployeeLabel" runat="server" Text='<%# Bind("Total_Out_Employee") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Present</td>
                            <td>
                                <asp:Label ID="Total_Employee_PrasentLabel" runat="server" Text='<%# Bind("Total_Employee_Prasent") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Late</td>
                            <td>
                                <asp:Label ID="Total_Employee_LateLabel" runat="server" Text='<%# Bind("Total_Employee_Late") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Absent</td>
                            <td>
                                <asp:Label ID="Total_Employee_AbsentLabel" runat="server" Text='<%# Bind("Total_Employee_Absent") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>Total Late Absent</td>
                            <td>
                                <asp:Label ID="Total_Employee_Late_AbsentLabel" runat="server" Text='<%# Bind("Total_Employee_Late_Absent") %>' />
                            </td>
                        </tr>
                    </table>
                </div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="Emp_Att_Report_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select 

(SELECT  Count(EmployeeID)  FROM Employee_Info WHERE (Job_Status = N'Active') AND (SchoolID = @SchoolID))AS Total_Employee,

(SELECT COUNT(*)  FROM Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitConfirmed_Status = N'No') AND (SchoolID = @SchoolID))AS Current_IN_Employee,

(SELECT COUNT(*)  FROM Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitConfirmed_Status = N'Yes') AND (SchoolID = @SchoolID))AS Total_Out_Employee,

(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Pre'))AS Total_Employee_Prasent,

(SELECT        COUNT(*)
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late')) AS Total_Employee_Late,

(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Abs'))AS Total_Employee_Absent,
(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late Abs'))AS Total_Employee_Late_Absent">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
