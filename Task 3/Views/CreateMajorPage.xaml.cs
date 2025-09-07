using C424Assessment.DataRepository;
using C424Assessment.Models;
namespace C424Assessment.Views;

[QueryProperty(nameof(MajorIdString), "MajorIdString")]
public partial class CreateMajorPage : BaseContentPage
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

    public int? MajorId { get; private set; }
    public CreateMajorPage(IDataRepository dataRepository) : base(dataRepository)
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        var majors = await DataRepository.GetMajors();
        List<String> majorNames = majors.Select(m => m.Major).ToList();
        MajorPicker.ItemsSource = majorNames;
        if (MajorId.HasValue)
        {
            var major = majors.FirstOrDefault(m => m.Id == MajorId.Value);
            if (major != null)
            {
                MajorPicker.SelectedItem = major.Major;
                MajorEntry.Text = major.Major;
                MajorEntry.IsEnabled = false; // Disable editing of the major name
                MajorPicker.IsEnabled = false; // Disable major selection
                var courses = await DataRepository.GetCourses();
                var majorCourses = courses.Where(c => c.MajorId == major.Id).ToList();
                listCourses.ItemsSource = majorCourses;
            }
        }
        else
        {
            MajorPicker.SelectedIndex = -1; // No major selected
            listCourses.ItemsSource = new List<CourseModel>();
        }
    }
    private async void MajorPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (MajorPicker.SelectedItem is string selectedMajor)
        {
            MajorEntry.Text = selectedMajor;
            MajorEntry.IsEnabled = false;
            var existingMajors = await DataRepository.GetMajors();
            var major = existingMajors.FirstOrDefault(m => m.Major == selectedMajor);
            var courses = await DataRepository.GetCourses();
            if (major is null)
            {
                await OnError("Major not found.");
                return;
            }
            var majorCourses = courses.Where(c => c.MajorId == major.Id).ToList();
            listCourses.ItemsSource = majorCourses;
        }
    }

    private async void AddCourseButton_Clicked(object sender, EventArgs e)
    {
        if (MajorPicker.SelectedItem is null || string.IsNullOrWhiteSpace(MajorEntry.Text))
        {
            await OnError("Please select a major to add a course.");
            return;
        }
        // no more than 20 courses
        if (listCourses.ItemsSource is not null && listCourses.ItemsSource.Cast<CourseModel>().Count() >= 20)
        {
            await OnError("You cannot add more than 20 courses to a major.");
            return;
        }
        var existingMajors = await DataRepository.GetMajors();
        var major = existingMajors.FirstOrDefault(m => m.Major == MajorPicker.SelectedItem.ToString());
        if (major is null)
        {
            await OnError("Major not found.");
            return;
        }
        // Navigate to CreateCoursePage with the MajorId
        await Shell.Current.GoToAsync($"{nameof(EditCoursePage)}?MajorIdString={major.Id.ToString()}&ActiveUserId={ActiveUserId} ");
    }
    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(SelectTaskPage)}?ActiveUserId={ActiveUserId}");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        if (MajorPicker.SelectedIndex != -1)
        {
            // nothing to update but prevents a crash
            await OnSuccess($"Major '{MajorPicker.SelectedItem}' updated successfully.");
            await Shell.Current.GoToAsync($"..");
            return;
        }
        if (string.IsNullOrWhiteSpace(MajorEntry.Text))
        {
            await OnError("Please enter a major name.");
            return;
        }
        var existingMajors = await DataRepository.GetMajors();
        if (existingMajors.Any(m => m.Major == MajorEntry.Text.Trim()))
        {
            await OnError("Major already exists. Name must be unique.");
            return;
        }
        var major = new MajorModel
        {
            Major = MajorEntry.Text.Trim()
        };
        await DataRepository.AddOrUpdateMajor(major);
        await LogEvent($"Created new major: {major.Major}");
        await OnSuccess($"Major '{major.Major}' created successfully.");
        await Shell.Current.GoToAsync($"{nameof(SelectTaskPage)}?ActiveUserId={ActiveUserId}");
    }

    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        if (MajorPicker.SelectedItem is null || string.IsNullOrWhiteSpace(MajorEntry.Text))
        {
            await OnError("Please Select a major to delete.");
            return;
        }
        var existingMajors = await DataRepository.GetMajors();
        var major = existingMajors.FirstOrDefault(m => m.Major == MajorEntry.Text.Trim());
        if (major != null)
        { 
            if (await DataRepository.CanDeleteMajor(major.Id) == false)
            {
                await OnError("This major is assigned to a student and cannot be deleted.");
                return;
            }
        }
        if (major is null)
        {
            await OnError("Major not found.");
            return;
        }
        // delete associated courses
        var courses = await DataRepository.GetCourses();
        var majorCourses = courses.Where(c => c.MajorId == major.Id).ToList();
        foreach (var course in majorCourses)
        {
            await DataRepository.DeleteCourse(course.CourseId);
            await LogEvent($"Deleted course: {course.Name} from major: {major.Major}");
        }

        await DataRepository.DeleteMajor(major.Id);
        await LogEvent($"Deleted major: {major.Major}");
        await OnSuccess($"Major '{major.Major}' deleted successfully.");
        await Shell.Current.GoToAsync($"{nameof(SelectTaskPage)}?ActiveUserId={ActiveUserId}");
    }

    private async  void listCourses_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (listCourses.SelectedItem != null)
        {
            var existingMajors = await DataRepository.GetMajors();
            var major = existingMajors.FirstOrDefault(m => m.Major == MajorPicker.SelectedItem.ToString());
            if (major is null)
            {
                await OnError("Major not found.");
                return;
            }
            await Shell.Current.GoToAsync($"{nameof(EditCoursePage)}?CourseId={((CourseModel)listCourses.SelectedItem).CourseId}&" +
                $"MajorIdString={major.Id.ToString()}&ActiveUserId={ActiveUserId}");
        }
    }
}