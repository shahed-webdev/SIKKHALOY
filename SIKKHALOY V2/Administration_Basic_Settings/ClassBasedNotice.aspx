<%@ Page Title="Class Based Notice" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="ClassBasedNotice.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.ClassBasedNotice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row mt-4">
        <div class="col-xl-10 mx-auto">
            <div class="card card-body">
                <h4 class="font-weight-bold mb-3">Class Based Notice</h4>

                <asp:CheckBoxList ID="ClassCheckBoxList" CssClass="DefaultCheckBoxList" runat="server" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" RepeatDirection="Horizontal" RepeatLayout="Flow">
                </asp:CheckBoxList>
                <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <div class="form-group mt-3">
                    <label>Notice Title</label>
                    <asp:TextBox ID="NoticeTitleTextBox" runat="server" CssClass="form-control" placeholder="Notice Title" required=""></asp:TextBox>
                </div>

                <div class="form-group">
                    <label>Notice</label>
                    <asp:TextBox ID="NoticeTextBox" runat="server" CssClass="form-control" Height="90px" TextMode="MultiLine" placeholder="Notice Text Write here" required=""></asp:TextBox>
                </div>

                <div class="mb-3">
                    <asp:Button ID="NoticeButton" OnClick="NoticeButton_Click" runat="server" Text="Submit" ValidationGroup="1" CssClass="btn btn-primary ml-0 mt-3" />

                    <asp:SqlDataSource ID="StudentNoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        InsertCommand="INSERT INTO [StudentNotice] ([RegistrationId], [SchoolId], [EducationYearId], [NoticeTitle], [Notice]) 
                        VALUES (@RegistrationId, @SchoolId, @EducationYearId, @NoticeTitle, @Notice);
                        SELECT @StudentNoticeId = SCOPE_IDENTITY();"
                        SelectCommand="SELECT StudentNoticeId, NoticeTitle, Notice FROM StudentNotice WHERE (SchoolId = @SchoolId)"
                        DeleteCommand="DELETE  FROM  StudentNoticeClass WHERE (StudentNoticeId = @StudentNoticeId)
DELETE FROM [StudentNotice] WHERE [StudentNoticeId] = @StudentNoticeId" OnInserted="StudentNoticeSQL_Inserted">
                        <DeleteParameters>
                            <asp:Parameter Name="StudentNoticeId" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="RegistrationId" SessionField="RegistrationID" Type="Int32" />
                            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" Type="Int32" />
                            <asp:ControlParameter ControlID="NoticeTitleTextBox" Name="NoticeTitle" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="NoticeTextBox" Name="Notice" PropertyName="Text" Type="String" />
                            <asp:Parameter Name="StudentNoticeId" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolId" SessionField="SchoolId" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <asp:SqlDataSource ID="StudentNoticeClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        InsertCommand="INSERT INTO [StudentNoticeClass] ([StudentNoticeId], [ClassId]) VALUES (@StudentNoticeId, @ClassId)"
                        SelectCommand="SELECT * FROM [StudentNoticeClass]">
                        <InsertParameters>
                            <asp:Parameter Name="StudentNoticeId" Type="Int32" />
                            <asp:Parameter Name="ClassId" Type="Int32" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                </div>

                <asp:GridView ID="NoticeGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="StudentNoticeId" DataSourceID="StudentNoticeSQL">
                    <Columns>
                        <asp:BoundField DataField="NoticeTitle" HeaderText="Notice Title" SortExpression="NoticeTitle" />
                        <asp:BoundField DataField="Notice" HeaderText="Notice" SortExpression="Notice" />
                        <asp:TemplateField HeaderText="Classes">
                            <ItemTemplate>
                                <asp:HiddenField ID="IdHiddenField" runat="server" Value='<%# Eval("StudentNoticeId") %>' />
                                <asp:DataList ID="ClassDataList" runat="server" DataSourceID="ClassSQL">
                                    <ItemTemplate>
                                        Class:
                                        <asp:Label ID="ClassLabel" runat="server" Text='<%# Eval("Class") %>' />
                                        <br />
                                        <br />
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class FROM StudentNoticeClass INNER JOIN CreateClass ON StudentNoticeClass.ClassId = CreateClass.ClassID WHERE (StudentNoticeClass.StudentNoticeId = @StudentNoticeId) ORDER BY CreateClass.SN">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="IdHiddenField" Name="StudentNoticeId" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:CommandField HeaderText="Delete" ShowDeleteButton="True" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
