using System.ComponentModel.DataAnnotations;

namespace A3_G4.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }

        // Foreign key to a remote table entry
        public enum UserTypes { Member, Coach, Admin };
        public UserTypes UserType { get; set; }

        // Nullable. E.g User: UserId = 1, CoachId = NULL 
        public int? UserId { get; set; }
        public int? CoachId { get; set; }
    }
}
