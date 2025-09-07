using C424Assessment.Models;

namespace C424Assessment.DataRepository
{
    public struct CourseReportItem
    {
        public string Name { get; set; }
        public string Major { get; set; }
        public int Enrolled { get; set; }
    }

    public struct StudentData
    {
        public string Major { get; set; }
        public string Name { get; set; }
        public int ActiveCourseCount { get; set; }
        public int CompletedCourseCount { get; set; }
        public DateTime StartDate { get; set; }
    }

    public static class Constants
    {
        public const int MAX_ASSESSMENTS = 2;
        public const int MAX_COURSES = 6;
        public const int TERM_LENGTH_MONTHS = 6;
        public const int UNITS_REQUIRED = 120;
        public const int UNITS_PER_COURSE = 3;
    }

    public interface IDataRepository
    {


        /// <summary>
        /// This will drop all data and seed the database
        /// </summary>
        /// <returns></returns>
        abstract Task SeedDatabase();
        abstract Task AddOrUpdateAssessment(AssessmentModel assessment, bool skipInit = false);
         abstract Task AddOrUpdateCourse(CourseModel course, bool skipInit = false);
         abstract Task AddOrUpdateInstructor(InstructorModel instructorModel, bool skipInit = false);
         abstract Task AddOrUpdateNote(NotesModel note);
         abstract Task AddOrUpdateTerm(TermModel term, bool skipInit = false);
         abstract Task AddOrUpdateUser(UserModel user, bool skipInit = false);
        abstract Task AddOrUpdateEventLog(EventLogModel eventLog);
        abstract Task AddOrUpdateMajor(MajorModel major, bool skipInit = false);
        abstract Task AddOrUpdateMajorMap(MajorMapModel major, bool skipInit = false);
        abstract Task AddOrUpdateCourseMap(CourseMapModel major, bool skipInit = false);
        abstract Task UpdateStudentCourses(int userId, bool skipInit = false);
        abstract Task<List<MajorModel>> GetMajors(bool skipInit = false);
        abstract Task<List<MajorModel>> GetActiveMajors(bool skipInit = false);
        abstract Task<List<UserModel>> GetStudents(bool skipInit = false);
        abstract Task<MajorModel> GetMajorById(int id, bool skipInit = false);
        abstract Task<List<MajorMapModel>> GetMajorMaps(bool skipInit = false);
        abstract Task<List<CourseMapModel>> GetCourseMaps(bool skipInit = false);
        abstract Task<MajorMapModel> GetMajorMapModel(int id, bool skipInit = false);
        abstract Task<List<EventLogModel>> GetEventLog();
        abstract Task DeleteAssessment(int id);
        abstract Task DeleteCourse(int id);
         abstract Task DeleteInstructor(int id);
         abstract Task DeleteNote(int id);
        abstract Task DeleteMajor(int id, bool skipInit = false);
        abstract Task DeleteMajorMap(MajorMapModel majorMapModel, bool skipInit = false);
        abstract Task DeleteCourseAssessment(AssessmentModel? assessment);
        abstract Task DeleteUser(int userId, bool skipInit = false);
        abstract Task DeleteCourseMapModel(CourseMapModel courseMapModel);
        abstract Task<bool> CanDeleteUser(int userId, bool skipInit = false);
        abstract Task<bool> CanDeleteMajor(int majorId, bool skipInit = false);
        abstract Task<AssessmentModel> GetAssessmentById(int assessmentId, bool skipInit = false);
         abstract Task<List<AssessmentModel>> GetAssessments(bool skipInit = false);
         abstract Task<List<AssessmentModel>> GetAssessmentsByCourseId(int courseId, bool skipInit = false);
         abstract Task<CourseModel> GetCourseById(int id);
         abstract Task<CourseModel> GetCourseByName(string courseName);
         abstract Task<List<CourseModel>> GetCourses(bool skipInit = false);
        abstract Task<List<CourseReportItem>> GetCourseReportData(bool skipInit = false);
        abstract Task<List<CourseModel>> GetCoursesByTermId(int termId, int userId, bool skipInit = false);
        abstract Task<List<CourseModel>> GetCoursesByUserId(int userId, bool skipInit = false);
        abstract Task<string> GetMajor(int majorId, bool skipInit = false);
        abstract Task<UserModel> GetUserById(int userId, bool skipInit = false);
        abstract Task<string> GetMajorByUserId(int userId, bool skipInit = false);
        abstract Task<List<CourseMapModel>> GetCourseMapDataByUserId(int userId, bool skipInit = false);
        abstract Task<InstructorModel> GetInstructorById(int id, bool skipInit = false);
         abstract Task<List<InstructorModel>> GetInstructors(bool skipInit = false);
         abstract Task<NotesModel> GetNoteById(int id);
        abstract Task<List<NotesModel>> GetNotes(bool skipInit = false);
        abstract Task<NotesModel> GetLatestNote();
         abstract Stream? GetStreamReader(string fileName);
         abstract TermModel GetTermById(Dictionary<int, List<CourseMapModel>> termDictionary, int termId, bool skipInit = false);
         abstract Task<List<TermModel>> GetTerms(bool skipInit = false);
         abstract Task<UserModel?> GetUserByUsername(string username, bool skipInit = false);
        abstract Task<StudentData> GetStudentData(int userId, bool skipInit = false);
         abstract Task Init(bool skip = false);
        abstract TermModel GetCurrentTerm(Dictionary<int, List<CourseMapModel>> termDictionary);
        abstract Task<Dictionary<int, List<CourseMapModel>>> GetStudentTerms(int studentId);

        /// <summary>
        /// removes the course from the student's term and removes the course from the course map table.'
        /// if the course is a major course it will be moved to a later term.
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        abstract Task RemoveCourseFromTerm(int courseId, int  userId);
        abstract Task CreateStudent(string firstName, string lastName, int majorId, DateTime startDate, bool skipInit = false);
    }
}