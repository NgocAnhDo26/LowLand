using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage;

namespace DemoListBinding.View.ViewModel
{
    public class LoginViewModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; } = false;

        public bool CanLogin()
        {
            return true;
        }

        public bool Login()
        {
            if (((Username == "admin")
                || Username == "tester"
                )
                && (Password == "1234")
            )
            {
                return true;
            }
            return false;
        }
        public LoginViewModel()
        {
            LoadSavedCredentials();
        }
        public void SaveCredentials()
        {
            if (!RememberMe) return;

            var passwordBytes = Encoding.UTF8.GetBytes(Password);
            var entropyBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(entropyBytes);
            }

            var encryptedBytes = ProtectedData.Protect(
                passwordBytes,
                entropyBytes,
                DataProtectionScope.CurrentUser
            );

            var localStorage = ApplicationData.Current.LocalSettings;
            localStorage.Values["Username"] = Username;
            localStorage.Values["Password"] = Convert.ToBase64String(encryptedBytes);
            localStorage.Values["Entropy"] = Convert.ToBase64String(entropyBytes);

            Debug.WriteLine($"Encrypted password in base 64 is: {Convert.ToBase64String(encryptedBytes)}");
        }
        private void LoadSavedCredentials()
        {
            var localStorage = ApplicationData.Current.LocalSettings;
            var username = (string)localStorage.Values["Username"];
            var encryptedPasswordBase64 = (string)localStorage.Values["Password"];
            var entropyBase64 = (string)localStorage.Values["Entropy"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(encryptedPasswordBase64) || string.IsNullOrEmpty(entropyBase64))
                return;

            try
            {
                var encryptedPasswordBytes = Convert.FromBase64String(encryptedPasswordBase64);
                var entropyBytes = Convert.FromBase64String(entropyBase64);

                var passwordBytes = ProtectedData.Unprotect(
                    encryptedPasswordBytes,
                    entropyBytes,
                    DataProtectionScope.CurrentUser
                );

                Password = Encoding.UTF8.GetString(passwordBytes);
                Username = username;
                RememberMe = true; // Nếu đã lưu thì nhớ luôn
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load saved credentials: {ex.Message}");
            }
        }
    }
}
