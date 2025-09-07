using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

public partial class UpdatePasswordPage : BaseContentPage
{
    public string UserName { get; set; } = string.Empty;
	public UpdatePasswordPage(IDataRepository dataRepository) : base(dataRepository)
    {
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        var user =  await DataRepository.GetUserById(ActiveUserId);
        UserNameEntry.Text = user.Username;
        UserNameEntry.IsEnabled = false;
    }
    private async void UpdatePasswordButton_Clicked(object sender, EventArgs e)
    {
        var username = UserNameEntry.Text;
        var password = CurrentPasswordEntry.Text;
        var newPassword = NewPasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;
        // Simple validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await OnError("Please enter current password, new password, and confirm password.");
            return;
        }
        if ( newPassword != confirmPassword)
        {
            await OnError("Passwords do not match");
        }
        if (password == newPassword)
        {
            await OnError("New password cannot be the same as the current password.");
            return;
        }
        var user = await DataRepository.GetUserByUsername(username);
        if (user == null)
        {
            await OnError("User not found.");
            return;
        }
        if (user.Id != ActiveUserId)
        {
            await OnError("You cannot update another user's password.");
            return;
        }
        if (PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            // Update the user password
            var hashandsalt = PasswordHelper.HashPassword(newPassword);
            user.PasswordHash = hashandsalt.Hash;
            user.PasswordSalt = hashandsalt.Salt;
            user.IsDefaultPassword = false;
            await DataRepository.AddOrUpdateUser(user);
            await OnSuccess("Password updated successfully.");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await OnError("Incorrect password.");
        }
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}