using System.ComponentModel.DataAnnotations;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class User
    {
        public int Id { get; set; }
        public bool IsAdmin => Role == "admin";
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public string Role { get; set; }
    }
}
