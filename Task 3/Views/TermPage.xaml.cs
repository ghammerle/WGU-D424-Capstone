using C424Assessment.DataRepository;
using C424Assessment.Models;
using System.Collections.ObjectModel;
using Plugin.LocalNotification;
using C424Assessment.Controls;
using static C424Assessment.Models.Enumerations;


#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
#endif

namespace C424Assessment.Views;

[QueryProperty(nameof(TermId), "TermId")]
public partial class TermPage : BaseContentPage
{
    public string TermId { get; set; } = string.Empty;

    private Dictionary<int, List<CourseMapModel>> _termDictionary = new Dictionary<int, List<CourseMapModel>>();
    public TermPage(IDataRepository SQLiteRepository) : base(SQLiteRepository)
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        TermView.ActiveUserId = ActiveUserId;

        var courseMap = await DataRepository.GetCourseMapDataByUserId(ActiveUserId);
        // termId will contain multple entires inthe map we need only unique ids
        _termDictionary = await DataRepository.GetStudentTerms(ActiveUserId);

        if (!_termDictionary.Any())
        {
            await OnError("No terms found. Student has no course mapped.");
            return;
        }

        await UpdatePickerSource();
        if (string.IsNullOrEmpty(TermId))
        {
            // if no term is selected, we will select the current term
            // this is the default entry point after student login
            var currentTerm = DataRepository.GetCurrentTerm(_termDictionary);
            await UpdateTerm(currentTerm);
        }
        else
        {
            // if TermId is set, we will update the term with that id
            // this is the entry point when navigating back to this page
            // after some task
            var term = DataRepository.GetTermById(_termDictionary, int.Parse(TermId));
            await UpdateTerm(term);
        }
        // all courses are 3 units
        var unitsCompleted = _termDictionary.Sum(t => t.Value.Count(c => c.CourseStatusId == (int)CourseStatus.Completed)) * Constants.UNITS_PER_COURSE;
        var percentComplete = (double)unitsCompleted / (double)Constants.UNITS_REQUIRED * 100.00;
        PercentCompleteLabel.Text = $"{percentComplete:F2}%";

        var unitsEnrolled = _termDictionary.Sum(t => t.Value.Count()) * Constants.UNITS_PER_COURSE;
        UnitsEnrolledLabel.Text = unitsEnrolled.ToString();
        UnitsEnrolledLabel.TextColor = unitsEnrolled < Constants.UNITS_REQUIRED ? Colors.Red : Colors.Black;
        UnitsRequiredLabel.Text = $" out of {Constants.UNITS_REQUIRED} units";

    }

    private async Task UpdatePickerSource()
    {
        _termDictionary = await DataRepository.GetStudentTerms(ActiveUserId);

        // Term Name will be "Term" + TermId
        var termNames = _termDictionary
            .Select(t => $"Term {t.Key}")
            .ToList();
        TermPicker.ItemsSource = termNames;

        if (!string.IsNullOrEmpty(TermId) && int.TryParse(TermId, out int parsedId))
        {
            // Make sure this term actually exists in the dictionary
            if (_termDictionary.ContainsKey(parsedId))
            {
                TermPicker.SelectedIndex = termNames.IndexOf($"Term {parsedId}");
            }
        }
    }

    public async Task UpdateTerm(TermModel term)
    {
        
        if (term == null)
        {
            await OnError("Term not found.");
            return;
        }
        var courses = await DataRepository.GetCoursesByTermId(term.TermId, ActiveUserId);
        term.Courses = new ObservableCollection<CourseModel>(courses);
        TermView.BindingContext = term;
        TermView.TermId = term.TermId;
        TermPicker.SelectedItem = $"Term {term.TermId}";
        var currentTermCourses = _termDictionary[term.TermId];
        TermView.StartDate = currentTermCourses.Min(c => c.StartDate);
        TermView.EndDate = currentTermCourses.Max(c => c.EndDate);
        TermId = term.TermId.ToString();
        if (currentTermCourses.Max(c => c.EndDate).Date < DateTime.Today)
        {
            AddCourseButton.IsEnabled = false;
        }
    }

    private async void TermPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (TermPicker.SelectedItem is string selectedTerm)
        {
            // get selected term id from the selected term string
            var termId = int.Parse(selectedTerm.Split(' ')[1]);
            try
            { 
                var term = DataRepository.GetTermById(_termDictionary, termId);
                await UpdateTerm(term);
            }
            catch (Exception ex) 
            {
                await OnError($"Term {termId} not found. {ex.Message}");
            }
        }
    }

    private async void AddCourseButton_Clicked(object sender, EventArgs e)
    {
        if (_termDictionary.Sum(t => t.Value.Count()) * Constants.UNITS_PER_COURSE >= Constants.UNITS_REQUIRED)
        {
            await OnError("The student has enough courses to satisfy the requirements.");
            return;
        }
        if (string.IsNullOrEmpty(TermId))
        {
           await OnError("TermId is not set.");
            return;
        }

        if (_termDictionary[int.Parse(TermId)].Count >= Constants.MAX_COURSES)
        {
            await OnError($"This Term is full. no more than {Constants.MAX_COURSES} allowed per term.");
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(CourseSelectorPage)}?TermId={TermId}&ActiveUserId={ActiveUserId}");
    }

    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(UserLoginPage)}");
    }
}

