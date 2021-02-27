using System.ComponentModel.DataAnnotations;

namespace SilkierQuartz.Models
{
    public class AuthenticateViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        public bool IsPersist { get; set; }

        public bool IsLoginError { get; set; }
    }
}