using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using TeamTaskManager_DotNet_WebAPI.Models;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;



namespace TeamTaskManager_DotNet_WebAPI.Services
{
   public interface ITaskService
    {
        Task<List<Tasks>> getAllTasksByUserId(int userId);
        Task<User> getUserWithTasksByUserId(int userId);
        Task<int> addTask(Tasks task);
        Task<int> UpdateTaskStatusByTaskId(int taskId, int updatedStatusId);
        Task<bool> DeleteTaskByTaskId(int taskId);
    }
    
    public class TaskService : ITaskService 
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserService _userService;

        public TaskService(ITaskRepository taskRepository, IUserService userService)
        {
            this._taskRepository = taskRepository;
            this._userService = userService;
        }

        public async Task<List<Tasks>> getAllTasksByUserId(int userId)
        {
            List<Tasks> taskList = null;

            if (await _userService.UserExistsById(userId))
            {
                taskList = await _taskRepository.getAllTasksByUserId(userId);
                return taskList;
            }
            else
            {
                throw new Exception("user not exist");
            }

            return taskList;
        }

        public async Task<User> getUserWithTasksByUserId(int userId)
        {
            User userWithTasks = await _userService.getUserById(userId);

            if (userWithTasks != null)
            {
                List<Tasks> TasksOfUser = await _taskRepository.getAllTasksByUserId(userWithTasks.Id);

                userWithTasks.TaskList = TasksOfUser;
            }

            return userWithTasks;

        }


        public async Task<int> addTask(Tasks task)   // -1 user not found , -2 user is admin cannot add Task , 0 could not be added , >0 task added
        {
            User user = await _userService.getUserById(task.UserId);

            if (user == null)
            {
                return -1;
            }
            else if (user.UserTypeId == 1)
            {
                return -2;
            }
            else
            {
                return await _taskRepository.addTask(task);
            }
            return 0;
        }

        public async Task<int> UpdateTaskStatusByTaskId(int taskId, int updatedStatusId)   // returns: -1 task not found , 0 could not be added , 1 changes done 
        {

            if (await _taskRepository.isTaskExist(taskId))
            {
                return await _taskRepository.UpdateTaskStatusByTaskId(taskId, updatedStatusId);
            }
            else
            {
                return -1;
            }
        }

        public async Task<bool> DeleteTaskByTaskId(int taskId)
        {
            return await _taskRepository.DeleteTaskByTaskId(taskId);
        }

    }
}
