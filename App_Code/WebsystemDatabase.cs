
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Web;

/// <summary>
/// vlastni namespace, ktery obsahuje zakladni tridy a funkce pro praci s databazi beh systemu
/// </summary>
/// 

namespace WebSystem
{

    public class Database
    {
        public static Boolean TableExists(string tableName, string connectionStringName)
        {
            //SELECT count(*) FROM abcdef_Users
            Boolean result = false;

            string command = "SELECT count(*) FROM " + tableName;

            string connString;

            connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            SqlConnection conn = new SqlConnection(connString);
            string sql = command;

            SqlCommand cmd = new SqlCommand(command, conn);

            conn.Open();

            String retVal;

            try
            {
                retVal = Convert.ToString(cmd.ExecuteScalar());
                result = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + " HResult: " + e.HResult);
                result = false;
                //throw;
            }

            conn.Close();
            return result;
        }


        public static DataTable getDataTableFromReader(string myCmd, string connStringName)
        {

            string connString = ReturnConnectionString(connStringName);

            try
            {
                //tvorime dotaz do DB
                string myCmdText = myCmd;
                //Debug.WriteLine(myCmd);

                //tvorime nove pripojeni do Db
                using (SqlConnection connDB = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand(myCmdText, connDB))
                {
                    connDB.Open();
                    SqlDataReader myPole = cmd.ExecuteReader();
                    DataTable myTable = new DataTable();
                    myTable.Load(myPole);
                    connDB.Close();
                    return myTable;
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public static Boolean ExecuteNonQuery(string myCommand, string ConnectionStringName)
        {
            string connString;
            int ret = 0;
            connString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connString))
            {
                SqlCommand sqlcommand = new SqlCommand(myCommand, connection);

                try
                {
                    ret = 1;
                    sqlcommand.Connection.Open();
                    ret = sqlcommand.ExecuteNonQuery();
                    sqlcommand.Connection.Close();
                    sqlcommand.Connection.Dispose();

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Chyba ExeNonQuery: " + e.Message);
                    ret = 0;
                    //throw;
                }
            }

            if (ret != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        //provádí Execute Scalar a vraci true, pokud je hodnota NULL
        public static bool isDBNull(string myCmd1, string ConnectionStringName)
        {
            string myConnectionString = ReturnConnectionString(ConnectionStringName);
            bool retval = false;

            try
            {
                using (SqlConnection connDB1 = new SqlConnection(myConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand(myCmd1, connDB1);
                    connDB1.Open();

                    if (DBNull.Value == cmd1.ExecuteScalar())
                    {

                        retval = true;
                    }


                    connDB1.Close();
                    return retval;

                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }


        //provádí Execute Scalar vracející Int
        public static int ExecuteScalarInt(string myCmd1, string ConnectionStringName)
        {
            string myConnectionString = ReturnConnectionString(ConnectionStringName);


            try
            {
                using (SqlConnection connDB1 = new SqlConnection(myConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand(myCmd1, connDB1);
                    connDB1.Open();

                    //int myValue;
                    int myValue = Convert.ToInt16(cmd1.ExecuteScalar());
                    connDB1.Close();
                    return myValue;

                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return 0;
            }
        }

        //provádí Execute Scalar vracející Int
        public static string ExecuteScalarStringOld(string myCmd1, string ConnectionStringName)
        {
            string myConnectionString = ReturnConnectionString(ConnectionStringName);


            try
            {
                using (SqlConnection connDB1 = new SqlConnection(myConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand(myCmd1, connDB1);
                    connDB1.Open();

                    var myValue = cmd1.ExecuteScalar().ToString();

                    if (String.IsNullOrEmpty(myValue) == false)
                    {
                        connDB1.Close();
                        return myValue;
                    }
                    else
                    {
                        return "Error executing function ExecuteScalarString";
                    }


                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return "Error executing ExecuteScalarString";
            }
        }


        public static string ExecuteScalarString(string sql, string ConnectionStringName)
        {
            string retVal = String.Empty;

            string myConnectionString = ReturnConnectionString(ConnectionStringName);
            SqlConnection connection = new SqlConnection(myConnectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();

            object myValue = command.ExecuteScalar();

            if (myValue == null)
            {
                retVal = String.Empty;
            }
            else
            {
                retVal = myValue.ToString();
            }

            connection.Close();

            return retVal;
        }






        //funkce vraci connection string podle zadaneho jmena
        public static string ReturnConnectionString(string ConnectionStringName)
        {
            string myCS = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            return myCS;
        }

        //funkce plni tabulku vysledky SQL prikazu
        public static DataTable GetDataTable(string myCmd, string ConnectionStringName)
        {

            string myConnectionString = ReturnConnectionString(ConnectionStringName);

            try
            {
                //tvorime dotaz do DB
                string myCmdText = myCmd;
                //Debug.WriteLine(myCmd);

                //tvorime nove pripojeni do Db
                using (SqlConnection connDB = new SqlConnection(myConnectionString))
                using (SqlCommand cmd = new SqlCommand(myCmdText, connDB))
                {
                    connDB.Open();
                    SqlDataReader myPole = cmd.ExecuteReader();
                    DataTable myTable = new DataTable();
                    myTable.Load(myPole);
                    connDB.Close();
                    return myTable;
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }


    }
}
