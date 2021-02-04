using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _192226P_IT2163_Assignment2
{
    public partial class _192226P_MainPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInAuth"] != null && Session["RandomAuthToken"] != null && Request.Cookies["RandomAuthToken"] != null)
            {
                if (Session["RandomAuthToken"].ToString().Equals(Request.Cookies["RandomAuthToken"].Value))
                {
                    mainpage_error.Text = "You are logged in";
                    mainpage_error.ForeColor = System.Drawing.Color.Green;
                    Logout_Btn.Visible = true;
                }
            }
            else
            {
                Response.Redirect("192226P_LoginForm.aspx", false);
            }
        }

        protected void Logout_Btn_Click(object sender, EventArgs e)
        {
            //Remove the Session values
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();


            // Remove all cookies
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["RandomAuthToken"] != null)
            {
                Response.Cookies["RandomAuthToken"].Value = string.Empty;
                Response.Cookies["RandomAuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }

            //Return to the login page
            Response.Redirect("192226P_LoginForm.aspx", false);

        }
    }
}