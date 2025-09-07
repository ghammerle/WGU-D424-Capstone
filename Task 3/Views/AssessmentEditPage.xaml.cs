using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(AssessmentId), "AssessmentId")]
public partial class AssessmentEditPage : BaseContentPage
{
    public string CourseId { get; set; } = string.Empty;
    public string AssessmentId { get; set; } = string.Empty;
    private AssessmentModel? _assessment;
    private CourseModel? _course;
    public AssessmentEditPage(Task<IDataRepository> dataRepository) : base(dataRepository.Result)
    {
		InitializeComponent();
        AssessTypePicker.ItemsSource = Enum.GetNames(typeof(Enumerations.AssementType)).ToList();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(CourseId) || !string.IsNullOrEmpty(AssessmentId))
        {
            try
            {
                _course =  await DataRepository.GetCourseById(int.Parse(CourseId));
                if (_course is null)
                {
                    await OnError($"Course not found with Id {CourseId}");
                    return;
                }

                _assessment = await DataRepository.GetAssessmentById(int.Parse(AssessmentId));
                if (_assessment is null)
                {
                    await OnError($"Assessment not found with Id {AssessmentId}");
                    return;
                }
                AssessNameEntry.Text = _assessment.Name;
                AssessStartDatePicker.Date = _assessment.StartDate;
                AssessEndDatePicker.Date = _assessment.EndDate;
                AssessTypePicker.SelectedIndex = (int)_assessment.Type;

            }
            catch (NullReferenceException)
            {
                await OnError($"Course not found with Id {CourseId}");
            }
        }
    }

    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        if (await  IsValid())
        {
            await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}");
        }
    }

    public async Task<bool> IsValid()
    {
        var assessment = new AssessmentModel
        {
            Name = AssessNameEntry.Text,
            StartDate = AssessStartDatePicker.Date,
            EndDate = AssessEndDatePicker.Date,
            Type = (Enumerations.AssementType)AssessTypePicker.SelectedIndex,
            CourseId = int.Parse(CourseId)
        };

       if (!await ValidateAssessmentModel(assessment))
        {
            return false;
        }
       // get other assessment from associated course

        // check if assessment already exists
        if (_assessment is not null && _assessment.Id != 0)
        {
            assessment.Id = _assessment.Id;
        }
        // check if there is another assessment of the same type
        var assessments = await DataRepository.GetAssessmentsByCourseId(int.Parse(CourseId));
        var otherAssessment = assessments.FirstOrDefault(a => a.Id != assessment.Id && a.Type == assessment.Type);
        if ( otherAssessment is not null &&  assessment.Type == otherAssessment.Type)
        {
            await OnError("Courses require one objective and one performance assessments.");
            return false;
        }
        // save changes to database
        await DataRepository.AddOrUpdateAssessment(assessment);
        return true;
    }

    public async Task<bool> ValidateAssessmentModel(AssessmentModel assessment)
    {
        if (string.IsNullOrEmpty(assessment.Name))
        {
            await OnError("Assessment Name can not be empty.");
            return false;
        }
        //if (!await ValidateDates(assessment.StartDate, assessment.EndDate))
        //{
        //    return false;
        //}
        return true;
    }

    //private async Task<bool> ValidateDates(DateTime start, DateTime end)
    //{
    //    if (_course == null)
    //    {
    //        throw new NullReferenceException("Course is not set. Can not validate dates.");
    //    }

    //    if (start > end)
    //    {
    //        await OnError("Assessment Start Date must come before End Date");
    //        return false;
    //    }
    //    if (start < _course.StartDate.Date || start > _course.EndDate)
    //    {
    //        await OnError($"Assessments must start during the course {_course.StartDate.Date.ToString("MMMM d, yyyy")} - {_course.EndDate.Date.ToString("MMMM d, yyyy")}.");
    //        return false;
    //    }
    //    if (end > _course.EndDate)
    //    {
    //        await OnError($"Assessments must be completed during the course. Course end {_course.EndDate.Date.ToString("MMMM d, yyyy")}.");
    //        return false;
    //    }
    //    return true;
    //}

    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        await DataRepository.DeleteCourseAssessment(_assessment);
        await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}");
    }
}