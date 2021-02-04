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
    public partial class PasswordChange : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string NewfinalHash;
        static string Newsalt;
        static string FirstfinalHash;
        static string Firstsalt;
        static string SecondfinalHash;
        static string Secondsalt;
        int errorCount = 0;
        string errorMsg = "";
        byte[] NewKey;
        byte[] NewIV;

        public class MyObject
        {
            public string success { get; set; }
            public string ErrorMessage { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (checkMaxPasswordTime(HttpUtility.HtmlEncode(Email_TB.Text.ToString().Trim())) == false)
            {
                ErrorMessage.Text = "Password has expired. You need to change it.";
                ErrorMessage.ForeColor = Color.Red;
            }
        }

        //Check if the user need to change their password
        protected bool checkMaxPasswordTime(string userid)
        {
            bool PasswordChange = false;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string LockOutTimingQuery = "select MaxPasswordChange FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(LockOutTimingQuery, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        //This is to get the timing that the password has to be changed afterwards
                        DateTime PasswordChangeEndTime = Convert.ToDateTime(reader["MaxPasswordChange"]);
                        DateTime NowTime = DateTime.Now;
                        //Check if the end password time has been passed
                        int LogoutValid = DateTime.Compare(PasswordChangeEndTime, NowTime);
                        if (LogoutValid <= 0)
                        {
                            PasswordChange = true;
                        }
                        else
                        {
                            //If the end lockout time has been passed, the lockout time has been passed
                            PasswordChange = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return PasswordChange;
        }

        protected void ChangePasswordBtn_Click(object sender, EventArgs e)
        {
            string SecondOldPasswordHash = "";
            string Oldpwd = HttpUtility.HtmlEncode(OldPassword_TB.Text.ToString().Trim());
            string userid = HttpUtility.HtmlEncode(Email_TB.Text.ToString().Trim());
            string Newpwd = HttpUtility.HtmlEncode(NewPassword_TB.Text.ToString().Trim());
            string ConfirmNewpwd = HttpUtility.HtmlEncode(NewPasswordConfirm_TB.Text.ToString().Trim());
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(userid);
            FirstfinalHash = dbHash;
            string dbSalt = getDBSalt(userid);
            Firstsalt = dbSalt;
            if (String.IsNullOrEmpty(userid))
            {
                errorMsg += "Email cannot be empty. Please try again.<br />";
                errorCount += 1;
            }
            if (String.IsNullOrEmpty(Oldpwd))
            {
                errorMsg += "Password cannot be empty. Please try again.<br />";
                errorCount += 1;
            }
            //If the new password and confirm new password is not empty
            if (Newpwd != "" && ConfirmNewpwd != "")
            {
                if (Newpwd != ConfirmNewpwd)
                {
                    errorMsg += "New password does not match. Please try again.<br />";
                    errorCount += 1;
                }
                if (Newpwd == Oldpwd)
                {
                    errorMsg += "The new password cannot be the same as the old password. Please try again.<br />";
                    errorCount += 1;
                }
            }
            if (errorCount != 0)
            {
                ErrorMessage.Text = errorMsg;
                ErrorMessage.ForeColor = Color.Red;
            }
            else
            {
                ErrorMessage.Text = "";
                if (checkMinPasswordTime(userid) == false)
                {
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string pwdWithSalt = Oldpwd + dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);
                        if (userHash.Equals(dbHash))
                        {
                            HashNewPassword(Newpwd);
                            //If the new password is not the same as the last 2 passwords in the password history, then it is false 
                            if (VerifyNewPassword(userid, NewfinalHash) == false)
                            {
                                Get1stPassword(userid);
                                //Get the last used password in the password history and save it in the 2nd old password slot later
                                SecondOldPasswordHash = SecondfinalHash;
                                string SecondOldPasswordSalt = Secondsalt;
                                changePassword(userid, SecondOldPasswordHash, SecondOldPasswordSalt);
                                ErrorMessage.Text = "Password Changed";
                                ErrorMessage.ForeColor = Color.Green;
                            }
                            else
                            {
                                ErrorMessage.Text = "Cannot use the last 2 passwords. Please try again.";
                                ErrorMessage.ForeColor = Color.Red;
                            }
                            //Response.Redirect("192226P_MainPage.aspx", false);
                        }
                        else
                        {
                            ErrorMessage.Text = "Email or password is not valid. Please try again.";
                            ErrorMessage.ForeColor = Color.Red;
                        }
                    }
                }
                else
                {
                    ErrorMessage.Text = "Password change was too soon. Please wait for at least 5 minutes.";
                    ErrorMessage.ForeColor = Color.Red;
                }
            }
        }

        protected void changePassword(string userid, string SecondOldPwd, string SecondPwdSalt)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET PasswordHash=@NewPasswordHash, PasswordSalt=@PasswordSalt, FirstOldPasswordHash=@OldPasswordHash, FirstOldPasswordSalt=@FirstOldPasswordSalt, SecondOldPasswordHash=@SecondOldPasswordHash, SecondOldPasswordSalt=@SecondOldPasswordSalt, MinPasswordChange=@MinTime, MaxPasswordChange=@MaxTime where Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@NewPasswordHash", NewfinalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", Newsalt);
                            cmd.Parameters.AddWithValue("@OldPasswordHash", FirstfinalHash);
                            cmd.Parameters.AddWithValue("@FirstOldPasswordSalt", Firstsalt);
                            cmd.Parameters.AddWithValue("@SecondOldPasswordHash", SecondOldPwd);
                            cmd.Parameters.AddWithValue("@SecondOldPasswordSalt", SecondPwdSalt);
                            cmd.Parameters.AddWithValue("@MinTime", DateTime.Now.AddMinutes(5));
                            cmd.Parameters.AddWithValue("@MaxTime", DateTime.Now.AddMinutes(15));
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

        protected void HashNewPassword(string password)
        {
            //string pwd = get value from your Textbox
            string Newpwd = password;

            //Generate random "salt"
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] NewsaltByte = new byte[8];

            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(NewsaltByte);
            Newsalt = Convert.ToBase64String(NewsaltByte);

            SHA512Managed hashing = new SHA512Managed();

            string NewpwdWithSalt = Newpwd + Newsalt;
            byte[] NewhashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(NewpwdWithSalt));

            NewfinalHash = Convert.ToBase64String(NewhashWithSalt);

            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            NewKey = cipher.Key;
            NewIV = cipher.IV;
        }

        protected bool checkMinPasswordTime(string userid)
        {
            bool PasswordChange = false;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string LockOutTimingQuery = "select MinPasswordChange FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(LockOutTimingQuery, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        //This is to get the timing till the password can be changed.
                        DateTime PasswordChangeEndTime = Convert.ToDateTime(reader["MinPasswordChange"]);
                        DateTime NowTime = DateTime.Now;
                        //Check if the end password time has been passed
                        int LogoutValid = DateTime.Compare(PasswordChangeEndTime, NowTime);
                        if (LogoutValid >= 0)
                        {
                            PasswordChange = true;
                        }
                        else
                        {
                            //If the end lockout time has been passed, the lockout time has been passed
                            PasswordChange = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return PasswordChange;
        }

        //Get the hash, salt, IV and key from the 1st password to transfer to the 2nd password
        protected void Get1stPassword(string userid)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select FirstOldPasswordHash, FirstOldPasswordSalt FROM Accounts where Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["FirstOldPasswordHash"] != null)
                        {
                            if (reader["FirstOldPasswordHash"] != DBNull.Value)
                            {
                                SecondfinalHash = reader["FirstOldPasswordHash"].ToString();
                                Secondsalt = reader["FirstOldPasswordSalt"].ToString();

                            }
                            else
                            {
                                SecondfinalHash = "";
                                Secondsalt = "";
                            }
                        }
                        else
                        {
                            SecondfinalHash = ""; //firstpasswordHash = "";
                            Secondsalt = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
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

        protected bool VerifyNewPassword(string userid, string NewPwd)
        {
            string userID = userid;
            string NewPassword = NewPwd;
            //lockedOut to indicate if the new password is the same. Set to false first.
            bool PasswordSame = false;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string LockOutTimingQuery = "select FirstOldPasswordHash, FirstOldPasswordSalt, SecondOldPasswordHash, SecondOldPasswordSalt FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(LockOutTimingQuery, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        string FirstoldPwdHash = Convert.ToString(reader["FirstOldPasswordHash"]);
                        string FirstoldPwdSalt = Convert.ToString(reader["FirstOldPasswordSalt"]);
                        string SecondoldPwdHash = Convert.ToString(reader["SecondOldPasswordHash"]);
                        string SecondoldPwdSalt = Convert.ToString(reader["SecondOldPasswordSalt"]);
                        if (string.IsNullOrEmpty(FirstoldPwdHash))
                        {
                            PasswordSame = false;
                            PasswordError.Text = PasswordSame.ToString();
                        }
                        else
                        {
                            if (NewPassword.Equals(FirstoldPwdHash))
                            {
                                //If the New password Hash is the same as the 1st old password Hash, set the Password Same to true
                                PasswordSame = true;
                                PasswordError.Text = PasswordSame.ToString();
                            }
                            if (string.IsNullOrEmpty(SecondoldPwdHash)) 
                            {
                                PasswordSame = false;
                                PasswordError.Text = PasswordSame.ToString();
                            }
                            else
                            {
                                if (NewPassword.Equals(SecondoldPwdHash))
                                {
                                    //If the New password Hash is the same as the 2nd old password Hash, set the Password Same to true
                                    PasswordSame = true;
                                    PasswordError.Text = PasswordSame.ToString();
                                }
                                else
                                {
                                    PasswordSame = false;
                                    PasswordError.Text = PasswordSame.ToString();
                                }
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
            return PasswordSame;
        }
    }
}