using ArisaTwitchBot.Database.Models;
using ArisaTwitchBot.Services;

namespace ArisaTwitchBot
{
    public class User
    {
        public readonly string UserId;
        public readonly string Username;
        public readonly Balance Balance;

        public User(UserService usersService, UserModel userModel)
        {
            UserId = userModel.UserId;
            Username = userModel.Username;
            Balance = new Balance(UserId, userModel.Balance, usersService);
        }
    }
}
