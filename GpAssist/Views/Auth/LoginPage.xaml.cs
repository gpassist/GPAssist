using GpAssist.Helpers;
using GpAssist.Views.PatientPages;
using GpAssist.Views.StaffPages;
namespace GpAssist.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegistrationPage());
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter username and password", "OK");
            return;
        }

        var user = await AuthenticationService.Login(username, password);

        if (user != null)
        {
            Application.Current.MainPage = new NavigationPage(
                user.Role == "Patient" ?
                new MakeAppointmentPage() as Page
                : new StaffDashboardPage() as Page
            );
        }
        else
        {
            await DisplayAlert("Error", "Invalid username or password", "OK");
        }
    }
}