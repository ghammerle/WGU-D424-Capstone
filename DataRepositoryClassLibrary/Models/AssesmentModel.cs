using SQLite;
using static C424Assessment.Models.Enumerations;

namespace C424Assessment.Models
{
    public class AssessmentModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Type")]
        public AssementType Type { get; set; }

        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime EndDate { get; set; }

        [Column("CourseId")]
        public int CourseId { get; set; }

        [Column("NotificationsEnabled")]
        public bool NotificationsEnabled { get; set; }
    }
}
