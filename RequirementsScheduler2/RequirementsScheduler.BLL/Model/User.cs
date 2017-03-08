using System.ComponentModel.DataAnnotations;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class User
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
