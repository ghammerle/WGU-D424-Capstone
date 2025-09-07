using C424Assessment.Models;
using SQLite;
using System.Data.Common;
using System.Reflection;

namespace C424Assessment.DataRepository
{
    public class SQLiteRepository : DataRepositoryBase
    {

        SQLiteAsyncConnection? _dbConnection;

        string BaseDataPath { get; set; }
        public SQLiteRepository(string dbPath)
        {
            _dbConnection = new SQLiteAsyncConnection(dbPath);
            BaseDataPath = Path.GetDirectoryName(dbPath)!;
        }

        public override async Task SeedDatabase()
        {
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var tables = await _dbConnection.QueryScalarsAsync<string>(
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");

            foreach (var table in tables)
            {
                await _dbConnection.ExecuteAsync($"DROP TABLE IF EXISTS \"{table}\";");
            }
            // Reset initialization flag
            _isInitialized = false; 
            await Init();
        }
        public  override Stream? GetStreamReader(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly
            .GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(fileName));

            if (resourceName == null)
                throw new FileNotFoundException("Resource not found", fileName);

            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }

        #region Get Methods


        public override async Task<MajorMapModel> GetMajorMapModel(int id, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var map = await _dbConnection.Table<MajorMapModel>()
                .FirstOrDefaultAsync(m => m.StudentId == id);
            return map;
        }
        public override  async Task<UserModel?> GetUserByUsername(string username, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the user by username
            var user = await _dbConnection.Table<UserModel>()
                .FirstOrDefaultAsync(u => u.Username == username);
            return user;
        }

        /// <summary>
        /// Get Course Table content from the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public  override async Task<List<CourseModel>> GetCourses(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all courses from the database
            var courses = await _dbConnection.Table<CourseModel>().ToListAsync();
            return courses;
        }

        public override async Task<List<CourseReportItem>> GetCourseReportData(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query course report data
            var courses = await _dbConnection.Table<CourseModel>().ToListAsync();
            // TODO: for each course we need to find out how many students are planned or in the course
            // for now just use 2, we also need to add major to the database
            List<CourseReportItem> result = new List<CourseReportItem> ();
            foreach (var course in courses)
            {
                var item = new CourseReportItem();
                // For now, just return the course name, start date, and a dummy student count of 2
                var courseMaps = await _dbConnection.Table<CourseMapModel>().ToListAsync();
                item.Enrolled = courseMaps.Count(c => c.CourseId == course.CourseId);
                item.Name = course.Name;
                if (course.MajorId == null)
                {
                    item.Major = "General Studies";
                }
                else
                {
                    item.Major = await GetMajor((int)course.MajorId);
                }
                result.Add(item);
            }
            return result;
        }

        public override async Task<List<CourseMapModel>> GetCourseMapDataByUserId(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // get associated mapData to get the courses by id
            var mapData = await _dbConnection.Table<CourseMapModel>()
                .Where(m => m.UserId == userId)
                .ToListAsync();
            return mapData;
        }

        /// <summary>
        /// Get all terms from the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public  override  async Task<List<TermModel>> GetTerms(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all terms from the database
            var terms = await _dbConnection.Table<TermModel>().ToListAsync();
            return terms;
        }

        /// <summary>
        /// Get all instructors from the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public  override async Task<List<InstructorModel>> GetInstructors(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all instructors from the database
            var instructors = await _dbConnection.Table<InstructorModel>().ToListAsync();
            return instructors;
        }

        /// <summary>
        /// Get all notes from the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task<List<NotesModel>> GetNotes(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all notes from the database
            var notes = await _dbConnection.Table<NotesModel>().ToListAsync();
            return notes;
        }

        /// <summary>
        /// Get all assessments from the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task<List<AssessmentModel>> GetAssessments(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all assessments from the database
            var assessments = await _dbConnection.Table<AssessmentModel>().ToListAsync();
            return assessments;
        }

        public  override async Task<AssessmentModel> GetAssessmentById(int assessmentId, bool skipInit = false)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the assessment by Id
            var assessment = await _dbConnection.Table<AssessmentModel>()
                .FirstOrDefaultAsync(a => a.Id == assessmentId);
            return assessment;
        }

        public  override async Task<List<AssessmentModel>> GetAssessmentsByCourseId(int courseId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query assessments by CourseId
            var assessments = await _dbConnection.Table<AssessmentModel>()
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
            return assessments;
        }

        public  override async Task<InstructorModel> GetInstructorById(int id, bool skipInit = false)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the instructor by Id
            var instructor = await _dbConnection.Table<InstructorModel>()
                .FirstOrDefaultAsync(i => i.Id == id);
            return instructor;
        }

         public  override async Task<CourseModel> GetCourseById(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the course by Id
            var course = await _dbConnection.Table<CourseModel>()
                .FirstOrDefaultAsync(c => c.CourseId == id);
            return course;
        }

        public  override async Task<CourseModel> GetCourseByName(string courseName)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the course by Name
            var course = await _dbConnection.Table<CourseModel>()
                .FirstOrDefaultAsync(c => c.Name == courseName);
            return course;
        }

        public override async Task<List<CourseModel>> GetCoursesByUserId(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the course by Name

            var mapData = await GetCourseMapDataByUserId(userId);
            var courses = await _dbConnection.Table<CourseModel>()
                .Where(c => c.CourseId == mapData.First().CourseId)
                .ToListAsync();

            return courses;
        }

        public async Task<UserModel> GetUserId(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var user = await _dbConnection.Table<UserModel>()
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }

        public override async Task<StudentData> GetStudentData(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var majorMap = await _dbConnection.Table<MajorMapModel>()
                .FirstOrDefaultAsync(u => u.StudentId == userId);
            if (majorMap == null)
            {
                throw new Exception("MajorMap not found for the given userId.");
            }
            var studentData = new StudentData();
            studentData.Major = majorMap != null ? (await _dbConnection.Table<MajorModel>()
                .FirstOrDefaultAsync(m => m.Id == majorMap.MajorId))?.Major ?? string.Empty : string.Empty;
            studentData.StartDate = majorMap.StartDate;
            studentData.Name = (await _dbConnection.Table<UserModel>()
                .FirstOrDefaultAsync(u => u.Id == userId))?.Username ?? string.Empty;
            var mapData = await GetCourseMapDataByUserId(userId);
            studentData.ActiveCourseCount = mapData.Count(c => (Enumerations.CourseStatus)c.CourseStatusId == Enumerations.CourseStatus.Active);
            studentData.CompletedCourseCount = mapData.Count(c => (Enumerations.CourseStatus)c.CourseStatusId == Enumerations.CourseStatus.Completed);

            return studentData;
        }

        public override async Task<string> GetMajor(int majorId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var major = await _dbConnection.Table<MajorModel>()
                .FirstOrDefaultAsync(m => m.Id == majorId);
            return major.Major;
        }
        public  override async Task<string> GetMajorByUserId(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var majorMap = await _dbConnection.Table<MajorMapModel>()
                .FirstOrDefaultAsync(u => u.StudentId == userId);
            var major = await GetMajor(majorMap.MajorId);
            return major;
        }

        public override async Task<List<MajorMapModel>> GetMajorMaps(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all major maps from the database
            var majorMaps = await _dbConnection.Table<MajorMapModel>().ToListAsync();
            return majorMaps;
        }

        public override async Task<List<CourseMapModel>> GetCourseMaps(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query all course maps from the database
            var courseMaps = await _dbConnection.Table<CourseMapModel>().ToListAsync();
            return courseMaps;
        }
        public  override async Task<NotesModel> GetNoteById(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Query the note by Id
            var note = await _dbConnection.Table<NotesModel>()
                .FirstOrDefaultAsync(n => n.Id == id);
            return note;
        }


        #endregion

        private  readonly SemaphoreSlim _initLock = new(1, 1);
        private  bool _isInitialized = false;

        /// <summary>
        /// intitialize the database
        /// </summary>
        /// <param name="skip">set to  ture for initial seeding setup</param>
        /// <returns></returns>
        public  override async Task Init(bool skip = false)
        {
            if (skip ||  _isInitialized)
                return;

            await _initLock.WaitAsync();

            try
            {
                if (_isInitialized)
                    return;

                Console.WriteLine("Seeding Database");
                // Create tables and seed only if just created — in dependency-safe order
                await CreateTableAndSeedIfMissing<UserModel>(DbSeeding.SeedUser);
                await CreateTableAndSeedIfMissing<InstructorModel>(DbSeeding.SeedInstructors);
                await CreateTableAndSeedIfMissing<AssessmentModel>(DbSeeding.SeedAssessments);
                await CreateTableAndSeedIfMissing<NotesModel>(null); // No seeding
                await CreateTableAndSeedIfMissing<CourseModel>(DbSeeding.SeedCourses);
                await CreateTableAndSeedIfMissing<TermModel>(DbSeeding.SeedTerms); // Depends on courses
                await CreateTableAndSeedIfMissing<EventLogModel>(null); // No seeding
                await CreateTableAndSeedIfMissing<MajorModel>(DbSeeding.SeedMajors);
                await CreateTableAndSeedIfMissing<MajorMapModel>(DbSeeding.SeedMajorMap);
                await CreateTableAndSeedIfMissing<CourseMapModel>(DbSeeding.SeedCourseMap);

                _isInitialized = true;
                Console.WriteLine("Seeding Database Complete.");

            }
            catch (Exception e)
            {
                throw new Exception($"Database Initialization Failed: {e.Message}");
            }
            finally
            {
                _initLock.Release();
            }
        }

        private  async Task CreateTableAndSeedIfMissing<T>(Func<IDataRepository, string , Task>? seedFunc) where T : new()
        {
            try
            {
                await Init(true);
                if (_dbConnection == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }
                var tableInfo = await _dbConnection.GetTableInfoAsync(typeof(T).Name);
                if (tableInfo.Count == 0)
                {
                    await _dbConnection.CreateTableAsync<T>();
                    if (seedFunc != null)
                        await seedFunc(this, BaseDataPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Creating table");
            }
        }

        #region Add Methods

        public override async Task AddOrUpdateCourseMap(CourseMapModel courseMap, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingMap = await _dbConnection.Table<CourseMapModel>().FirstOrDefaultAsync(c => c.Id == courseMap.Id);
            if (existingMap != null)
            {
                await _dbConnection.UpdateAsync(courseMap);
            }
            else
            {
                // Insert the course into the database
                await _dbConnection.InsertAsync(courseMap);
            }
        }
        public override async Task AddOrUpdateEventLog(EventLogModel eventLog)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            await _dbConnection.InsertAsync(eventLog);
        }

        public override async Task AddOrUpdateMajor(MajorModel major, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            await _dbConnection.InsertAsync(major);
        }

        public override async Task AddOrUpdateMajorMap(MajorMapModel majorMap, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            await _dbConnection.InsertOrReplaceAsync(majorMap);
        }
        public  override async Task AddOrUpdateUser(UserModel user, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingUser = await _dbConnection.Table<UserModel>().FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser != null)
            {
                if (user.IsDefaultPassword)
                {
                    var userName = user.Username.Replace(".", string.Empty);
                    var saltAndHash = PasswordHelper.HashPassword($"{userName}PW");
                    user.PasswordSalt = saltAndHash.Salt;
                    user.PasswordHash = saltAndHash.Hash;
                }
                await _dbConnection.UpdateAsync(user);
            }
            else
            {
                // Insert the user into the database
                await _dbConnection.InsertAsync(user);
            }
        }

        public override  async Task AddOrUpdateTerm(TermModel term, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var existingTerm = await _dbConnection.Table<TermModel>().FirstOrDefaultAsync(t => t.TermId == term.TermId);
            if (existingTerm != null)
            {
                await _dbConnection.UpdateAsync(term);
            }
            else
            {
                // Insert the term into the database
                await _dbConnection.InsertAsync(term);
            }
        }

        public  override async Task AddOrUpdateNote(NotesModel note)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingNote = await _dbConnection.Table<NotesModel>().FirstOrDefaultAsync(n => n.Id == note.Id);
            if (existingNote != null)
            {
                await _dbConnection.UpdateAsync(note);
            }
            else
            {
                // Insert the note into the database
                await _dbConnection.InsertAsync(note);
            }
        }

        public  override async Task AddOrUpdateCourse(CourseModel course, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingCourse = await _dbConnection.Table<CourseModel>().FirstOrDefaultAsync(c => c.CourseId == course.CourseId);
            if (existingCourse != null)
            {
                await _dbConnection.UpdateAsync(course);
            }
            else
            {
                // Insert the course into the database
                await _dbConnection.InsertAsync(course);
            }
        }

        public override async Task AddOrUpdateInstructor(InstructorModel instructorModel, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingInstructor = await _dbConnection.Table<InstructorModel>().FirstOrDefaultAsync(i => i.Id == instructorModel.Id);
            if (existingInstructor != null)
            {
                await _dbConnection.UpdateAsync(instructorModel);
            }
            else
            {
                // Insert the instructor into the database
                await _dbConnection.InsertAsync(instructorModel);
            }
        }

        public  override async Task AddOrUpdateAssessment(AssessmentModel assessment, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var existingAssessment = await _dbConnection.Table<AssessmentModel>().FirstOrDefaultAsync(a => a.Id == assessment.Id);
            if (existingAssessment != null)
            {           // If the assessment already exists, update it
                await _dbConnection.UpdateAsync(assessment);
            }
            else
            {
                // Insert the assessment into the database
                await _dbConnection.InsertAsync(assessment);
            }
        }



        #endregion

        #region Delete Methods

        public override async Task DeleteCourseMapModel(CourseMapModel courseMapModel)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the course map model
            await _dbConnection.DeleteAsync(courseMapModel);
        }

        public  override async Task DeleteCourseAssessment(AssessmentModel? assessment)
        {
            if (assessment == null)
            {
                throw new ArgumentNullException(nameof(assessment), "Assessment cannot be null.");
            }
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the assessment from the database
            await _dbConnection.DeleteAsync(assessment);
        }

        public override async Task DeleteInstructor(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the instructor by Id
            await _dbConnection.DeleteAsync<InstructorModel>(id);
        }
        public  override async Task DeleteNote(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var maps = await GetCourseMaps();
            var relatedMap = maps.FirstOrDefault(m => m.NotesId == id);
            if (relatedMap == null)
            {
                throw new ArgumentNullException(nameof(relatedMap), "Note is not related to a map");
            }
            relatedMap.NotesId = null;
            await AddOrUpdateCourseMap(relatedMap);

            // Delete the note by Id
            await _dbConnection.DeleteAsync<NotesModel>(id);
        }

        public override async Task DeleteMajor(int id, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the major by Id
            await _dbConnection.DeleteAsync<MajorModel>(id);
        }

        public  override async Task DeleteCourse(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete course by Id
            await _dbConnection.DeleteAsync<CourseModel>(id);
        }

       public  override async Task DeleteAssessment(int id)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the assessment by Id
            await _dbConnection.DeleteAsync<AssessmentModel>(id);
        }

        public override async Task DeleteUser(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the user by Id
            if (!await CanDeleteUser(userId, skipInit))
            {
                // just return
                return;
            }
            var mappedCourses = await GetCourseMapDataByUserId(userId);
            foreach ( var mappedCourse in mappedCourses )
            {
                await DeleteCourseMapModel(mappedCourse);
            }
            mappedCourses = await GetCourseMapDataByUserId(userId);
            if (mappedCourses.Count > 0)
            {
                throw new Exception($"Failed to delete mapped data for {userId}");
            }
            var majorMap = await GetMajorMaps();
            if (majorMap.Any(m => m.StudentId == userId))
            {
                var mapToDelete = majorMap.First(m => m.StudentId == userId);
                await DeleteMajorMap(mapToDelete);
            }
            await _dbConnection.DeleteAsync<UserModel>(userId);
        }

        public override async Task DeleteMajorMap(MajorMapModel majorMapModel, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // Delete the major map model
            await _dbConnection.DeleteAsync(majorMapModel);
        }

        public override async Task<bool> CanDeleteUser(int userId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // do not delete the sutdent if they have any completed or active course
            // instead show a message that they cannot be deleted
            var mapData = await GetCourseMapDataByUserId(userId) ?? new List<CourseMapModel>();
            var activeOrCompletedCourses = mapData.Where(c => (Enumerations.CourseStatus)c.CourseStatusId == Enumerations.CourseStatus.Active ||
            (Enumerations.CourseStatus)c.CourseStatusId == Enumerations.CourseStatus.Completed).ToList();
            return activeOrCompletedCourses.Count == 0; 
        }

        public override async Task<bool> CanDeleteMajor(int majorId, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            // do not delete the major if it has any students associated with it
            var majorMap = await _dbConnection.Table<MajorMapModel>()
                .Where(m => m.MajorId == majorId).ToListAsync();
            return majorMap.Count == 0;
        }

        #endregion Delete Methods

        #region
        public override async Task<List<MajorModel>> GetMajors(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var majors = await  _dbConnection.Table<MajorModel>().ToListAsync();
            return majors;
        }

        public override async Task<List<MajorModel>> GetActiveMajors(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var majors = await GetMajors();
            // is a major has 20 courses is it active
            var courses = await GetCourses();
            var activeMajors = majors.Where(m =>
                courses.Count(c => c.MajorId == m.Id) >= 20).ToList();
            return activeMajors;
        }
        public  override async Task<List<UserModel>> GetStudents(bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var students = await _dbConnection.Table<UserModel>()
                .Where(u => (Enumerations.UserRole)u.UserType == Enumerations.UserRole.Student).ToListAsync();
            return students;

        }

        public override async Task<MajorModel> GetMajorById(int id, bool skipInit = false)
        {
            await Init(skipInit);
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var major = await _dbConnection.Table<MajorModel>()
                .FirstOrDefaultAsync(m => m.Id == id);
            return major;
        }

        public override async Task<List<EventLogModel>> GetEventLog()
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var log = await _dbConnection.Table<EventLogModel>().ToListAsync();
            return log;
        }

        public override async  Task<UserModel> GetUserById(int userId, bool skipInit = false)
        {
            await Init();
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }
            var user = await _dbConnection.Table<UserModel>().FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }
        #endregion


    }
}
