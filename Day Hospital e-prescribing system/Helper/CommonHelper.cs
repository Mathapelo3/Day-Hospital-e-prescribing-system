using Day_Hospital_e_prescribing_system.ViewModel;
using System.Data.SqlClient;

namespace Day_Hospital_e_prescribing_system.Helper
{
    public class CommonHelper
    {
        private IConfiguration _config;

        public CommonHelper(IConfiguration config)
        {
            _config = config;
        }

        public int DMLTransaction(string Query)
        {
            int Result;
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = Query;
                SqlCommand command = new SqlCommand(sql, connection);
                Result = command.ExecuteNonQuery();
                connection.Close();
            }
            return Result;
        }

        public bool UserAlreadyExists(string username)
        {
            bool flag = false;
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM [UserTbl] WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader rd = command.ExecuteReader())
                    {
                        if (rd.HasRows)
                        {
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        public UserViewModel GetUserByUsername(string query, string username)
        {
            UserViewModel user = null;
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        user = new UserViewModel
                        {
                            UserID = Convert.ToInt32 (dataReader["UserID"].ToString()),
                            Username = dataReader["Username"].ToString(),
                            Password = dataReader["Password"].ToString(),
                            RoleId = Convert.ToInt32(dataReader["RoleId"].ToString())
                        };
                    }
                }

                connection.Close();
            }

            return user;
        }


        public SingleEntity GetEntityById(string query, string roleId)
        {
            SingleEntity singleEntity = new SingleEntity();
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoleId", roleId); // Assuming you're using roleId as a parameter

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        singleEntity.Id = Convert.ToInt32(dataReader["RoleId"].ToString());
                        singleEntity.Name = dataReader["Name"].ToString();

                    }
                }
                connection.Close();
            }
            return singleEntity;
        }


    }
}
