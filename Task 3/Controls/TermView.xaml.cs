using C424Assessment.Models;
using C424Assessment.Views;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;

namespace C424Assessment.Controls;

public partial class TermView : ContentView
{
    public static readonly BindableProperty CoursesProperty =
    BindableProperty.Create(
        nameof(Courses),
        typeof(ObservableCollection<CourseModel>),
        typeof(TermView),
        default(ObservableCollection<CourseModel>));

    public ObservableCollection<CourseModel> Courses
    {
        get => (ObservableCollection<CourseModel>)GetValue(CoursesProperty);
        set => SetValue(CoursesProperty, value);
    }
    public TermView()
	{
		InitializeComponent();
        // use Term Edit page to change dates
        StartDatePicker.IsEnabled = false;
        EndDatePicker.IsEnabled = false;
    }

    public static readonly BindableProperty TermIdProperty =
BindableProperty.Create(nameof(TermId), typeof(int), typeof(TermView), 0);

    public int TermId
    {
        get => (int)GetValue(TermIdProperty);
        set => SetValue(TermIdProperty, value);
    }

    public static readonly BindableProperty ActiveUserIdProperty =
BindableProperty.Create(nameof(ActiveUserIdProperty), typeof(int), typeof(TermView), 0);

    public int ActiveUserId
    {
        get => (int)GetValue(ActiveUserIdProperty);
        set => SetValue(ActiveUserIdProperty, value);
    }

    public DateTime StartDate
    {
        set => StartDatePicker.Date = value;
    }

    public DateTime EndDate
    {
        set => EndDatePicker.Date = value;
    }

    private void listCourses_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (listCourses.SelectedItem != null)
        {
            Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={((CourseModel)listCourses.SelectedItem).CourseId}&" +
                $"TermId={TermId}&" +
                $"ActiveUserId={ActiveUserId}");
        }
    }

    private void listCourses_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        listCourses.SelectedItem = null;
    }
}