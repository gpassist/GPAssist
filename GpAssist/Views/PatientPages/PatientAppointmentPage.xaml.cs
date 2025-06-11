using GpAssist.Helpers;
using GpAssist.Models;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;


namespace GpAssist.Views.PatientPages
{


    /// ViewModel za prikaz termina
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }

        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Scheduled" => "Green",
                    "Completed" => "Blue",
                    "Cancelled" => "Red",
                    "Canceled" => "Red",
                    _ => "Black"
                };
            }
        }
    }

    public partial class PatientAppointmentsPage : ContentPage
    {

        private ObservableCollection<AppointmentViewModel> _appointments;

        private Patient _currentPatient;


        public PatientAppointmentsPage()
        {
            InitializeComponent();

            //BindingContext = this;
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadAppointments();
        }


        //ucitavanje svih termina
        private async void LoadAppointments()
        {
            try
            {
                // trenutni korisnik
                _currentPatient = await App.Database.GetPatientByUserId(App.CurrentUser.Id);


                var appointments = await App.Database.GetAppointmentByPatientIdAsync(_currentPatient.Id);

                Debug.WriteLine($"Number of appointments: {appointments.Count}");
                // kreiranje view modela

                _appointments = new ObservableCollection<AppointmentViewModel>();

                foreach (var appointment in appointments)
                {
                    // povlacim dokotra

                    var doctor = await App.Database.GetDoctorByIdAsync(appointment.DoctorId);

                    Debug.WriteLine($"Appointment ID: {appointment.Id}, PatientId: {appointment.PatientId}, DoctorId: {appointment.DoctorId}, Date: {appointment.AppointmentDate}, Status: {appointment.Status}");

                    // pravim view model

                    _appointments.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        DoctorName = doctor.Name,
                        AppointmentDate = appointment.AppointmentDate.Date,
                        AppointmentTime = appointment.AppointmentDate.TimeOfDay,
                        Status = appointment.Status

                    });
                }

                // izvor collection viewa

                ApointmentsCollectionView.ItemsSource = _appointments;


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data in PatientAppointments LoadData: {ex.Message}", "OK");
            }

        }

        //dodavanje novog termina 

        private async void OnAddAppointmentClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MakeAppointmentPage());
        }

        //logout

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure that you want to log out", "Yes", "No");
            if (confirm)
            {
                try
                {
                    AuthenticationService.Logout();
                    await Navigation.PushAsync(new MainPage());
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to logout: {ex.Message}", "OK");
                }
            }
        }



        // edit
        private async void OnEditAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                //kako uzimamo button koji je kliknut
                var button = sender as Button;

                if (button == null || button.CommandParameter == null)
                {
                    await DisplayAlert("Error", "Invalid appointment selection", "OK");
                    return;
                }

                Debug.WriteLine($"Button CommandParameter: {button.CommandParameter}");
                Debug.WriteLine($"Button CommandParameter Type: {button.CommandParameter?.GetType().Name ?? "null"}");

                //dohvacamo id termina

                var appointmentId = (int)button.CommandParameter;

                //na osnovi id-a trba izvuci termin iz baze
                var appointment = await App.Database.GetAppointmentById(appointmentId);

                //prebaci korisnika na stranicu za editovanje termina
                await Navigation.PushAsync(new EditAppointmentPage(appointment));

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to edit appointment: {ex.Message}", "OK");
            }
        }


        private async void OnCancelAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                //uzmi dugme

                var button = sender as Button;

                if (button == null || button.CommandParameter == null)
                {
                    await DisplayAlert("Error", "Invalid appointment selection", "OK");
                    return;
                }

                //isto id treba

                int appointmentId;
                if (!int.TryParse(button.CommandParameter.ToString(), out appointmentId))
                {
                    await DisplayAlert("Error", $"Invalid appointment id: {button.CommandParameter}", "OK");
                    return;
                }

                bool confirm = await DisplayAlert("Cancel", "Are you sure that you want to cancel this appointment", "Yes", "No");

                if (!confirm)
                {
                    return;
                }

                //izvuci termin iz baze

                var appointment = await App.Database.GetAppointmentById(appointmentId);

                if (appointment == null)
                {
                    await DisplayAlert("Error", $"Appointment not found: {appointmentId} {button.CommandParameter}", "OK");
                    return;
                }

                //promjeni status termina sa "Scheduled" na "Cancelled"

                appointment.Status = "Cancelled";


                //spasi promjene
                await App.Database.SaveAppointemntAsync(appointment);


                //osvjezi podatke
                LoadAppointments();

                await DisplayAlert("Success", "Appointment canceled successfully", "OK");

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to cancel appointment: {ex.Message}", "OK");
            }
        }
    }
}

