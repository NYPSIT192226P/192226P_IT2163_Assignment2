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

        protected void ChangePasswordBtn_Click(object sender, EventArgs e)
        {
            string SecondOldPassword = "";
            string Oldpwd = OldPassword_TB.Text.ToString().Trim();
            string userid = Email_TB.Text.ToString().Trim();
            string Newpwd = NewPassword_TB.Text.ToString().Trim();
            string ConfirmNewpwd = NewPasswordConfirm_TB.Text.ToString().Trim();
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(userid);
            string dbSalt = getDBSalt(userid);
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
                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                {
                    string pwdWithSalt = Oldpwd + dbSalt;
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                    string userHash = Convert.ToBase64String(hashWithSalt);
                    if (userHash.Equals(dbHash))
                    {
                        //If the new password is not the same as the last 2 passwords in the password history, then it is false 
                        if (VerifyNewPassword(userid, Newpwd) == false)
                        {
                            //Get the last used password in the password history and save it in the 2nd old password slot later
                            SecondOldPassword = Get1stPassword(userid);
                            changePassword(userid, Newpwd, Oldpwd, SecondOldPassword);
                        }
                        //Response.Redirect("192226P_MainPage.aspx", false);
                    }
                }
            }
        }

        protected void changePassword(string userid, string newPwd, string oldPwd, string SecondOldPwd)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET Password=@NewPassword, 1stOldPassword=@OldPassword, 2ndOldPassword=@SecondOldPassword where Email=@USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@NewPassword", newPwd);
                            cmd.Parameters.AddWithValue("@OldPassword", oldPwd);
                            cmd.Parameters.AddWithValue("@SecondOldPassword", SecondOldPwd);
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

        protected string Get1stPassword(string userid)
        {
            string firstpassword = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select 1stOldPassword FROM Accounts where Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["1stOldPassword"] != null)
                        {
                            if (reader["1stOldPassword"] != DBNull.Value)
                            {
                                firstpassword = reader["1stOldPassword"].ToString();
                            }
                        }
                        else
                        {
                            firstpassword = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return firstpassword;
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
            string LockOutTimingQuery = "select 1stOldPassword, 2ndOldPassword FROM Accounts WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(LockOutTimingQuery, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        string FirstoldPwd = Convert.ToString(reader["1stOldPassword"]);
                        string SecondoldPwd = Convert.ToString(reader["2ndOldPassword"]);
                        if (string.IsNullOrEmpty(FirstoldPwd))
                        {
                            PasswordSame = false;
                            PasswordError.Text = PasswordSame.ToString();
                        }
                        else
                        {
                            if (NewPassword == FirstoldPwd)
                            {
                                //If the New password is the same as the 1st old password, set the Password Same to true
                                PasswordSame = true;
                                PasswordError.Text = PasswordSame.ToString();
                            }
                            if (string.IsNullOrEmpty(SecondoldPwd))
                            {
                                PasswordSame = false;
                                PasswordError.Text = PasswordSame.ToString();
                            }
                            else
                            {
                                if (NewPassword == SecondoldPwd)
                                {
                                    //If the New password is the same as the 2nd old password, set the Password Same to true
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