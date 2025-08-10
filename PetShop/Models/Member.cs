using System;
using System.ComponentModel.DataAnnotations;

namespace PetShop.Models
{
    public class Member
    {
        [Key]
        [Required]
        public string Account { get; set; }
        public string Password { get; set; }
        public string RealName { get; set; }
        public string Phone { get; set; }
        public int Birthyear { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpire { get; set; }
    }
}