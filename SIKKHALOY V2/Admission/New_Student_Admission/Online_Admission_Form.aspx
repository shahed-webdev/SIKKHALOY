<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .online-form { color: #000;}
        .online-form h3{border:none;}
        .online-form .border-bottom { margin-bottom:20px; border-bottom: 1px solid #777 !important; }
        .online-form .cb label { margin-bottom: 0; }

       .online-form fieldset { border-radius:3px;}
         .online-form legend { padding: 0 7px; font-size: 1rem; font-weight:bold; }
        .online-form .border{border: 1px solid #333 !important;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="online-form">
        <h3 class="text-center">Student Online Admission Form</h3>

        <input type="button" value="Print" class="btn btn-sm btn-blue d-print-none" onclick="window.print();"/>
        <fieldset class="border p-2 mb-3">
            <legend class="w-auto">Student Information</legend>
            <div class="row">
                <div class="col">
                    *SMS Mobile Number:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    *Student's Name:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col-4">*Gender:<div class="border-bottom"></div></div>
                <div class="col">
                    Date of Birth (day/month/year):<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    Blood Group:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Religion:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    Student's Permanent Address:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>
                <div class="col">
                    Student's Local Address:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 mb-3">
            <legend class="w-auto">Parents Information</legend>
            <div class="row">
                <div class="col">
                    *Father's Name:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Father's Phone:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Father's Occupation:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    *Mother's Name:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Mother's Phone:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Mother's Occupation:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 mb-3">
            <legend class="w-auto">Previous Institution Information (If Any)</legend>
            <div class="row">
                <div class="col">
                    Institution Name:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>
                <div class="col">
                    Class:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Exam Year:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Grade:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 mb-3">
            <legend class="w-auto">Second Guardian Information(Optional)</legend>
            <div class="row">
                <div class="col">
                    Guardian Name:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Relationship:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Mobile No.:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2">
            <legend class="w-auto">Academic Information</legend>
            <div class="row">
                <div class="col">
                    *Class:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Section:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    Group:<div class="border-bottom"></div>
                </div>
                 <div class="col">
                    Roll No:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>
                 <div class="col">
                    Optional Subject:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>
    </div>
</asp:Content>
