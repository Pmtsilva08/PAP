using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Projeto_PAP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPasswordDialog : UserControl
    {
        private bool _isPasswordVisible = false;

        public AddPasswordDialog()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        // Properties to get values
        // Properties to get and set values
        public string SiteName
        {
            get => SiteNameBox.Text.Trim();
            set => SiteNameBox.Text = value;
        }

        public string Url
        {
            get => UrlBox.Text.Trim();
            set => UrlBox.Text = value;
        }

        public string Email
        {
            get => EmailBox.Text.Trim();
            set => EmailBox.Text = value;
        }

        public string Password
        {
            get => PasswordBox.Password;
            set => PasswordBox.Password = value;
        }

        public string Category
        {
            get => (CategoryBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Other";
            set
            {
                foreach (ComboBoxItem item in CategoryBox.Items)
                {
                    if (item.Content?.ToString() == value)
                    {
                        CategoryBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        public string Notes
        {
            get => NotesBox.Text.Trim();
            set => NotesBox.Text = value;
        }

        // Validate form
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SiteName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        // Show/Hide error message
        public void ShowError()
        {
            ErrorMessage.Visibility = Visibility.Visible;
        }

        public void HideError()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        // Toggle password visibility
        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                PasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
                TogglePasswordIcon.Glyph = "\uED1A"; // Eye with line
            }
            else
            {
                PasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
                TogglePasswordIcon.Glyph = "\uE7B3"; // Eye
            }
        }

        // Generate strong password
        private void GeneratePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string generatedPassword = GenerateStrongPassword(16);
            PasswordBox.Password = generatedPassword;
        }

        private string GenerateStrongPassword(int length)
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string allChars = uppercase + lowercase + numbers + special;
            char[] password = new char[length];

            // Ensure at least one of each type
            password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            password[2] = numbers[RandomNumberGenerator.GetInt32(numbers.Length)];
            password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            // Shuffle the password
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        // Password strength indicator
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
            int strength = CalculatePasswordStrength(password);

            PasswordStrengthBar.Value = strength;

            if (strength < 25)
            {
                PasswordStrengthBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Red);
                PasswordStrengthText.Text = "Weak";
                PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Red);
            }
            else if (strength < 50)
            {
                PasswordStrengthBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Orange);
                PasswordStrengthText.Text = "Fair";
                PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Orange);
            }
            else if (strength < 75)
            {
                PasswordStrengthBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.YellowGreen);
                PasswordStrengthText.Text = "Good";
                PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.YellowGreen);
            }
            else
            {
                PasswordStrengthBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Green);
                PasswordStrengthText.Text = "Strong";
                PasswordStrengthText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Green);
            }
        }

        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int score = 0;

            // Length points
            if (password.Length >= 8) score += 25;
            if (password.Length >= 12) score += 15;
            if (password.Length >= 16) score += 10;

            // Character variety
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]")) score += 10;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]")) score += 10;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]")) score += 10;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]")) score += 20;

            return Math.Min(score, 100);
        }
    }
}
