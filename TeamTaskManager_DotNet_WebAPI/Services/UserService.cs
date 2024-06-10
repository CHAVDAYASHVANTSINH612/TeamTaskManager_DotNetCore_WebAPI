using TeamTaskManager_DotNet_WebAPI.Models;

namespace TeamTaskManager_DotNet_WebAPI.Services
{
   public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User> getUserById(int userId);
        Task<bool> UserExistsById(int userId);
        Task<int> addUser(User user);
        Task<bool> deleteUserById(int userId);
    }

    public class UserService:IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository UserRepository)
        {
            this._userRepository = UserRepository;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _userRepository.getAllUsers();
        }

        public async Task<User> getUserById(int userId)
        {
            return await _userRepository.getUserById(userId);
        }

        public async Task<bool> UserExistsById(int userId)
        {
            return await _userRepository.UserExistsById(userId);
        }

        public async Task<int> addUser(User user) //  returns 0 user could not be added   id >0 if user added successfully , -1   if user is adding admin and admin already exists
        {
            int generatedUserId = await _userRepository.addUser(user);

            return generatedUserId;
        }

        public async Task<bool> deleteUserById(int userId)
        {
            return await _userRepository.deleteUserById(userId);
        }
    }
}
