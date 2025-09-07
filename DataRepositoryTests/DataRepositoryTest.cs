using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace DataRepositoryTests
{
    public class Tests
    {
        public DataRepositoryBase DataRepository { get; private set; }


        [SetUp]
        public void Setup()
        {   // This is a shunt for the IDataRepository interface
            // It will not perform any actual database operations
            DataRepository = new DatabaseShunt();
        }

        [Test]
        public async Task GetStudentTermsTests()
        {
            var sut = DataRepository;

            var terms = await sut.GetStudentTerms(1);
            Assert.IsNotNull(terms);
            Assert.That(terms.Count == 2);
        }

        [Test]
        public async Task GetCurrentTermTest()
        {
            var sut = DataRepository;
            var courseMap = await sut.GetCourseMapDataByUserId(2);
            // termId will contain multiple entries in the map we need only unique ids
            var termDictionary = courseMap.GroupBy(t => t.TermId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var currentTerm = sut.GetCurrentTerm(termDictionary);
            Assert.IsNotNull(currentTerm);
            Assert.That(currentTerm.TermId == 2);
        }

        [Test]
        public async Task GetCourseForTerm()
        {
            var sut = DataRepository;
            var courses = await sut.GetCoursesByTermId(1, 1);
            Assert.IsNotNull(courses);
            Assert.That(courses.Count == 3);
        }
    }

    public class DatabaseShunt : DataRepositoryBase
    {
        public override Task AddOrUpdateAssessment(AssessmentModel assessment, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateCourse(CourseModel course, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateCourseMap(CourseMapModel major, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateEventLog(EventLogModel eventLog)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateInstructor(InstructorModel instructorModel, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateMajor(MajorModel major, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateMajorMap(MajorMapModel major, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateNote(NotesModel note)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateTerm(TermModel term, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task AddOrUpdateUser(UserModel user, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CanDeleteMajor(int majorId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CanDeleteUser(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAssessment(int id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteCourse(int id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteCourseAssessment(AssessmentModel? assessment)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteCourseMapModel(CourseMapModel courseMapModel)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteInstructor(int id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteMajor(int id, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteNote(int id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteUser(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<MajorModel>> GetActiveMajors(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<AssessmentModel> GetAssessmentById(int assessmentId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<AssessmentModel>> GetAssessments(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<AssessmentModel>> GetAssessmentsByCourseId(int courseId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override async Task<CourseModel> GetCourseById(int id)
        {
            var courses = await GetCourses();
            var course = courses.FirstOrDefault(c => c.CourseId == id);
            return course;
        }

        public override Task<CourseModel> GetCourseByName(string courseName)
        {
            throw new NotImplementedException();
        }

        public override Task<List<CourseMapModel>> GetCourseMapDataByUserId(int userId, bool skipInit = false)
        {
            var allCourseMaps = GetCourseMaps(skipInit).Result;
            var userCourseMaps = allCourseMaps.Where(cm => cm.UserId == userId).ToList();
            return Task.FromResult(userCourseMaps);
        }

        public override Task<List<CourseMapModel>> GetCourseMaps(bool skipInit = false)
        {
            // return some mock data
            List<CourseMapModel> courseMaps = new List<CourseMapModel>
            {
                new CourseMapModel
                {
                    UserId = 1,
                    TermId = 1,
                    CourseId = 1,
                    CourseStatusId = 2,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    NotificationsEnabled = true,
                    NotesId = 800,
                },
                new CourseMapModel
                {
                    UserId = 1,
                    TermId = 1,
                    CourseId = 2,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    NotificationsEnabled = true,
                    NotesId = 801,
                },
                new CourseMapModel
                {
                    UserId = 1,
                    TermId = 2,
                    CourseId = 5,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(60),
                    EndDate = DateTime.Now.AddDays(90),
                    NotificationsEnabled = true,
                    NotesId = 800,
                },
                new CourseMapModel
                {
                    UserId = 1,
                    TermId = 2,
                    CourseId = 3,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(60),
                    EndDate = DateTime.Now.AddDays(90),
                    NotificationsEnabled = true,
                    NotesId = null,
                },
                                new CourseMapModel
                {
                    UserId = 1,
                    TermId = 1,
                    CourseId = 1,
                    CourseStatusId = 2,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    NotificationsEnabled = true,
                    NotesId = 800,
                },
                new CourseMapModel
                {
                    UserId = 2,
                    TermId = 1,
                    CourseId = 4,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(-60),
                    EndDate = DateTime.Now.AddDays(-30),
                    NotificationsEnabled = true,
                    NotesId = 801,
                },
                new CourseMapModel
                {
                    UserId = 2,
                    TermId = 2,
                    CourseId = 5,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    NotificationsEnabled = true,
                    NotesId = 800,
                },
               new CourseMapModel
                {
                    UserId = 2,
                    TermId = 3,
                    CourseId = 6,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(60),
                    EndDate = DateTime.Now.AddDays(90),
                    NotificationsEnabled = true,
                    NotesId = 800,
                },
                new CourseMapModel
                {
                    UserId = 3,
                    TermId = 1,
                    CourseId = 3,
                    CourseStatusId = 1,
                    StartDate = DateTime.Now.AddDays(60),
                    EndDate = DateTime.Now.AddDays(90),
                    NotificationsEnabled = true,
                    NotesId = null,
                },
            };
            return Task.FromResult(courseMaps);
        }

        public override Task<List<CourseReportItem>> GetCourseReportData(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<CourseModel>> GetCourses(bool skipInit = false)
        {
            // return some mock data 
            List<CourseModel> courses = new List<CourseModel>
            {
                new CourseModel { CourseId = 1, InstructorId = 1, MajorId = 1, Name = "Math", EstimatedCompletionTime = 3 },
                new CourseModel { CourseId = 2, InstructorId = 2, MajorId = 2, Name = "Science", EstimatedCompletionTime = 2 },
                new CourseModel { CourseId = 3, InstructorId = 3, MajorId = 3, Name = "English", EstimatedCompletionTime = 1 },
                new CourseModel { CourseId = 4, InstructorId = 4, MajorId = 4, Name = "History", EstimatedCompletionTime = 4 },
                new CourseModel { CourseId = 5, InstructorId = 5, MajorId = 5, Name = "Geography", EstimatedCompletionTime = 3 },
                new CourseModel { CourseId = 6, InstructorId = 6, MajorId = 6, Name = "Computer Science", EstimatedCompletionTime = 2 },
              };
            return Task.FromResult(courses);
        }

        public override Task<List<CourseModel>> GetCoursesByUserId(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<EventLogModel>> GetEventLog()
        {
            throw new NotImplementedException();
        }

        public override Task<InstructorModel> GetInstructorById(int id, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<InstructorModel>> GetInstructors(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<string> GetMajor(int majorId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<MajorModel> GetMajorById(int id, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<string> GetMajorByUserId(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<MajorMapModel> GetMajorMapModel(int id, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<MajorMapModel>> GetMajorMaps(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<MajorModel>> GetMajors(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<NotesModel> GetNoteById(int id)
        {
            throw new NotImplementedException();
        }

        public override Task<List<NotesModel>> GetNotes(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Stream? GetStreamReader(string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task<StudentData> GetStudentData(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<UserModel>> GetStudents(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<List<TermModel>> GetTerms(bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<UserModel> GetUserById(int userId, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task<UserModel?> GetUserByUsername(string username, bool skipInit = false)
        {
            throw new NotImplementedException();
        }

        public override Task Init(bool skip = false)
        {
            throw new NotImplementedException();
        }

        public override Task SeedDatabase()
        {
            throw new NotImplementedException();
        }

    }
}