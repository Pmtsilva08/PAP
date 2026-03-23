using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OtpNet;
using System;
using System.Security.Cryptography;
using Windows.Globalization;

namespace Projeto_PAP
{
    public sealed partial class SettingsPage : Page
    {
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private string _totpSecret;

        public SettingsPage()
        {
            InitializeComponent();
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

            // Carregar dados do utilizador
            await LoadUserData();

            // Carregar idioma atual
            LoadCurrentLanguage();
        }

        // ═══════════════════════════════════════════════════════════════
        // CARREGAR DADOS DO UTILIZADOR
        // ═══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task LoadUserData()
        {
            var userData = await LoginQuery.GetUserData(_userEmail);

            if (userData != null)
            {
                FirstNameText.Text = userData.Value.FirstName ?? "";
                LastNameText.Text = userData.Value.LastName ?? "";
                EmailText.Text = userData.Value.Email ?? "";
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // IDIOMA
        // ═══════════════════════════════════════════════════════════════
        private void LoadCurrentLanguage()
        {
            string currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;

            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == currentLanguage)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }

            if (LanguageComboBox.SelectedItem == null)
            {
                LanguageComboBox.SelectedIndex = 0;
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string newLanguage = selectedItem.Tag?.ToString();

                if (!string.IsNullOrEmpty(newLanguage) &&
                    newLanguage != ApplicationLanguages.PrimaryLanguageOverride)
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

                    ContentDialog confirmDialog = new ContentDialog()
                    {
                        Title = resourceLoader.GetString("SettingsPage_LanguageChangeTitle"),
                        Content = resourceLoader.GetString("SettingsPage_LanguageChangeMessage"),
                        PrimaryButtonText = resourceLoader.GetString("SettingsPage_Continue"),
                        CloseButtonText = resourceLoader.GetString("CancelButton"),
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = this.XamlRoot
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        // Guardar sessão completa antes de reiniciar
                        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                        localSettings.Values["PendingLanguageChange"] = true;
                        localSettings.Values["SessionEmail"] = _userEmail;
                        localSettings.Values["SessionTotp"] = _totpSecret;
                        localSettings.Values["SessionKey"] = Convert.ToBase64String(_encryptor.GetKey());

                        ApplicationLanguages.PrimaryLanguageOverride = newLanguage;
                        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
                    }
                    else
                    {
                        LoadCurrentLanguage();
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MUDAR PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            // Criar conteúdo do dialog
            var content = new StackPanel() { Spacing = 20, MinWidth = 320 };

            // Ícone
            var iconBorder = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 243, 232)),
                CornerRadius = new CornerRadius(35),
                Width = 70,
                Height = 70,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new FontIcon()
                {
                    Glyph = "\uE72E",
                    FontSize = 32,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 126, 34))
                }
            };
            content.Children.Add(iconBorder);

            // Título
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_VerifyIdentity"),
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Instruções
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_VerifyIdentityDesc"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            // Separador
            content.Children.Add(new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 240, 240)),
                Height = 1
            });

            // Password atual
            var passwordPanel = new StackPanel() { Spacing = 8 };
            passwordPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_CurrentPassword"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            var passwordBox = new PasswordBox()
            {
                PlaceholderText = "••••••••",
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            passwordPanel.Children.Add(passwordBox);
            content.Children.Add(passwordPanel);

            // Código TOTP
            var totpPanel = new StackPanel() { Spacing = 8 };
            totpPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_CodeLabel"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            var totpBox = new TextBox()
            {
                PlaceholderText = "000000",
                MaxLength = 6,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            totpPanel.Children.Add(totpBox);
            content.Children.Add(totpPanel);

            // Mensagem de erro
            var errorPanel = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 235, 235)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Visibility = Visibility.Collapsed
            };
            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            errorPanel.Child = errorText;
            content.Children.Add(errorPanel);

            ContentDialog dialog = new ContentDialog()
            {
                Content = content,
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_Continue"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            string verificationCode = "";

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                string password = passwordBox.Password;
                string totpCode = totpBox.Text.Trim();

                // Validar campos
                if (string.IsNullOrEmpty(password))
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_PasswordRequired");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                if (string.IsNullOrEmpty(totpCode) || totpCode.Length != 6)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar password
                var loginResult = await LoginQuery.LoginVerification(_userEmail, password);
                if (!loginResult.Success)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidPassword");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar TOTP
                byte[] secretKey = Base32Encoding.ToBytes(_totpSecret);
                var totp = new Totp(secretKey);
                if (!totp.VerifyTotp(totpCode, out long timeStepMatched, new VerificationWindow(previous: 0, future: 0)))
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Gerar código de verificação e enviar email
                verificationCode = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
                bool emailSent = await EmailService.SendPasswordChangeCodeAsync(_userEmail, verificationCode);

                if (!emailSent)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_EmailFailed");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                deferral.Complete();
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Pedir código de verificação do email
                await VerifyEmailCode(verificationCode);
            }
        }

        private async System.Threading.Tasks.Task VerifyEmailCode(string correctCode)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            DateTime codeTime = DateTime.Now;

            // Criar TextBoxes para o código
            var panel = new StackPanel() { Spacing = 15, HorizontalAlignment = HorizontalAlignment.Center };

            panel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_EmailSent"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
            });

            var codePanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };

            TextBox[] boxes = new TextBox[8];
            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new TextBox()
                {
                    MaxLength = 1,
                    Width = 40,
                    FontSize = 20,
                    TextAlignment = TextAlignment.Center
                };

                int index = i;
                boxes[i].TextChanged += (s, args) =>
                {
                    if (boxes[index].Text.Length == 1 && index < 7)
                    {
                        boxes[index + 1].Focus(FocusState.Programmatic);
                    }
                };

                codePanel.Children.Add(boxes[i]);
            }
            panel.Children.Add(codePanel);

            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(errorText);

            ContentDialog codeDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_EnterEmailCode"),
                Content = panel,
                PrimaryButtonText = resourceLoader.GetString("TOTP_VerifyButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            codeDialog.PrimaryButtonClick += (s, args) =>
            {
                // Verificar se expirou (5 minutos)
                if ((DateTime.Now - codeTime).TotalSeconds > 300)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_CodeExpired");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                // Juntar código
                string enteredCode = "";
                foreach (var box in boxes)
                {
                    enteredCode += box.Text;
                }

                if (enteredCode != correctCode)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_InvalidCode");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
            };

            codeDialog.Resources["ContentDialogMinWidth"] = 600;  


            var result = await codeDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Navegar para ChangePasswordPage
                var parameters = new NavigationsParameters
                {
                    Encryptor = _encryptor,
                    UserEmail = _userEmail,
                    TotpSecret = _totpSecret
                };
                Frame.Navigate(typeof(ChangePasswordPage), parameters);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LOGOUT
        // ═══════════════════════════════════════════════════════════════
        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            ContentDialog confirmDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_LogoutConfirmTitle"),
                Content = resourceLoader.GetString("SettingsPage_LogoutConfirmMessage"),
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_Logout"),  // <- Mudou aqui
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _encryptor?.Clear();
                App.MainFrame.Navigate(typeof(LoginPage));
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // APAGAR CONTA
        // ═══════════════════════════════════════════════════════════════
        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            // ═══════════════════════════════════════════════════════════════
            // PASSO 1: Primeiro aviso
            // ═══════════════════════════════════════════════════════════════
            ContentDialog warningDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_DeleteAccountTitle"),
                Content = resourceLoader.GetString("SettingsPage_DeleteAccountWarning"),
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_Continue"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var warningResult = await warningDialog.ShowAsync();
            if (warningResult != ContentDialogResult.Primary) return;

            // ═══════════════════════════════════════════════════════════════
            // PASSO 2: Pedir password e TOTP
            // ═══════════════════════════════════════════════════════════════
            var content = new StackPanel() { Spacing = 15, MinWidth = 300 };

            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_DeleteAccountConfirm"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60))
            });

            // Password
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_CurrentPassword"),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 0)
            });
            var passwordBox = new PasswordBox() { PlaceholderText = "••••••••" };
            content.Children.Add(passwordBox);

            // TOTP
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_CodeLabel"),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 0)
            });
            var totpBox = new TextBox()
            {
                PlaceholderText = "000000",
                MaxLength = 6,
                FontFamily = new FontFamily("Consolas"),
                TextAlignment = TextAlignment.Center
            };
            content.Children.Add(totpBox);

            // Erro
            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                Visibility = Visibility.Collapsed,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0)
            };
            content.Children.Add(errorText);

            ContentDialog verifyDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_DeleteAccountTitle"),
                Content = content,
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_Continue"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            string verificationCode = "";

            verifyDialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                // Validar campos vazios
                if (string.IsNullOrEmpty(passwordBox.Password))
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_PasswordRequired");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                if (string.IsNullOrEmpty(totpBox.Text) || totpBox.Text.Length != 6)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar password
                var loginResult = await LoginQuery.LoginVerification(_userEmail, passwordBox.Password);
                if (!loginResult.Success)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidPassword");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar TOTP
                byte[] secretKey = Base32Encoding.ToBytes(_totpSecret);
                var totp = new Totp(secretKey);
                if (!totp.VerifyTotp(totpBox.Text.Trim(), out long timeStepMatched, new VerificationWindow(previous: 0, future: 0)))
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Enviar código de verificação por email
                verificationCode = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
                bool emailSent = await EmailService.SendDeleteAccountCodeAsync(_userEmail, verificationCode);

                if (!emailSent)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_EmailFailed");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                deferral.Complete();
            };

            var verifyResult = await verifyDialog.ShowAsync();
            if (verifyResult != ContentDialogResult.Primary) return;

            // ═══════════════════════════════════════════════════════════════
            // PASSO 3: Pedir código do email
            // ═══════════════════════════════════════════════════════════════
            DateTime codeTime = DateTime.Now;

            var codePanel = new StackPanel() { Spacing = 15, HorizontalAlignment = HorizontalAlignment.Center };

            codePanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_EmailSent"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
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
                    Padding = new Thickness(0)
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

            var codeErrorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            codePanel.Children.Add(codeErrorText);

            ContentDialog codeDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_EnterEmailCode"),
                Content = codePanel,
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_DeleteButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            codeDialog.Resources["ContentDialogMinWidth"] = 600;

            codeDialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                // Verificar se expirou
                if ((DateTime.Now - codeTime).TotalSeconds > 300)
                {
                    codeErrorText.Text = resourceLoader.GetString("SettingsPage_CodeExpired");
                    codeErrorText.Visibility = Visibility.Visible;
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

                if (enteredCode.Length != 8)
                {
                    codeErrorText.Text = resourceLoader.GetString("SettingsPage_InvalidCode");
                    codeErrorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                if (enteredCode != verificationCode)
                {
                    codeErrorText.Text = resourceLoader.GetString("SettingsPage_InvalidCode");
                    codeErrorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Apagar conta
                bool deleted = await LoginQuery.DeleteAccount(_userEmail);
                if (!deleted)
                {
                    codeErrorText.Text = resourceLoader.GetString("SettingsPage_DeleteFailed");
                    codeErrorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                deferral.Complete();
            };

            var deleteResult = await codeDialog.ShowAsync();

            if (deleteResult == ContentDialogResult.Primary)
            {
                _encryptor?.Clear();
                App.MainFrame.Navigate(typeof(LoginPage));
            }
        }

        private async void ChangeSecurityQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            // ═══════════════════════════════════════════════════════════════
            // PASSO 1: Verificar identidade (password + TOTP)
            // ═══════════════════════════════════════════════════════════════
            var content = new StackPanel() { Spacing = 20, MinWidth = 320 };

            // Ícone
            var iconBorder = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 245, 253)),
                CornerRadius = new CornerRadius(35),
                Width = 70,
                Height = 70,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new FontIcon()
                {
                    Glyph = "\uE72E",
                    FontSize = 32,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 152, 219))
                }
            };
            content.Children.Add(iconBorder);

            // Título
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_VerifyIdentity"),
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Instruções
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_VerifyIdentityDesc"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            // Password atual
            var passwordPanel = new StackPanel() { Spacing = 8 };
            passwordPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_CurrentPassword"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            var passwordBox = new PasswordBox()
            {
                PlaceholderText = "••••••••",
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            passwordPanel.Children.Add(passwordBox);
            content.Children.Add(passwordPanel);

            // Código TOTP
            var totpPanel = new StackPanel() { Spacing = 8 };
            totpPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_CodeLabel"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            var totpBox = new TextBox()
            {
                PlaceholderText = "000000",
                MaxLength = 6,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            totpPanel.Children.Add(totpBox);
            content.Children.Add(totpPanel);

            // Mensagem de erro
            var errorPanel = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 235, 235)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Visibility = Visibility.Collapsed
            };
            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            errorPanel.Child = errorText;
            content.Children.Add(errorPanel);

            ContentDialog dialog = new ContentDialog()
            {
                Content = content,
                PrimaryButtonText = resourceLoader.GetString("SettingsPage_Continue"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            string verificationCode = "";

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                string password = passwordBox.Password;
                string totpCode = totpBox.Text.Trim();

                // Validar campos
                if (string.IsNullOrEmpty(password))
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_PasswordRequired");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                if (string.IsNullOrEmpty(totpCode) || totpCode.Length != 6)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar password
                var loginResult = await LoginQuery.LoginVerification(_userEmail, password);
                if (!loginResult.Success)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidPassword");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar TOTP
                byte[] secretKey = Base32Encoding.ToBytes(_totpSecret);
                var totp = new Totp(secretKey);
                if (!totp.VerifyTotp(totpCode, out long timeStepMatched, new VerificationWindow(previous: 0, future: 0)))
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Gerar código de verificação e enviar email
                verificationCode = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString();
                bool emailSent = await EmailService.SendSecurityQuestionsChangeCodeAsync(_userEmail, verificationCode);

                if (!emailSent)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_EmailFailed");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                deferral.Complete();
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Pedir código de verificação do email
                await VerifyEmailCodeForSecurityQuestions(verificationCode);
            }
        }

        private async System.Threading.Tasks.Task VerifyEmailCodeForSecurityQuestions(string correctCode)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            DateTime codeTime = DateTime.Now;

            // Criar TextBoxes para o código
            var panel = new StackPanel() { Spacing = 15, HorizontalAlignment = HorizontalAlignment.Center };

            panel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_EmailSent"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
            });

            var codePanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };

            TextBox[] boxes = new TextBox[8];
            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new TextBox()
                {
                    MaxLength = 1,
                    Width = 45,
                    Height = 50,
                    FontSize = 22,
                    TextAlignment = TextAlignment.Center
                };

                int index = i;
                boxes[i].TextChanged += (s, args) =>
                {
                    if (boxes[index].Text.Length == 1 && index < 7)
                    {
                        boxes[index + 1].Focus(FocusState.Programmatic);
                    }
                };

                codePanel.Children.Add(boxes[i]);
            }
            panel.Children.Add(codePanel);

            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(errorText);

            ContentDialog codeDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_EnterEmailCode"),
                Content = panel,
                PrimaryButtonText = resourceLoader.GetString("TOTP_VerifyButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            codeDialog.Resources["ContentDialogMinWidth"] = 600;

            bool codeCorrect = false;

            codeDialog.PrimaryButtonClick += (s, args) =>
            {
                // Verificar se expirou (5 minutos)
                if ((DateTime.Now - codeTime).TotalSeconds > 300)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_CodeExpired");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                // Juntar código
                string enteredCode = "";
                foreach (var box in boxes) 
                {
                    enteredCode += box.Text;
                }

                if (enteredCode != correctCode)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_InvalidCode");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                codeCorrect = true;
            };

            var result = await codeDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && codeCorrect)
            {
                // Pedir novas respostas de segurança
                await EnterNewSecurityQuestions();
            }
        }

        private async System.Threading.Tasks.Task EnterNewSecurityQuestions()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            var panel = new StackPanel() { Spacing = 15, MinWidth = 320 };

            panel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("SettingsPage_NewSecurityQuestionsDesc"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100))
            });

            // Pergunta 1
            panel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("CreateAccount_MaleNameQuestion"),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 0)
            });
            var maleNameBox = new TextBox()
            {
                PlaceholderText = resourceLoader.GetString("CreateAccount_MaleNamePlaceholder")
            };
            panel.Children.Add(maleNameBox);

            // Pergunta 2
            panel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("CreateAccount_FemaleNameQuestion"),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 0)
            });
            var femaleNameBox = new TextBox()
            {
                PlaceholderText = resourceLoader.GetString("CreateAccount_FemaleNamePlaceholder")
            };
            panel.Children.Add(femaleNameBox);

            var errorText = new TextBlock()
            {
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                Visibility = Visibility.Collapsed,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0)
            };
            panel.Children.Add(errorText);

            ContentDialog dialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_ChangeSecurityQuestions"),
                Content = panel,
                PrimaryButtonText = resourceLoader.GetString("SaveButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                string maleName = maleNameBox.Text.Trim();
                string femaleName = femaleNameBox.Text.Trim();

                if (string.IsNullOrEmpty(maleName) || string.IsNullOrEmpty(femaleName))
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_FillAllFields");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Atualizar no MongoDB
                bool updated = await LoginQuery.UpdateSecurityAnswers(_userEmail, maleName, femaleName);

                if (!updated)
                {
                    errorText.Text = resourceLoader.GetString("SettingsPage_UpdateFailed");
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Enviar email informativo
                _ = EmailService.SendSecurityQuestionsChangedAsync(_userEmail);

                deferral.Complete();
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Sucesso!
                ContentDialog successDialog = new ContentDialog()
                {
                    Title = resourceLoader.GetString("SettingsPage_SecurityQuestionsChanged"),
                    Content = resourceLoader.GetString("SettingsPage_SecurityQuestionsChangedMessage"),
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
    }
}