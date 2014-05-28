using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MiniJSON;
using System.Data.SqlClient;
using System.Data;

namespace WebApplication2
{
    /// <summary>
    /// Summary description for Smaple
    /// </summary>
    public class Smaple : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest _request = context.Request;
            HttpResponse _response = context.Response;
            context.Response.ContentType = "text/plain";

            string constr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;";
            List<string> _tables = GetTables(constr);
            Dictionary<string, object> _dict = Json.Deserialize(context.Request.Form["JSON"]) as Dictionary<string, object>;


            switch ((string)_dict["Service"])
            {
                case "CheckIfUserRegistered": CheckIfUserRegistered(context, (string)_dict["Email"]); break;
                case "DeleteUser": DeleteUser(context, (string)_dict["Email"]); break;
                case "SignUpFacebook": SignUpFacebook(context, _dict); break;
                case "UpdateUserCurrency": UpdateUserCurrency(context, (string)_dict["Email"], (string)_dict["VirtualCurrency"], (string)_dict["Action"]); break;
                case "CheckIfHaveCurrrency": CheckIfHaveCurrrency(context, (string)_dict["Email"], (string)_dict["VirtualCurrency"]); break;
            }

            //AddUser(constr, (string)_dict["01"], (string)_dict["02"]);


            //string s = "";

            //foreach (string a in _tables)
            //    s += a + System.Environment.NewLine;

           // context.Response.Write(s);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        private static List<string> GetTables(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");
                List<string> TableNames = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString());
                }
                connection.Close();
                return TableNames;
            }
        }

        private void CheckIfUserRegistered(HttpContext context,string _Email)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;"))
            {
               connection.Open();
               try
               {
                   using (SqlCommand sqlCommand = new SqlCommand("SELECT count(1) FROM GamyTechUsers WHERE Email = @Email", connection))
                   {
                       sqlCommand.Parameters.AddWithValue("@Email", _Email);
                       int userCount = (int)sqlCommand.ExecuteScalar();
                       connection.Close();

                       if (userCount == 1)
                       {
                           context.Response.Write("CheckIfUserRegistered = User Exist");
                       }
                       else
                       {
                           context.Response.Write("CheckIfUserRegistered = User not Exist");
                       }
                   }
               }
               catch
               {
                   context.Response.Write("CheckIfUserRegistered = User not Exist");
               }
            }
        }

        private void SignUpFacebook(HttpContext context, Dictionary<string, object> _ResultDictionary)
        {
            bool isSuccess = true;
            using (SqlConnection connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;"))
            {
                connection.Open();
                try
                {
                    string _sqlCommand = @"INSERT INTO GamyTechUsers(Email,UserName,FirstName,LastName,FacebookId,FacebookAccessToken,Password,VirtualCurrency,RealCurrency) 
                                           VALUES(@Email,@UserName,@FirstName,@LastName,@FacebookId,@FacebookAccessToken,@Password,@VirtualCurrency,@RealCurrency)";
                    using (SqlCommand command = new SqlCommand(_sqlCommand , connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Email", _ResultDictionary["Email"]));
                        command.Parameters.Add(new SqlParameter("@UserName", _ResultDictionary["UserName"]));
                        command.Parameters.Add(new SqlParameter("@FirstName", _ResultDictionary["FirstName"]));
                        command.Parameters.Add(new SqlParameter("@LastName", _ResultDictionary["LastName"]));
                        command.Parameters.Add(new SqlParameter("@FacebookId",int.Parse((string)_ResultDictionary["FacebookId"])));
                        command.Parameters.Add(new SqlParameter("@FacebookAccessToken", _ResultDictionary["FacebookAccessToken"]));
                        command.Parameters.Add(new SqlParameter("@Password", _ResultDictionary["Password"]));
                        command.Parameters.Add(new SqlParameter("@VirtualCurrency",int.Parse((string)_ResultDictionary["VirtualCurrency"])));
                        command.Parameters.Add(new SqlParameter("@RealCurrency",int.Parse((string)_ResultDictionary["RealCurrency"])));
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    isSuccess = false;
                }
                connection.Close();
            }

            if (isSuccess)
                context.Response.Write("SignUpFacebook " + isSuccess);
            else context.Response.Write("SignUpFacebook" + isSuccess);
        }

        private void UpdateUserCurrency(HttpContext context, string Email,string _VirtualCurrencyAmount,string _Action)
        {
            int _virtualCurrency = int.Parse(_VirtualCurrencyAmount);

            string commandText = "";

            using (SqlConnection connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;"))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(commandText, connection))
                    {
                        command.CommandText = "SELECT VirtualCurrency FROM GamyTechUsers WHERE Email = '" + Email.ToString() + "'";

                        int _currentVirtualCurrency = (int)command.ExecuteScalar();
                        if (_Action == "+")
                            _currentVirtualCurrency += _virtualCurrency;
                        else if (_Action == "-") 
                            _currentVirtualCurrency -= _virtualCurrency;

                        command.CommandText = "UPDATE GamyTechUsers SET VirtualCurrency= '" + _currentVirtualCurrency + "' WHERE Email = '" + Email.ToString() + "'";
                        command.ExecuteNonQuery();

                        context.Response.Write("UpdateUserCurrency - Currency updated to " + _currentVirtualCurrency.ToString());
                        //SqlDataAdapter adpt = new SqlDataAdapter("SELECT VirtualCurrency FROM GamyTechUsers WHERE Email =" + Email.ToString(), connection);
                        //DataTable dt = new DataTable();
                        //adpt.Fill(dt);
                    }
                }
                catch
                {
                    context.Response.Write("UpdateUserCurrency - Cant update");
                }
                connection.Close();
            }
        }

        private void CheckIfHaveCurrrency(HttpContext context, string Email, string _VirtualCurrencyAmount)
        {
            int _virtualCurrency = int.Parse(_VirtualCurrencyAmount);

            string commandText = "";

            using (SqlConnection connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;"))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(commandText, connection))
                    {
                        command.CommandText = "SELECT VirtualCurrency FROM GamyTechUsers WHERE Email = '" + Email.ToString() + "'";
                        int _currentVirtualCurrency = (int)command.ExecuteScalar();
                        if (_virtualCurrency <= _currentVirtualCurrency)
                            context.Response.Write("True");
                        else context.Response.Write("False");
                    }
                }
                catch
                {
                    context.Response.Write("False");
                }
                connection.Close();
            }
        }
        //For Debug Purpose
        private void DeleteUser(HttpContext context, string _Email)
        {
            bool isSuccess = true;
            using (SqlConnection connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\ServerDB.mdf;Integrated Security=True;User Instance=True;"))
            {
                connection.Open();

                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM GamyTechUsers WHERE Email = @Email";
                        cmd.Parameters.AddWithValue("@Email", _Email);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    isSuccess = false;
                }

                connection.Close();
            }

            if (isSuccess)
                context.Response.Write("DeleteUser " + isSuccess);
            else context.Response.Write("DeleteUser " + isSuccess);
        }
    }

}