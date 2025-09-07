namespace C424Assessment.Views;
using CommunityToolkit.Maui.Views;

public partial class ReportPopupHelper : Popup
{
	public ReportPopupHelper(ContentView contentView)
	{
       InitializeComponent();
        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 12,
            Children =
            {
                new Label { Text = "Report Preview", FontSize = 18, FontAttributes = FontAttributes.Bold },
                contentView
            }
        };
    }
}