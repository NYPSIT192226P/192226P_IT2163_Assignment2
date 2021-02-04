<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PasswordChange.aspx.cs" Inherits="_192226P_IT2163_Assignment2.PasswordChange" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Change password</h1>
        <asp:Label ID="Label1" runat="server" Text="Email"></asp:Label>
        <asp:TextBox ID="Email_TB" runat="server"></asp:TextBox>
            <br />
        <asp:Label ID="Label2" runat="server" Text="Old Password"></asp:Label>
        <asp:TextBox ID="OldPassword_TB" runat="server" TextMode="Password"></asp:TextBox>
            <br />
        <asp:Label ID="Label3" runat="server" Text="New Password"></asp:Label>
        <asp:TextBox ID="NewPassword_TB" runat="server" TextMode="Password"></asp:TextBox>
            <br />
        <asp:Label ID="Label4" runat="server" Text="New Password Confrim"></asp:Label>
        <asp:TextBox ID="NewPasswordConfirm_TB" runat="server" TextMode="Password"></asp:TextBox>
        </div>
        <asp:Button ID="ChangePasswordBtn" runat="server" Text="Change password" OnClick="ChangePasswordBtn_Click" />
        <p>
        <asp:Label ID="ErrorMessage" runat="server"></asp:Label>
        </p>
        <p>
        <asp:Label ID="ErrorPassword1" runat="server"></asp:Label>
        </p>
        <p>
        <asp:Label ID="ErrorPassword2" runat="server"></asp:Label>
        </p>
        <p>
        <asp:Label ID="PasswordError" runat="server"></asp:Label>
        </p>
    </form>
</body>
</html>
