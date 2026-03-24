using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Storage;

namespace Projeto_PAP
{
    public sealed partial class CreateAccount : Page
    {
        private ResourceLoader _resourceLoader;
        private ResourceMap _resourceMap;
        private ResourceContext _context;

        // Strings de erro
        private string _errorTitle;
        private string _errorFillAllFields;
        private string _errorInvalidEmail;
        private string _errorPasswordsNotMatch;
        private string _errorAttemptsExceeded;
        private string _errorCodeExpired;
        private string _errorInvalidCode;
        private string _errorEmailFailed;
        private string _errorEmailDuplicate;
        private string _errorConnection;
        private string _errorSecurityQuestions;
        private string _enterCode;
        private string _verify;
        private string _cancelButton;

        // Strings de força da password
        private string _passwordWeak;
        private string _passwordMedium;
        private string _passwordStrong;

        public CreateAccount()
        {
            InitializeComponent();
            LoadCurrentLanguage();
            LoadStrings();
            SetupPasswordStrengthChecker();
        }

        // ═══════════════════════════════════════════════════════════════
        // CARREGAMENTO DE STRINGS
        // ═══════════════════════════════════════════════════════════════
        private void LoadStrings()
        {
            _context = new ResourceContext();
            _context.QualifierValues["Language"] = ApplicationLanguages.PrimaryLanguageOverride;

            _resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Strings da UI
            FirstNameLabel.Text = GetString("CreateAccount_FirstName", "First Name");
            FirstNameTextBox.PlaceholderText = GetString("CreateAccount_FirstNamePlaceholder", "Enter your first name...");
            LastNameLabel.Text = GetString("CreateAccount_LastName", "Last Name");
            LastNameTextBox.PlaceholderText = GetString("CreateAccount_LastNamePlaceholder", "Enter your last name...");
            EmailLabel.Text = GetString("CreateAccount_Email", "Email Address");
            EmailTextBox.PlaceholderText = GetString("CreateAccount_EmailPlaceholder", "Enter your email...");
            PasswordLabel.Text = GetString("CreateAccount_Password", "Password");
            PasswordBox.PlaceholderText = GetString("CreateAccount_PasswordPlaceholder", "Enter your password...");
            ConfirmPasswordLabel.Text = GetString("CreateAccount_ConfirmPassword", "Confirm Password");
            ConfirmPasswordBox.PlaceholderText = GetString("CreateAccount_ConfirmPasswordPlaceholder", "Confirm your password...");

            // Perguntas de segurança
            SecurityQuestionsTitle.Text = GetString("CreateAccount_SecurityQuestionsTitle", "Security Questions");
            SecurityQuestionsSubtitle.Text = GetString("CreateAccount_SecurityQuestionsSubtitle", "These will help you recover your account");
            MaleNameLabel.Text = GetString("CreateAccount_MaleNameQuestion", "What is your favorite male name?");
            MaleNameTextBox.PlaceholderText = GetString("CreateAccount_MaleNamePlaceholder", "Enter a male name...");
            FemaleNameLabel.Text = GetString("CreateAccount_FemaleNameQuestion", "What is your favorite female name?");
            FemaleNameTextBox.PlaceholderText = GetString("CreateAccount_FemaleNamePlaceholder", "Enter a female name...");

            // Botões e links
            CreateAccountButton.Content = GetString("CreateAccount_Button", "Create Account");
            HasAccountText.Text = GetString("CreateAccount_HasAccount", "Already have an account?");
            LoginLink.Content = GetString("CreateAccount_LoginLink", "Login");

            // Strings de erro
            _errorTitle = GetString("Login_Error", "Error");
            _errorFillAllFields = GetString("Login_FillAllFields", "Please fill in all fields!");
            _errorInvalidEmail = GetString("Login_InvalidEmail", "Invalid email!");
            _errorPasswordsNotMatch = GetString("CreateAccount_PasswordsNotMatch", "The passwords do not match!");
            _errorAttemptsExceeded = GetString("CreateAccount_AttemptsExceeded", "Number of attempts per email exceeded, try again in 5 minutes!");
            _errorCodeExpired = GetString("Login_CodeExpired", "The verification code has expired. Please try again.");
            _errorInvalidCode = GetString("Login_InvalidCode", "Invalid verification code!");
            _errorEmailFailed = GetString("Login_EmailFailed", "Failed to send verification email. Please try again.");
            _errorEmailDuplicate = GetString("CreateAccount_EmailDuplicate", "This email is already registered!");
            _errorConnection = GetString("CreateAccount_ConnectionError", "Connection error. Please try again.");
            _errorSecurityQuestions = GetString("CreateAccount_SecurityQuestionsError", "Please answer both security questions!");
            _enterCode = GetString("Login_EnterCode", "Enter Code");
            _verify = GetString("Login_Verify", "Verify");
            _cancelButton = GetString("CancelButton", "Cancel");

            // Strings de força da password
            _passwordWeak = GetString("CreateAccount_PasswordWeak", "Weak! Your password must contain at least 8 characters.\nAn uppercase, lowercase letter, a number, and at least one special character.");
            _passwordMedium = GetString("CreateAccount_PasswordMedium", "Medium! Your password must contain at least 8 characters.\nAn uppercase, lowercase letter, a number, and at least one special character.");
            _passwordStrong = GetString("CreateAccount_PasswordStrong", "Strong");

            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
        }

        private string GetString(string key, string defaultValue)
        {
            var value = _resourceMap.GetValue(key, _context)?.ValueAsString;
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        // ═══════════════════════════════════════════════════════════════
        // VERIFICADOR DE FORÇA DA PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private void SetupPasswordStrengthChecker()
        {
            var colorRed = new SolidColorBrush(Colors.Red);
            var colorOrange = new SolidColorBrush(Colors.Orange);
            var colorGreen = new SolidColorBrush(Colors.Green);

            PasswordBox.PasswordChanged += (s, args) =>
            {
                string password = PasswordBox.Password;
                int score = 0;

                if (password.Length >= 8) score++;
                if (password.Length >= 12) score++;
                if (password.Any(char.IsUpper)) score++;
                if (password.Any(char.IsLower)) score++;
                if (password.Any(char.IsDigit)) score++;
                if (password.Any(c => !char.IsLetterOrDigit(c))) score++;

                if (password.Length == 0)
                {
                    PasswordStrength.Text = "";
                }
                else if (score <= 3)
                {
                    PasswordStrength.Foreground = colorRed;
                    PasswordStrength.Text = _passwordWeak;
                }
                else if (score == 4)
                {
                    PasswordStrength.Foreground = colorOrange;
                    PasswordStrength.Text = _passwordMedium;
                }
                else
                {
                    PasswordStrength.Foreground = colorGreen;
                    PasswordStrength.Text = _passwordStrong;
                }
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // IDIOMA
        // ═══════════════════════════════════════════════════════════════
        private void LoadCurrentLanguage()
        {
            string currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;

            switch (currentLanguage)
            {
                case "pt-PT":
                    CurrentFlag.Text = "🇵🇹";
                    CurrentLanguageName.Text = "Português";
                    break;
                case "es-ES":
                    CurrentFlag.Text = "🇪🇸";
                    CurrentLanguageName.Text = "Español";
                    break;
                case "fr-FR":
                    CurrentFlag.Text = "🇫🇷";
                    CurrentLanguageName.Text = "Français";
                    break;
                default:
                    CurrentFlag.Text = "🇬🇧";
                    CurrentLanguageName.Text = "English";
                    break;
            }
        }

        private void LanguageOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string newLanguage)
            {
                if (newLanguage != ApplicationLanguages.PrimaryLanguageOverride)
                {
                    ApplicationLanguages.PrimaryLanguageOverride = newLanguage;
                    ResourceContext.SetGlobalQualifierValue("Language", newLanguage);

                    LoadCurrentLanguage();
                    LoadStrings();

                    LanguageButton.Flyout.Hide();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // CRIAR CONTA
        // ═══════════════════════════════════════════════════════════════
        private async void CreateAccountClick(object sender, RoutedEventArgs e)
        {
            // Validar campos vazios
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                await ShowErrorDialog(_errorFillAllFields);
                return;
            }

            // Validar perguntas de segurança
            if (string.IsNullOrWhiteSpace(MaleNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FemaleNameTextBox.Text))
            {
                await ShowErrorDialog(_errorSecurityQuestions);
                return;
            }

            // Validar email
            try
            {
                MailAddress mailAddress = new MailAddress(EmailTextBox.Text);
            }
            catch (Exception)
            {
                await ShowErrorDialog(_errorInvalidEmail);
                return;
            }

            // Rate limiting
            string localFolder = ApplicationData.Current.LocalFolder.Path;
            string pathFile = Path.Combine(localFolder, "email_attempts.json");
            Dictionary<string, List<DateTime>>? emails = new Dictionary<string, List<DateTime>>();
            bool newEmail = false;
            bool addTimeForEmail = false;

            if (!File.Exists(pathFile))
            {
                File.Create(pathFile).Close();
            }

            DateTime lastModified = File.GetLastWriteTime(pathFile);
            if ((DateTime.Now - lastModified).TotalSeconds > 3600)
            {
                File.WriteAllText(pathFile, "");
            }

            string jsonContent = File.ReadAllText(pathFile);
            if (!string.IsNullOrWhiteSpace(jsonContent))
            {
                emails = JsonSerializer.Deserialize<Dictionary<string, List<DateTime>>>(jsonContent);
            }

            if (emails.ContainsKey(EmailTextBox.Text))
            {
                int amountEmailDateTime = emails[EmailTextBox.Text].Count;
                DateTime lastTimeEmail = emails[EmailTextBox.Text].Last();
                if (amountEmailDateTime >= 5 && (DateTime.Now - lastTimeEmail).TotalSeconds < 300)
                {
                    await ShowErrorDialog(_errorAttemptsExceeded);
                    return;
                }
                else
                {
                    addTimeForEmail = true;
                }
            }
            else
            {
                newEmail = true;
            }

            // Validar passwords
            if (ConfirmPasswordBox.Password != PasswordBox.Password)
            {
                await ShowErrorDialog(_errorPasswordsNotMatch);
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                return;
            }

            // Enviar código de verificação
            string codeVerification = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
            bool result = await EmailService.SendVerificationCodeAsync(EmailTextBox.Text.Trim(), codeVerification);

            if (result)
            {
                if (addTimeForEmail)
                {
                    emails[EmailTextBox.Text].Add(DateTime.Now);
                }
                else if (newEmail)
                {
                    emails[EmailTextBox.Text] = new List<DateTime> { DateTime.Now };
                }

                DateTime timeCode = DateTime.Now;

                // Criar TextBoxes para o código
                TextBox[] boxes = new TextBox[8];
                StackPanel panel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 5
                };

                for (int i = 0; i < 8; i++)
                {
                    boxes[i] = new TextBox()
                    {
                        MaxLength = 1,
                        Width = 45,
                        FontSize = 22,
                        TextAlignment = TextAlignment.Center,
                    };

                    int index = i;
                    boxes[i].TextChanged += (s, args) =>
                    {
                        if (boxes[index].Text.Length == 1 && index < 7)
                        {
                            boxes[index + 1].Focus(FocusState.Programmatic);
                        }
                    };

                    panel.Children.Add(boxes[i]);
                }

                ContentDialog codeDialog = new ContentDialog()
                {
                    Title = _enterCode,
                    Content = panel,
                    CloseButtonText = _cancelButton,
                    PrimaryButtonText = _verify,
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                codeDialog.Resources["ContentDialogMinWidth"] = 600;

                ContentDialogResult resultCode = await codeDialog.ShowAsync();

                if (resultCode == ContentDialogResult.Primary)
                {
                    if ((DateTime.Now - timeCode).TotalSeconds > 300)
                    {
                        await ShowErrorDialog(_errorCodeExpired);
                        return;
                    }

                    string enteredCode = "";
                    foreach (var box in boxes)
                    {
                        enteredCode += box.Text;
                    }

                    if (enteredCode == codeVerification)
                    {
                        var hashedPassword = PasswordHash.HashPassword(PasswordBox.Password);
                        var client = new MongoClient(DataBaseConfig.ConnectionString);
                        var database = client.GetDatabase(DataBaseConfig.DatabaseName);
                        var collection = database.GetCollection<Account>(DataBaseConfig.CollectionName);

                        var hashedMaleName = PasswordHash.HashPassword(MaleNameTextBox.Text.Trim().ToLower());
                        var hashedFemaleName = PasswordHash.HashPassword(FemaleNameTextBox.Text.Trim().ToLower());

                        var user = new Account
                        {
                            FirstName = FirstNameTextBox.Text.Trim(),
                            Lastname = LastNameTextBox.Text.Trim(),
                            EmailAddress = EmailTextBox.Text.Trim().ToLower(),
                            Password = hashedPassword,
                            SecurityAnswerMale = hashedMaleName,
                            SecurityAnswerFemale = hashedFemaleName
                        };

                        try
                        {
                            collection.InsertOne(user);
                            emails.Remove(EmailTextBox.Text);
                            jsonContent = JsonSerializer.Serialize(emails);
                            File.WriteAllText(pathFile, jsonContent);

                            // ═══════════════════════════════════════════════════════════════
                            // DERIVAR CHAVE DE ENCRIPTAÇÃO E IR PARA SETUP 2FA
                            // ═══════════════════════════════════════════════════════════════

                            // Extrair o salt do hash (primeiros 16 bytes do Base64)
                            byte[] fullHash = Convert.FromBase64String(hashedPassword);
                            byte[] salt = new byte[16];
                            Array.Copy(fullHash, 0, salt, 0, 16);

                            // Derivar a chave de encriptação com PBKDF2
                            byte[] encryptionKey = Rfc2898DeriveBytes.Pbkdf2(
                                PasswordBox.Password,
                                salt,
                                100000,
                                HashAlgorithmName.SHA256,
                                32
                            );

                            // Criar o encriptador
                            var encryptor = new PasswordEncrypt(encryptionKey);

                            // Navegar para Setup2FAPage
                            // Navegar para Setup2FAPage
                            Frame.Navigate(typeof(Setup2FAPage), new Setup2FAParameters
                            {
                                Encryptor = encryptor,
                                UserEmail = EmailTextBox.Text.Trim().ToLower()
                            });
                        }
                        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                        {
                            EmailTextBox.Text = string.Empty;
                            await ShowErrorDialog(_errorEmailDuplicate);
                            return;
                        }
                        catch (Exception)
                        {
                            await ShowErrorDialog(_errorConnection);
                            return;
                        }
                    }
                    else
                    {
                        jsonContent = JsonSerializer.Serialize(emails);
                        File.WriteAllText(pathFile, jsonContent);
                        await ShowErrorDialog(_errorInvalidCode);
                        return;
                    }
                }
            }
            else
            {
                await ShowErrorDialog(_errorEmailFailed);
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string message)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = _errorTitle,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}