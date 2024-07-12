using Microsoft.Data.SqlClient;
using System.Data;
using TeamTaskManager_DotNet_WebAPI.Models;
using Dapper;

namespace TeamTaskManager_DotNet_WebAPI.Services
{

   public interface ITaskRepository
    {
        Task<List<Tasks>> getAllTasksByUserId(int userId);
        Task<int> addTask(Tasks task);
        Task<bool> isTaskExist(int taskId);
        Task<int> UpdateTaskStatusByTaskId(int taskId, int updatedStatus);
        Task<int> getTaskStatusByTaskId(int taskId);
        Task<bool> DeleteTaskByTaskId(int taskId);
    }


    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public TaskRepository( String ConnectionString)
        {
            this._connectionString =ConnectionString;
        }
        public async Task<List<Tasks>> getAllTasksByUserId(int userId)
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    /*string sqlJoiNTask = "SELECT t.id,t.title,t.content, t.modified, t.task_status_id AS TaskStatusId, ts.task_status AS TaskStatus, t.task_user_id AS UserId " +
                                        "FROM Tasks t JOIN TaskStatus ts ON t.task_status_id = ts.task_status_id WHERE task_user_id=@task_user_id;";   */

                    var parameters = new { UserId = userId };
                    List<Tasks> tasksList = (await connection.QueryAsync<Tasks>(
                                  "GetTasksByUserId",
                                  parameters,
                                  commandType: CommandType.StoredProcedure
                              )).ToList();
                    return tasksList;
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<int> addTask(Tasks task)
        {
            int generatedTaskId = 0;
            try
            {
                if (task != null)
                {
                    using (IDbConnection connection = new SqlConnection(_connectionString))
                    {
                        if (task.TaskStatusId < 1 || task.TaskStatusId > 3)
                        {
                            task.TaskStatusId = 1;
                        }

                        var parameters = new DynamicParameters();
                        parameters.Add("@Title", task.Title);
                        parameters.Add("@Content", task.Content);
                        parameters.Add("@TaskStatusId", task.TaskStatusId);
                        parameters.Add("@UserId", task.UserId);
                        parameters.Add("@GeneratedTaskId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        var result = await connection.QuerySingleAsync<int>(
                            "InsertTask",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        generatedTaskId = parameters.Get<int>("@GeneratedTaskId");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception();
            }
            return generatedTaskId;
        }

        public async Task<bool> isTaskExist(int taskId)
        {
            bool isTaskExistVar = false;
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TaskId", taskId);
                    parameters.Add("@Exists", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("CheckTaskExists", parameters, commandType: CommandType.StoredProcedure);

                    bool exists = parameters.Get<bool>("@Exists");
                    return exists;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return isTaskExistVar;
        }

        public async Task<int> UpdateTaskStatusByTaskId(int taskId, int updatedStatus)
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    // string sqlUpdateTaskStatus1 = "UPDATE Tasks SET task_status_id= @updated_status WHERE id=@task_id";

                    var parameters = new DynamicParameters();
                    parameters.Add("@TaskId", taskId);
                    parameters.Add("@UpdatedStatus", updatedStatus);
                    parameters.Add("@RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("UpdateTaskStatus", parameters, commandType: CommandType.StoredProcedure);

                    int rowsEffected = parameters.Get<int>("@RowsAffected");

                    return rowsEffected > 0 ? 1 : 0;
                }

            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return 0;
        }


        public async Task<int> getTaskStatusByTaskId(int taskId)
        {
            try
            {
                if (await isTaskExist(taskId))
                {
                    using (IDbConnection connection = new SqlConnection(_connectionString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TaskId", taskId);
                        parameters.Add("@TaskStatusId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync("GetTaskStatusId", parameters, commandType: CommandType.StoredProcedure);

                        int taskStatusId = parameters.Get<int>("@TaskStatusId");

                        return taskStatusId;
                    }
                }
                else
                {
                    throw new Exception("Error: task does not Exist");
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public async Task<bool> DeleteTaskByTaskId(int taskId)
        {
            try
            {
                int TaskStatusId = await this.getTaskStatusByTaskId(taskId);

                if (TaskStatusId == 1 || TaskStatusId == 3)
                {
                    using (IDbConnection connection = new SqlConnection(_connectionString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TaskId", taskId);
                        parameters.Add("@RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync("DeleteTask", parameters, commandType: CommandType.StoredProcedure);

                        int rowsEffected = parameters.Get<int>("@RowsAffected");

                        return rowsEffected > 0;
                    }
                }
                else
                {
                    throw new Exception("Error: task can not be deleted which is in progress");
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}
