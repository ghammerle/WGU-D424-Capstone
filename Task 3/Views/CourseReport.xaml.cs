using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

public partial class CourseReport : ContentView
{
    List<CourseReportItem> _courseData;
    public List<CourseModel> Courses
    {
        set => CourseCollection.ItemsSource = value;
    }
    public CourseReport(List<CourseReportItem> courseData)
    {
        InitializeComponent();
        CourseCollection.ItemsSource = courseData;
    }
}
