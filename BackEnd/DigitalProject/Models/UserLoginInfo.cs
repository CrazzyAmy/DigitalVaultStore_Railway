namespace DigitalProject.Models
{
    public class UserLoginInfo
    {
        public string Email { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }

        public UserLoginInfo(string email, string salt, string passwordHash)
        {
            Email = email;
            Salt = salt;
            PasswordHash = passwordHash;
        }
    }
}
