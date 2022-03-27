<%@ Page Title="Donation Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationReceipt.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationReceipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        @page { margin: 0 13.3rem !important; }

        @media print {
            .logo-waper { display: none; }
            #header { margin-bottom: 10px; border-bottom: none !important; }

            /*for black and white page*/
            .bg-main { background-color: #fff; color: #000; box-shadow: none !important }
            #InstitutionName { font-weight: bold; color: #000 !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

</asp:Content>
