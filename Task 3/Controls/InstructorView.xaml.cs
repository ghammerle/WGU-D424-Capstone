using C424Assessment.Models;
using C424Assessment.Views;

namespace C424Assessment.Controls;

public partial class InstructorView : ContentView
{
    public InstructorModel Instructor { get; set; }

    public string CourseID { get; set; } = string.Empty;

    public InstructorView()
	{
		InitializeComponent();
	}

    public void SetInstructor(InstructorModel model)
    {
        Instructor = model;
        InstuctorLabel.Text = model.Name;
        PhoneLabel.Text = model.Phone;
        EmailLabel.Text = model.Email;
    }




}