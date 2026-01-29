using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPlatform.Models
{
    //public class User
    //{
    //    public int Id { get; set; }
    //    public string Username { get; set; } = "";
    //    public string Role { get; set; } = "User";
    //}


    public class LoginDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RefreshDto
    {
        public string RefreshToken { get; set; } = "";
    }

    public class LogoutDto
    {
        public string RefreshToken { get; set; } = "";
    }
}
