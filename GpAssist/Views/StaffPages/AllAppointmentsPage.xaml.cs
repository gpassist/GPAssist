using System.Collections.ObjectModel;

namespace GpAssist.Views.StaffPages
{

    public class AllAppointmentsViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
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
                    _ => "Black",
                };
            }
        }
    }
    public partial class AllAppointmentsPage : ContentPage
    {
        private ObservableCollection<AllAppointmentsViewModel> _allAppointments = new ObservableCollection<AllAppointmentsViewModel>();
        private ObservableCollection<AllAppointmentsViewModel> _filteredAppointments = new ObservableCollection<AllAppointmentsViewModel>();

        public AllAppointmentsPage()
        {
            InitializeComponent();
            StatusPicker.SelectedIndex = 0;


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadAppointments();
        }



        private async void LoadAppointments()
        {
            try
            {

                if (StatusPicker == null || AppointmentsCollectionView == null)
                {
                    await DisplayAlert("Error", "UI components not properly initialized", "OK");
                    return;
                }

                // svi termini
                var appointments = await App.Database.GetAllAppointmentsAsync();


                if (appointments == null)
                {
                    await DisplayAlert("Warning", "No appointments found in database", "OK");
                    _allAppointments = new ObservableCollection<AllAppointmentsViewModel>();
                    ApplyFilter();
                    return;
                }

                // sortiranje
                appointments = appointments.OrderByDescending(a => a.AppointmentDate).ToList();

                _allAppointments = new ObservableCollection<AllAppointmentsViewModel>();

                foreach (var appointment in appointments)
                {
                    var doctor = await App.Database.GetDoctorByIdAsync(appointment.DoctorId);
                    var patient = await App.Database.GetPatientByIdAsync(appointment.PatientId);


                    _allAppointments.Add(new AllAppointmentsViewModel
                    {
                        Id = appointment.Id,
                        PatientName = patient?.Name ?? "Unknown",
                        DoctorName = doctor?.Name ?? "Unknown",
                        AppointmentDate = appointment.AppointmentDate,
                        Status = appointment.Status
                    });
                }

                ApplyFilter();


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load appointments in AllAppointmentsPage: {ex.Message}", "OK");
            }
        }

        private void ApplyFilter()
        {
            try
            {
                if (StatusPicker == null || StatusPicker.SelectedItem == null)
                {
                    return;
                }

                if (_allAppointments == null)
                {
                    _allAppointments = new ObservableCollection<AllAppointmentsViewModel>();
                }


                string statusFilter = StatusPicker.SelectedItem as string ?? "All";

                if (statusFilter == "All")
                {
                    _filteredAppointments = new ObservableCollection<AllAppointmentsViewModel>(_allAppointments);
                }
                else
                {
                    _filteredAppointments = new ObservableCollection<AllAppointmentsViewModel>(_allAppointments?.Where(a => a.Status == statusFilter)
                        ?? new List<AllAppointmentsViewModel>());
                }

                AppointmentsCollectionView.ItemsSource = _filteredAppointments;

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to apply filter: {ex.Message}", "OK");
            }



        }


        private void OnStatusChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private async void OnEditAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;

                if (button?.CommandParameter is not int appointmentId)
                {
                    return;
                }

                var appointment = await App.Database.GetAppointmentById(appointmentId);

                await Navigation.PushAsync(new StaffEditAppointmentPage(appointment));

                //Navigation.PopAsync += (object sender,EventArgs e) => LoadAppointments();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to edit appointment: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;

                if (button?.CommandParameter is not int appointmentId)
                {

                    await DisplayAlert("Error", "Failed to delete appointment: Invalid appointment id", "OK");
                    return;
                }

                bool confirm = await DisplayAlert("Warning", "Are you sure that you wan to delete this appointment?", "YES", "NO");

                if (!confirm)
                {
                    return;
                }

                var appointment = await App.Database.GetAppointmentById(appointmentId);

                await App.Database.DeleteAppointmentAsync(appointment);

                await DisplayAlert("Success", "Appointment deleted", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete appointment: {ex.Message}", "OK");
            }
            finally
            {
                LoadAppointments();
            }

        }
    }
}

