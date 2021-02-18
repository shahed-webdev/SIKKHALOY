<%@ Page Title="Single ID Card" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Single_ID.aspx.cs" Inherits="EDUCATION.COM.ID_Cards.Single_ID" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/All_ID_Cards.css?v=4" rel="stylesheet" />
    <style>
        .border { margin: 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="Contain">
        <h3>Students ID Cards</h3>
        <a href="All_ID_Cards.aspx" class="NoPrint"><< All ID Card</a>
        <div class="form-inline NoPrint" style="margin-bottom: 20px;">
            <div class="form-group">
                <asp:TextBox ID="IDTextBox" runat="server" CssClass="form-control" placeholder="Enter Student ID"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" />
            </div>
            <div class="form-group">
                <label class="btn btn-default btn-file">
                    Signature Browse <input id="Hfileupload" type="file" style="display: none;" />
                </label>
            </div>
        </div>

        <asp:FormView ID="IDCardFV" runat="server" DataSourceID="ICardInfoSQL" Width="100%">
            <ItemTemplate>
                <div class="border col-sm-6">
                    <div class="row card-header">
                        <div class="col-md-2 col-sm-2 text-center" style="padding-left: 0; padding-right: 0">
                            <img src="../Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" class="Ins_Logo" />
                        </div>
                        <div class="col-md-10 col-sm-10" style="padding-left: 0; padding-right: 0">
                            <div class="Ins_Name">
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("SchoolName") %>' />
                            </div>
                            <div class="Hidden_Ins_Name">
                                <asp:Label ID="Label10" runat="server" Text='<%# Bind("SchoolName") %>' />
                            </div>
                            <div class="Institution_Dialog">
                                <asp:Label ID="Label9" CssClass="Instit_Dialog" runat="server" Text='<%# Bind("Institution_Dialog") %>' />
                            </div>
                        </div>
                    </div>

                    <div class="Student_ID">
                        Student ID Card
                    </div>

                    <div class="row student-info">
                        <div class="col-md-3 col-sm-3 col-xs-12 text-center" style="padding-left: 0">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="img-circle S_Photo" /><br />
                            <div class="S-ID">ID:<asp:Label ID="Label2" runat="server" Text='<%# Bind("ID") %>' Font-Bold="True" /></div>
                        </div>

                        <div class="col-md-9 col-sm-9 col-xs-12 S_Info">
                            <ul>
                                <li>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("StudentsName")%>' Font-Bold="True"  Font-Size="12px"/></li>
                                <li>Class:
                                 <asp:Label ID="Label4" runat="server" Text='<%# Bind("Class") %>' />
                                    , Roll No:
                                 <asp:Label ID="Label5" runat="server" Text='<%# Bind("RollNo") %>' />
                                </li>
                                <li>Phone:
                                 <asp:Label ID="Label7" runat="server" Text='<%# Bind("SMSPhoneNo") %>' /></li>
                                <li>Blood Group:
                                 <asp:Label ID="Label8" runat="server" Text='<%# Bind("BloodGroup") %>' /></li>
                                <li>D.O.B:
                                 <asp:Label ID="Label11" runat="server" Text='<%# Bind("DateofBirth") %>' /></li>
                            </ul>
                        </div>
                    </div>
                    <div class="sign">
                        Principal signature
                    </div>

                    <div class="row card-footer text-center">
                        <asp:Label ID="AddressLabel" runat="server" Text='<%# Bind("Address") %>' />
                    </div>
                </div>
            </ItemTemplate>
        </asp:FormView>

        <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.BloodGroup, Student.SMSPhoneNo, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (Student.ID = @ID)">
            <SelectParameters>
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" DefaultValue="" />
            </SelectParameters>
        </asp:SqlDataSource>

        <%if (IDCardFV.DataItemCount > 0)
            {%>
        <b class="Help">Page orientation will be "portrait"</b><br />
        <input onclick="window.print()" type="submit" value="Print" class="btn btn-primary" />
        <%} %>
    </div>
    <br />
    <script>
        $(function () {
            $("#<%=IDTextBox.ClientID%>").autocomplete("/IDSearch.ashx");

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
                              img.attr("style", "height: 24px;width: 75px;position: absolute;right: 0;bottom: 15px;");
                              img.attr("src", e.target.result);
                              dvPreview.append(img);
                              dvPreview.append("Principal signature");
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
