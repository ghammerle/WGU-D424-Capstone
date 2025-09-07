using SQLite;

namespace C424Assessment.Models
{
    /// <summary>
    ///  Contains data specific to a course
    /// </summary>
    public class CourseModel
    {
        public CourseModel() { }
        public CourseModel(CourseModel course)
        {
            CourseId = course.CourseId;
            Name = course.Name;
            InstructorId = course.InstructorId;
            EstimatedCompletionTime = course.EstimatedCompletionTime;
            MajorId = course.MajorId;
        }

        [PrimaryKey, AutoIncrement]
        public int CourseId { get; set; }

        [Column("Name"), NotNull, Unique]
        public string Name { get; set; } = string.Empty;

        [NotNull]
        [Column("InstructorID")]
        public int InstructorId { get; set; }

        [Column("EstimatedCompetionTime")]
        public int? EstimatedCompletionTime { get; set; }

        [Column("MajorId")]
        public int? MajorId { get; set; }

    }
 }
