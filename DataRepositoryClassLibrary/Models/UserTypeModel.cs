using SQLite;

namespace C424Assessment.Models
{
    public class UserTypeModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }
        
        [Column("TypeName")]
        [NotNull]
        public string TypeName { get; set; } = string.Empty;
        public UserTypeModel()
        {
        }

    }
}
