using GpAssist.Models;
using System.Collections.ObjectModel;

namespace GpAssist.Views.StaffPages
{

    public partial class StaffEditAppointmentPage : ContentPage
    {

        private Appointment _appointment;

        private ObservableCollection<Doctor> _doctors;

        private ObservableCollection<Patient> _patients;

        public StaffEditAppointmentPage(Appointment appointment)
        {
            InitializeComponent();

            _appointment = appointment;

            //ucitaj podatke
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // uzmi doktore

                var doctors = await App.Database.GetAllDoctorsAsync();

                _doctors = new ObservableCollection<Doctor>(doctors);

                // stavljam ih na picker

                DoctorPicker.ItemsSource = _doctors;

                DoctorPicker.ItemDisplayBinding = new Binding("Name");

                // uzmi pacijente

                var patients = await App.Database.GetPatientsAsync();
                _patients = new ObservableCollection<Patient>(patients);

                // stavljam ih na picker

                PatientPicker.ItemsSource = _patients;
                PatientPicker.ItemDisplayBinding = new Binding("Name");

                // postavi podatke u kontrole

                var doctor = await App.Database.GetDoctorByIdAsync(_appointment.DoctorId);

                var patient = await App.Database.GetPatientByIdAsync(_appointment.PatientId);

                DoctorPicker.SelectedItem = _doctors.FirstOrDefault(d => d.Id == doctor.Id);
                PatientPicker.SelectedItem = _patients.FirstOrDefault(p => p.Id == patient.Id);

                StatusPicker.SelectedItem = _appointment.Status;
                AppointmentDatePicker.Date = _appointment.AppointmentDate;
                AppointmentTimePicker.Time = _appointment.AppointmentDate.TimeOfDay;
                NotesEditor.Text = _appointment.Notes;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data in StaffEditAppointmenst: {ex.Message}", "OK");
            }


        }


        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            try
            {

                if (DoctorPicker.SelectedItem == null || PatientPicker.SelectedItem == null || StatusPicker.SelectedItem == null)
                {
                    await DisplayAlert("Error", "Please select doctor, patient and status", "OK");
                    return;
                }

                var selectedDoctor = DoctorPicker.SelectedItem as Doctor;
                var selectedPatient = PatientPicker.SelectedItem as Patient;
                var selectedStatus = StatusPicker.SelectedItem.ToString();

                // kreiraj novi appointment

                var appointmentDateTime = AppointmentDatePicker.Date.Add(AppointmentTimePicker.Time);

                _appointment.DoctorId = selectedDoctor.Id;
                _appointment.PatientId = selectedPatient.Id;
                _appointment.AppointmentDate = appointmentDateTime;
                _appointment.Status = selectedStatus;
                _appointment.Notes = NotesEditor.Text;


                // sacuvaj promjene

                await App.Database.SaveAppointemntAsync(_appointment);

                await DisplayAlert("Success", "Appointment changes saved", "OK");

                await Navigation.PopAsync();


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save changes in StaffEditAppointmenst: {ex.Message}", "OK");
            }
        }
    }
}