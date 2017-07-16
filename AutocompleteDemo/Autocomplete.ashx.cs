using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;

namespace AutocompleteDemo
{
    /// <summary>
    /// Summary description for Autocomplete
    /// </summary>
    public class Autocomplete : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // Custom variables
            const string columnName = "FirstName";
            const string tableName = "Names";
            const int resultsToDisplay = 10;
            string sqlStatement = "SELECT DISTINCT TOP " + resultsToDisplay + " " + columnName + " FROM " + tableName + " WHERE " + columnName + " LIKE @term + '%' ORDER BY " + columnName + " ASC";
            
            // Create DataTable of results
            context.Response.ContentType = "application/javascript";
            DataTable dataTable = ExecuteStatement(sqlStatement, context.Request.QueryString["term"]);

            // Add DataTable items to ArrayList
            ArrayList items = new ArrayList();
            foreach (DataRow row in dataTable.Rows)
            {
                items.Add(row[columnName]);
            }

            // Convert the ArrayList to a string array, serialise to Javascript and write back
            context.Response.Write(new JavaScriptSerializer().Serialize(items.ToArray(typeof(string))));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Method which returns the database connection
        /// </summary>
        /// <returns>An SqlConnection object</returns>
        private static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SampleDatabase"].ConnectionString;

            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Method which executes an SQL query on the database and returns a DataTable
        /// </summary>
        /// <param name="sqlStatement">The SQL command to execute</param>
        /// <param name="term">The search term to use</param>
        /// <returns>The return from the query as a DataTable</returns>
        private static DataTable ExecuteStatement(string sqlStatement, string term)
        {
            using (SqlConnection sqlConnection = GetConnection())
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(sqlStatement, sqlConnection))
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                using (DataTable dataTable = new DataTable())
                {
                    sqlCommand.Parameters.AddWithValue("@term", term);
                    sqlDataAdapter.Fill(dataTable);

                    return dataTable;
                }
            }
        }
    }
}