using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    public sealed partial class HomePage : Page
    {
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private string _totpSecret;
        private PasswordQuery _passwordQuery;

        public HomePage()
        {
            InitializeComponent();
            _passwordQuery = new PasswordQuery();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
                _totpSecret = parameters.TotpSecret;
            }

            // Buscar nome do utilizador
            string firstName = await LoginQuery.GetUserFirstName(_userEmail);
            if (!string.IsNullOrEmpty(firstName))
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
                GreetingText.Text = $"{resourceLoader.GetString("HomePage_GreetingWithName")} {firstName}!";
            }

            await LoadStatistics();
        }

        private async Task LoadStatistics()
        {
            var passwords = await _passwordQuery.GetPasswordsAsync(_userEmail);
            var passwordList = passwords.ToList();

            // Estatísticas por categoria
            TotalCount.Text = passwordList.Count.ToString();
            SocialCount.Text = passwordList.Count(p => p.Category == "Social").ToString();
            WorkCount.Text = passwordList.Count(p => p.Category == "Work").ToString();
            BankCount.Text = passwordList.Count(p => p.Category == "Bank").ToString();
            OtherCount.Text = passwordList.Count(p => p.Category == "Other").ToString();

            // Analisar segurança
            await AnalyzeSecurity(passwordList);
        }

        private async Task AnalyzeSecurity(List<PasswordItem> passwords)
        {
            var weakPasswords = new List<string>();
            var duplicatePasswords = new List<string>();
            var decryptedPasswords = new Dictionary<string, List<string>>(); // password -> lista de sites

            foreach (var item in passwords)
            {
                try
                {
                    // Desencriptar password
                    string decrypted = _encryptor.Decrypt(item.EncryptedPassword);

                    // Verificar força
                    int strength = CalculatePasswordStrength(decrypted);
                    if (strength < 50) // Weak ou Fair
                    {
                        weakPasswords.Add(item.SiteName);
                    }

                    // Guardar para verificar duplicados
                    if (!decryptedPasswords.ContainsKey(decrypted))
                    {
                        decryptedPasswords[decrypted] = new List<string>();
                    }
                    decryptedPasswords[decrypted].Add(item.SiteName);
                }
                catch
                {
                    // Se falhar a desencriptação, ignora
                }
            }

            // Encontrar duplicados (passwords usadas em mais de 1 site)
            foreach (var kvp in decryptedPasswords)
            {
                if (kvp.Value.Count > 1)
                {
                    string sites = string.Join(", ", kvp.Value);
                    duplicatePasswords.Add(sites);
                }
            }

            // Atualizar UI - Passwords Fracas
            WeakPasswordsCount.Text = weakPasswords.Count.ToString();
            if (weakPasswords.Count > 0)
            {
                WeakPasswordsList.ItemsSource = weakPasswords;
                WeakPasswordsList.Visibility = Visibility.Visible;
            }
            else
            {
                WeakPasswordsList.Visibility = Visibility.Collapsed;
            }

            // Atualizar UI - Passwords Duplicadas
            DuplicatePasswordsCount.Text = duplicatePasswords.Count.ToString();
            if (duplicatePasswords.Count > 0)
            {
                DuplicatePasswordsList.ItemsSource = duplicatePasswords;
                DuplicatePasswordsList.Visibility = Visibility.Visible;
            }
            else
            {
                DuplicatePasswordsList.Visibility = Visibility.Collapsed;
            }

            // Mostrar mensagem "Tudo seguro" se não houver problemas
            if (weakPasswords.Count == 0 && duplicatePasswords.Count == 0 && passwords.Count > 0)
            {
                AllSecurePanel.Visibility = Visibility.Visible;
            }
            else
            {
                AllSecurePanel.Visibility = Visibility.Collapsed;
            }
        }

        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int score = 0;

            // Pontos por comprimento
            if (password.Length >= 8) score += 25;
            if (password.Length >= 12) score += 15;
            if (password.Length >= 16) score += 10;

            // Variedade de caracteres
            if (Regex.IsMatch(password, "[a-z]")) score += 10;
            if (Regex.IsMatch(password, "[A-Z]")) score += 10;
            if (Regex.IsMatch(password, "[0-9]")) score += 10;
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) score += 20;

            return Math.Min(score, 100);
        }
    }
}