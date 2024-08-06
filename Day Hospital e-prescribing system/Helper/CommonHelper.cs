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

        public bool UserAlreadyExists(string username)
        {
            bool flag = false;
            string connectionString = _config["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM [User] WHERE Username = @Username";
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

        public User GetUserByUsername(string query, string username)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = (int)reader["UserID"],
                                Username = reader["Username"].ToString(),
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


        public Admin GetAdminByUsername(string query, string username)
        {
            Admin admin = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Admin
                            {
                                AdminID = (int)reader["AdminID"],
                                Username = reader["Username"].ToString(),
                                HashedPassword = reader["HashedPassword"].ToString(),
                                RoleId = (int)reader["RoleId"]
                            };
                        }
                    }
                }
            }

            return admin;
        }

        public Nurse GetNurseByUsername(string query, string username)
        {
            Nurse nurse = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nurse = new Nurse
                            {
                                NurseID = reader.GetInt32(reader.GetOrdinal("NurseID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            };
                        }
                    }
                }
            }

            return nurse;
        }

        public Surgeon GetSurgeonByUsername(string query, string username)
        {
            Surgeon surgeon = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            surgeon = new Surgeon
                            {
                                SurgeonID = reader.GetInt32(reader.GetOrdinal("SurgeonID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            };
                        }
                    }
                }
            }

            return surgeon;
        }

        public Pharmacist GetPharmacistByUsername(string query, string username)
        {
            Pharmacist pharmacist = null;

            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pharmacist = new Pharmacist
                            {
                                PharmacistID = reader.GetInt32(reader.GetOrdinal("PharmacistID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                // Add more fields here if necessary
                            };
                        }
                    }
                }
            }

            return pharmacist;
        }
        public Anaesthesiologist GetAnaesthesiologistByUsername(string query, string username)
        {
            Anaesthesiologist anaesthesiologist = null;

            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            anaesthesiologist = new Anaesthesiologist
                            {
                                AnaesthesiologistID = reader.GetInt32(reader.GetOrdinal("AnaesthesiologistID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                // Add more fields here if necessary
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



