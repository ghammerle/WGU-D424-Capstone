using SQLite;

namespace C424Assessment.Models
{
    public class NotesModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }
        [Column("LastEdited")]
        public DateTime LastEdited { get; set; }
        [Column("Content")]
        public string Content { get; set; } =  string.Empty ;

        public NotesModel()
        {
        }

        public NotesModel(NotesModel? note)
        {
            if (note != null)
            {
                Id = note.Id;
                LastEdited = note.LastEdited;
                Content = note.Content;
            }
        }
    }
}
