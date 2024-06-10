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
        public TaskRepository(string ConnectionString)
        {
            this._connectionString = ConnectionString;
        }
        public async Task<List<Tasks>> getAllTasksByUserId(int userId)
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    string sqlJoiNTask = "SELECT t.id,t.title,t.content, t.modified, t.task_status_id AS TaskStatusId, ts.task_status AS TaskStatus, t.task_user_id AS UserId " +
                                        "FROM Tasks t JOIN TaskStatus ts ON t.task_status_id = ts.task_status_id WHERE task_user_id=@task_user_id;";

                    List<Tasks> tasksList = (await connection.QueryAsync<Tasks>(sqlJoiNTask, new { task_user_id = userId })).ToList();
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
                        string sqlInsertTask = "INSERT INTO Tasks OUTPUT INSERTED.id  VALUES " +
                              "(@title,@content,SYSUTCDATETIME(),@TaskStatusId,@UserId);";
                        if (task.TaskStatusId < 1 || task.TaskStatusId > 3)
                        {
                            task.TaskStatusId = 1;
                        }

                        generatedTaskId = await connection.ExecuteScalarAsync<int>(sqlInsertTask, task);
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
                    string sqlTaskExists = "SELECT COUNT(*) FROM Tasks WHERE id=@task_id";

                    int count = await connection.ExecuteScalarAsync<int>(sqlTaskExists, new { task_id = taskId });
                    return count > 0;
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
                    string sqlUpdateTaskStatus1 = "UPDATE Tasks SET task_status_id= @updated_status WHERE id=@task_id";
                    int rowsEffected = await connection.ExecuteAsync(sqlUpdateTaskStatus1, new { updated_status = updatedStatus, task_id = taskId });

                    if (rowsEffected > 0)
                    {
                        return 1;
                    }
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
                        string sqlTaskStatus = "SELECT task_status_id FROM Tasks WHERE id= @task_id; ";
                        int taskStatusId = await connection.ExecuteScalarAsync<int>(sqlTaskStatus, new { task_id = taskId });

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
                        string sqlDeleteTask = "DELETE FROM Tasks WHERE id= @task_id; ";
                        int rowsEffected = await connection.ExecuteAsync(sqlDeleteTask, new { task_id = taskId });

                        if (rowsEffected > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
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
