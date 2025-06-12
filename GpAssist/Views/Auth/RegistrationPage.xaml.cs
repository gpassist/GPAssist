using GpAssist.Helpers;
using GpAssist.Models;
using System.Threading.Tasks;
namespace GpAssist.Views.Auth;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
    }

    private async void Register_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(UsernameEntry.Text) ||
           string.IsNullOrEmpty(EmailEntry.Text) ||
           string.IsNullOrEmpty(PasswordEntry.Text) ||
           string.IsNullOrEmpty(NameEntry.Text) ||
           string.IsNullOrEmpty(PhoneEntry.Text) ||
           string.IsNullOrEmpty(AddressEditor.Text)
           )
        {

            await DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        var user = new User
        {
            Username = UsernameEntry.Text,
            Email = EmailEntry.Text,
            Password = PasswordEntry.Text,
            Role = "Patient",
            CreatedAt = DateTime.Now,
        };

        var patient = new Patient
        {
            Name = UsernameEntry.Text,
            DateOfBirth = DateOfBirthPicker.Date,
            PhoneNumber = PhoneEntry.Text,
            Address = AddressEditor.Text,
        };

        //register process

        bool success = await AuthenticationService.Register(user, patient);

        if (success)
        {
            await DisplayAlert("Success", "Registration successful! Please login.", "OK");
            await Navigation.PopAsync(); // Go back to login page
        }
        else
        {
            await DisplayAlert("Error", "Username already exists", "OK");
        }




    }

    private async void LoginTapped_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync(); // Go back to login page
    }


}