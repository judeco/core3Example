namespace Interfaces.Models
{
    public interface IUser
    {
        int? Id { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        AdditionalData? AdditionalData { get; set; }
    }

    public class UserProfile : IUser
    {
        public int? Id { get; set; }

        private string _username = string.Empty;

        public string Username
        {
            get => _username;
            set => _username = value ?? string.Empty;
        }

        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set => _password = value ?? string.Empty;
        }

        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set => _email = value ?? string.Empty;
        }

        public AdditionalData? AdditionalData { get; set; } = null;
    }
}