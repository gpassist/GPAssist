using GpAssist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GpAssist.Helpers
{
    public static class AuthenticationService
    {
        public static async Task<User> Login(string username, string password)
        {
            var user = await App.Database.GetUserByUsername(username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                App.CurrentUser = user;
                return user;
            }

            return null;
        }


        public static async Task<bool> Register(User user, Patient patientDetails = null)
        {
            var existingUser = await App.Database.GetUserByUsername(user.Username);

            if (existingUser != null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.CreatedAt = DateTime.Now;

            int userId = await App.Database.SaveUserAsync(user);

            if (user.Role == "Patient" && patientDetails != null)
            {
                patientDetails.UserId = userId;
                await App.Database.SavePatientAsync(patientDetails);
            }

            return true;
        }


        public static void Logout()
        {
            App.CurrentUser = null;
        }
    }
}
