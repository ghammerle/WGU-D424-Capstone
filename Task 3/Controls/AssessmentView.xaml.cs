using C424Assessment.Views;

namespace C424Assessment.Controls;

public partial class AssessmentView : ContentView
{
	public AssessmentView()
	{
		InitializeComponent();
	}

    public string CourseId { get; set; }
    public string AssessmentId { get; set; } = string.Empty;

    private async void OnTapped(object sender, TappedEventArgs e)
    {
            //await Shell.Current.GoToAsync($"{nameof(AssessmentEditPage)}?CourseId={CourseId}&AssessmentId={AssessmentId}");
    }

    public string Name
    {
        set => AssessmentLabel.Text = value;
    }

    public string Type
    {
        set => HeaderLabel.Text = $"{value} Assessment";
    }
    public string DueDate
    {
        set => DueDateLabel.Text = value;
    }
    public bool Notify
    {
        get => EnableNotificationsCB.IsChecked;
        set => EnableNotificationsCB.IsChecked = value;
    }
}