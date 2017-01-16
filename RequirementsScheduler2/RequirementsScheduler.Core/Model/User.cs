using System.ComponentModel.DataAnnotations;

namespace RequirementsScheduler.Core.Model
{
    public sealed class User : IRepositoryModel
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
