using C424Assessment.Models;


namespace C424Assessment.DataRepository
{
    public abstract class DataRepositoryBase : IDataRepository
    {
        public abstract Task AddOrUpdateAssessment(AssessmentModel assessment, bool skipInit = false);
        public abstract Task AddOrUpdateCourse(CourseModel course, bool skipInit = false);
        public abstract Task AddOrUpdateCourseMap(CourseMapModel major, bool skipInit = false);
        public abstract Task AddOrUpdateEventLog(EventLogModel eventLog);
        public abstract Task AddOrUpdateInstructor(InstructorModel instructorModel, bool skipInit = false);
        public abstract Task AddOrUpdateMajor(MajorModel major, bool skipInit = false);
        public abstract Task AddOrUpdateMajorMap(MajorMapModel major, bool skipInit = false);
        public abstract Task AddOrUpdateNote(NotesModel note);
        public abstract Task AddOrUpdateTerm(TermModel term, bool skipInit = false);
        public abstract Task AddOrUpdateUser(UserModel user, bool skipInit = false);
        public abstract Task<bool> CanDeleteMajor(int majorId, bool skipInit = false);
        public abstract Task<bool> CanDeleteUser(int userId, bool skipInit = false);
        public abstract Task DeleteAssessment(int id);
        public abstract Task DeleteCourse(int id);
        public abstract Task DeleteCourseAssessment(AssessmentModel? assessment);
        public abstract Task DeleteInstructor(int id);
        public abstract Task DeleteMajor(int id, bool skipInit = false);
        public abstract Task DeleteNote(int id);
        public abstract Task DeleteUser(int userId, bool skipInit = false);
        public abstract Task DeleteCourseMapModel(CourseMapModel courseMapModel);
        public abstract Task DeleteMajorMap(MajorMapModel majorMapModel, bool skipInit = false);
        public abstract Task<List<MajorModel>> GetActiveMajors(bool skipInit = false);
        public abstract Task<AssessmentModel> GetAssessmentById(int assessmentId, bool skipInit = false);
        public abstract Task<List<AssessmentModel>> GetAssessments(bool skipInit = false);
        public abstract Task<List<AssessmentModel>> GetAssessmentsByCourseId(int courseId, bool skipInit = false);
        public abstract Task<CourseModel> GetCourseById(int id);
        public abstract Task<CourseModel> GetCourseByName(string courseName);
        public abstract Task<List<CourseMapModel>> GetCourseMapDataByUserId(int userId, bool skipInit = false);
        public abstract Task<List<CourseMapModel>> GetCourseMaps(bool skipInit = false);
        public abstract Task<List<CourseReportItem>> GetCourseReportData(bool skipInit = false);
        public abstract Task<List<CourseModel>> GetCourses(bool skipInit = false);

        public abstract Task<List<CourseModel>> GetCoursesByUserId(int userId, bool skipInit = false);
        public abstract Task<List<EventLogModel>> GetEventLog();
        public abstract Task<InstructorModel> GetInstructorById(int id, bool skipInit = false);
        public abstract Task<List<InstructorModel>> GetInstructors(bool skipInit = false);
        public abstract Task<string> GetMajor(int majorId, bool skipInit = false);
        public abstract Task<MajorModel> GetMajorById(int id, bool skipInit = false);
        public abstract Task<string> GetMajorByUserId(int userId, bool skipInit = false);
        public abstract Task<MajorMapModel> GetMajorMapModel(int id, bool skipInit = false);
        public abstract Task<List<MajorMapModel>> GetMajorMaps(bool skipInit = false);
        public abstract Task<List<MajorModel>> GetMajors(bool skipInit = false);
        public abstract Task<List<NotesModel>> GetNotes(bool skipInit = false);
        public abstract Task<NotesModel> GetNoteById(int id);
        public abstract Stream? GetStreamReader(string fileName);
        public abstract Task<StudentData> GetStudentData(int userId, bool skipInit = false);
        public abstract Task<List<UserModel>> GetStudents(bool skipInit = false);
        public abstract Task<List<TermModel>> GetTerms(bool skipInit = false);
        public abstract Task<UserModel> GetUserById(int userId, bool skipInit = false);
        public abstract Task<UserModel?> GetUserByUsername(string username, bool skipInit = false);
        public abstract Task Init(bool skip = false);
        public abstract Task SeedDatabase();

        public TermModel GetTermById(Dictionary<int, List<CourseMapModel>> termDictionary, int termId, bool skipInit = false)
        {
            // current term is the one that is active today or the next active term

            // Term start date will be the earliest course startdate
            // Term end will b the last course enddate
            var courses = termDictionary[termId];
            var termStart = courses.Min(c => c.StartDate);
            var termEnd = courses.Max(c => c.EndDate);

            TermModel term = new TermModel
            {
                TermId = termId,
                Name = $"Term {termId}",
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1)
            };
           
            return term;
        }

        public async Task<List<CourseModel>> GetCoursesByTermId(int termId, int userId, bool skipInit = false)
        {
            var result = new List<CourseModel>();
            var _termDictionary = await GetStudentTerms(userId);
            var term = _termDictionary.FirstOrDefault(t => t.Key == termId);
            var maps = term.Value;
            foreach (var map in maps)
            {
                var course = await GetCourseById(map.CourseId);
                if (course != null)
                {
                    result.Add(course);
                }
            }
            return result;
        }
        public async Task<Dictionary<int, List<CourseMapModel>>> GetStudentTerms(int studentId)
        {
            var courseMap = await GetCourseMapDataByUserId(studentId);
            // termId will contain multiple entries in the map we need only unique ids
            var termDictionary = courseMap.GroupBy(t => t.TermId)
               .ToDictionary(g => g.Key, g => g.ToList());
            return termDictionary; ;
        }
        public TermModel GetCurrentTerm(Dictionary<int, List<CourseMapModel>> termDictionary)
        {
            // current term is the one that is active today or the next active term
            var today = DateTime.Now;
            var currentTerm = termDictionary
                .Where(t => t.Value.Any(c => c.StartDate <= today && c.EndDate >= today))
                .Select(t => t.Value.FirstOrDefault())
                .FirstOrDefault();
            TermModel term = new TermModel();
            if (currentTerm != null)
            {
                term.TermId = currentTerm.TermId;
                term.Name = $"Term {currentTerm.TermId}";
                term.StartDate = currentTerm.StartDate;
                term.EndDate = currentTerm.EndDate;
            }
            else
            {
                // If no current term found, return a default term
                term.TermId = 0;
                term.Name = "No Active Term";
                term.StartDate = DateTime.Now;
                term.EndDate = DateTime.Now.AddMonths(6);
            }
            return term;
        }

        public async Task RemoveCourseFromTerm(int courseId, int userId)
        {
            var termDictionary = await GetStudentTerms(userId);
            var termId = termDictionary.FirstOrDefault(t => t.Value.Any(c => c.CourseId == courseId)).Key;
            if (termId > 0)
            {
                var map = termDictionary[termId].FirstOrDefault(c => c.CourseId == courseId);
                if (map != null)
                {
                    var courseModel = await GetCourseById(courseId);
                    if ( courseModel is null)
                    {
                        throw new ArgumentNullException(nameof(courseModel));
                    }
                    if (!courseModel.MajorId.HasValue)
                    {
                        // a general ed course so simply remove it
                        await DeleteCourseMapModel(map);
                        return;
                    }
                    bool added = false;
                    var finalTermId = termDictionary.Keys.Max();
                    if (termId == finalTermId && termDictionary[termId].Count == 1)
                    {
                        // do not leave the term empty
                        return;
                    }

                    var nextTermId = map.TermId + 1;
                    do
                    {
                        // move this course to the next term that can hold it
                        if (!termDictionary.ContainsKey(nextTermId))
                        {
                            // there was not a next term so we add the course to a new term

                            // use the previous term end date to set the new term dates
                            var previousTermCourse = termDictionary[nextTermId - 1].FirstOrDefault();

                            map.TermId = nextTermId;
                            map.StartDate = previousTermCourse.EndDate.AddDays(1);
                            map.EndDate = previousTermCourse.EndDate.AddMonths(Constants.TERM_LENGTH_MONTHS);
                        }
                        else if (termDictionary[nextTermId].Count < Constants.MAX_COURSES)
                        {
                            // this term has space so we can map the course

                            // use the first course in the term to set the dates
                            var firstCourseInTerm = termDictionary[nextTermId].FirstOrDefault();
                            map.TermId = nextTermId;
                            map.StartDate = firstCourseInTerm.StartDate;
                            map.EndDate = firstCourseInTerm.EndDate;
                        }
                        else
                        {
                            nextTermId += 1;
                            continue; // try the next term
                        }
                        await AddOrUpdateCourseMap(map);
                        added = true;
                    } while (!added);
                }
            }
        }

        public async Task UpdateStudentCourses(int userId, bool skipInit = false)
        {
            // fetch existing mapdata
            List<CourseMapModel> mapData = new List<CourseMapModel>();
            mapData = await GetCourseMapDataByUserId(userId, skipInit);
            var major = await GetMajorMapModel(userId, true);
            var courses = await GetCourses(skipInit);
            var allCourses = courses.Where(c => c.MajorId == major.MajorId).ToList();
            if (allCourses.Count < 20)
            {
                throw new InvalidOperationException("Not enough courses available for the major.");
            }
            // Get 20 random gen ed courses
            var studentCourseMap = new List<CourseMapModel>();
            allCourses.AddRange(courses.Where(c => c.MajorId == null).OrderBy(c => Guid.NewGuid()).Take(20).ToList());
            var startDate = major.StartDate; 

            studentCourseMap.AddRange(allCourses.Select(c => new CourseMapModel
            {
                CourseId = c.CourseId,
                UserId = userId,
                CourseStatusId = (int)Enumerations.CourseStatus.Planned,
                StartDate = startDate, // less 6 days so we can test notifications
                EndDate = startDate.AddMonths(6),
                TermId = 1, // Assuming all courses are in Term 1 for simplicity
            }));
            // update TermId so that there are 8 terms with 5 courses 
            foreach (var map in studentCourseMap)
            {
                // Distribute courses across 8 terms
                map.TermId = (map.CourseId % 8) + 1; 
                // Calculate start and end dates for each term
                map.StartDate = map.StartDate.AddMonths((map.TermId - 1) * 6);
                map.EndDate = map.StartDate.AddMonths(6);
                map.CourseStatusId = map.EndDate < DateTime.Now? (int)Enumerations.CourseStatus.Completed : (int)Enumerations.CourseStatus.Planned; // close course from previous terms
                await AddOrUpdateCourseMap(map, true);
            }

        }

        public async Task CreateStudent(string firstName, string lastName, int majorId, DateTime startDate, bool skipInit = false)
        {

            // Create new student
            var hashandsalt = PasswordHelper.HashPassword($"{firstName}{lastName}PW");

            var newStudent = new UserModel
            {
                Username = $"{firstName}.{lastName}",
                IsDefaultPassword = true,
                PasswordHash = hashandsalt.Hash,
                PasswordSalt = hashandsalt.Salt,
                UserType = (int)Enumerations.UserRole.Student,
            };
            await AddOrUpdateUser(newStudent);
            // Get the newly created student ID (should match data above but with the provided id
            newStudent = await GetUserByUsername(newStudent.Username);
            if (newStudent == null)
            {
                throw new InvalidOperationException("Student not found after creation.");
            }
            // Create major map for the new student
            var majormap = new MajorMapModel
            {
                StudentId = newStudent.Id,
                MajorId = majorId,
                StartDate = startDate,
            };
            await AddOrUpdateMajorMap(majormap);
            await UpdateStudentCourses(newStudent.Id, skipInit);

        }

        public async Task<NotesModel> GetLatestNote()
        {
            var notes = await GetNotes();
            var latestNote = notes.OrderByDescending(n => n.LastEdited).FirstOrDefault();
            if (latestNote == null)
            {
                throw new InvalidOperationException("No notes found.");
            }
            return latestNote;
        }

    }
}
