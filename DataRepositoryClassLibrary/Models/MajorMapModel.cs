using SQLite;

namespace C424Assessment.Models
{
    public class MajorMapModel
    {
        
        [Column("StudentId"), NotNull, PrimaryKey]
        public int StudentId { get; set; }

        [Column("MajorId"), NotNull]
        public int MajorId { get; set; }

        [Column("StartDate"), NotNull]
        public DateTime StartDate { get; set; }

        public MajorMapModel()
        {
        }
    }
}
