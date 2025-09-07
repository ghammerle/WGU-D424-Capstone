using SQLite;

namespace C424Assessment.Models
{
    public class MajorModel
    {
       
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Major"), NotNull, Unique]
        public string Major { get; set; } = string.Empty;

        public MajorModel()
        {
        }
    }
}
