using C424Assessment.Views;

namespace C424Assessment
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            Console.WriteLine("AppShell constructor started");
            InitializeComponent();
            Console.WriteLine("AppShell constructor completed");
            // Routes
            Routing.RegisterRoute(nameof(UserLoginPage), typeof(UserLoginPage));
            Routing.RegisterRoute(nameof(TermPage), typeof(TermPage));
            Routing.RegisterRoute(nameof(CoursePage), typeof(CoursePage));
            Routing.RegisterRoute(nameof(InstructorEditPage), typeof(InstructorEditPage));
            Routing.RegisterRoute(nameof(AssessmentEditPage), typeof(AssessmentEditPage));
            Routing.RegisterRoute(nameof(NotesPage), typeof(NotesPage));
            Routing.RegisterRoute(nameof(EditTermPage), typeof(EditTermPage));
            Routing.RegisterRoute(nameof(EditCoursePage), typeof(EditCoursePage));
            Routing.RegisterRoute(nameof(SelectTaskPage), typeof(SelectTaskPage));
            Routing.RegisterRoute(nameof(UpdatePasswordPage), typeof(UpdatePasswordPage));
            Routing.RegisterRoute(nameof(EditCreateStudentPage), typeof(EditCreateStudentPage));
            Routing.RegisterRoute(nameof(CreateMajorPage), typeof(CreateMajorPage));
            Routing.RegisterRoute(nameof(CourseSelectorPage), typeof(CourseSelectorPage));
        }
    }
}
