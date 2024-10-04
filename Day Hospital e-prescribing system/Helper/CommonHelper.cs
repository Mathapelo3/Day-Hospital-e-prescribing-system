using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public bool UserAlreadyExists(string email)
        {
            bool flag = false;
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM [User] WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
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

        public User GetUserByEmail(string query, string email)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = (int)reader["UserID"],
                                Username = reader["Username"].ToString(),  // Retrieve Username
                                Email = reader["Email"].ToString(),
                                HashedPassword = reader["HashedPassword"].ToString(),
                                RoleId = (int)reader["RoleId"]
                            };
                        }
                    }
                }
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


        public Admin GetAdminByEmail(string query, string email)
        {
            Admin admin = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email); // Add @ symbol for parameter
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Admin
                            {
                                AdminID = (int)reader["AdminID"],
                                Username = reader["Username"].ToString(),  // Retrieve Username
                                Email = reader["Email"].ToString(),
                                HashedPassword = reader["HashedPassword"].ToString(),
                                RoleId = (int)reader["RoleId"]
                            };
                        }
                    }
                }
            }

            return admin;
        }

        public NurseWithUserDetails GetNurseByEmail(string query, string email)
        {
            NurseWithUserDetails nurseDetails = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nurseDetails = new NurseWithUserDetails
                            {
                                NurseID = reader.GetInt32(reader.GetOrdinal("NurseID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader["Username"].ToString()
                            };
                        }
                    }
                }
            }

            return nurseDetails;
        }

        public SurgeonWithUserDetails GetSurgeonByEmail(string query, string email)
        {
            SurgeonWithUserDetails surgeonDetails = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            surgeonDetails = new SurgeonWithUserDetails
                            {
                                SurgeonID = reader.GetInt32(reader.GetOrdinal("SurgeonID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                                Surname = reader["Surname"].ToString()
                            };
                        }
                    }
                }
            }

            return surgeonDetails;
        }

        public PharmacistWithUserDetails GetPharmacistByEmail(string query, string email)
        {
            PharmacistWithUserDetails pharmacistDetails = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pharmacistDetails = new PharmacistWithUserDetails
                            {
                                PharmacistID = reader.GetInt32(reader.GetOrdinal("PharmacistID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader["Username"].ToString()
                            };
                        }
                    }
                }
            }

            return pharmacistDetails;
        }
        public AnaesthesiologistWithUserDetails GetAnaesthesiologistByEmail(string query, string email)
        {
            AnaesthesiologistWithUserDetails anaesthesiologist = null;

            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            anaesthesiologist = new AnaesthesiologistWithUserDetails
                            {
                                AnaesthesiologistID = reader.GetInt32(reader.GetOrdinal("AnaesthesiologistID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                                Surname = reader["Surname"].ToString()
                            };
                        }
                    }
                }
            }

            return anaesthesiologist;
        }





        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }




    }



