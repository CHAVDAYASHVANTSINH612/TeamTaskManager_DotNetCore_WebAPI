using Microsoft.AspNetCore.Mvc;
using TeamTaskManager_DotNet_WebAPI.Models;
using TeamTaskManager_DotNet_WebAPI.Services;

namespace TeamTaskManager_DotNet_WebAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class TeamTaskManagerController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;

        private static int visit_count = 0;
        public TeamTaskManagerController(IUserService userService, ITaskService taskService)
        {
            _userService = userService;
            _taskService = taskService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                List<User> allUsers = await _userService.GetAllUsers();

                if (allUsers != null && allUsers.Count > 0)
                {
                    return Ok(allUsers);
                }
                else
                {
                    return NotFound("list may be empty");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
        }


        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<User>> GetUserById([FromRoute] int userId)
        {
            try
            {
                User user = await _userService.getUserById(userId);

                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound("user not found with id : " + userId);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
        }



        [HttpPost("user")]
        public async Task<ActionResult<string>> AddUser([FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int generatedId = await this._userService.addUser(user);

                if (generatedId > 0)
                {
                    return NoContent();
                }
                else if (generatedId == -1)
                {
                    throw new Exception("admin already exsists you can not add second admin : " + generatedId);
                }
                else
                {
                    throw new Exception("user can not be added " + generatedId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("something went wrong : " + ex.Message);
            }

        }

        [HttpDelete("user")]
        public async Task<ActionResult> DeleteUserById(int userId)
        {
            try
            {
                Boolean isUserDeleted = await this._userService.deleteUserById(userId);

                if (isUserDeleted)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }

        }


        // task related requests:

        [HttpGet("tasks/{userId}")]

        public async Task<ActionResult<Tasks>> GetAllTaskByUserId([FromRoute] int userId)
        {
            try
            {
                List<Tasks> tasksList = (await _taskService.getAllTasksByUserId(userId));
                return Ok(tasksList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
        }


        [HttpGet("userwithtasks/{userId:int}")]
        public async Task<ActionResult<User>> GetUserWithTasks(int userId)
        {
            try
            {
                User userWithTasks = await _taskService.getUserWithTasksByUserId(userId);
                if (userWithTasks != null)
                {
                    return userWithTasks;
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("task")]
        public async Task<ActionResult> AddTask([FromBody] Tasks task)
        {  //returns: -1 user not found , -2 user is admin cannot add , 0 could not be added , >0 task added

            try
            {
                int task_id = await _taskService.addTask(task);

                if (task_id > 0)
                {
                    return NoContent();
                }
                else if (task_id == -1)
                {
                    return NotFound("user not found " + task_id);
                }
                else if (task_id == -2)
                {
                    return BadRequest("Admin cannot add task  " + task_id);
                }
                else
                {
                    return BadRequest("task could not be added");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
        }


        [HttpPut("task/{TaskId}")]
        public async Task<ActionResult> UpdateTaskStatusByTaskId([FromRoute] int TaskId, int updatedStatus)  // returns: -1 task not found , 0 could not be added , 1 changes done 
        {

            try
            {
                int resultStatus = await _taskService.UpdateTaskStatusByTaskId(TaskId, updatedStatus);

                if (resultStatus == 1)
                {
                    return NoContent();
                }
                else if (resultStatus == -1)
                {
                    return BadRequest("Task can not be found result: " + resultStatus);
                }
                else
                {
                    return StatusCode(500, "Task status could not be updated result: " + resultStatus);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("task/{taskId}")]
        public async Task<ActionResult> DeleteTaskByTaskId([FromRoute] int taskId)
        {
            try
            {
                bool isDeleted = await _taskService.DeleteTaskByTaskId(taskId);

                if (isDeleted)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Something went wrong ! task may not exist");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }

        }

    }
}
