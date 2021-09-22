using System;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;


namespace Login_and_Register_Page
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        string storedProcedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            Label1.Text = DateTime.Now.ToLongTimeString();
        }

        private bool IsValidEmailId(string inputEmail)
        {
            try
            {
                var m = new MailAddress(inputEmail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool DomainExists(string input)
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(input.Substring(input.IndexOf("@") + 1));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void buttonClicked(object sender, EventArgs args)
        {
            string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
            bool emailExists = false;
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            SqlDataReader dataReader;
            SqlCommand cmd = new SqlCommand("select email from info", con);
            dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                System.Diagnostics.Debug.WriteLine(dataReader.GetString(0));
                if (dataReader.GetString(0) == email.Text)
                {
                    emailExists = true;
                }
            }
            dataReader.Close();
            {
                if (emailExists == false)
                {
                    alreadyExists.Visible = false;
                    alreadyExists.Attributes.CssStyle.Add("display", "none");
                    if (password.Text != "" && passwordConfirm.Text != "")
                    {
                        missingFields.Visible = false;
                        missingFields.Attributes.CssStyle.Add("display", "none");
                        if (password.Text == passwordConfirm.Text)
                        {
                            passwordDontMatch.Visible = false;
                            passwordDontMatch.Attributes.CssStyle.Add("display", "none");
                            if (firstName.Text != "" && lastName.Text != "" && email.Text != "" && FileUpload1.HasFile)
                            {
                                missingFields.Visible = false;
                                missingFields.Attributes.CssStyle.Add("display", "none");
                                firstName.Text.Trim();
                                lastName.Text.Trim();
                                email.Text.Trim();
                                storedProcedure = "insertNewAcc";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                //cmd = new SqlCommand("insert into info(first,last,email,password,confirmpass,balance)values('" + firstName.Text + "','" + lastName.Text + "','" + email.Text + "','" + password.Text + "','" + passwordConfirm.Text + "','" + 0 + "')", con);
                                cmd.Parameters.Add("@first", SqlDbType.VarChar).Value = firstName.Text;
                                cmd.Parameters.Add("@last", SqlDbType.VarChar).Value = lastName.Text;
                                cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email.Text;
                                cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = password.Text;
                                cmd.Parameters.Add("@confirmpass", SqlDbType.VarChar).Value = passwordConfirm.Text;
                                cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = 0;
                                string fileName = Path.GetFileName(FileUpload1.PostedFile.FileName);
                                string filePath = "/Uploads/" + fileName;
                                FileUpload1.PostedFile.SaveAs(Server.MapPath(filePath));
                                cmd.Parameters.Add("@filePath", SqlDbType.VarChar).Value = filePath;
                                cmd.ExecuteNonQuery();
                                Server.Transfer("SignIn.aspx");
                                firstName.Text = "";
                                lastName.Text = "";
                                email.Text = "";
                                password.Text = "";
                                passwordConfirm.Text = "";
                            }
                            else
                            {
                                missingFields.Visible = true;
                                missingFields.Attributes.CssStyle.Add("display", "normal");
                            }
                        }
                        else
                        {
                            passwordDontMatch.Visible = true;
                            passwordDontMatch.Attributes.CssStyle.Add("display", "normal");
                        }
                    }
                    else
                    {
                        missingFields.Visible = true;
                        missingFields.Attributes.CssStyle.Add("display", "normal");
                    }
                }
                else
                {
                    alreadyExists.Visible = true;
                    alreadyExists.Attributes.CssStyle.Add("display", "normal");
                }
            }
            con.Close();
        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            Label1.Text = DateTime.Now.ToLongTimeString();
        }
    }
}