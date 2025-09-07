using C424Assessment.DataRepository;
using C424Assessment.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C424Assessment.Views;

[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(InstructorId), "InstructorId")]
public partial class InstructorEditPage : BaseContentPage
{
    private string phoneNumberPattern = @"\d{3}-\d{3}-\d{4}";
    private string emailPattern = @"\b[\w\.-]+@[\w\.-]+\.\w{2,}\b";
    public string CourseId { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    private InstructorModel? _instructor;
    public InstructorEditPage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(InstructorId))
        {
            _instructor = await DataRepository.GetInstructorById(int.Parse(InstructorId));
            
            NameEntry.Text = _instructor.Name;
            EmailEntry.Text = _instructor.Email;
            PhoneEntry.Text = _instructor.Phone;
        }
        await UpdatePickerSource();
    }

    private async Task UpdatePickerSource()
    {
        var instructors = await DataRepository.GetInstructors();
        // add instructor names to source
        InstructorPicker.ItemsSource = instructors.Select(i => i.Name).ToList();
        InstructorPicker.SelectedItem = NameEntry.Text;
    }

    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(InstructorId) && string.IsNullOrEmpty(CourseId))
        {
            AddButton_Clicked(sender, e);
            return;
        }
        if (await IsValid())
        {
            var instructorModel = new InstructorModel {
                Name = NameEntry.Text,
                Email = EmailEntry.Text,
                Phone = PhoneEntry.Text,
                Id =!string.IsNullOrEmpty(InstructorId)? int.Parse(InstructorId) : 0 }; // if id is not set, it will be auto incremented

            await  DataRepository.AddOrUpdateInstructor(instructorModel);
            // if the picker was used we may have a different id to pass
            await UpdateCourseInstructorId(instructorModel);
            await OnSuccess($"{instructorModel.Name} updated");
            await LogEvent("Instructor updated");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task UpdateCourseInstructorId(InstructorModel instructor)
    {
        if (string.IsNullOrEmpty(CourseId) || instructor is null)
        {
            return;
        }

        // update the course insutctor id
        var course = await DataRepository.GetCourseById(int.Parse(CourseId));
        if (course != null)
        {
            course.InstructorId = instructor.Id;
            await DataRepository.AddOrUpdateCourse(course);
        }
        else
        {
            throw new InvalidOperationException("Course not found for the given CourseId.");
        }
    }

    public async Task<bool> IsValid()
    {
        if (string.IsNullOrEmpty(NameEntry.Text))
        {
            await OnError("Instructor name is null or empty");
            return false;
        }
        if (string.IsNullOrEmpty(PhoneEntry.Text))
        {
            await OnError("Phone number is null or empty");
            return false;
        }
        var match = Regex.Match(PhoneEntry.Text, phoneNumberPattern);
        if (!match.Success)
        {
            await OnError("Phone number is not valid");
            return false;
        }
        if (string.IsNullOrEmpty(EmailEntry.Text))
        {
            await OnError("Email is null or empty");
            return false;
        }
        match = Regex.Match(EmailEntry.Text, emailPattern);
        if (!match.Success)
        {
            await OnError("Email is not valid");
            return false;
        }
        return true;
    }
    private async void InstructorPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // get selected instructor name
        var selectedInstructorName = InstructorPicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedInstructorName))
        {
            return;
        }
        // find instructor by name
        var instructors = await DataRepository.GetInstructors();
        var instructor = instructors.FirstOrDefault(i => i.Name == selectedInstructorName);
        if (instructor != null)
        {
            NameEntry.Text = instructor.Name;
            EmailEntry.Text = instructor.Email;
            PhoneEntry.Text = instructor.Phone;
            InstructorId = instructor.Id.ToString();
        }
    }

    private async void  FinishButtons_OnDelete(object sender, EventArgs e)
    {
        // delete selected instructor do not navigate back with out an instructor
        if (string.IsNullOrEmpty(InstructorId))
        {
            await OnError("Instructor ID is null or empty");
            return;
        }
        var instructorId = int.Parse(InstructorId);
        await DataRepository.DeleteInstructor(instructorId);

        InstructorPicker.SelectedItem = null;
        NameEntry.Text = "";
        PhoneEntry.Text = "";
        EmailEntry.Text = "";
        await UpdatePickerSource();
    }

    private async void AddButton_Clicked(object sender, EventArgs e)
    {
        // if add is clicked and not data is changed do not add a new instructor
        if (!await IsValid()) { return;  }
        var instructors = await DataRepository.GetInstructors();
        if (instructors.Any(i => i.Name == NameEntry.Text && i.Email == EmailEntry.Text && i.Phone == PhoneEntry.Text))
        {
            await OnError("Instructor already exists with the same name, email and phone number.");
            return;
        }
        var instructorModel = new InstructorModel
        {
            Name = NameEntry.Text,
            Email = EmailEntry.Text,
            Phone = PhoneEntry.Text
        };
        // set id to 0, id will be auto incremented
        instructorModel.Id = 0;
        await DataRepository.AddOrUpdateInstructor(instructorModel);
        var instructorsList = await DataRepository.GetInstructors();
        var latestInstructor = instructorsList.OrderByDescending(i => i.Id).FirstOrDefault();
        if (latestInstructor == null)
        {
            await OnError("Failed to retrieve the latest instructor after adding.");
            return;
        }
        instructorModel.Id = latestInstructor.Id;
        // after adding instructor, update the course instructor id
        if (!string.IsNullOrWhiteSpace(CourseId))
        {
            // if course id is set, update the course instructor id
            await UpdateCourseInstructorId(instructorModel);
            await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}&InstructorId={instructorModel.Id}");
        }
        await Shell.Current.GoToAsync($"..?InstructorId=[instructorModel.Id]");

    }

}