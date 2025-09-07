using System.Threading.Tasks;

namespace C424Assessment.Views;

public partial class ReportPage : ContentPage
{
	public ReportPage(ContentView view)
	{
		InitializeComponent();
        ReportView.Content = view;
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}