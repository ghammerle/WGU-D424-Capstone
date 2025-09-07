using C424Assessment.DataRepository;
using C424Assessment.Models;
using System.Threading.Tasks;
using static C424Assessment.Models.Enumerations;

namespace C424Assessment.Views;

[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(TermId), "TermId")]
[QueryProperty(nameof(InstructorId), "InstructorId")]
public partial class CoursePage : BaseContentPage
{
    public string TermId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    private CourseModel? _courseModel;
    private CourseMapModel? _courseMapModel;
	
    
    public CoursePage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
        StatusPicker.ItemsSource = Enum.GetNames(typeof(Enumerations.CourseStatus)).ToList();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(CourseId))
        {
            _courseModel = await DataRepository.GetCourseById(int.Parse(CourseId));
            if (_courseModel == null)
            {
                await OnError($"Course not found with Id {CourseId}");
                return;
            }
            var userMappedCourses = await DataRepository.GetCourseMapDataByUserId(ActiveUserId);
            _courseMapModel = userMappedCourses.FirstOrDefault(c => c.CourseId == _courseModel.CourseId);

            // if this course is the only course in a term disable Remove button
            RemoveCourseButton.IsEnabled = userMappedCourses.Count(c => c.TermId == int.Parse(TermId)) > 1;

            if (_courseMapModel == null)
            {
                await OnError($"Course {CourseId} not mapped for user {ActiveUserId}");
                return;
            }
            // do not allow the status to be changed if the term & course have not started or if term is over
            if (_courseMapModel.StartDate > DateTime.Today || _courseMapModel.EndDate < DateTime.Today)
            {
                StatusPicker.IsEnabled = false;
            }
            EnableNotificationsCB.IsChecked = _courseMapModel.NotificationsEnabled;
            CourseNameLabel.Text = _courseModel.Name;
            StartDatePicker.IsEnabled = false;
            EndDatePicker.IsEnabled = false;
            StartDatePicker.Date = _courseMapModel.StartDate;
            EndDatePicker.Date = _courseMapModel.EndDate;
            var courseStatus = Enum.GetName(typeof(CourseStatus), _courseMapModel.CourseStatusId);
            StatusPicker.SelectedItem = courseStatus;
            // update instructor data to latest
            var instructor = await DataRepository.GetInstructorById(_courseModel.InstructorId);
            InstructorView.SetInstructor(instructor);
            InstructorView.CourseID = CourseId;
            await UpdateAssessmentView();
            if (_courseMapModel.CourseStatusId == (int)CourseStatus.Completed)
            {
                UpdateButton.IsEnabled = false;
                RemoveCourseButton.IsEnabled = false;
            }
        }
        else
        {
            _courseModel = new CourseModel();
        }
    }

    private async Task UpdateAssessmentView()
    {
        var assessments = await DataRepository.GetAssessmentsByCourseId(int.Parse(CourseId));

        if (assessments == null)
        {
            AssessmentViewOne.IsVisible = false;
            AssessmentViewTwo.IsVisible = false;
            return;
        }
        AssessmentViewOne.IsVisible = assessments.Count >= 1;
        AssessmentViewTwo.IsVisible = assessments.Count >= 2;

        if (assessments.Count >= 1)
        {
            AssessmentViewOne.CourseId = CourseId.ToString();
            AssessmentViewOne.Name = assessments[0].Name;
            AssessmentViewOne.Type = assessments[0].Type.ToString();
            AssessmentViewOne.DueDate = _courseMapModel.EndDate.ToString("MM/dd/yyyy");
            AssessmentViewOne.AssessmentId = assessments[0].Id.ToString();
            AssessmentViewOne.Notify = assessments[0].NotificationsEnabled;
        }
        if (assessments.Count >= 2)
        {
            AssessmentViewTwo.CourseId = CourseId.ToString();
            AssessmentViewTwo.Name = assessments[1].Name;
            AssessmentViewTwo.Type = assessments[1].Type.ToString();
            AssessmentViewTwo.DueDate = _courseMapModel.EndDate.ToString("MM/dd/yyyy");
            AssessmentViewTwo.AssessmentId = assessments[1].Id.ToString();
            AssessmentViewTwo.Notify = assessments[1].NotificationsEnabled;
        }
        return;
    }

    private async void notesButton_Clicked(object sender, EventArgs e)
    {
        if (_courseModel == null || _courseMapModel == null)
        {
            await OnError("CourseModel  or CourseMapModel should not be null here. Cannot delete course without a valid model.");
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(NotesPage)}?NoteId={_courseMapModel.NotesId}&CourseId={CourseId}&ActiveUserId={ActiveUserId}&TermId={TermId}");
    }

     private async void RemoveCourseButton_Clicked(object sender, EventArgs e)
    {
        if (TermId == null && CourseId == null)
        {
            // go to previous page called Add Course, this is basically cancel
            await Shell.Current.GoToAsync("..");
            return;
        }
        // to delete a course, we need to remove all assessments and notes first
        if (_courseModel == null)
        {
            await OnError("CourseModel should not be null here. Cannot delete course without a valid model.");
            return;
        }
        // if this course is a major course notify user this course will be moved to a later term
        if (_courseModel.MajorId.HasValue)
        {
            var result = await DisplayAlert("Drop Course", "This course is part of your major and will be moved to a later term.", "Continue", "Cancel");
            if (!result)
            {
                return;
            }
        }
        else
        {
            var result = await DisplayAlert("Drop Course", "This course is part of your degree plan.  You will need to add another course to reach 120 units. Are you sure you want to drop it?", "Yes", "No");
            if (!result)
            {
                return;
            }
        }
        await DataRepository.RemoveCourseFromTerm(_courseModel.CourseId, ActiveUserId);
        await LogEvent($"Removed course: {_courseModel.Name} from term {TermId}");
        await OnSuccess($"Course '{_courseModel.Name}' removed successfully from term {TermId}.");
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");

    }

    private async void ReturnButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");

    }

    private async void UpdateButton_Clicked(object sender, EventArgs e)
    {
        if (_courseMapModel == null)
        {
            await OnError("CourseModel should not be null here.");
            return;
        }
        _courseMapModel.NotificationsEnabled = EnableNotificationsCB.IsChecked;
        _courseMapModel.CourseStatusId = (int)Enum.Parse<CourseStatus>(StatusPicker.SelectedItem.ToString());

        await DataRepository.AddOrUpdateCourseMap(_courseMapModel);

        await LogEvent($"Updated course: {_courseModel.Name} in term {TermId}");
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");
    }
}