using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace Login_and_Register_Page
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        string curUser;
        StringBuilder table;
        int accNum;
        int pageNum;
        int numPages;
        static string holdMonth;
        static string holdYear;
        string storedProcedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            curUser = Session["email"].ToString();
            System.Diagnostics.Debug.WriteLine(curUser);
            string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            Label1.Text = DateTime.Now.ToLongTimeString();
            //SqlCommand cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            storedProcedure = "getBalance";
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
            object bal = cmd.ExecuteScalar();
            if (bal != null)
            {
                double cur = Convert.ToDouble(bal);
                Console.WriteLine(cur);
                balance.Text = "$" + cur;
            }
            table = new StringBuilder();
            //cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
            storedProcedure = "getAccNum";
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
            object readAccNum = cmd.ExecuteScalar();
            accNum = Convert.ToInt32(readAccNum);
            storedProcedure = "showUsingPagingNew";
            //cmd = new SqlCommand("with selectRes as (select transactions.transact, transactions.amount, transactions.time from transactions where transactions.accNum = '"+accNum+"' union all select transfers.transact, transfers.amount, transfers.time from transfers where transfers.accNum = '"+accNum+"')select top 10 * from selectRes order by time desc", con);
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            if (Convert.ToInt64(Session["@PageNum"]) == 0)
            {
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = 1;
            }
            else
            {
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = Convert.ToInt64(Session["PageNum"]);
            }
            cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = accNum;
            holdMonth = Convert.ToString(Session["holdMonth"]);
            holdYear = Convert.ToString(Session["holdYear"]);
            cmd.Parameters.Add("@month", SqlDbType.VarChar).Value = holdMonth;
            cmd.Parameters.Add("@year", SqlDbType.VarChar).Value = holdYear;
            DataTable displayTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(displayTable);
            DataToHTML(displayTable,accNum);
            storedProcedure = "getPageCount";
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@accNum", SqlDbType.Int).Value=accNum;
            object NumPages = cmd.ExecuteScalar();
            numPages= (Convert.ToInt32(NumPages)-1) /20 + 1;
            if (Convert.ToString(Session["pageNum"]) == "")
            {
                pageNumber.InnerText = "1/" + numPages + "";
            }
            else
            {
                pageNumber.InnerText = "" + Convert.ToString(Session["pageNum"]) + "/" + numPages + "";
            }
            con.Close();
        }

        protected void depositAmount(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(dAmount.Text))
            {
                double value = Convert.ToDouble(dAmount.Text);
                double cur;
                double newBalance;
                if (value > 0)
                {
                    string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
                    SqlConnection con = new SqlConnection(ConnectionString);
                    con.Open();
                    //SqlCommand cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    storedProcedure = "getBalance";
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                    object bal = cmd.ExecuteScalar();
                    //double cur;
                    if (bal != null)
                    {
                        cur = Convert.ToDouble(bal);
                        Console.WriteLine(cur);
                        balance.Text = "$" + cur;
                        //cmd = new SqlCommand("UPDATE info SET balance='" + (cur + value) + "' WHERE email='" + curUser + "'", con);
                        storedProcedure = "updateBalanceAdd";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@addX", SqlDbType.Decimal).Value = cur;
                        cmd.Parameters.Add("@addY", SqlDbType.Decimal).Value = value;
                        cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                        cmd.ExecuteNonQuery();
                        //cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
                        storedProcedure = "getAccNum";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                        object readAccNum = cmd.ExecuteScalar();
                        accNum = Convert.ToInt32(readAccNum);
                        //cmd = new SqlCommand("insert into transactions(accNum,transact,amount,time)values('" + accNum + "','Deposit','" + value + "',CURRENT_TIMESTAMP)", con);
                        storedProcedure = "insertNewTrans";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter("@tablename", SqlDbType.VarChar)).Value = "transactionsNew";
                        cmd.Parameters.Add(new SqlParameter("@accNum", SqlDbType.Int)).Value = accNum;
                        cmd.Parameters.Add(new SqlParameter("@transact", SqlDbType.VarChar)).Value = "D";
                        cmd.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal)).Value = value;
                        cmd.Parameters.Add(new SqlParameter("@transfer", SqlDbType.Int)).Value = accNum;
                        cmd.ExecuteNonQuery();
                    }
                    //cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                    cmd = new SqlCommand();
                    cmd.Connection = con;
                    storedProcedure = "getBalance";
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                    object newBal = cmd.ExecuteScalar();
                    //double cur;
                    if (newBal != null)
                    {
                        newBalance = Convert.ToDouble(newBal);
                        Console.WriteLine(newBalance);
                        balance.Text = "$" + newBalance;
                    }
                    con.Close();
                }
            }
            dAmount.Text = "";
            refresh(sender, e);
        }

        protected void withdrawAmount(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(wAmount.Text))
            {
                double value = Convert.ToDouble(wAmount.Text);
                double cur;
                double newBalance;
                if (value > 0)
                {
                    string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
                    SqlConnection con = new SqlConnection(ConnectionString);
                    con.Open();
                    //SqlCommand cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    storedProcedure = "getBalance";
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                    object bal = cmd.ExecuteScalar();
                    //double cur;
                    if (bal != null)
                    {
                        cur = Convert.ToDouble(bal);
                        Console.WriteLine(cur);
                        balance.Text = "$" + cur;
                        if (cur - value >= 0)
                        {
                            insufficientFundsW.Visible = false;
                            insufficientFundsW.Attributes.CssStyle.Add("display", "none");
                            //cmd = new SqlCommand("UPDATE info SET balance='" + (cur - value) + "' WHERE email='" + curUser + "'", con);
                            storedProcedure = "updateBalanceSub";
                            cmd.CommandText = storedProcedure;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@subX", SqlDbType.Decimal).Value = cur;
                            cmd.Parameters.Add("@subY", SqlDbType.Decimal).Value = value;
                            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                            cmd.ExecuteNonQuery();
                            //cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
                            storedProcedure = "getAccNum";
                            cmd.CommandText = storedProcedure;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                            object readAccNum = cmd.ExecuteScalar();
                            accNum = Convert.ToInt32(readAccNum);
                            //cmd = new SqlCommand("insert into transactions(accNum,transact,amount,time)values('" + accNum + "','Withdraw','" + value + "',CURRENT_TIMESTAMP)", con);
                            storedProcedure = "insertNewTrans";
                            cmd.CommandText = storedProcedure;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter("@tablename", SqlDbType.VarChar)).Value = "transactionsNew";
                            cmd.Parameters.Add(new SqlParameter("@accNum", SqlDbType.Int)).Value = accNum;
                            cmd.Parameters.Add(new SqlParameter("@transact", SqlDbType.VarChar)).Value = "W";
                            cmd.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal)).Value = value;
                            cmd.Parameters.Add(new SqlParameter("@transfer", SqlDbType.Int)).Value = accNum;
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            insufficientFundsW.Visible = true;
                            insufficientFundsW.Attributes.CssStyle.Add("display", "normal");
                        }
                    }
                    cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                    object newBal = cmd.ExecuteScalar();
                    //double cur;
                    if (newBal != null)
                    {
                        newBalance = Convert.ToDouble(newBal);
                        Console.WriteLine(newBalance);
                        balance.Text = "$" + newBalance;
                    }
                    con.Close();
                }
            }
            wAmount.Text = "";
            refresh(sender, e);
        }

        protected void sendMoneyToUser(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(sendTo.Text) && !String.IsNullOrWhiteSpace(sendAmount.Text))
            {
                if (IsValidEmailId(sendTo.Text) && DomainExists(sendTo.Text) && sendTo.Text != curUser)
                {
                    bool emailExists = false;
                    string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
                    SqlConnection con = new SqlConnection(ConnectionString);
                    con.Open();
                    SqlDataReader dataReader;
                    SqlCommand cmd = new SqlCommand("select email from info", con);
                    dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine(dataReader.GetString(0));
                        if (dataReader.GetString(0) == sendTo.Text)
                        {
                            emailExists = true;
                        }
                    }
                    dataReader.Close();
                    if (emailExists)
                    {
                        double value = Convert.ToDouble(sendAmount.Text);
                        double newBalance;
                        double cur;
                        double otherBal;
                        invalidUser.Visible = false;
                        invalidUser.Attributes.CssStyle.Add("display", "none");
                        //cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                        cmd = new SqlCommand();
                        cmd.Connection = con;
                        storedProcedure = "getBalance";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                        object bal = cmd.ExecuteScalar();
                        //cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + sendTo.Text + "'", con);
                        cmd = new SqlCommand();
                        cmd.Connection = con;
                        storedProcedure = "getBalance";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = sendTo.Text;
                        object otherUser = cmd.ExecuteScalar();
                        if (bal != null && otherUser != null)
                        {
                            cur = Convert.ToDouble(bal);
                            Console.WriteLine(cur);
                            otherBal = Convert.ToDouble(otherUser);
                            balance.Text = "$" + cur;
                            if (cur - value >= 0)
                            {
                                insufficientFunds.Visible = false;
                                insufficientFunds.Attributes.CssStyle.Add("display", "none");
                                //cmd = new SqlCommand("UPDATE info SET balance='" + (cur - value) + "' WHERE email='" + curUser + "'", con);
                                storedProcedure = "updateBalanceSub";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@subX", SqlDbType.Decimal).Value = cur;
                                cmd.Parameters.Add("@subY", SqlDbType.Decimal).Value = value;
                                cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                                cmd.ExecuteNonQuery();
                                //cmd = new SqlCommand("UPDATE info SET balance='" + (otherBal + value) + "' WHERE email='" + sendTo.Text + "'", con);storedProcedure = "updateBalance";
                                storedProcedure = "updateBalanceAdd";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@addX", SqlDbType.Decimal).Value = otherBal;
                                cmd.Parameters.Add("@addY", SqlDbType.Decimal).Value = value;
                                cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = sendTo.Text;
                                cmd.ExecuteNonQuery();
                                //cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
                                storedProcedure = "getAccNum";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                                object readAccNum = cmd.ExecuteScalar();
                                accNum = Convert.ToInt32(readAccNum);
                                //cmd = new SqlCommand("insert into transactions(accNum,transact,amount,time)values('" + accNum + "','Sent to "+sendTo.Text+"','" + value + "',CURRENT_TIMESTAMP)", con);
                                storedProcedure = "getAccNum";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = sendTo.Text;
                                object receiverAccNum = cmd.ExecuteScalar();
                                receiverAccNum = Convert.ToInt32(receiverAccNum);
                                storedProcedure = "insertNewTrans";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("@tablename", SqlDbType.VarChar)).Value = "transactionsNew";
                                cmd.Parameters.Add(new SqlParameter("@accNum", SqlDbType.Int)).Value = accNum;
                                cmd.Parameters.Add(new SqlParameter("@transact", SqlDbType.VarChar)).Value = "TOut";
                                cmd.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal)).Value = value;
                                cmd.Parameters.Add(new SqlParameter("@transfer", SqlDbType.Int)).Value = receiverAccNum;
                                cmd.ExecuteNonQuery();
                                //
                                storedProcedure = "insertNewTrans";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("@tablename", SqlDbType.VarChar)).Value = "transactionsNew";
                                cmd.Parameters.Add(new SqlParameter("@accNum", SqlDbType.Int)).Value = receiverAccNum;
                                cmd.Parameters.Add(new SqlParameter("@transact", SqlDbType.VarChar)).Value = "TIn";
                                cmd.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal)).Value = value;
                                cmd.Parameters.Add(new SqlParameter("@transfer", SqlDbType.Int)).Value = accNum;
                                cmd.ExecuteNonQuery();
                                /*cmd = new SqlCommand("select accNum from info where email='" + sendTo.Text + "'", con);
                                storedProcedure = "getAccNum";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = sendTo.Text;
                                object receiverAccNum = cmd.ExecuteScalar();
                                //receiverAccNum = Convert.ToInt32(receiverAccNum);
                                //cmd = new SqlCommand("insert into transfers(accNum,transact,amount,time)values('" + Convert.ToInt32(receiverAccNum) + "','Sent from " + curUser + "','" + value + "',CURRENT_TIMESTAMP)", con);
                                storedProcedure = "insertNewTrans";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("@tablename", SqlDbType.VarChar)).Value = "transfers";
                                cmd.Parameters.Add(new SqlParameter("@accNum", SqlDbType.Int)).Value = Convert.ToInt32(receiverAccNum);
                                cmd.Parameters.Add(new SqlParameter("@transact", SqlDbType.VarChar)).Value = "Sent from " + curUser + "";
                                cmd.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal)).Value = value;
                                cmd.ExecuteNonQuery();*/
                            }
                            else
                            {
                                insufficientFunds.Attributes.CssStyle.Add("display", "normal");
                                insufficientFunds.Visible = true;
                            }
                        }
                        //cmd = new SqlCommand("SELECT balance FROM info WHERE email='" + curUser + "'", con);
                        cmd = new SqlCommand();
                        cmd.Connection = con;
                        storedProcedure = "getBalance";
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
                        object newBal = cmd.ExecuteScalar();
                        //double cur;
                        if (newBal != null)
                        {
                            newBalance = Convert.ToDouble(newBal);
                            Console.WriteLine(newBalance);
                            balance.Text = "$" + newBalance;
                        }
                    }
                    else
                    {
                        invalidUser.Attributes.CssStyle.Add("display", "normal");
                        invalidUser.Visible = true;
                    }
                    con.Close();
                }
            }
            sendTo.Text = "";
            sendAmount.Text = "";
            refresh(sender, e);
        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            Label1.Text = DateTime.Now.ToLongTimeString();
        }
















        protected void DataToHTML(DataTable displayTable,int accNum)
        {
            string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            displayTable.Columns["transact"].ColumnName = "Transactions";
            displayTable.Columns["amount"].ColumnName = "Amount";
            displayTable.Columns["time"].ColumnName = "Time";
            StringBuilder dT = new StringBuilder();
            dT.Append("<div style='overflow-y:auto;'>");
            dT.Append("<table id='dtHTML' border='1' runat='server'>");
            dT.Append("<thead>");
            dT.Append("<tr>");
            foreach (DataColumn tableCol in displayTable.Columns)
            {
                if (tableCol.ColumnName != "transfer" && tableCol.ColumnName != "filePath")
                {
                    dT.Append("<th>");
                    dT.Append(tableCol.ColumnName);
                    dT.Append("</th>");
                }
            }
            dT.Append("</tr>");
            dT.Append("</thead>");
            dT.Append("<tbody>");
            foreach (DataRow tableRow in displayTable.Rows)
            {
                dT.Append("<tr>");
                foreach (DataColumn matchCol in displayTable.Columns)
                {
                    if (matchCol.ColumnName != "transfer" && matchCol.ColumnName != "filePath")
                    {
                        dT.Append("<td>");
                        if (matchCol.ColumnName == "Transactions")
                        {
                            if (tableRow["Transactions"].ToString() == "D")
                            {
                                dT.Append("<img src=" + tableRow["filePath"].ToString() + " height=50px width=50px>");
                                dT.Append("Deposit");
                            }
                            if (tableRow["Transactions"].ToString() == "W")
                            {
                                dT.Append("<img src=" + tableRow["filePath"].ToString() + " height=50px width=50px>");
                                dT.Append("Withdraw");
                            }
                            if (tableRow["Transactions"].ToString() == "TOut")
                            {
                                cmd.CommandText = "getFilePath";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = accNum;
                                object filePath = cmd.ExecuteScalar().ToString();
                                dT.Append("<img src=" + filePath + " height =50px width=50px>");
                                storedProcedure = "getEmail";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = Convert.ToInt32(tableRow["transfer"]);
                                object receiverEmail = cmd.ExecuteScalar();
                                receiverEmail = receiverEmail.ToString();
                                dT.Append("Sent to " + receiverEmail);
                            }
                            if (tableRow["Transactions"].ToString() == "TIn")
                            {
                                dT.Append("<img src=" + tableRow["filePath"].ToString() + " height=50px width=50px>");
                                storedProcedure = "getEmail";
                                cmd.CommandText = storedProcedure;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = Convert.ToInt32(tableRow["transfer"]);
                                object senderEmail = cmd.ExecuteScalar();
                                senderEmail = senderEmail.ToString();
                                dT.Append("Sent from " + senderEmail);
                            }
                        }
                        else if (matchCol.ColumnName == "Amount")
                        {
                            if (tableRow["Transactions"].ToString() == "D")
                            {
                                dT.Append("+" + Convert.ToDecimal(tableRow[matchCol]).ToString("C2"));
                            }
                            else if (tableRow["Transactions"].ToString() == "W")
                            {
                                dT.Append("-" + Convert.ToDecimal(tableRow[matchCol]).ToString("C2"));
                            }
                            else if (tableRow["Transactions"].ToString() == "TOut")
                            {
                                dT.Append("-" + Convert.ToDecimal(tableRow[matchCol]).ToString("C2"));
                            }
                            else if (tableRow["Transactions"].ToString() == "TIn")
                            {
                                dT.Append("+" + Convert.ToDecimal(tableRow[matchCol]).ToString("C2"));
                            }
                            else
                            {
                                dT.Append("-" + Convert.ToDecimal(tableRow[matchCol]).ToString("C2"));
                            }
                        }
                        else if (matchCol.ColumnName == "Time")
                        {
                            dT.Append(Convert.ToDateTime(tableRow[matchCol]).ToString("g"));
                        }
                        else
                        {
                            dT.Append(tableRow[matchCol].ToString());
                        }

                        dT.Append("</td>");
                    }
                }
                dT.Append("</tr>");
            }
            dT.Append("</tbody>");
            dT.Append("</table>");
            dT.Append("</div>");
            PlaceHolder.Controls.Clear();
            PlaceHolder.Controls.Add(new Literal { Text = dT.ToString() });
            con.Close();
        }

        protected void refresh(object sender, EventArgs e)
        {
            string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            table = new StringBuilder();
            //SqlCommand cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
            storedProcedure = "getAccNum";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
            object readAccNum = cmd.ExecuteScalar();
            accNum = Convert.ToInt32(readAccNum);
            storedProcedure = "showUsingPagingNew";
            //cmd = new SqlCommand("with selectRes as (select transactions.transact, transactions.amount, transactions.time from transactions where transactions.accNum = '"+accNum+"' union all select transfers.transact, transfers.amount, transfers.time from transfers where transfers.accNum = '"+accNum+"')select top 10 * from selectRes order by time desc", con);
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            if (Convert.ToInt64(Session["@PageNum"]) == 0)
            {
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = 1;
            }
            else
            {
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = Convert.ToInt64(Session["PageNum"]);
            }
            //cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNum;
            cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = accNum;
            holdMonth = Convert.ToString(Session["holdMonth"]);
            holdYear = Convert.ToString(Session["holdYear"]);
            cmd.Parameters.Add("@month", SqlDbType.VarChar).Value = holdMonth;
            cmd.Parameters.Add("@year", SqlDbType.VarChar).Value = holdYear;
            DataTable displayTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(displayTable);
            DataToHTML(displayTable, accNum);
            pageNumber.InnerText = "" + Convert.ToString(Session["pageNum"]) + "/" + numPages + "";
            con.Close();
        }

        protected void viewAllTransactions(object sender, EventArgs e)
        {
            string ConnectionString = "Data Source=DESKTOP-UGAJVC1\\SQLEXPRESS;Initial Catalog=login;Integrated Security=True";
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            table = new StringBuilder();
            //SqlCommand cmd = new SqlCommand("select accNum from info where email='" + curUser + "'", con);
            storedProcedure = "getAccNum";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@curUser", SqlDbType.VarChar).Value = curUser;
            object readAccNum = cmd.ExecuteScalar();
            accNum = Convert.ToInt32(readAccNum);
            //cmd = new SqlCommand("select transactions.transact, transactions.amount, transactions.time from transactions where transactions.accNum = '"+accNum+"' union all select transfers.transact, transfers.amount, transfers.time from transfers where transfers.accNum = '"+accNum+"' order by time desc", con);
            storedProcedure = "showUsingPagingNew";
            cmd.CommandText = storedProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = 1;
            cmd.Parameters.Add("@accNum", SqlDbType.Int).Value = accNum;
            DateTime curDateTime = DateTime.Now;
            cmd.Parameters.Add("@month", SqlDbType.VarChar).Value = selectMonth.Value;
            cmd.Parameters.Add("@year", SqlDbType.VarChar).Value = selectYear.Value;
            DataTable fullTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(fullTable);
            if (selectMonth.Value == "" || selectYear.Value=="")
            {
                noResults.Visible = false;
                selectError.Visible = true;
                refresh(sender, e);
            }
            else if (fullTable.Rows.Count==0)
            {
                noResults.Visible = true;
                selectError.Visible = false;
            }
            else
            {
                Session["holdMonth"] = selectMonth.Value;
                Session["holdYear"] = selectYear.Value;
                DataToHTML(fullTable,accNum);
                noResults.Visible = false;
                selectError.Visible = false;
            }
            con.Close();
        }

        protected void NextRecords(object sender, EventArgs e)
        {
            if (Convert.ToInt32(Session["pageNum"]) == 0)
            {
                Session["pageNum"] = 1;
            }
            if (Convert.ToInt32(Session["pageNum"]) < numPages)
            {
                pageNum = Convert.ToInt32(Session["pageNum"]);
                pageNum += 1;
                Session["pageNum"] = pageNum;
                refresh(sender, e);
            }
        }

        protected void PreviousRecords(object sender, EventArgs e)
        {
            pageNum = Convert.ToInt32(Session["pageNum"]);
            if (pageNum > 1)
            {
                pageNum -= 1;
                Session["pageNum"] = pageNum;
            }
            refresh(sender, e);
        }


        //HTML functions
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

        protected void depositButton(object sender, EventArgs e)
        {
            dAmount.Visible = true;
            deposit.Visible = true;
            cancelD.Visible = true;
        }
        protected void withdrawButton(object sender, EventArgs e)
        {
            wAmount.Visible = true;
            withdraw.Visible = true;
            cancelW.Visible = true;
        }
        protected void sendButton(object sender, EventArgs e)
        {
            sendTo.Visible = true;
            sendAmount.Visible = true;
            sendMoney.Visible = true;
            cancelS.Visible = true;
        }

        protected void cancelDeposit(object sender, EventArgs e)
        {
            dAmount.Visible = false;
            deposit.Visible = false;
            cancelD.Visible = false;
        }
        protected void cancelWithdraw(object sender, EventArgs e)
        {
            wAmount.Visible = false;
            withdraw.Visible = false;
            cancelW.Visible = false;
            insufficientFundsW.Visible = false;
            insufficientFundsW.Attributes.CssStyle.Add("display", "none");
        }
        protected void cancelSend(object sender, EventArgs e)
        {
            sendTo.Visible = false;
            sendAmount.Visible = false;
            sendMoney.Visible = false;
            cancelS.Visible = false;
            invalidUser.Visible = false;
            invalidUser.Attributes.CssStyle.Add("display", "none");
            insufficientFunds.Visible = false;
            insufficientFunds.Attributes.CssStyle.Add("display", "none");
        }
    }
}





/*SqlDataReader transChart = cmd.ExecuteReader();
            table.Append("<table id='dataTable' border='1'>");
            table.Append("<tr><th>Transaction</th><th>Amount</th><th>Time</th>");
            table.Append("</tr>");
            if (transChart.HasRows)
            {
                while (transChart.Read())
                {
                    table.Append("<tr>");
                    table.Append("<td>" + transChart[0] + "</td>");
                    if (transChart[0].ToString() == "Deposit" || transChart[0].ToString().Contains("Sent from"))
                    {
                        table.Append("<td>" + "+$" + transChart[1].ToString() + "</td>");
                    }
                    else if (transChart[0].ToString() == "Withdraw" || transChart[0].ToString().Contains("Sent to"))
                    {
                        table.Append("<td>" + "-$" + transChart[1].ToString() + "</td>");
                    }
                    table.Append("<td>" + transChart[2] + "</td>");
                    table.Append("</tr>");

                }
            }
            table.Append("</table>");
            PlaceHolder1.Controls.Add(new Literal { Text = table.ToString() });
            transChart.Close();*/