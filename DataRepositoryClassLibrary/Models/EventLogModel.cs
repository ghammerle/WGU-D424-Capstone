using SQLite;

namespace C424Assessment.Models
{
    public class EventLogModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("EventDescription")]
        public string EventDescription { get; set; } = string.Empty;
        [Column("EventDateTime")]
        public DateTime EventDateTime { get; set; }
        public EventLogModel()
        {
        }
    }
}
