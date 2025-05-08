using GpAssist.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpAssist.Services
{


    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                throw new ArgumentException("Database path is empty.", nameof(dbPath));
            }

            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTablesAsync<User, Patient, Doctor, Appointment>().Wait();

            Debug.WriteLine(dbPath);
        }

        public Task<List<User>> GetUsersAsync()
        {
            return _database.Table<User>().ToListAsync();
        }

        public Task<User> GetUserByIdAsync(int userId)
        {
            return _database.Table<User>().FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<User> GetUserByUsername(string username)
        {
            return _database.Table<User>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<int> SaveUserAsync(User user)
        {
            if (user.Id == 0)
            {
                await _database.InsertAsync(user);
                return user.Id;
            }
            else
            {
                await _database.UpdateAsync(user);
                return user.Id;
            }
        }

        public Task<int> DeleteUserAsync(User user)
        {
            return _database.DeleteAsync(user);
        }

        public Task<List<Patient>> GetPatientsAsync()
        {
            return _database.Table<Patient>().ToListAsync();
        }

        public Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return _database.Table<Patient>().FirstOrDefaultAsync(p => p.Id == patientId);
        }


        public Task<Patient> GetPatientByUserId(int userId)
        {
            return _database.Table<Patient>().FirstOrDefaultAsync(p => p.UserId == userId);
        }


        public async Task<int> SavePatientAsync(Patient patient)
        {
            //return patient.Id == 0 ? _database.InsertAsync(patient) : _database.UpdateAsync(patient);

            if (patient.Id == 0)
            {
                await _database.InsertAsync(patient);
                return patient.Id;
            }
            else
            {
                await _database.UpdateAsync(patient);
                return patient.Id;
            }
        }

        public Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return _database.Table<Doctor>().ToListAsync();
        }

        public Task<Doctor> GetDoctorByIdAsync(int doctorId)
        {
            return _database.Table<Doctor>().FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        public Task<int> SaveDoctorAsync(Doctor doctor)
        {
            return doctor.Id == 0 ? _database.InsertAsync(doctor) : _database.UpdateAsync(doctor);
        }
        public async Task SeedDatabaseAsync()
        {
            try
            {
                var doctors = await GetAllDoctorsAsync();

                if (doctors.Count == 0)
                {
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Kerim Fadzan", Specialization = "Cardiology" });
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Armin Djulepa", Specialization = "Dermatology" });
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Kenan Catic", Specialization = "Neurology" });
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Mirza Djono", Specialization = "Pediatrics" });
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Denis Kukuljac", Specialization = "Pediatrics" });
                    await SaveDoctorAsync(new Doctor { Name = "Dr. Alma Pandur", Specialization = "Pediatrics" });
                }

                var users = await GetUsersAsync();

                var staffUsers = users.Where(u => u.Role == "Staff").ToList();

                if (staffUsers.Count == 0)
                {
                    await SaveUserAsync(new User
                    {
                        Username = "admin",
                        Email = "gpAssist@gmail.com",
                        Password = HashPassword("admin123"),
                        Role = "Staff",
                        CreatedAt = DateTime.Now,
                    }
                    );

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
            }

        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


    }
}
