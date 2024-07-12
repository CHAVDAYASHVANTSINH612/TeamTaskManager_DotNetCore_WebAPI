using Microsoft.Data.SqlClient;
using System.Data;
using TeamTaskManager_DotNet_WebAPI.Models;
using Dapper;

namespace TeamTaskManager_DotNet_WebAPI.Services
{
   public interface IUserRepository
   {
        Task<List<User>> getAllUsers();
        Task<User> getUserById(int userId);
        Task<Boolean> isAdminAlreadyExists();
        Task<int> addUser(User user);
        Task<bool> UserExistsById(int userId);
        Task<bool> deleteUserById(int userId);
   }

    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(string ConnectionString)
        {
            this._connectionString = ConnectionString;
        }

        public async Task<List<User>> getAllUsers()
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                   /* string sql = "SELECT u.id, u.name , u.user_type_id AS UserTypeId , ut.user_type AS UserType " +
                                         "FROM Users u JOIN UserType ut ON u.user_type_id = ut.user_type_id ";  */

                    List<User> userList = (await connection.QueryAsync<User>("GetAllUsers", commandType: CommandType.StoredProcedure)).ToList();
                    return userList;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> getUserById(int userId)
        {
            try
            {
                if (await this.UserExistsById(userId))
                {
                    using (IDbConnection connection = new SqlConnection(_connectionString))
                    {
                        /* string sqlJoin = "SELECT u.id,u.name,u.user_type_id AS UserTypeId,ut.user_type As UserType " +
                                          "FROM Users u JOIN UserType ut ON u.user_type_id = ut.user_type_id WHERE u.id=@user_id"; */

                        var parameters = new DynamicParameters();
                        parameters.Add("@UserId", userId);

                        var user = await connection.QueryFirstOrDefaultAsync<User>("GetUserById", parameters, commandType: CommandType.StoredProcedure);

                        return user;
                    }
                }
                else
                {
                    throw new Exception("something went wrong ! user may not exist");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Boolean> isAdminAlreadyExists()
        {
            bool isAdminAlreadyExistsVar = true;
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    /*string sqlAdminExist = "select Count(*) From Users WHERE user_type_id=1"; */


                    int admins = await connection.ExecuteScalarAsync<int>("CountAdminUsers", commandType: CommandType.StoredProcedure);
                    

                    if (admins > 0)
                    {
                        isAdminAlreadyExistsVar = true;
                        throw new Exception("Error: Admin already exists ! , there can not be more than one admin");
                    }
                    else
                    {
                        isAdminAlreadyExistsVar = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return isAdminAlreadyExistsVar;
        }


        public async Task<int> addUser(User user) // returns 0 for user could not added  ,-1 for admin already exists >0 for successfully added
        {
            try
            {
                int userId = 0;

                if (user.UserTypeId == 1 && (await this.isAdminAlreadyExists()))
                {
                    return -1;
                }

                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                   /* string insertSql = " INSERT INTO Users (name, user_type_id) " +
                                        "OUTPUT INSERTED.id  VALUES (@userName, @userTypeId);";   */

                    var parameters = new DynamicParameters();
                    parameters.Add("@Name", user.Name);
                    parameters.Add("@UserTypeId", user.UserTypeId);
                    parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("InsertUser", parameters, commandType: CommandType.StoredProcedure);

                     userId = parameters.Get<int>("@UserId");

                    if (userId > 0)
                    {
                        return userId;
                    }

                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UserExistsById(int userId)
        {
            int count = 0;
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                   /* string selectSql = "SELECT COUNT(*) FROM Users WHERE id = @UserId";   */

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    count = await connection.ExecuteScalarAsync<int>("CheckUserExistenceById", parameters, commandType: CommandType.StoredProcedure);

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> deleteUserById(int userId)
        {
            try
            {
                if (await this.UserExistsById(userId))
                {

                    int taskDeleted = 0, userDeleted = 0;
                    using (IDbConnection connection = new SqlConnection(_connectionString))
                    {
                        /* string DeleteTaskSql = "DELETE  FROM Tasks WHERE task_user_id = @UserId";
                         string deleteUserSql = "DELETE  FROM Users WHERE id = @UserId"; */


                        var parameters = new DynamicParameters();
                        parameters.Add("@UserId", userId);
                        parameters.Add("@TasksRowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        parameters.Add("@UserRowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync("DeleteUserAndTasks", parameters, commandType: CommandType.StoredProcedure);

                        int tasksRowsAffected = parameters.Get<int>("@TasksRowsAffected");
                        int userRowsAffected = parameters.Get<int>("@UserRowsAffected");
                        
                        return tasksRowsAffected > 0 || userRowsAffected > 0;
                    }

                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}

