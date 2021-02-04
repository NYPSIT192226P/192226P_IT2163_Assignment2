using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Drawing;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace _192226P_IT2163_Assignment2
{
    public partial class LoginForm : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        int accountLogoutCount = 0;
        int errorCount = 0;
        string errorMsg = "";

        public class MyObject
        {
            public string success { get; set; }
            public string ErrorMessage { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //Captcha V3 codes
        public bool ValidateCaptcha()
        {
            bool results = true;

            //When user submits the recaptcha form, the user gets a response POST parameter,
            //captchaResponse consist of the user click pattern. Behaviour analytics! AI :)
            string captchaResponse = Request.Form["g-recaptcha-response"];

            //To send a GET request to Google along with the response and Secret key.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
            ("https://www.google.com//recaptcha/api/siteverify?secret=6LepajUaAAAAAImIfKn10w5Zl_kAvQ6pjzJC9YIt &response=" + captchaResponse);

            try
            {

                //Codes to receive the Response in JSON format from Google Server
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        //The response in JSON format
                        string jsonResponse = readStream.ReadToEnd();

                        //To show the JSON response string for learning purpose
                        lbl_gScore.Text = jsonResponse.ToString();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        //Create jsonObject to handle the response e.g. success or Wrror
                        //Deserialize Json
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        //Convert the string "False" to bool false or "True" to vool true
                        results = Convert.ToBoolean(jsonObject.success);//


                    }
                }
                return results;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        //Retrieve the Login Attempts
        protected int RetrieveLoginAttempts(string userid)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Attempts FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["Attempts"] == null)
                        {
                            accountLogoutCount = 0;
                        }
                        else
                        {
                            accountLogoutCount = Convert.ToInt32(reader["Attempts"]);
                            // Log 1
                            System.Diagnostics.Debug.WriteLine(accountLogoutCount);
                            testMsg.Text = accountLogoutCount.ToString();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }

            return accountLogoutCount;
        }

        protected void submit_btm_Click(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                string pwd = HttpUtility.HtmlEncode(LoginPassword_TB.Text.ToString().Trim());
                string userid = HttpUtility.HtmlEncode(LoginEmail_TB.Text.ToString().Trim());
                SHA512Managed hashing = new SHA512Managed();
                string dbHash = getDBHash(userid);
                string dbSalt = getDBSalt(userid);
                if (String.IsNullOrEmpty(userid))
                {
                    errorMsg += "Email cannot be empty. Please try again.<br />";
                    errorCount += 1;
                }
                if (String.IsNullOrEmpty(pwd))
                {
                    errorMsg += "Password cannot be empty. Please try again.";
                    errorCount += 1;
                }
                if (errorCount != 0)
                {
                    LoginerrorMsg.Text = errorMsg;
                    LoginerrorMsg.ForeColor = Color.Red;
                }
                else
                {
                    RetrieveLoginAttempts(userid);
                    bool LockoutVerified = VerifyLockout(userid);
                    //If the lockout verified is false, that means that it is not locked out
                    if (LockoutVerified == false)
                    {
                        if (accountLogoutCount != 3)
                        {
                            try
                            {
                                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                                {
                                    string pwdWithSalt = pwd + dbSalt;
                                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                    string userHash = Convert.ToBase64String(hashWithSalt);
                                    if (userHash.Equals(dbHash))
                                    {
                                        //Practical 4 creates a session
                                        Session["LoggedInAuth"] = LoginEmail_TB.Text.Trim();

                                        // create a new GUID and save into the session
                                        string guid = Guid.NewGuid().ToString();
                                        Session["RandomAuthToken"] = guid;

                                        // now create a new cookie with this guid value
                                        Response.Cookies.Add(new HttpCookie("RandomAuthToken", guid));

                                        //Revert the Account Attempts to 0 when it the login is accepted.
                                        RevertAccountAttempts(userid);

                                        Response.Redirect("192226P_MainPage.aspx", false);
                                    }
                                    else
                                    {
                                        LoginerrorMsg.Text = "Email or password is not valid. Please try again.";
                                        LoginerrorMsg.ForeColor = Color.Red;
                                        accountLogoutCount += 1;
                                        UpdateAccountAttempts(userid);
                                        accountAttemptCheck(userid);
                                        testMsg.Text = accountLogoutCount.ToString();
                                    }
                                }
                                else
                                {
                                    LoginerrorMsg.Text = "Email or password is not valid. Please try again.";
                                    LoginerrorMsg.ForeColor = Color.Red;
                                    testMsg.Text = accountLogoutCount.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.ToString());
                            }
                            finally { }
                        }
                        else
                        {
                            LoginerrorMsg.Text = "Account Lockout. Please try again in 5 minutes.";
                            UpdateLockoutTime(userid);
                            RevertAccountAttempts(userid);
                            LoginerrorMsg.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        RevertAccountAttempts(userid);
                        LoginerrorMsg.Text = "Account Lockout. Please try again in 5 minutes.";
                        LoginerrorMsg.ForeColor = Color.Red;
                    }

                }
            }
        }

        //If account attempts = 3, then the lockout time is updated
        protected void accountAttemptCheck(string userid)
        {
            if (accountLogoutCount == 3)
            {
                UpdateLockoutTime(userid);
            }
        }

        //Update the Attempt numbers in the database
        protected void UpdateAccountAttempts(string userid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET Attempts=@currentAttempt where Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@currentAttempt", accountLogoutCount);
                            cmd.Parameters.AddWithValue("@USERID", userid);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        //Revert the Attempt numbers in the database to 0
        protected void RevertAccountAttempts(string userid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET Attempts=@currentAttempt where Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@currentAttempt", 0);
                            cmd.Parameters.AddWithValue("@USERID", userid);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        //Update the lockout time in the database
        protected void UpdateLockoutTime(string userid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET LockoutTime=@currentLockout where Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@currentLockout", DateTime.Now);
                            cmd.Parameters.AddWithValue("@USERID", userid);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        protected string getDBHash(string userid)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }
        protected string getDBSalt(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PASSWORDSALT FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PASSWORDSALT"] != null)
                        {
                            if (reader["PASSWORDSALT"] != DBNull.Value)
                            {
                                s = reader["PASSWORDSALT"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        //Used to Verify the Lockout
        protected bool VerifyLockout(string userid)
        {
            string userID = userid;
            //lockedOut to indicate if account is locked out. Set to false first.
            bool lockedOut = false;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string LockOutTimingQuery = "select LockoutTime FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(LockOutTimingQuery, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        //This is to add 5 minutes to the last lockout time.
                        DateTime LockoutEndTiming = Convert.ToDateTime(reader["LockoutTime"]).AddMinutes(5);
                        //Log 2
                        System.Diagnostics.Debug.WriteLine(LockoutEndTiming);
                        DateTime NowTime = DateTime.Now;
                        //Check if the end lockout time has been passed
                        int LogoutValid = DateTime.Compare(LockoutEndTiming, NowTime);
                        if (LogoutValid >= 0)
                        {
                            lockedOut = true;
                            LogoutError.Text = lockedOut.ToString();
                            LogoutTime.Text = LogoutValid.ToString();
                        }
                        else
                        {
                            //If the end lockout time has been passed, the lockout time has been passed
                            lockedOut = false;
                            LogoutError.Text = lockedOut.ToString();
                            LogoutTime.Text = LogoutValid.ToString();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return lockedOut;
        }
    }
}