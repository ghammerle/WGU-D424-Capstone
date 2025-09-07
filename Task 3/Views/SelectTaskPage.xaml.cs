using C424Assessment.DataRepository;
using System.Threading.Tasks;

namespace C424Assessment.Views;

public partial class SelectTaskPage : BaseContentPage
{
	public SelectTaskPage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
	}

    private async void EditStudentButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(EditCreateStudentPage)}?ActiveUserId={ActiveUserId}");
    }

    private async void EditInstructorButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(InstructorEditPage)}?ActiveUserId={ActiveUserId}");
    }

    private async void EditCourseButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(EditCoursePage)}?ActiveUserId={ActiveUserId}");
    }

    private async void EditMajorButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(CreateMajorPage)}?ActiveUserId={ActiveUserId}");
    }

    private async void ReportCoursesButton_Clicked(object sender, EventArgs e)
    {
        var courseData = await DataRepository.GetCourseReportData();
        var coursesReport = new CourseReport(courseData);
        await ShowReportPopupAsync(coursesReport);
    }

    private async void SeedDataBase_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!await DisplayAlert("Verify", "This will delete all existing data and seed the database with test data. Do you want to continue?", "Yes", "No"))
            {
                return;
            }

            await DataRepository.SeedDatabase();
            await OnSuccess("Database Seeded");
        }
        catch (Exception ex)
        {
            await OnError($"Error Seeding Database: {ex.Message}");
        }
    }

    private async void ReportLogButton_Clicked(object sender, EventArgs e)
    {
        var logData = await DataRepository.GetEventLog();
        var logReport = new EventLogReport(logData);
        await ShowReportPopupAsync(logReport);

    }

    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        await LogEvent($"User logged out {ActiveUserId}");
        await Shell.Current.GoToAsync($"{nameof(UserLoginPage)}");
    }
}