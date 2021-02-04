<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="192226P_RegistrationForm.aspx.cs" Inherits="_192226P_IT2163_Assignment2._192226P_RegistrationForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration Form</title>
    <script type="text/javascript">
        function checkScore() {
            var pass = document.getElementById('<%=Password_TB.ClientID %>').value;

            var score = 5;

            if (pass.length == 0) {
                return (score = 0);
            }

            if (pass.length < 8) {
                return (score = 1);
            }
            else {
                score = 5
            }

            if (pass.search(/[0-9]/) == -1) {
                score = score - 1;
            }

            if (pass.search(/[A-Z]/) == -1) {
                score = score - 1;
            }

            if (pass.search(/[a-z]/) == -1) {
                score = score - 1;
            }

            if (pass.search(/[^A-Za-z0-9]/) == -1) {
                score = score - 1;
            }
            return (score);
        }
        function validate() {

            var passScore = checkScore();

            if (passScore == 0) {
                document.getElementById("lbl_passwordCheck").innerHTML = null;
            }

            if (passScore == 1) {
                document.getElementById("lbl_passwordCheck").innerHTML = "Status : Very Weak";
                document.getElementById("lbl_passwordCheck").style.color = "Red";
            }

            else if (passScore == 2) {
                document.getElementById("lbl_passwordCheck").innerHTML = "Status : Weak";
                document.getElementById("lbl_passwordCheck").style.color = "Red";
            }

            else if (passScore == 3) {
                document.getElementById("lbl_passwordCheck").innerHTML = "Status : Medium";
                document.getElementById("lbl_passwordCheck").style.color = "Red";
            }

            else if (passScore == 4) {
                document.getElementById("lbl_passwordCheck").innerHTML = "Status : Strong";
                document.getElementById("lbl_passwordCheck").style.color = "Green";
            }

            else if (passScore == 5) {
                document.getElementById("lbl_passwordCheck").innerHTML = "Status : Execllent!";
                document.getElementById("lbl_passwordCheck").style.color = "Green";
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Registration Form</h1></div>
        <asp:Label ID="Label1" runat="server" Text="First Name"></asp:Label>
        <asp:TextBox ID="FName_TB" runat="server"></asp:TextBox>
        <asp:Label ID="lbl_FNameCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label2" runat="server" Text="Last Name"></asp:Label>
        <asp:TextBox ID="LName_TB" runat="server"></asp:TextBox>
        <asp:Label ID="lbl_LNameCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label3" runat="server" Text="Credit Card Info"></asp:Label>
        <asp:TextBox ID="CreditCard_TB" runat="server"></asp:TextBox>
        <asp:Label ID="lbl_CreditCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label4" runat="server" Text="Email"></asp:Label>
        <asp:TextBox ID="Email_TB" runat="server" TextMode="Email"></asp:TextBox>
        <asp:Label ID="lbl_EmailCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label7" runat="server" Text="Mobile Number"></asp:Label>
        <asp:TextBox ID="Number_TB" runat="server" TextMode="Number"></asp:TextBox>
        <asp:Label ID="lbl_MobileCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label5" runat="server" Text="Password"></asp:Label>
        <asp:TextBox ID="Password_TB" runat="server" TextMode="Password" onkeyup="javascript:validate()"></asp:TextBox>
        <asp:Label ID="lbl_passwordCheck" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label6" runat="server" Text="Date Of Birth"></asp:Label>
        <asp:TextBox ID="DOB_TB" runat="server" TextMode="Date"></asp:TextBox>
        <asp:Label ID="lbl_DOBCheck" runat="server"></asp:Label>
        <br />
        <asp:Button ID="submit_btm" runat="server" OnClick="submit_btm_Click" Text="Submit" />
        <br />
        <br />
        <asp:Button ID="passwordImproveBtn" runat="server" OnClick="passwordImproveBtn_Click" Text="How to improve password" />
        <br />
        <asp:Label ID="err_msg" runat="server" Visible="False">To improve password security:</asp:Label>
        <br />
        <asp:Label ID="passworderror_msg" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Label ID="error_msg" runat="server"></asp:Label>
        <br />
    </form>
</body>
</html>
