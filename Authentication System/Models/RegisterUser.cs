using System.ComponentModel.DataAnnotations;

namespace Authentication_System.Model
{
    public class RegisterUserRequest
    {
        [Required(ErrorMessage = "UserName Is Mandatory")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password Is Mandatory")]
        public string PassWord { get; set; }


        [Required(ErrorMessage = "Role Is Mandatory")]
        public string Role { get; set; }
    }

    public class RegisterUserResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
