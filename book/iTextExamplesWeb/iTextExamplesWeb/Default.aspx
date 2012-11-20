<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="iTextExamplesWeb._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Welcome to iTextExamples!
    </h2>
    <p>
    <label>Select chapter:</label>
    <asp:DropDownList ID="DropDownListChapters" runat="server" AutoPostBack="True" 
            onselectedindexchanged="ChapterSelected">
    </asp:DropDownList>
    </p>
    <p>
    <label>Select example:</label>
    <asp:DropDownList ID="DropDownListExamples" runat="server">
    </asp:DropDownList>
    </p>    
    <asp:Button ID="ButtonRun" runat="server" Text="Run example" onclick="Submit" />    
</asp:Content>
