using SQLite;

namespace C424Assessment.Models
{
    public class UserModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("PasswordSalt")]
        public string PasswordSalt { get; set; } = string.Empty;

        [Column("UserType")]
        public int UserType { get; set; } 

        [Column("IsDefaultPasword")]
        public bool IsDefaultPassword { get; set; } = true;
        public UserModel()
        {
        }

    }
}
