<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="192226P_LoginForm.aspx.cs" Inherits="_192226P_IT2163_Assignment2.LoginForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login Form</title>
    <script src="https://www.google.com/recaptcha/api.js?render="></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        <div>
            <h1>SITConnect Login</h1></div>
        <asp:Label ID="Label1" runat="server" Text="Email"></asp:Label>
        <asp:TextBox ID="LoginEmail_TB" runat="server"></asp:TextBox>
            <br />
        <asp:Label ID="Label2" runat="server" Text="Password"></asp:Label>
        <asp:TextBox ID="LoginPassword_TB" runat="server" TextMode="Password"></asp:TextBox>
            <br />
            <br />
        <asp:Button ID="login_btm" runat="server" OnClick="submit_btm_Click" Text="Login" />
            <br />
            <br />
            <asp:Label ID="LoginerrorMsg" runat="server"></asp:Label>
            <br />
            <asp:Label ID="testMsg" runat="server"></asp:Label>
            <asp:Label ID="lbl_gScore" runat="server"></asp:Label>
            <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
            <br />
            <asp:Label ID="LogoutError" runat="server"></asp:Label>
            <br />
            <asp:Label ID="LogoutTime" runat="server"></asp:Label>
        </div>
    </form>
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>
