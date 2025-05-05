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

    }
}
