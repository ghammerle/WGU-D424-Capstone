using C424Assessment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace C424Assessment.DataRepository
{
    /// <summary>
    /// This class is responsible for seeding the database with initial mock data.
    /// </summary>
    public static class DbSeeding
    {
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var value = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        // Escaped quote
                        value.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }
            result.Add(value.ToString());
            return result.ToArray();
        }

        private static Stream? GetDataStream(IDataRepository repo, string basePath, string fileName)
        {
            var localPath = Path.Combine(basePath, fileName);
            if (File.Exists(localPath))
                return File.OpenRead(localPath);
            return repo.GetStreamReader(fileName);
        }

        public static Stream? GetStreamReader(string fileName)
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



        #region Seeding Methods

        public static async Task SeedUser(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Users----");
            using var stream = GetDataStream(repo, basePath, "UserMockData.csv");
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null) throw new InvalidOperationException("No headers found");
                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = line.Split(',');

                    var hashAndSalt = GetUserHashAndSat(values[headerMap["Name"]]); 

                    var user = new UserModel
                    {
                        Username = values[headerMap["Name"]],
                        PasswordHash = hashAndSalt.hash,
                        PasswordSalt = hashAndSalt.salt,
                        IsDefaultPassword = bool.Parse(values[headerMap["IsDefaultPassword"]]),
                        UserType = int.Parse(values[headerMap["TypeId"]]),
                        Id = int.Parse(values[headerMap["Id"]])
                    };
                    // Add user to Database
                    try
                    {
                        await repo.AddOrUpdateUser(user, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding user: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----USER DATA-----");
            var users = await repo.GetStudents(true);
            if (users == null || users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }
            foreach (var user in users)
            {
                // write to console
                Console.WriteLine($"Id: {user.Id}, Username: {user.Username}, IsDefaultPassword: {user.IsDefaultPassword}, UserType: {user.UserType}");
            }
        }

        private static (string hash, string salt) GetUserHashAndSat(string userName)
        {
            switch (userName)
            {
                case "admin":
                    return PasswordHelper.HashPassword("Admin123!");
                case "Beth.Santos":
                    return  PasswordHelper.HashPassword("BethSantosPW");
                case "Jake.Smith":
                    return PasswordHelper.HashPassword("JakeSmithPW");
                default:
                    throw new ArgumentException("Invalid userName passed");
            }
        }

        public static async Task SeedInstructors(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Instructors----");
            using var stream = GetDataStream(repo, basePath, "InstructorMockData.csv");
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null) throw new InvalidOperationException("No headers found");

                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = line.Split(',');
                    var instructor = new InstructorModel
                    {
                        Name = values[headerMap["Name"]],
                        Email = values[headerMap["Email"]],
                        Phone = values[headerMap["Phone"]],
                        Id = int.Parse(values[headerMap["Id"]])
                    };
                    // Add instructor to Database
                    try
                    {
                        await repo.AddOrUpdateInstructor(instructor, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding instructor: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----INSTRUCTOR DATA-----");
            var instructors = await repo.GetInstructors(true);
            if (instructors == null || instructors.Count == 0)
            {
                Console.WriteLine("No instructors found.");
                return;
            }
            foreach (var instructor in instructors)
            {
                // write to console
                Console.WriteLine($"Id: {instructor.Id}, Name: {instructor.Name}, Email: {instructor.Email}, Phone: {instructor.Phone} ");
            }
        }

        public static async Task SeedAssessments(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Assessments----");
            using var stream = GetDataStream(repo, basePath, "AssessmentMockData.csv");
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null)
                    throw new InvalidOperationException("No headers found");
                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = ParseCsvLine(line);
                    var assessmentModel = new AssessmentModel
                    {
                        Id = int.Parse(values[headerMap["Id"]]),
                        Name = string.IsNullOrEmpty(values[headerMap["Name"]]) ? "Assessment" : values[headerMap["Name"]],
                        Type = Enum.Parse<Enumerations.AssementType>(values[headerMap["Type"]]),
                        StartDate = values[headerMap["StartDate"]] != string.Empty ? DateTime.Parse(values[headerMap["StartDate"]]) : DateTime.MinValue,
                        EndDate = values[headerMap["EndDate"]] != string.Empty ? DateTime.Parse(values[headerMap["EndDate"]]) : DateTime.MinValue,
                        NotificationsEnabled = bool.Parse(values[headerMap["NotificationsEnabled"]]),
                        CourseId = int.Parse(values[headerMap["CourseId"]])
                    };
                    // Add assessment to Database
                    try
                    {
                        await repo.AddOrUpdateAssessment(assessmentModel, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding assessment: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----ASSESSMENT DATA-----");
            var assessments = await repo.GetAssessments(true);
            if (assessments == null || assessments.Count == 0)
            {
                Console.WriteLine("No assessments found.");
                return;
            }
            foreach (var assessment in assessments)
            {
                // write to console
                Console.WriteLine($"Id: {assessment.Id}, Name: {assessment.Name}, Type: {assessment.Type}, StartDate: {assessment.StartDate}, EndDate: {assessment.EndDate}, NotificationsEnabled: {assessment.NotificationsEnabled}, CourseId: {assessment.CourseId} ");
            }
        }

        public static async Task SeedCourses(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Courses----");
            using var stream = GetDataStream(repo, basePath, "CourseMockData.csv");
            var courses = new List<CourseModel>();
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null) throw new InvalidOperationException("No headers found");

                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = line.Split(',');

                    //Id,Name,InstructorId,EstimatedDuration,MajorId
                    int? majorId = null;
                    if (values[headerMap["MajorId"]]!= string.Empty)
                    {
                        var value = int.Parse(values[headerMap["MajorId"]]);
                        if (value > 0)
                        {
                            majorId = value;
                        }
                    }
                    var course = new CourseModel
                    {
                        CourseId = int.Parse(values[headerMap["Id"]]),
                        Name = values[headerMap["Name"]],
                        InstructorId = int.Parse(values[headerMap["InstructorId"]]),
                        EstimatedCompletionTime = int.TryParse(values[headerMap["EstimatedDuration"]], out int duration) ? duration : null,
                        MajorId =majorId
                    };
                    try
                    {
                        await repo.AddOrUpdateCourse(course, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding course: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----COURSE DATA-----");
            var coursesList = await repo.GetCourses(true);
            if (coursesList == null || coursesList.Count == 0)
            {
                Console.WriteLine("No courses found.");
                return;
            }
            foreach (var course in coursesList)
            {
                // write to console
                Console.WriteLine($"CourseId: {course.CourseId}, Name: {course.Name}, InstructorId: {course.InstructorId}, EstimatedCompletionTime: {course.EstimatedCompletionTime}, MajorId: {course.MajorId}");
            }
        }

        public static async Task SeedTerms(IDataRepository repo, string basePath)
        {
            var skipInit = true;
            // use coursemap data to popluate terms

            List<TermModel> terms = new List<TermModel>();
            terms = new List<TermModel>
            {
                new TermModel
                {
                    TermId = 1,
                    Name = "Term 1",
                    StartDate = DateTime.Now.AddMonths(-24),
                    EndDate = DateTime.Now.AddMonths(-18),
                },
                new TermModel
                {
                    TermId = 2,
                    Name = "Term 2",
                    StartDate = DateTime.Now.AddMonths(-12),
                    EndDate = DateTime.Now.AddMonths(-6),
                },
                new TermModel
                {
                    TermId = 3,
                    Name = "Term 3",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(6),
                },
                new TermModel
                {
                    TermId = 4,
                    Name = "Term 4",
                    StartDate = DateTime.Now.AddMonths(12),
                    EndDate = DateTime.Now.AddMonths(18),
                }
            };
            foreach (var term in terms)
            {
                await repo.AddOrUpdateTerm(term, skipInit);
            }
            var termsList = await repo.GetTerms(skipInit);
            Console.WriteLine("-----TERM DATA-----");
            if (termsList == null || termsList.Count == 0)
            {
                Console.WriteLine("No terms found.");
                return;
            }
            foreach (var term in termsList)
            {
                // write to console
                Console.WriteLine($"TermId: {term.TermId}, Term: {term.Name}, StartDate: {term.StartDate}, EndDate: {term.EndDate}, ");
            }
        }

        public static async Task SeedMajors(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Majors----");
            using var stream = GetDataStream(repo, basePath, "MajorSeedData.csv");
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null) throw new InvalidOperationException("No headers found");
                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = line.Split(',');
                    var major = new MajorModel
                    {
                        Id = int.Parse(values[headerMap["Id"]]),
                        Major = values[headerMap["Name"]],
                    };
                    // Add major to Database
                    try
                    {
                        await repo.AddOrUpdateMajor(major, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding major: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----MAJOR DATA-----");
            var majors = await repo.GetMajors(true);
            if (majors == null || majors.Count == 0)
            {
                Console.WriteLine("No majors found.");
                return;
            }
            foreach (var major in majors)
            {
                // write to console
                Console.WriteLine($"MajorId: {major.Id}, Major: {major.Major}");
            }
        }

        public static async Task SeedMajorMap(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Major Map----");
            using var stream = GetDataStream(repo, basePath, "MajorMapSeedData.csv");
            if (stream == null)
                return;
            using (var reader = new StreamReader(stream))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers == null) throw new InvalidOperationException("No headers found");
                // Map header names to indices
                var headerMap = headers
                    .Select((h, i) => new { Header = h.Trim(), Index = i })
                    .ToDictionary(x => x.Header, x => x.Index);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    var values = line.Split(',');
                    // seed so that the students have been enrolled for  a while
                    var startDate = DateTime.Now.AddYears(-int.Parse(values[headerMap["StudentId"]])).AddMonths(-5).AddDays(-6);
                    var majorMap = new MajorMapModel
                    {
                        MajorId = int.Parse(values[headerMap["MajorId"]]),
                        StudentId = int.Parse(values[headerMap["StudentId"]]),
                        StartDate = startDate
                    };
                    // Add major map to Database
                    try
                    {
                        await repo.AddOrUpdateMajorMap(majorMap, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error adding major map: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("-----MAJOR MAP DATA-----");
            var majorMaps = await repo.GetMajorMaps(true);
            if (majorMaps == null || majorMaps.Count == 0)
            {
                Console.WriteLine("No major maps found.");
                return;
            }
            foreach (var majorMap in majorMaps)
            {
                // write to console
                Console.WriteLine($"StudentId: {majorMap.StudentId}, MajorId: {majorMap.MajorId}, StartDate: {majorMap.StartDate}");
            }
        }


        public static async Task SeedCourseMap(IDataRepository repo, string basePath)
        {
            Console.WriteLine("----Seeding Course Map----");
            // this will seed the course map data based on students and courses seeded previously
            var courses = await repo.GetCourses(true);
            var students = await repo.GetStudents(true);
            if (courses == null || students == null || courses.Count == 0 || students.Count == 0)
            {
                Console.WriteLine("No courses or students found to seed course map data.");
                return;
            }
            var studentCourseMap = new List<CourseMapModel>();
            // add data for each student
            foreach ( var student in students)
            {
                await repo.UpdateStudentCourses(student.Id, true);
            }
            Console.WriteLine("-----COURSE MAP DATA-----");
            var courseMaps = await repo.GetCourseMaps(true);
            if (courseMaps == null || courseMaps.Count == 0)
            {
                Console.WriteLine("No course maps found.");
                return;
            }
            foreach (var courseMap in courseMaps)
            {
                // write to console
                Console.WriteLine($"CourseId: {courseMap.CourseId}, UserId: {courseMap.UserId}, CourseStatusId: {courseMap.CourseStatusId}, StartDate: {courseMap.StartDate}, EndDate: {courseMap.EndDate}, TermId: {courseMap.TermId}");
            }
        }
        #endregion

    }
}
