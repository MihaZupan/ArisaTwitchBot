using System.ComponentModel.DataAnnotations;

namespace ArisaTwitchBot.Database.Models
{
    public class UserModel
    {
        [Key]
        public string UserId { get; set; }

        public string Username { get; set; }

        public long Balance { get; set; }

        public UserModel(string userId, string username, long balance)
        {
            UserId = userId;
            Username = username;
            Balance = balance;
        }
    }
}
