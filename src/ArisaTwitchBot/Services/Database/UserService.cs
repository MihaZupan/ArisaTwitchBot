using ArisaTwitchBot.Database;
using ArisaTwitchBot.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ArisaTwitchBot.Services.Database
{
    public class UserService : ServiceBase
    {
        public UserService(ArisaTwitchClient arisaTwitchClient)
            : base(arisaTwitchClient, nameof(UserService))
        {
            _usernameUserIdMap = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _usersMap = new ConcurrentDictionary<string, User>(StringComparer.OrdinalIgnoreCase);

            using (var ctx = new UsersDbContext())
            {
                foreach (var user in ctx.Users)
                {
                    _usernameUserIdMap.TryAdd(
                        user.Username, user.UserId);

                    _usersMap.TryAdd(
                        user.UserId, new User(this, user));
                }
            }
        }

        private readonly ConcurrentDictionary<string, string> _usernameUserIdMap;
        private readonly ConcurrentDictionary<string, User> _usersMap;

        public bool TryGetUserById(string userId, out User user)
        {
            return _usersMap.TryGetValue(userId, out user);
        }
        public bool TryGetUserByUsername(string username, out User user)
        {
            if (_usernameUserIdMap.TryGetValue(username, out string userId))
            {
                return TryGetUserById(userId, out user);
            }
            else
            {
                user = default;
                return false;
            }
        }

        public void OnUserSpotted(string userId, string username)
        {
            if (_usernameUserIdMap.TryAdd(username, userId))
            {
                var userModel = new UserModel(userId, username, Constants.InitialUserBalance);
                _usersMap.TryAdd(userId, new User(this, userModel));
                using (var ctx = new UsersDbContext())
                {
                    ctx.Users.Add(userModel);
                    ctx.SaveChanges();
                }
            }
        }

        public void SaveChanges(string userId, long newBalance)
        {
            using (var ctx = new UsersDbContext())
            {
                UserModel user = ctx.Users.Single(b => b.UserId == userId);
                Debug.Assert(user.Balance != newBalance, "SaveChanges should only be called if changes were made");
                user.Balance = newBalance;
                ctx.SaveChanges();
            }
        }

        public List<User> GetAllUsersUnsafe()
        {
            return _usersMap.Values.ToList();
        }
    }
}
