using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Cryptography;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace Projeto_PAP;

public sealed partial class LoginPage : Page
{
    private ResourceLoader _resourceLoader;
    private ResourceMap _resourceMap;
    private ResourceContext _context;

    // Strings de erro (carregadas dinamicamente)
    private string _errorTitle;
    private string _errorFillAllFields;
    private string _errorInvalidEmail;
    private string _errorEmailNotFound;
    private string _errorIncorrectPassword;
    private string _errorConnectionError;
    private string _errorEnterCode;
    private string _errorVerify;
    private string _errorCodeExpired;
    private string _errorInvalidCode;
    private string _errorEmailFailed;
    private string _cancelButton;

    public LoginPage()
    {
        InitializeComponent();
        LoadCurrentLanguage();
        LoadStrings();
    }

    // ═══════════════════════════════════════════════════════════════
    // CARREGAMENTO DE STRINGS
    // ═══════════════════════════════════════════════════════════════
    private void LoadStrings()
    {
        // Forçar o ResourceLoader a usar o idioma atual
        _context = new ResourceContext();
        _context.QualifierValues["Language"] = ApplicationLanguages.PrimaryLanguageOverride;

        _resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

        // Strings da UI
        EmailLabel.Text = GetString("Login_Email", "Email Address");
        EmailTextBox.PlaceholderText = GetString("Login_EmailPlaceholder", "Enter your email...");
        PasswordLabel.Text = GetString("Login_Password", "Password");
        PasswordBox.PlaceholderText = GetString("Login_PasswordPlaceholder", "Enter your password...");
        ForgotPasswordLink.Content = GetString("Login_ForgotPassword", "Forgot your password?");
        LoginButton.Content = GetString("Login_Button", "Login");
        NoAccountText.Text = GetString("Login_NoAccount", "Don't have an account yet?");
        CreateAccountLink.Content = GetString("Login_CreateAccount", "Create Account");

        // Strings de erro
        _errorTitle = GetString("Login_Error", "Error");
        _errorFillAllFields = GetString("Login_FillAllFields", "Please fill in all fields!");
        _errorInvalidEmail = GetString("Login_InvalidEmail", "Invalid email!");
        _errorEmailNotFound = GetString("Login_EmailNotFound", "Email not found!");
        _errorIncorrectPassword = GetString("Login_IncorrectPassword", "Incorrect password!");
        _errorConnectionError = GetString("Login_ConnectionError", "Connection to the database was lost. Check your internet connection.");
        _errorEnterCode = GetString("Login_EnterCode", "Enter Code");
        _errorVerify = GetString("Login_Verify", "Verify");
        _errorCodeExpired = GetString("Login_CodeExpired", "The verification code has expired. Please try again.");
        _errorInvalidCode = GetString("Login_InvalidCode", "Invalid verification code!");
        _errorEmailFailed = GetString("Login_EmailFailed", "Failed to send verification email. Please try again.");
        _cancelButton = GetString("CancelButton", "Cancel");

        // Atualizar também o _resourceLoader para compatibilidade
        _resourceLoader = ResourceLoader.GetForViewIndependentUse();
    }

    private string GetString(string key, string defaultValue)
    {
        var value = _resourceMap.GetValue(key, _context)?.ValueAsString;
        return string.IsNullOrEmpty(value) ? defaultValue : value;
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
                // Mudar o idioma
                ApplicationLanguages.PrimaryLanguageOverride = newLanguage;

                // Resetar o contexto de recursos
                ResourceContext.SetGlobalQualifierValue("Language", newLanguage);

                // Atualizar o botão principal
                LoadCurrentLanguage();

                // Recarregar strings imediatamente
                LoadStrings();

                // Fechar o flyout
                LanguageButton.Flyout.Hide();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // LOGIN
    // ═══════════════════════════════════════════════════════════════
    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
            string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ContentDialog erroNullTextBox = new ContentDialog()
            {
                Title = _errorTitle,
                Content = _errorFillAllFields,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await erroNullTextBox.ShowAsync();
            return;
        }

        try
        {
            MailAddress mailAddress = new MailAddress(EmailTextBox.Text);
        }
        catch (Exception)
        {
            ContentDialog erroMailAddress = new ContentDialog()
            {
                Title = _errorTitle,
                Content = _errorInvalidEmail,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await erroMailAddress.ShowAsync();
            return;
        }

        var result = await LoginQuery.LoginVerification(EmailTextBox.Text, PasswordBox.Password);

        if (result.Success)
        {
            string code_verification = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
            bool send_email_result = await EmailService.SendVerificationCodeAsync(EmailTextBox.Text, code_verification);

            if (send_email_result)
            {
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

                ContentDialog code_dialog = new ContentDialog()
                {
                    Title = _errorEnterCode,
                    Content = panel,
                    CloseButtonText = _cancelButton,
                    PrimaryButtonText = _errorVerify,
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                code_dialog.Resources["ContentDialogMinWidth"] = 600;

                ContentDialogResult resultCode = await code_dialog.ShowAsync();

                if (resultCode == ContentDialogResult.Primary)
                {
                    if ((DateTime.Now - timeCode).TotalSeconds > 300)
                    {
                        ContentDialog erroTimeCode = new ContentDialog()
                        {
                            Title = _errorTitle,
                            Content = _errorCodeExpired,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await erroTimeCode.ShowAsync();
                        return;
                    }

                    string enteredCode = "";
                    foreach (var box in boxes)
                    {
                        enteredCode += box.Text;
                    }

                    if (enteredCode == code_verification)
                    {
                        // Verifica se precisa configurar 2FA
                        if (result.TotpSecret == null)
                        {
                            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                                PasswordBox.Password,
                                result.Salt,
                                100000,
                                HashAlgorithmName.SHA256,
                                32
                            );

                            PasswordEncrypt encryptor = new PasswordEncrypt(key);

                            var setup2FAParams = new Setup2FAParameters
                            {
                                UserEmail = EmailTextBox.Text.Trim(),
                                Encryptor = encryptor
                            };
                            Frame.Navigate(typeof(Setup2FAPage), setup2FAParams);
                            return;
                        }

                        // Já tem 2FA configurado
                        byte[] keyExisting = Rfc2898DeriveBytes.Pbkdf2(
                            PasswordBox.Password,
                            result.Salt,
                            100000,
                            HashAlgorithmName.SHA256,
                            32
                        );

                        PasswordEncrypt encryptorExisting = new PasswordEncrypt(keyExisting);
                        var parameters = new NavigationsParameters
                        {
                            Encryptor = encryptorExisting,
                            UserEmail = EmailTextBox.Text.Trim(),
                            TotpSecret = result.TotpSecret
                        };
                        Frame.Navigate(typeof(MainPage), parameters);
                    }
                    else
                    {
                        ContentDialog erroCodeVerification = new ContentDialog()
                        {
                            Title = _errorTitle,
                            Content = _errorInvalidCode,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await erroCodeVerification.ShowAsync();
                    }
                }
            }
            else
            {
                ContentDialog erroemail = new ContentDialog()
                {
                    Title = _errorTitle,
                    Content = _errorEmailFailed,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await erroemail.ShowAsync();
            }
        }
        else
        {
            string errorMessage = result.Message switch
            {
                "Email not found" => _errorEmailNotFound,
                "Connection to the Data Base was lost. Verify your Wifi connection" => _errorConnectionError,
                _ => _errorIncorrectPassword
            };

            ContentDialog erroDialog = new ContentDialog()
            {
                Title = _errorTitle,
                Content = errorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await erroDialog.ShowAsync();
        }
    }

    private async void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        string localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        string recoveryAttemptsFile = System.IO.Path.Combine(localFolder, "recovery_attempts.json");

        // ═══════════════════════════════════════════════════════════════
        // PASSO 1: Pedir email
        // ═══════════════════════════════════════════════════════════════
        var emailPanel = new StackPanel() { Spacing = 10, MinWidth = 300 };

        emailPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_EnterEmail", "Enter your email address to recover your password."),
            TextWrapping = TextWrapping.Wrap
        });

        var emailBox = new TextBox()
        {
            PlaceholderText = GetString("Login_EmailPlaceholder", "Enter your email...")
        };
        emailPanel.Children.Add(emailBox);

        var emailError = new TextBlock()
        {
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
            Visibility = Visibility.Collapsed,
            TextWrapping = TextWrapping.Wrap
        };
        emailPanel.Children.Add(emailError);

        ContentDialog emailDialog = new ContentDialog()
        {
            Title = GetString("ForgotPassword_Title", "Recover Password"),
            Content = emailPanel,
            PrimaryButtonText = GetString("SettingsPage_Continue", "Continue"),
            CloseButtonText = _cancelButton,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        string userEmail = "";

        emailDialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();

            string email = emailBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(email))
            {
                emailError.Text = _errorFillAllFields;
                emailError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Validar formato email
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                emailError.Text = _errorInvalidEmail;
                emailError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Verificar se email existe
            var emailExists = await LoginQuery.EmailExists(email);
            if (!emailExists)
            {
                emailError.Text = _errorEmailNotFound;
                emailError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Verificar rate limiting - 1 recuperação por mês e 1 tentativa a cada 2 horas
            Dictionary<string, RecoveryAttempt> attempts = new Dictionary<string, RecoveryAttempt>();

            if (System.IO.File.Exists(recoveryAttemptsFile))
            {
                string json = System.IO.File.ReadAllText(recoveryAttemptsFile);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    attempts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, RecoveryAttempt>>(json);
                }
            }

            if (attempts.ContainsKey(email))
            {
                var attempt = attempts[email];

                // Verificar se já recuperou este mês
                if (attempt.LastSuccessfulRecovery.HasValue &&
                    (DateTime.Now - attempt.LastSuccessfulRecovery.Value).TotalDays < 30)
                {
                    int daysRemaining = 30 - (int)(DateTime.Now - attempt.LastSuccessfulRecovery.Value).TotalDays;
                    emailError.Text = GetString("ForgotPassword_MonthlyLimit", $"You can only recover your password once per month. Try again in {daysRemaining} days.");
                    emailError.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar se falhou nas últimas 2 horas
                if (attempt.LastFailedAttempt.HasValue &&
                    (DateTime.Now - attempt.LastFailedAttempt.Value).TotalHours < 2)
                {
                    int minutesRemaining = 120 - (int)(DateTime.Now - attempt.LastFailedAttempt.Value).TotalMinutes;
                    emailError.Text = GetString("ForgotPassword_TwoHourLimit", $"Too many failed attempts. Try again in {minutesRemaining} minutes.");
                    emailError.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }
            }

            userEmail = email;
            deferral.Complete();
        };

        var emailResult = await emailDialog.ShowAsync();
        if (emailResult != ContentDialogResult.Primary) return;

        // ═══════════════════════════════════════════════════════════════
        // PASSO 2: Pedir respostas às perguntas de segurança (1 TENTATIVA)
        // ═══════════════════════════════════════════════════════════════
        var securityPanel = new StackPanel() { Spacing = 15, MinWidth = 320 };

        securityPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_SecurityQuestions", "Answer your security questions to verify your identity."),
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
        });

        // Aviso de 1 tentativa
        securityPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_OneAttemptWarning", "⚠️ Warning: You only have ONE attempt. If you fail, you must wait 2 hours."),
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 126, 34)),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 5, 0, 5)
        });

        // Pergunta 1
        securityPanel.Children.Add(new TextBlock()
        {
            Text = GetString("CreateAccount_MaleNameQuestion", "What is your favorite male name?"),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 0)
        });
        var maleNameBox = new TextBox()
        {
            PlaceholderText = GetString("CreateAccount_MaleNamePlaceholder", "Enter a male name...")
        };
        securityPanel.Children.Add(maleNameBox);

        // Pergunta 2
        securityPanel.Children.Add(new TextBlock()
        {
            Text = GetString("CreateAccount_FemaleNameQuestion", "What is your favorite female name?"),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 0)
        });
        var femaleNameBox = new TextBox()
        {
            PlaceholderText = GetString("CreateAccount_FemaleNamePlaceholder", "Enter a female name...")
        };
        securityPanel.Children.Add(femaleNameBox);

        var securityError = new TextBlock()
        {
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
            Visibility = Visibility.Collapsed,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0)
        };
        securityPanel.Children.Add(securityError);

        ContentDialog securityDialog = new ContentDialog()
        {
            Title = GetString("ForgotPassword_Title", "Recover Password"),
            Content = securityPanel,
            PrimaryButtonText = GetString("SettingsPage_Continue", "Continue"),
            CloseButtonText = _cancelButton,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        string verificationCode = "";
        bool securityAnswersCorrect = false;

        securityDialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();

            string maleName = maleNameBox.Text.Trim();
            string femaleName = femaleNameBox.Text.Trim();

            if (string.IsNullOrEmpty(maleName) || string.IsNullOrEmpty(femaleName))
            {
                securityError.Text = _errorFillAllFields;
                securityError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Verificar respostas
            bool answersCorrect = await LoginQuery.VerifySecurityAnswers(userEmail, maleName, femaleName);

            // Carregar tentativas
            Dictionary<string, RecoveryAttempt> attempts = new Dictionary<string, RecoveryAttempt>();
            if (System.IO.File.Exists(recoveryAttemptsFile))
            {
                string json = System.IO.File.ReadAllText(recoveryAttemptsFile);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    attempts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, RecoveryAttempt>>(json);
                }
            }

            if (!answersCorrect)
            {
                // Registar falha
                if (!attempts.ContainsKey(userEmail))
                {
                    attempts[userEmail] = new RecoveryAttempt();
                }
                attempts[userEmail].LastFailedAttempt = DateTime.Now;

                // Guardar
                string jsonToSave = System.Text.Json.JsonSerializer.Serialize(attempts);
                System.IO.File.WriteAllText(recoveryAttemptsFile, jsonToSave);

                // Enviar email de alerta
                _ = EmailService.SendSecurityAlertFailedRecoveryAsync(userEmail);

                securityError.Text = GetString("ForgotPassword_WrongAnswersBlocked", "Incorrect answers. For security, you must wait 2 hours before trying again. An alert email has been sent.");
                securityError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Respostas corretas - enviar código
            securityAnswersCorrect = true;
            verificationCode = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
            bool emailSent = await EmailService.SendPasswordRecoveryCodeAsync(userEmail, verificationCode);

            if (!emailSent)
            {
                securityError.Text = _errorEmailFailed;
                securityError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            deferral.Complete();
        };

        var securityResult = await securityDialog.ShowAsync();
        if (securityResult != ContentDialogResult.Primary || !securityAnswersCorrect) return;

        // ═══════════════════════════════════════════════════════════════
        // PASSO 3: Pedir código do email (1 TENTATIVA)
        // ═══════════════════════════════════════════════════════════════
        DateTime codeTime = DateTime.Now;
        bool codeAttemptUsed = false;

        var codePanel = new StackPanel() { Spacing = 15, HorizontalAlignment = HorizontalAlignment.Center };

        codePanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_CodeSent", "We sent a verification code to your email. Enter it below."),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
        });

        // Aviso de 1 tentativa
        codePanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_OneCodeAttempt", "⚠️ You only have ONE attempt to enter the code correctly."),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 126, 34)),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        var boxesPanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };

        TextBox[] boxes = new TextBox[8];
        for (int i = 0; i < 8; i++)
        {
            boxes[i] = new TextBox()
            {
                MaxLength = 1,
                Width = 45,
                Height = 50,
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

            boxesPanel.Children.Add(boxes[i]);
        }
        codePanel.Children.Add(boxesPanel);

        var codeError = new TextBlock()
        {
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        codePanel.Children.Add(codeError);

        ContentDialog codeDialog = new ContentDialog()
        {
            Title = _errorEnterCode,
            Content = codePanel,
            PrimaryButtonText = _errorVerify,
            CloseButtonText = _cancelButton,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        codeDialog.Resources["ContentDialogMinWidth"] = 600;

        bool codeCorrect = false;

        codeDialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();

            // Verificar se expirou
            if ((DateTime.Now - codeTime).TotalSeconds > 300)
            {
                codeError.Text = _errorCodeExpired;
                codeError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Juntar código
            string enteredCode = "";
            foreach (var box in boxes)
            {
                enteredCode += box.Text;
            }

            if (enteredCode.Length != 8 || enteredCode != verificationCode)
            {
                // Código errado - bloquear e enviar alerta
                codeAttemptUsed = true;

                // Registar falha
                Dictionary<string, RecoveryAttempt> attempts = new Dictionary<string, RecoveryAttempt>();
                if (System.IO.File.Exists(recoveryAttemptsFile))
                {
                    string json = System.IO.File.ReadAllText(recoveryAttemptsFile);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        attempts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, RecoveryAttempt>>(json);
                    }
                }

                if (!attempts.ContainsKey(userEmail))
                {
                    attempts[userEmail] = new RecoveryAttempt();
                }
                attempts[userEmail].LastFailedAttempt = DateTime.Now;

                string jsonToSave = System.Text.Json.JsonSerializer.Serialize(attempts);
                System.IO.File.WriteAllText(recoveryAttemptsFile, jsonToSave);

                // Enviar email de alerta (alguém passou as perguntas mas falhou o código)
                _ = EmailService.SendSecurityAlertFailedCodeAsync(userEmail);

                codeError.Text = GetString("ForgotPassword_WrongCodeBlocked", "Incorrect code. For security, the process has been cancelled and an alert has been sent to this account.");
                codeError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            codeCorrect = true;
            deferral.Complete();
        };

        var codeResult = await codeDialog.ShowAsync();
        if (codeResult != ContentDialogResult.Primary || !codeCorrect) return;

        // ═══════════════════════════════════════════════════════════════
        // PASSO 4: Criar nova password
        // ═══════════════════════════════════════════════════════════════
        var passwordPanel = new StackPanel() { Spacing = 15, MinWidth = 300 };

        passwordPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ForgotPassword_CreateNew", "Create your new password."),
            TextWrapping = TextWrapping.Wrap
        });

        // Nova password
        passwordPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ChangePassword_NewPasswordLabel", "New Password:"),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        var newPasswordBox = new PasswordBox()
        {
            PlaceholderText = "••••••••"
        };
        passwordPanel.Children.Add(newPasswordBox);

        // Confirmar password
        passwordPanel.Children.Add(new TextBlock()
        {
            Text = GetString("ChangePassword_ConfirmPasswordLabel", "Confirm Password:"),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 0)
        });
        var confirmPasswordBox = new PasswordBox()
        {
            PlaceholderText = "••••••••"
        };
        passwordPanel.Children.Add(confirmPasswordBox);

        var passwordError = new TextBlock()
        {
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
            Visibility = Visibility.Collapsed,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0)
        };
        passwordPanel.Children.Add(passwordError);

        ContentDialog passwordDialog = new ContentDialog()
        {
            Title = GetString("ForgotPassword_Title", "Recover Password"),
            Content = passwordPanel,
            PrimaryButtonText = GetString("SaveButton", "Save"),
            CloseButtonText = _cancelButton,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        bool passwordUpdated = false;

        passwordDialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();

            string newPassword = newPasswordBox.Password;
            string confirmPassword = confirmPasswordBox.Password;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                passwordError.Text = _errorFillAllFields;
                passwordError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            if (newPassword != confirmPassword)
            {
                passwordError.Text = GetString("CreateAccount_PasswordsNotMatch", "The passwords do not match!");
                passwordError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            if (newPassword.Length < 8)
            {
                passwordError.Text = GetString("SettingsPage_PasswordTooShort", "Password must be at least 8 characters.");
                passwordError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Atualizar password
            bool updated = await LoginQuery.UpdatePassword(userEmail, newPassword);
            if (!updated)
            {
                passwordError.Text = GetString("SettingsPage_UpdateFailed", "Failed to update password. Please try again.");
                passwordError.Visibility = Visibility.Visible;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Registar recuperação bem-sucedida
            Dictionary<string, RecoveryAttempt> attempts = new Dictionary<string, RecoveryAttempt>();
            if (System.IO.File.Exists(recoveryAttemptsFile))
            {
                string json = System.IO.File.ReadAllText(recoveryAttemptsFile);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    attempts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, RecoveryAttempt>>(json);
                }
            }

            if (!attempts.ContainsKey(userEmail))
            {
                attempts[userEmail] = new RecoveryAttempt();
            }
            attempts[userEmail].LastSuccessfulRecovery = DateTime.Now;
            attempts[userEmail].LastFailedAttempt = null; // Limpar falha

            string jsonToSave = System.Text.Json.JsonSerializer.Serialize(attempts);
            System.IO.File.WriteAllText(recoveryAttemptsFile, jsonToSave);

            passwordUpdated = true;
            deferral.Complete();
        };

        var passwordResult = await passwordDialog.ShowAsync();

        if (passwordResult == ContentDialogResult.Primary && passwordUpdated)
        {
            // Sucesso!
            ContentDialog successDialog = new ContentDialog()
            {
                Title = GetString("ForgotPassword_Success", "Password Changed"),
                Content = GetString("ForgotPassword_SuccessMessage", "Your password has been updated. You can now login with your new password."),
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(CreateAccount));
    }
}