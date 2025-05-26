using GpAssist.Helpers;
using System.Collections.ObjectModel;
using GpAssist.Views.PatientPages;
using GpAssist.Models;
using System.Diagnostics;

namespace GpAssist.Views.PatientPages;

public partial class MakeAppointmentPage : ContentPage
{
    private Patient _currentPatient;

    private ObservableCollection<Doctor> _doctors;

    public DateTime TodayDate => DateTime.Today;
    public MakeAppointmentPage()
    {
        InitializeComponent();

        BindingContext = this;

        LoadData();
    }

    private async void LoadData()
    {
        try
        {

            // ucitavamo doktore

            var doctors = await App.Database.GetAllDoctorsAsync();

            if (doctors == null || !doctors.Any())
            {
                await DisplayAlert("Info", "No doctors available.", "OK");
                return;
            }

            _doctors = new ObservableCollection<Doctor>(doctors);

            // postavljamo doktore u doctorpicker element

            DoctorPicker.ItemsSource = _doctors;
            DoctorPicker.ItemDisplayBinding = new Binding("Name");

            // dohvacanje trenutnog pacijenta

            _currentPatient = await App.Database.GetPatientByUserId(App.CurrentUser.Id);

            if (_currentPatient == null)
            {
                await DisplayAlert("Error", "Patient profile not found", "OK");
                return;
            }

            // postavljanje defaultnog datuma i vremena
            // ne zelim da se rezervacija moze napraviti u istom danu u kojem korisnik pristupa aplikacij

            AppointmentDatePicker.Date = DateTime.Today.AddDays(1);
            AppointmentTimePicker.Time = new TimeSpan(9, 0, 0); // od 9 ujutru


        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data in LoadData: {ex.Message}", "OK");
        }


    }

    private async void OnScheduledAppointmentClicked(object sender, EventArgs e)
    {
        // validacija input polja prvo

        try
        {
            if (DoctorPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a doctor", "OK");
                return;
            }

            if (_currentPatient == null)
            {
                await DisplayAlert("Error", "Patient not found. Please log in again.", "OK");
                return;
            }

            //dohvati doktora kojeg odabere korisnik

            var selectedDoctor = DoctorPicker.SelectedItem as Doctor;

            if (selectedDoctor == null)
            {
                return;
            }

            var appoinmentDateTime = AppointmentDatePicker.Date.Add(AppointmentTimePicker.Time);

            // validacija odabranog vremena

            if (appoinmentDateTime <= DateTime.Now)
            {
                await DisplayAlert("Error", "Please select a future date and time", "OK");
                return;
            }

            // napravi novi termin

            var appointment = new Appointment
            {
                PatientId = _currentPatient.Id,
                DoctorId = selectedDoctor.Id,
                AppointmentDate = appoinmentDateTime,
                Status = "Scheduled",
                Notes = NotesEditor.Text

            };

            // spasi termin u bazu

            int newAppointmentId = await App.Database.SaveAppointemntAsync(appointment);

            Debug.WriteLine($"New appointment id: {newAppointmentId}");

            // ako je dobro

            await DisplayAlert("Horray", "Appointemnt scheduled successfully", "OK");


            // prebaci korisnika na sve zakazane termine
            await Navigation.PushAsync(new PatientAppointmentPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error ScheduleAppointment: {ex.Message}", "OK");
        }
    }

    private async void OnMyAppointmentsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PatientAppointmentPage());
    }

    private async void OnLogoutCliked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure that you want to log out", "Yes", "No");

        if (confirm)
        {
            AuthenticationService.Logout();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}