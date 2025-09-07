using C424Assessment.Models;

namespace C424Assessment.Views;

public partial class EventLogReport : ContentView
{
	List<EventLogModel> _eventLogData;

    public List<EventLogModel> EventLogData
	{
        set => EventLogCollection.ItemsSource = value;
    }

    public EventLogReport(List<EventLogModel> events)
	{
		InitializeComponent();
        EventLogCollection.ItemsSource = events;

    }
}