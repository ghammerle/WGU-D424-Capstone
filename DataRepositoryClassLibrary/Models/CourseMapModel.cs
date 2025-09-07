using SQLite;

namespace C424Assessment.Models
{
    /// <summary>
    /// Contains the details of a course assinged to a student
    /// </summary>
    public class CourseMapModel
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("CourseId"), NotNull]
        public int CourseId { get; set; }

        [Column("CourseStatusId"), NotNull]
        public int CourseStatusId { get; set; }

        [Column("StartDate"), NotNull]
        public DateTime StartDate { get; set; }

        [Column("EndDate"), NotNull]
        public DateTime EndDate { get; set; }

        [Column("NotificationsEnabled"), NotNull]
        public bool NotificationsEnabled { get; set; } = true;

        [Column("NotesId")]
        public int? NotesId { get; set; }

        [Column("UserId"), NotNull]
        public int UserId { get; set; }

        [Column("TermId")]
        public int TermId { get; set; }

        public CourseMapModel()
        {
        }
    }
}
