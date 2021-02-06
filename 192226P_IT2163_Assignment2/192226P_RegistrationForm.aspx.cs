using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace _192226P_IT2163_Assignment2
{
    public partial class _192226P_RegistrationForm : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        DateTime LockoutTimeStart = DateTime.Now.AddMinutes(-10);
        DateTime MinPasswordTime = DateTime.Now.AddMinutes(5);
        DateTime MaxPasswordTime = DateTime.Now.AddMinutes(15);
        int AttemptStartNo = 0;
        byte[] Key;
        byte[] IV;
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
            ("https://www.google.com//recaptcha/api/siteverify?secret= &response=" + captchaResponse);

            try
            {

                //Codes to receive the Response in JSON format from Google Server
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        //The response in JSON format
                        string jsonResponse = readStream.ReadToEnd();

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

        private Boolean validateInputs()
        {
            int invalidCount = 0;

            bool validation = false;

            if (String.IsNullOrEmpty(FName_TB.Text))
            {
                lbl_FNameCheck.Text = "First name must not be empty";
                lbl_FNameCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_FNameCheck.Text = null;
            if (String.IsNullOrEmpty(LName_TB.Text))
            {
                lbl_LNameCheck.Text = "Last name must not be empty";
                lbl_LNameCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_LNameCheck.Text = null;
            if (String.IsNullOrEmpty(CreditCard_TB.Text))
            {
                lbl_CreditCheck.Text = "Credit Card info must not be empty";
                lbl_CreditCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_CreditCheck.Text = null;
            if (String.IsNullOrEmpty(Email_TB.Text))
            {
                lbl_EmailCheck.Text = "Email must not be empty";
                lbl_EmailCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_EmailCheck.Text = null;
            if (String.IsNullOrEmpty(Number_TB.Text))
            {
                lbl_MobileCheck.Text = "Mobile number must not be empty";
                lbl_MobileCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_MobileCheck.Text = null;
            if (String.IsNullOrEmpty(Password_TB.Text))
            {
                lbl_passwordCheck.Text = "Password must not be empty";
                lbl_passwordCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_passwordCheck.Text = null;
            if (String.IsNullOrEmpty(DOB_TB.Text))
            {
                lbl_DOBCheck.Text = "Date of birth must not be empty";
                lbl_DOBCheck.ForeColor = Color.Red;
                invalidCount += 1;
            }
            else
                lbl_DOBCheck.Text = null;

            if (invalidCount != 0)
            {
                validation = false;
            }
            else if (invalidCount == 0)
            {
                validation = true;
            }
            return validation;
        }

        private void checkPassword(string password)
        {

            string ErrMsg = "";

            var ErrorMessage = new Dictionary<int, string>()
                    {
                        {0, "Password length must be more than 8 characters long" },
                        {1, "Password should have special characters" },
                        {2, "Password should have numbers" },
                        {3, "Password should have capital letters" },
                        {4, "Password should have small letters" }
                    };
            //Score 1 is very week!
            // if length of password is less than 8 chars
            if (password.Length < 8)
            {
                //Returns all of the error messages
                //err_msg.Visible = true;
                //if (Regex.IsMatch(password, "[a-z]"))
                //{
                //    ErrorMessage.RemoveAt(4);
                //}
                //// Score 3 Medium
                //if (Regex.IsMatch(password, "[A-Z]"))
                //{
                //    ErrorMessage.RemoveAt(3);
                //}
                //// Score 4 Strong
                //if (Regex.IsMatch(password, "[0-9]"))
                //{
                //    ErrorMessage.RemoveAt(2);
                //}
                //// Score 5 Excellent
                //if (Regex.IsMatch(password, "[^A-Za-z0-9]"))
                //{
                //    ErrorMessage.RemoveAt(1);
                //}
                //for (int i = 0; i < ErrorMessage.Count(); i++)
                //{
                //    ErrMsg += ErrorMessage[i] + "<br />";
                //}
                //error_msg.Text = ErrMsg;

                //This will just return one error message
                ErrorMessage.Remove(4);
                ErrorMessage.Remove(3);
                ErrorMessage.Remove(2);
                ErrorMessage.Remove(1);
                err_msg.Visible = true;
                passworderror_msg.Text = ErrorMessage[0];
            }
            else
            {
                ErrorMessage.Remove(0);
            }
            // Score 2 Weak
            if (Regex.IsMatch(password, "[a-z]"))
            {
                ErrorMessage.Remove(4);
            }
            // Score 3 Medium
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                ErrorMessage.Remove(3);
            }
            // Score 4 Strong
            if (Regex.IsMatch(password, "[0-9]"))
            {
                ErrorMessage.Remove(2);
            }
            // Score 5 Excellent
            if (Regex.IsMatch(password, "[^A-Za-z0-9]"))
            {
                ErrorMessage.Remove(1);
            }
            if (ErrorMessage.Count == 0)
            {
                err_msg.Visible = false;
            }
            else
            {
                err_msg.Visible = true;
            }

            var errorList = ErrorMessage.ToList();

            for (int i = 0; i < ErrorMessage.Count(); i++)
            {
                ErrMsg += (errorList[i].Value + "<br />");
            }
            
            passworderror_msg.Text = ErrMsg;
        }

        protected void submit_btm_Click(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                bool validated = validateInputs();

                if (validated == true)
                {
                    //string pwd = get value from your Textbox
                    string pwd = HttpUtility.HtmlEncode(Password_TB.Text.ToString().Trim()); ;

                    //Generate random "salt"
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    byte[] saltByte = new byte[8];

                    //Fills array of bytes with a cryptographically strong sequence of random values.
                    rng.GetBytes(saltByte);
                    salt = Convert.ToBase64String(saltByte);

                    SHA512Managed hashing = new SHA512Managed();

                    string pwdWithSalt = pwd + salt;
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                    finalHash = Convert.ToBase64String(hashWithSalt);

                    RijndaelManaged cipher = new RijndaelManaged();
                    cipher.GenerateKey();
                    Key = cipher.Key;
                    IV = cipher.IV;

                    createAccount();
                    //Go to the login from if successful
                    Response.Redirect("192226P_LoginForm.aspx", false);
                }
                else
                {
                    error_msg.Text = "Account cannot be created. Please try again.";
                    error_msg.ForeColor = Color.Red;
                }

            }
        }

        public void createAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Accounts VALUES(@FName, @LName, @CreditCard, @Email, @Mobile, @PasswordHash, @PasswordSalt, @DOB, @MobileVerified, @EmailVerified, @IV, @Key,@Attempt,@LockoutTime,@FirstOldPasswordHash,@FirstOldPasswordSalt,@SecondOldPasswordHash,@SecondOldPasswordSalt,@MinPasswordChange,@MaxPasswordChange)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FName", HttpUtility.HtmlEncode(FName_TB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@LName", HttpUtility.HtmlEncode(LName_TB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@CreditCard", Convert.ToBase64String(encryptData(CreditCard_TB.Text.Trim())));
                            cmd.Parameters.AddWithValue("@Email", HttpUtility.HtmlEncode(Email_TB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Mobile", HttpUtility.HtmlEncode(Number_TB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@DOB", HttpUtility.HtmlEncode(DOB_TB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@MobileVerified", DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmailVerified", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@Attempt", AttemptStartNo);
                            cmd.Parameters.AddWithValue("@LockoutTime", LockoutTimeStart);
                            cmd.Parameters.AddWithValue("@FirstOldPasswordHash", DBNull.Value);
                            cmd.Parameters.AddWithValue("@FirstOldPasswordSalt", DBNull.Value);
                            cmd.Parameters.AddWithValue("@SecondOldPasswordHash", DBNull.Value);
                            cmd.Parameters.AddWithValue("@SecondOldPasswordSalt", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MinPasswordChange", MinPasswordTime);
                            cmd.Parameters.AddWithValue("@MaxPasswordChange", MaxPasswordTime);
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
        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
               plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

        protected void passwordImproveBtn_Click(object sender, EventArgs e)
        {
            checkPassword(Password_TB.Text);
        }
    }
}
