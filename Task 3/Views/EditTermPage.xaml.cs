using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

[QueryProperty(nameof(TermId), "TermId")]
public partial class EditTermPage : BaseContentPage
{
    public string TermId { get; set; } = string.Empty;
    private TermModel? _term;
    private const int MAX_COURSES = 6;
    public EditTermPage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load the term data if TermId is set
        if (!string.IsNullOrEmpty(TermId))
        {
            //_term = await DataRepository.GetTermById(int.Parse(TermId));
            //TermNameEntry.Text = _term.Name;
            //TermStartDatePicker.Date = _term.StartDate;
            //TermEndDatePicker.Date = _term.EndDate;
            //listCourses.ItemsSource = await DataRepository.GetCoursesByTermId(int.Parse(TermId), ActiveUserId);
        }
        else
        {
            // Initialize with default values if no TermId is provided
            TermStartDatePicker.Date = DateTime.Now;
            TermEndDatePicker.Date = DateTime.Now.AddMonths(6);
        }
        await UpdateAvailableCoursesPicker();
    }

    private async Task UpdateAvailableCoursesPicker()
    {

        // TODO: this need to be updated

        // course picker will contain course not in another term or not in the listCourses (these are not associated until saving)
        // if a course was removed using Delete the db is not updated so we need to add that course to AvailableCoursesPicker
        var courses = await DataRepository.GetCourses();
        var currentCourses = listCourses.ItemsSource?.Cast<CourseModel>().ToList() ?? new List<CourseModel>();

        var availableCourses = courses
            .Where(c =>
                !currentCourses.Any(tc => tc.CourseId == c.CourseId)) // not currently assigned in UI
            .DistinctBy(c => c.CourseId) // avoids duplicates if both conditions match
            .Select(c => c.Name)
            .ToList();
        AvailableCoursesPicker.ItemsSource = availableCourses;
        if (currentCourses.Count >= MAX_COURSES)
        {
            AvailableCoursesPicker.IsEnabled = false;
            AvailableCoursesPicker.SelectedItem = null;
        }
        else
        {
            AvailableCoursesPicker.IsEnabled = true;
        }
    }

    private async void AvailableCoursesPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // when a course is selcted we will add that course to the Course List View
        // then update the list of available courses
        if (AvailableCoursesPicker.SelectedItem is string selectedCourseName)
        {
            
            var course = await DataRepository.GetCourseByName(selectedCourseName);
            if (course != null)
            {
                // Add the course to the listCourses
                var currentCourses = listCourses.ItemsSource?.Cast<CourseModel>().ToList() ?? new List<CourseModel>();
                currentCourses.Add(course);
                listCourses.ItemsSource = currentCourses;
                // Update the available courses picker
                AvailableCoursesPicker.SelectedItem = null;
                await UpdateAvailableCoursesPicker();
            }
        }
    }

    private async void listCourses_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        // when a course is selected we will show a dialog to validate removing the course from the term
        // if they say no do nothing, if yes remove the course and update the list of available courses
        if (listCourses.SelectedItem != null)
        {
            var selectedCourse = (CourseModel)listCourses.SelectedItem;
            var result = await DisplayAlert("Remove Course",
                $"Are you sure you want to remove {selectedCourse.Name} from this term?",
                "Yes", "No");
            if (result)
            {
                // Remove the course from the list
                var currentCourses = listCourses.ItemsSource?.Cast<CourseModel>().ToList() ?? new List<CourseModel>();
                currentCourses.Remove(selectedCourse);
                listCourses.ItemsSource = currentCourses;
                // Update the available courses picker
                await UpdateAvailableCoursesPicker();
            }
            listCourses.SelectedItem = null; 
        }
    }

    private void listCourses_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        listCourses.SelectedItem = null;
    }

    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        await OnError($"Implement this!!!!!!!!!!!!!!!!!");
        //// if the term is valid we will save it to the database
        //// then navigate back to the Term Page with this term in view
        //if (await IsValid())
        //{
        //    try
        //    {
        //        await DataRepository.AddOrUpdateTerm(new TermModel
        //        {
        //            // if there is not a term id we will set it to 0 for auto increment
        //            TermId = string.IsNullOrEmpty(TermId) ? 0 : int.Parse(TermId),
        //            Name = TermNameEntry.Text,
        //            StartDate = TermStartDatePicker.Date,
        //            EndDate = TermEndDatePicker.Date,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await OnError($"Error saving term: {ex.Message}");
        //        return;
        //    }
        //    // return to the TermPage with the TermId
        //    if (_term is null)
        //    {
        //        // this is a new term, we need to get the last added term to get the new TermId
        //        var terms = await DataRepository.GetTerms();
        //        if (terms != null && terms.Any())
        //        {
        //            TermId = terms.Last().TermId.ToString();
        //        }
        //        else
        //        {
        //            await OnError("No terms found after saving.");
        //            return;
        //        }
        //    }
        //    // associate courses first disasociate existing courses
        //    var existingCourses = await DataRepository.GetCoursesByTermId(int.Parse(TermId), ActiveUserId);
        //    foreach (var course in existingCourses)
        //    {
        //        course.TermId = null;
        //        await DataRepository.AddOrUpdateCourse(course);
        //    }
        //    var currentCourses = listCourses.ItemsSource?.Cast<CourseModel>().ToList() ?? new List<CourseModel>();
        //    foreach (var course in currentCourses)
        //    {
        //        course.TermId = int.Parse(TermId);
        //        await DataRepository.AddOrUpdateCourse(course);
        //    }
        //    existingCourses = await DataRepository.GetCoursesByTermId(int.Parse(TermId), ActiveUserId);
        //    await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}");
        //}
    }
    private async Task<bool> IsValid()
    {
        if (string.IsNullOrWhiteSpace(TermNameEntry.Text))
        {
            await OnError("Term name cannot be empty.");
            return false;
        }
        if (listCourses.ItemsSource is null || listCourses.ItemsSource.Cast<CourseModel>().Count() == 0)
        {
            await OnError($"You must have at least one course associated with this term.");
            return false;
        }
        if (listCourses.ItemsSource.Cast<CourseModel>().Count() > MAX_COURSES)
        {
            // just incase but this should not be possible
            await OnError($"You can only have {MAX_COURSES} courses associated with a term.");
            return false;
        }
        if (TermStartDatePicker.Date >= TermEndDatePicker.Date)
        {
            await OnError("Term start date must come before term end date. Term must be at least 1 day long.");
            return false;
        }
        var terms = await DataRepository.GetTerms();
        var isEditing = int.TryParse(TermId, out int currentId);
        if (terms.Any(t => (!isEditing || t.TermId != currentId) &&
                           t.StartDate < TermEndDatePicker.Date &&
                           t.EndDate > TermStartDatePicker.Date))
        {
            await OnError($"Term dates overlap with another term. Active Courses Span: {terms.Min(t => t.StartDate.Date).ToString("MMMM d, yyyy")}" +
                $" until {terms.Max(t => t.EndDate.Date).ToString("MMMM d, yyyy")}");
            return false;
        }
        if (terms.Any(t => t.Name.Equals(TermNameEntry.Text, StringComparison.OrdinalIgnoreCase) && (!isEditing || t.TermId != currentId)))
        {
            await OnError("Term name must be unique.");
            return false;
        }

        return true;
    }
    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        await OnError($"Implement this!!!!!!!!!!!!!!!!!");

    }

}