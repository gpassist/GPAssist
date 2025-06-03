using GpAssist.Helpers;
using System.Collections.ObjectModel;

namespace GpAssist.Views.StaffPages
{

    public class StaffAppointemntViewModel
    {
        public int Id { get; set; }

        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public DateTime AppointmentDate { get; set; }

        public DateTime AppointmentTime { get; set; }
        public string Status { get; set; }
        public string PatientEmail { get; set; }

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

    public partial class StaffDashboardPage : ContentPage
    {
        private ObservableCollection<StaffAppointemntViewModel> _appointments;

        public StaffDashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUpcoingAppointments();
        }

        private async void LoadUpcoingAppointments()
        {
            try
            {
                // dohvacanje termina za nardenih 7 danaa

                var now = DateTime.Now;

                var appointments = await App.Database.GetAllAppointmentsAsync();

                // na danasni dan dodao sam 7 i tako filter opalio

                var filteredAppointments = appointments.Where(a => a.AppointmentDate <= now.AddDays(7)).ToList();

                // sortiranje po datumu

                filteredAppointments = filteredAppointments.OrderBy(a => a.AppointmentDate).ToList();

                // mapiranje na view model
                //init
                _appointments = new ObservableCollection<StaffAppointemntViewModel>();

                foreach (var appointment in filteredAppointments)
                {

                    // dohvacanje doktora i pacijenata

                    var doctor = await App.Database.GetDoctorByIdAsync(appointment.DoctorId);

                    var patient = await App.Database.GetPatientByIdAsync(appointment.PatientId);

                    var patientUser = await App.Database.GetUserByIdAsync(patient.UserId);

                    //godinee

                    int age = DateTime.Now.Year - patient.DateOfBirth.Year;
                    if (DateTime.Now.DayOfYear < patient.DateOfBirth.DayOfYear) age--;

                    // kreiranje view modela

                    _appointments.Add(new StaffAppointemntViewModel
                    {
                        Id = appointment.Id,
                        DoctorName = doctor.Name,
                        PatientName = patientUser.Username,
                        PatientAge = age,
                        AppointmentDate = appointment.AppointmentDate.Date,
                        AppointmentTime = appointment.AppointmentDate.ToLocalTime(),
                        Status = appointment.Status,
                        PatientEmail = patientUser.Email
                    });

                    // postavljanje item source-a
                    AppointmentsCollectionView.ItemsSource = _appointments;
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load UpcommingAppointemnts: {ex.Message}", "OK");
            }
        }


        private async void OnAllAppointmentsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AllAppointmentsPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                AuthenticationService.Logout();
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
        }

        private async void OnEditAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                // dohvacanje selektovanog termina

                var button = sender as Button;

                // id termina

                var appointmentId = (int)button.CommandParameter;

                // dohvacanje termina

                var appointment = await App.Database.GetAppointmentById(appointmentId);

                // na edit page

                await Navigation.PushAsync(new StaffEditAppointmentPage(appointment));


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
                // dohvacanje selektovanog termina

                var button = sender as Button;

                // id termina

                var appointmentId = (int)button.CommandParameter;

                bool confirm = await DisplayAlert("Cancel Appointment", "Are you sure you want to cancel this appointment?", "Yes", "No");
                if (!confirm) return;

                // dohvacanje termina
                var appointment = await App.Database.GetAppointmentById(appointmentId);

                // update statusa

                appointment.Status = "Cancelled";

                //spasi izmjene

                await App.Database.SaveAppointemntAsync(appointment);

                LoadUpcoingAppointments();

                await DisplayAlert("Success", "Appointment canceled successfully", "OK");


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to cancel appointment: {ex.Message}", "OK");

            }
        }

    }

}
