<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Student_Entry_Log_Display.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Student_Log_Display" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <title></title>
   <meta http-equiv="X-UA-Compatible" content="IE=edge" />
   <link href="CSS/Style.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <marquee behavior="scroll" direction="left" width="890" scrollamount="3" behavior="scroll">
            <asp:DataList ID="StudentDataList" runat="server" DataSourceID="Student_Entry_LogSQL" RepeatDirection="Horizontal" Width="100%">
               <ItemTemplate>
                  <fieldset>
                     <legend>
                        <asp:Label ID="StudentsNameLabel" runat="server" Text='<%# Eval("StudentsName") %>' />
                        (<asp:Label ID="EntryTimeLabel" runat="server" Text='<%# Eval("EntryTime") %>' />)</legend>

                     <div class="Student_img">
                        <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>"/>
                     </div>

                     <div class="Student_info">
                       <table>
                          <tr>
                             <td class="TD_Size">ID</td>
                             <td>
                                <asp:Label ID="IDLabel" runat="server" Text='<%# Eval("ID") %>' />
                             </td>
                          </tr>
                          <tr>
                             <td>Class</td>
                             <td>
                                <asp:Label ID="ClassLabel" runat="server" Text='<%# Eval("Class") %>' />
                             </td>
                          </tr>
                          <tr>
                             <td>Roll No</td>
                             <td>
                                <asp:Label ID="RollNoLabel" runat="server" Text='<%# Eval("RollNo") %>' />
                             </td>
                          </tr>
                          <tr>
                             <td>Entry</td>
                             <td>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("EntryTime") %>' />
                                (<asp:Label ID="AttendanceLabel" runat="server" Text='<%# Eval("Attendance") %>' />)
                             </td>
                          </tr>
                       </table>
                     </div>
                  </fieldset>

               </ItemTemplate>
            </asp:DataList>
     </marquee>
        <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT(varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime
FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN
StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN
CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Attendance_Record.AttendanceDate = CONVERT(date, GETDATE())) AND (Attendance_Record.ExitConfirmed_Status = N'No') AND (Attendance_Record.SchoolID = @SchoolID)
ORDER BY Attendance_Record.EntryTime DESC">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
