namespace Interfaces.Models
{
    public class UserAuthentication
    {
        public UserAuthentication(string passwordSalt, string passwordHash)
        {
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
        }
        public int? Id { get; set; }

        public int? UserId { get; set; }
        public string PasswordSalt { get; }

        public string PasswordHash { get;  }
    }
}