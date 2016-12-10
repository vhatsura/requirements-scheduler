using System.ComponentModel.DataAnnotations;

namespace RequirementsScheduler2.Models
{
    public class User
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
