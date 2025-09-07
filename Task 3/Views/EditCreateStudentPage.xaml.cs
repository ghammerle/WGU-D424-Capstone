using C424Assessment.DataRepository;
using C424Assessment.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text.RegularExpressions;

namespace C424Assessment.Views;

public partial class EditCreateStudentPage : BaseContentPage
{
	public EditCreateStudentPage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        var students = await DataRepository.GetStudents();
        // studentNames should be userId - UserName
        List<String> studentNames = new List<string>();
        foreach (var s in students)
        {
            studentNames.Add($"{s.Id}-{s.Username}");
        }
        StudentPicker.ItemsSource = studentNames;

        var majors = await DataRepository.GetActiveMajors();

        var majorNames = majors.Select(m => m.Major).ToList();
        MajorPicker.ItemsSource = majorNames;
        DatePicker.Date = DateTime.Now;
        DatePicker.MinimumDate = DateTime.Now;

    }
    private async void StudentPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // if a student is picked populate revlevant data
        var userId = StudentPicker.SelectedItem?.ToString()?.Split('-')[0];
        if (!string.IsNullOrEmpty(userId))
        {
            var student = await DataRepository.GetUserById(int.Parse(userId));
            var studentData = await DataRepository.GetStudentData(int.Parse(userId));
            // Populate relevant fields with student data
            var names = studentData.Name.Split('.');
            FirstNameEntry.Text = names[0];
            LastNameEntry.Text = names[1];
            var studentMajor = studentData.Major;
            MajorPicker.SelectedItem = studentData.Major;
            FirstNameEntry.IsEnabled = false; // Disable editing of the name
            LastNameEntry.IsEnabled = false; // Disable editing of the name
            MajorPicker.IsEnabled = false; // Disable editing of the major
            ResetPasswordCB.IsChecked = student.IsDefaultPassword;
            DatePicker.MinimumDate = studentData.StartDate > DateTime.Now ? DateTime.Now : studentData.StartDate;
            DatePicker.Date = studentData.StartDate;
            if (studentData.CompletedCourseCount > 0)
            {
                // Disable date picker if student has started
                DatePicker.IsEnabled = false; 
            }

        }
    }

    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        var studentId = StudentPicker.SelectedItem?.ToString()?.Split('-')[0];
        if (await DataRepository.CanDeleteUser(int.Parse(studentId)))
        {
            // No active or completed courses, proceed with deletion
            await DataRepository.DeleteUser(int.Parse(studentId));
            await LogEvent($"Deleted student with ID: {studentId}");
            await OnSuccess("Student deleted successfully.");
            await Shell.Current.GoToAsync("..");
        }
        else
            await OnError("Cannot delete a student with active or completed courses. Please remove all courses before deleting the student.");
    }

    bool IsValidName(string input)
    {
        return Regex.IsMatch(input, @"^[a-zA-Z]+$");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        // check names
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) || string.IsNullOrWhiteSpace(LastNameEntry.Text))
        {
            await OnError("First name and last name cannot be empty.");
            return;
        }
        if (!IsValidName(FirstNameEntry.Text) ||!IsValidName(LastNameEntry.Text))
        {
            await OnError("First name and last name can only contain letters.");
            return;
        }

        // must have a major selected
        if (MajorPicker.SelectedIndex < 0)
        {
            await OnError("Please select a major.");
            return;
        }
        var studentId = StudentPicker.SelectedItem?.ToString()?.Split('-')[0];
        var majors = await DataRepository.GetMajors();
        var majorId = majors.Where(m => m.Major == MajorPicker.SelectedItem.ToString())
                .Select(m => m.Id)
                .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(studentId))
        {
            try
            {
                await DataRepository.CreateStudent(FirstNameEntry.Text, LastNameEntry.Text, majorId, DatePicker.Date);
            }
            catch (Exception ex)
            {
                await OnError($"Failed to create new student{FirstNameEntry.Text} {LastNameEntry.Text}. Error: {ex.Message}");
                return;
            }

            await LogEvent($"Created new student: {FirstNameEntry.Text} {LastNameEntry.Text}");
            await OnSuccess("New student created successfully.");
        }
        else
        {
            // Update existing student
            var existingStudent = await DataRepository.GetUserById(int.Parse(studentId));
            existingStudent.IsDefaultPassword = ResetPasswordCB.IsChecked;
            await DataRepository.AddOrUpdateUser(existingStudent);

            var majormap = await DataRepository.GetMajorMapModel(int.Parse(studentId));
            majormap.StartDate = DatePicker.Date;
            majormap.MajorId = majorId;
            await DataRepository.AddOrUpdateMajorMap(majormap);
            await LogEvent($"Updated student: {existingStudent.Username}");
            await OnSuccess("Student updated successfully.");
        }
        await Shell.Current.GoToAsync("..");
    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        DateTime selected = e.NewDate;
        // Do something with the selected date
        Console.WriteLine($"User picked: {selected}");
        DatePicker.Date = selected;
    }
}