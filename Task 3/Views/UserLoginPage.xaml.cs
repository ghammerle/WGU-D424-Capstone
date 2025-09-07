using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

public partial class UserLoginPage : BaseContentPage
{
	public UserLoginPage(IDataRepository SQLiteRepository) : base(SQLiteRepository)
    {
        Console.WriteLine($"User LoginPage CTOR");
        InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UserNameEntry.Text = "";
        PasswordEntry.Text = "";
    }
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        var username = UserNameEntry.Text;
        var password = PasswordEntry.Text;
        // Simple validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await OnError("Please enter both username and password.");
            return;
        }
        var user = await DataRepository.GetUserByUsername(username);
        if (user == null)
        {
            await OnError("User not found.");
            return;
        }
        if (PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            ActiveUserId = user.Id;
            await LogEvent("User logged in");
            // student users will navigate to thier current term page,
            // admins will go the the SelectTask page
            var userRole = (Enumerations.UserRole)user.UserType;
            if (user.IsDefaultPassword)
            {
                await Shell.Current.GoToAsync($"{nameof(UpdatePasswordPage)}?ActiveUserId={ActiveUserId}");
                return;
            }
            if (Enumerations.UserRole.Administrator == userRole)
            {
                await Shell.Current.GoToAsync($"{nameof(SelectTaskPage)}?ActiveUserId={ActiveUserId}");
            }
            else if (Enumerations.UserRole.Student == userRole)
            {
                //await Shell.Current.GoToAsync($"{nameof(TermPage)}?TermId={TermId}&CourseId={CourseId}");

                await NotificationHelper.CheckNotifications(DataRepository, ActiveUserId);
                await Shell.Current.GoToAsync($"{nameof(TermPage)}?ActiveUserId={ActiveUserId}");
            }
            else
            {
                await OnError("User Role is not defined");
            }
        }
        else
        {
            await OnError("Incorrect password.");
        }

    }
}