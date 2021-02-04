<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="192226P_MainPage.aspx.cs" Inherits="_192226P_IT2163_Assignment2._192226P_MainPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div>
                <h1>SITConnect Main Page</h1>
            </div>
        </div>
        <asp:Label ID="mainpage_error" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Button ID="Logout_Btn" runat="server" OnClick="Logout_Btn_Click" Text="Logout" Visible="False" />
        <br />
        <br />
            <asp:Button ID="ChangePwd" runat="server" OnClick="ChangePwd_Click" Text="Click here to change the password" />
    </form>
</body>
</html>
