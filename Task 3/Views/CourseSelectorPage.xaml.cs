using C424Assessment.DataRepository;
using C424Assessment.Models;
using System.Threading.Tasks;

namespace C424Assessment.Views;

[QueryProperty(nameof(TermId), "TermId")]
public partial class CourseSelectorPage : BaseContentPage
{
    public string TermId { get; set; } = string.Empty;

    private List<string> selectableCourses = new List<string>();
	public CourseSelectorPage(IDataRepository SQLiteRepository) : base(SQLiteRepository)
    {
		InitializeComponent();
	}


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var mappedCourses = await DataRepository.GetCourseMapDataByUserId(ActiveUserId);
        // Populate course selector with available courses
        var incompeteCourses = mappedCourses.Where(c => c.CourseStatusId != (int)Enumerations.CourseStatus.Completed &&
                c.TermId!= int.Parse(TermId)).ToList();
        foreach (var ic in incompeteCourses)
        {
            var course = await DataRepository.GetCourseById(ic.CourseId);
            selectableCourses.Add(course.Name);
        }
        var allCourses = await DataRepository.GetCourses();
        // get get courses major = null and course not in the enrolled list
        var availableCourses = allCourses.Where(c => c.MajorId is null &&
                !incompeteCourses.Select(ic => ic.CourseId).Contains(c.CourseId)).ToList();
        foreach (var c in availableCourses)
        {
            var course = await DataRepository.GetCourseById(c.CourseId);
            selectableCourses.Add(course.Name);
        }
        FilteredPicker.ItemsSource = selectableCourses;
    }
    private async void AddCourseButton_Clicked(object sender, EventArgs e)
    {
        // need to create a new mapping for this course if this couse was from the enrolled courses we need to update the mapping
        var maps = await DataRepository.GetCourseMapDataByUserId(ActiveUserId);
        var courseId = (await DataRepository.GetCourses()).FirstOrDefault(c => c.Name == CourseToAddLabel.Text)?.CourseId;
        if (courseId == null)
        {
            await OnError("Course not found. While trying to add.");
            return;
        }
        var map = maps.FirstOrDefault(m => m.CourseId == courseId);
        var referenceCourseMap = maps.FirstOrDefault(m => m.TermId == int.Parse(TermId));
        var startDate = referenceCourseMap.StartDate;
        var endDate = referenceCourseMap.EndDate;
        if (map == null)
        {
            // this is a new course, so create a new mapping
            map = new CourseMapModel
            {
                CourseId = (int)courseId,
                UserId = ActiveUserId,
                NotificationsEnabled = true,
                CourseStatusId = (int)Enumerations.CourseStatus.Planned,
            };
        }
        
        map.TermId = int.Parse(TermId);
        map.StartDate = referenceCourseMap.StartDate;
        map.EndDate = referenceCourseMap.EndDate;
        await DataRepository.AddOrUpdateCourseMap(map);
        await OnSuccess($"{CourseToAddLabel.Text} added to Term {TermId}.");
        await LogEvent($"Course {CourseToAddLabel.Text} added to Term {TermId}.");
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");
    }

    private void FilteredPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        CourseToAddLabel.Text = FilteredPicker.SelectedItem.ToString();
    }

    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        
        // Filter the course list based on search input
        var searchText = SearchEntry.Text.ToLower();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredPicker.ItemsSource = selectableCourses;
        }

        var filteredCourses = selectableCourses.Where(c => c.ToLower().Contains(searchText)).ToList();
        FilteredPicker.ItemsSource = filteredCourses;
    }
}