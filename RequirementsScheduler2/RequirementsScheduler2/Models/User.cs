using System.ComponentModel.DataAnnotations;
using RequirementsScheduler2.Repository;

namespace RequirementsScheduler2.Models
{
    public class User : IRepositoryModel
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
