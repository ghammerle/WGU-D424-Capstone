using SQLite;
using System.Collections.ObjectModel;


namespace C424Assessment.Models
{
    public class TermModel
    {
        [Column("Name"), NotNull]
        public string Name { get; set; } = string.Empty;
        [Column("StartDate")]
        public DateTime StartDate { get; set; }
        [Column("EndDate")]
        public DateTime EndDate { get; set; }
        [PrimaryKey, AutoIncrement]
        [Column("TermId")]
        public int TermId { get; set; }

        [Ignore]
        public ObservableCollection<CourseModel> Courses { get; set; } = new ();
    }
}
