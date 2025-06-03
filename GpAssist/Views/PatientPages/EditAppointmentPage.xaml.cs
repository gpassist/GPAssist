using GpAssist.Models;
using System.Collections.ObjectModel;

namespace GpAssist.Views.PatientPages;

public partial class EditAppointmentPage : ContentPage
{
    private Appointment _appointment;

    private ObservableCollection<Doctor> _doctors;

    public DateTime TodayDate => DateTime.Today;
    public EditAppointmentPage(Appointment appointment)
	{

        
        InitializeComponent();

        _appointment = appointment;
        BindingContext = this;

        LoadData();
    }

    private async void LoadData()
    {

        try
        {
            // uzmi doktore

            var doctors = await App.Database.GetAllDoctorsAsync();

            _doctors = new ObservableCollection<Doctor>(doctors);

            // postavi doktore u picker

            DoctorPicker.ItemsSource = _doctors;
            DoctorPicker.ItemDisplayBinding = new Binding("Name");

            // postavi podatke o pregledu

            var doctor = await App.Database.GetDoctorByIdAsync(_appointment.DoctorId);

            DoctorPicker.SelectedItem = _doctors.FirstOrDefault(d => d.Id == doctor.Id);

            AppointmentDatePicker.Date = _appointment.AppointmentDate.Date;
            AppointmentTimePicker.Time = _appointment.AppointmentDate.TimeOfDay;
            NotesEditor.Text = _appointment.Notes;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }

    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        try
        {
            // prvo validacija

            if (DoctorPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a doctor", "OK");
                return;
            }

            // vrati doktora

            var selectedDoctor = DoctorPicker.SelectedItem as Doctor;

            // postavi podatke o pregledu

            var appointmentDateTime = AppointmentDatePicker.Date.Add(AppointmentTimePicker.Time);

            // valicaija da li je datum u buducnosti

            if (appointmentDateTime <= DateTime.Now)
            {
                await DisplayAlert("Error", "Appointment date must be in the future", "OK");
                return;
            }

            _appointment.DoctorId = selectedDoctor.Id;
            _appointment.AppointmentDate = appointmentDateTime;
            _appointment.Notes = NotesEditor.Text;

            // sacuvaj promene

            await App.Database.SaveAppointemntAsync(_appointment);

            // ako je uspjesno

            await DisplayAlert("Success", "Appointment updated", "OK");

            // idi nazad
            await Navigation.PopAsync();

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }
}