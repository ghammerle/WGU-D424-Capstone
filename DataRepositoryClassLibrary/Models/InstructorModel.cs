using SQLite;


namespace C424Assessment.Models
{
    public class InstructorModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Name"), NotNull]
        public string Name { get; set; } = string.Empty;
        [Column("Phone"), NotNull]
        public string Phone { get; set; } = string.Empty;
        [Column("Email"), NotNull]
        public string Email { get; set; } = string.Empty;

        // copy ctor
        public InstructorModel(InstructorModel instructor)
        {
            Id = instructor.Id;
            Name = instructor.Name;
            Phone = instructor.Phone;
            Email = instructor.Email;
        }

        public InstructorModel()
        {

        }
    }


}
