using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(MajorIdString), "MajorIdString")]
public partial class EditCoursePage : BaseContentPage
{
    public string MajorIdString
    {
        set
        {
            if (int.TryParse(value, out var id))
                MajorId = id;
            else
                MajorId = null;
        }
    }
    public string CourseId { get; set; } = string.Empty;

    public int? MajorId { get; private set; }
    public EditCoursePage(IDataRepository repository) : base(repository)
    {
		InitializeComponent();
        AssessmentOneTypePicker.ItemsSource = Enum.GetNames(typeof(Enumerations.AssementType)).ToList();
        AssessmentTwoTypePicker.ItemsSource = Enum.GetNames(typeof(Enumerations.AssementType)).ToList();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        InstructorPicker.ItemsSource = (await DataRepository.GetInstructors()).Select(i => i.Name).ToList();
        var majors = await DataRepository.GetMajors();

        if (!string.IsNullOrWhiteSpace(CourseId))
        {
            var course = await DataRepository.GetCourseById(int.Parse(CourseId));
            if (course == null)
            {
                // this shoudl nto happen
                await OnError($"Course {CourseId} was not found");
                await LogEvent($"Course not found for Id: {CourseId}");
                return;
            }
            CourseNameEntry.Text = course.Name;
            var instructor = await DataRepository.GetInstructorById(course.InstructorId);
            if (instructor == null)
            {
                await OnError($"Instructor for course {CourseId} was not found");
                await LogEvent($"Instructor for course {CourseId} was not found");
                return;
            }
            InstructorPicker.SelectedItem = instructor.Name;
            CourseDurationEntry.Text = course.EstimatedCompletionTime.ToString();
            var assessments = await DataRepository.GetAssessmentsByCourseId(course.CourseId);
            if (assessments.Any())
            {
                AssessmentOneNameEntry.Text = assessments.First().Name;
                AssessmentOneTypePicker.SelectedItem = assessments.First().Type.ToString();
                if (assessments.Count == 2)
                {
                    AssessmentTwoNameEntry.Text = assessments.Last().Name;
                    AssessmentTwoTypePicker.SelectedItem = assessments.Last().Type.ToString();
                }
            }

            // if the course is mapped to a user do not allow editing
            var maps = await DataRepository.GetCourseMaps();
            var isMapped = maps.Any(m => m.CourseId == course.CourseId);
            if (isMapped)
            {
                // disable UI
                MajorPicker.IsEnabled = false;
                CourseDurationEntry.IsEnabled = false;
                CourseNameEntry.IsEnabled = false;
                AssessmentOneNameEntry.IsEnabled = false;
                AssessmentOneTypePicker.IsEnabled = false;
                AssessmentTwoNameEntry.IsEnabled = false;
                AssessmentTwoTypePicker.IsEnabled = false;

            }
        }
        
        if (MajorId.HasValue)
        {
            var major = majors.FirstOrDefault(m => m.Id == MajorId.Value);
            if (major != null)
            {
                MajorPicker.SelectedItem = major.Major;
                // if major is not null, we entered through major editor
                MajorPicker.IsEnabled = false;
            }
        }
        else
        {
            MajorPicker.SelectedIndex = -1; // No major selected
        }
        // if the major has 20 course remove major from majors
        var activeMajors = await DataRepository.GetActiveMajors();
        foreach (var am in activeMajors)
        {
            var itemToRemove = majors.FirstOrDefault(m => am.Id == m.Id);
            majors.Remove(itemToRemove);
        }

        MajorPicker.ItemsSource = majors.Select(i => i.Major).ToList();
    }
    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        if (MajorId == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }
        await Shell.Current.GoToAsync($"../{nameof(CreateMajorPage)}?MajorIdString={MajorId}&ActiveUserId={ActiveUserId}");
    }

    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        // if there is no course id simply return
        if (string.IsNullOrEmpty(CourseId))
        {
            if (MajorId == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }
            await Shell.Current.GoToAsync($"../{nameof(CreateMajorPage)}?MajorIdString={MajorId}&ActiveUserId={ActiveUserId}");
        }

        // only delete a course if it is not mapped to a user
        var courseMaps = await DataRepository.GetCourseMaps();
        if (courseMaps.Any(map => map.CourseId == int.Parse(CourseId)))
        {
            await OnError("This course cannot be deleted it is mapped to a student.");
            return;
        }
        var assessments = await DataRepository.GetAssessmentsByCourseId(int.Parse(CourseId));
        // delete the assessments
        foreach (var a in assessments)
        {
            await DataRepository.DeleteAssessment(a.Id);
            await LogEvent($"Assessment {a.Id} has been deleted.");
        }
        await DataRepository.DeleteCourse(int.Parse(CourseId));
        await LogEvent($"Course {CourseNameEntry.Text} has been deleted");
        if (MajorId == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }
        await Shell.Current.GoToAsync($"../{nameof(CreateMajorPage)}?MajorIdString={MajorId}&ActiveUserId={ActiveUserId}");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        if (await IsValid())
        {
            // create assessment models
            AssessmentModel? assessmentOne = null;
            AssessmentModel? assessmentTwo = null;
            if (!string.IsNullOrWhiteSpace(AssessmentOneNameEntry.Text))
            {
                assessmentOne = new AssessmentModel
                {
                    Name = AssessmentOneNameEntry.Text,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7), // default to 7 days from now
                    Type = (Enumerations.AssementType)AssessmentOneTypePicker.SelectedIndex,
                    NotificationsEnabled = true
                };
            }
            if (!string.IsNullOrWhiteSpace(AssessmentTwoNameEntry.Text))
            {
                assessmentTwo = new AssessmentModel
                {
                    Name = AssessmentTwoNameEntry.Text,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7), // default to 7 days from now
                    Type = (Enumerations.AssementType)AssessmentTwoTypePicker.SelectedIndex,
                    NotificationsEnabled = true
                };
            }

            CourseModel course = null; ;
            if (!string.IsNullOrEmpty(CourseId))
            {
                course = await DataRepository.GetCourseById(int.Parse(CourseId));
                // update an existing course
                course.InstructorId = InstructorView.Instructor.Id;
                course.MajorId = MajorId;
                course.Name = CourseNameEntry.Text;
                var assessments = await DataRepository.GetAssessmentsByCourseId(int.Parse(CourseId));
                if (assessmentOne != null)
                {
                    assessmentOne.Id = assessments.First().Id;
                }
                if (assessments.Count == 2)
                {
                    if (assessmentTwo != null)
                    {
                        // we may not have had two assessments originally
                        assessmentTwo.Id = assessments.Last().Id;
                    }
                }
                else if (assessments.Count > 2 || assessments.Count == 0)
                {
                    await OnError($"Invalid assessment data in the datbase for course {course.CourseId}");
                    return;
                }
            }

            // for a new course
            if (course == null)
            {
                // create a course model
                course = new CourseModel
                {
                    Name = CourseNameEntry.Text,
                    InstructorId = InstructorView.Instructor.Id,
                    MajorId = null, // default to a gen ed course
                };
                // save the course first to get the course id
                await DataRepository.AddOrUpdateCourse(course);
                // get last saved course
                var courses = await DataRepository.GetCourses();
                var savedCourse = courses?.LastOrDefault(c => c.Name == course.Name && c.InstructorId == course.InstructorId);
                if (savedCourse == null)
                {
                    await OnError("Failed to save course.");
                    return;
                }
                course = savedCourse;
            }

            // now save the assessments with the course id
            if (assessmentOne != null)
            {
                assessmentOne.CourseId = course.CourseId;
                await DataRepository.AddOrUpdateAssessment(assessmentOne);
            }
            if (assessmentTwo != null)
            {
                assessmentTwo.CourseId = course.CourseId;
                await DataRepository.AddOrUpdateAssessment(assessmentTwo);
            }
            if (MajorId.HasValue)
            {
                // if we have a major id, we are editing a course for a major'
                // therefore we need to navigate back to the major editor
                var major = await DataRepository.GetMajorById(MajorId.Value);
                if (major != null)
                {
                    course.MajorId = major.Id;
                    await DataRepository.AddOrUpdateCourse(course);
                }
                await LogEvent($"Created or Updated course: {course.Name} for major: {major?.Major}");
                await OnSuccess($"Course '{course.Name}' updated successfully for major: {major?.Major}.");
                await Shell.Current.GoToAsync($"../{nameof(CreateMajorPage)}?MajorIdString={major?.Id}&ActiveUserId={ActiveUserId}");
                return;
            }
            await LogEvent($"Created or updated course: {course.Name}");
            await OnSuccess($"Course '{course.Name}' updated successfully.");
            await Shell.Current.GoToAsync("..");    
        }
    }

    private async Task<bool> IsValid()
    {
        // course must have a name, an istructor and at least one assessment
        // assessments can not be of the same type
        if (string.IsNullOrWhiteSpace(CourseNameEntry.Text))
        {
            await OnError("Courses must have a name");
            return false;
        }
        // course name shall be unique
        var existingCourses = await DataRepository.GetCourses();
        // if not passed a course id we need to validate the name
        if (string.IsNullOrWhiteSpace(CourseId) && existingCourses.Any(c => c.Name.Equals(CourseNameEntry.Text, StringComparison.OrdinalIgnoreCase)))
        {
            await OnError("Course name must be unique.");
            return false;
        }
        if (InstructorView.Instructor is null || InstructorView.Instructor.Id == 0)
        {
            await OnError("Course must have an instructor.");
            return false;
        }
        if (!await AreAssessmentsValid())
        {
            return false;
        }
        return true;
    }

    private async Task<bool> AreAssessmentsValid()
    {
        // we need at least one assessment
        if (string.IsNullOrWhiteSpace(AssessmentOneNameEntry.Text) && string.IsNullOrWhiteSpace(AssessmentTwoNameEntry.Text))
        {
            await OnError("Course must have at least 1 assessment defined.");
            return false;
        }
        if (!string.IsNullOrWhiteSpace(AssessmentOneNameEntry.Text) && AssessmentOneTypePicker.SelectedIndex == -1)
        {
            await OnError("Assessment One must have a type.");
            return false;
        }
        if (!string.IsNullOrWhiteSpace(AssessmentTwoNameEntry.Text) && AssessmentTwoTypePicker.SelectedIndex == -1)
        {
            await OnError("Assessment Two must have a type.");
            return false;
        }
        // ensure the names a not the same
        var name1 = AssessmentOneNameEntry.Text?.Trim();
        var name2 = AssessmentTwoNameEntry.Text?.Trim();

        if (!string.IsNullOrEmpty(name1) && !string.IsNullOrEmpty(name2) &&
            name1.Equals(name2, StringComparison.OrdinalIgnoreCase))
        {
            await OnError("Assessments must have different names.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(AssessmentOneNameEntry.Text))
        {
            // if we have a name we must have a type
            if (AssessmentOneTypePicker.SelectedIndex == -1)
            {
                await OnError("Assessment One must have a type.");
                return false;
            }
        }
        if (!string.IsNullOrWhiteSpace(AssessmentTwoNameEntry.Text))
        {
            // if we have a name we must have a type
            if (AssessmentTwoTypePicker.SelectedIndex == -1)
            {
                await OnError("Assessment Two must have a type.");
                return false;
            }
        }
        // types must be different
        if (AssessmentOneTypePicker.SelectedIndex == AssessmentTwoTypePicker.SelectedIndex)
        {
            await OnError("Assessments must be of different types.");
            return false;
        }
        return true;
    }

    private void MajorPicker_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private async void InstructorPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedInstructor = InstructorPicker.SelectedItem?.ToString();
        var instructors = await DataRepository.GetInstructors();
        var instructor = instructors.FirstOrDefault(i => i.Name == selectedInstructor);
        InstructorView.SetInstructor(instructor);
    }
}