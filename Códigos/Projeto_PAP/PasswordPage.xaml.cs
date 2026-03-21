using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static QRCoder.PayloadGenerator.WiFi;

namespace Projeto_PAP
{
    public sealed partial class PasswordsPage : Page
    {
        public ObservableCollection<PasswordItem> Passwords { get; set; }
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private string _totpSecret;  // ADICIONAR
        private PasswordQuery _passwordQuery;

        public PasswordsPage()
        {
            InitializeComponent();
            Passwords = new ObservableCollection<PasswordItem>();
            _passwordQuery = new PasswordQuery();
            AddButton.PointerEntered += AddButton_PointerEntered;
            AddButton.PointerExited += AddButton_PointerExited;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
                _totpSecret = parameters.TotpSecret;  // ADICIONAR
            }

            await LoadPasswordsFromDatabase();
        }

        private async Task LoadPasswordsFromDatabase()
        {
            Passwords.Clear();

            var passwordsFromDb = await _passwordQuery.GetPasswordsAsync(_userEmail);

            foreach (var password in passwordsFromDb)
            {
                Passwords.Add(password);
            }

            if (Passwords.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                PasswordsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                PasswordsList.Visibility = Visibility.Visible;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO PARA VERIFICAR CÓDIGO TOTP
        // ═══════════════════════════════════════════════════════════════
        private async Task<bool> VerifyTotpAndPassword()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            // Conteúdo principal
            var content = new StackPanel() { Spacing = 20, MinWidth = 320 };

            // Ícone de segurança
            var iconBorder = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 243, 232)),
                CornerRadius = new CornerRadius(35),
                Width = 70,
                Height = 70,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new FontIcon()
                {
                    Glyph = "\uE72E",  // Lock icon
                    FontSize = 32,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 126, 34))
                }
            };
            content.Children.Add(iconBorder);

            // Título
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_DialogTitle"),
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Instruções
            content.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_DialogInstructionsWithPassword"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Separador
            content.Children.Add(new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 240, 240)),
                Height = 1,
                Margin = new Thickness(0, 5, 0, 5)
            });

            // ═══════════════════════════════════════════════════════════════
            // Campo do código TOTP
            // ═══════════════════════════════════════════════════════════════
            var codePanel = new StackPanel() { Spacing = 8 };

            // Label com ícone
            var codeLabelPanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            codeLabelPanel.Children.Add(new FontIcon()
            {
                Glyph = "\uE8D7",  // Shield
                FontSize = 14,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 126, 234))
            });
            codeLabelPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_CodeLabel"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 80, 80))
            });
            codePanel.Children.Add(codeLabelPanel);

            // TextBox do código
            TextBox codeBox = new TextBox()
            {
                PlaceholderText = "000000",
                MaxLength = 6,
                FontSize = 24,
                FontFamily = new FontFamily("Consolas"),
                TextAlignment = TextAlignment.Center,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 12, 15, 12),
                BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 200, 200)),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 250, 250, 250))
            };
            codePanel.Children.Add(codeBox);

            content.Children.Add(codePanel);

            // ═══════════════════════════════════════════════════════════════
            // Campo da password
            // ═══════════════════════════════════════════════════════════════
            var passwordPanel = new StackPanel() { Spacing = 8 };

            // Label com ícone
            var passwordLabelPanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            passwordLabelPanel.Children.Add(new FontIcon()
            {
                Glyph = "\uE8D7",  // Key
                FontSize = 14,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 126, 234))
            });
            passwordLabelPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("TOTP_PasswordLabel"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 80, 80))
            });
            passwordPanel.Children.Add(passwordLabelPanel);

            // PasswordBox
            PasswordBox passwordBox = new PasswordBox()
            {
                PlaceholderText = "••••••••",
                FontSize = 16,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 12, 15, 12),
                BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 200, 200)),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 250, 250, 250))
            };
            passwordPanel.Children.Add(passwordBox);

            content.Children.Add(passwordPanel);

            // ═══════════════════════════════════════════════════════════════
            // Mensagem de erro
            // ═══════════════════════════════════════════════════════════════
            var errorPanel = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 235, 235)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Visibility = Visibility.Collapsed
            };

            var errorContent = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            errorContent.Children.Add(new FontIcon()
            {
                Glyph = "\uE783",  // Error icon
                FontSize = 14,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60))
            });

            TextBlock errorText = new TextBlock()
            {
                Text = "",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60)),
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            };
            errorContent.Children.Add(errorText);

            errorPanel.Child = errorContent;
            content.Children.Add(errorPanel);

            // ═══════════════════════════════════════════════════════════════
            // Dialog
            // ═══════════════════════════════════════════════════════════════
            ContentDialog dialog = new ContentDialog()
            {
                Content = content,
                PrimaryButtonText = resourceLoader.GetString("TOTP_VerifyButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            bool verified = false;

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();

                string code = codeBox.Text.Trim();
                string password = passwordBox.Password;

                // Verificar se campos estão preenchidos
                if (string.IsNullOrEmpty(code) || code.Length != 6)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    errorText.Text = resourceLoader.GetString("TOTP_PasswordRequired");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar código TOTP
                byte[] secretKey = Base32Encoding.ToBytes(_totpSecret);
                var totp = new Totp(secretKey);
                bool isValidCode = totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(previous: 0, future: 0));

                if (!isValidCode)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidCode");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                // Verificar password
                var result = await LoginQuery.LoginVerification(_userEmail, password);
                if (!result.Success)
                {
                    errorText.Text = resourceLoader.GetString("TOTP_InvalidPassword");
                    errorPanel.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    deferral.Complete();
                    return;
                }

                verified = true;
                deferral.Complete();
            };

            var dialogResult = await dialog.ShowAsync();

            return dialogResult == ContentDialogResult.Primary && verified;
        }


        // ═══════════════════════════════════════════════════════════════
        // VER PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private async void ViewPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var passwordItem = button?.DataContext as PasswordItem;
            if (passwordItem == null) return;

            // Pedir código TOTP
            bool verified = await VerifyTotpAndPassword();
            if (!verified) return;

            // Desencriptar password
            string decryptedPassword = _encryptor.Decrypt(passwordItem.EncryptedPassword);

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            // Criar conteúdo bonito
            var content = new StackPanel() { Spacing = 15, MinWidth = 350 };

            // Ícone e título do site
            var headerPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var siteIcon = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 126, 234)),
                CornerRadius = new CornerRadius(8),
                Width = 45,
                Height = 45,
                Child = new TextBlock()
                {
                    Text = passwordItem.SiteName.Substring(0, 1).ToUpper(),
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    FontSize = 20,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            var siteNameText = new TextBlock()
            {
                Text = passwordItem.SiteName,
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(siteIcon);
            headerPanel.Children.Add(siteNameText);
            content.Children.Add(headerPanel);

            // Separador
            content.Children.Add(new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 230, 230)),
                Height = 1,
                Margin = new Thickness(0, 5, 0, 5)
            });

            // Email
            var emailPanel = new StackPanel() { Spacing = 5 };
            emailPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("ViewDialog_Email"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136))
            });
            var emailBorder = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10)
            };
            emailBorder.Child = new TextBlock()
            {
                Text = passwordItem.Email,
                FontSize = 14,
                IsTextSelectionEnabled = true
            };
            emailPanel.Children.Add(emailBorder);
            content.Children.Add(emailPanel);

            // Password
            var passwordPanel = new StackPanel() { Spacing = 5 };
            passwordPanel.Children.Add(new TextBlock()
            {
                Text = resourceLoader.GetString("ViewDialog_Password"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136))
            });
            var passwordBorder = new Border()
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10)
            };
            passwordBorder.Child = new TextBlock()
            {
                Text = decryptedPassword,
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                IsTextSelectionEnabled = true
            };
            passwordPanel.Children.Add(passwordBorder);
            content.Children.Add(passwordPanel);

            // URL (se existir)
            if (!string.IsNullOrWhiteSpace(passwordItem.Url))
            {
                var urlPanel = new StackPanel() { Spacing = 5 };
                urlPanel.Children.Add(new TextBlock()
                {
                    Text = "URL:",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136))
                });
                var urlBorder = new Border()
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 10, 12, 10)
                };
                urlBorder.Child = new TextBlock()
                {
                    Text = passwordItem.Url,
                    FontSize = 14,
                    IsTextSelectionEnabled = true,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 126, 234))
                };
                urlPanel.Children.Add(urlBorder);
                content.Children.Add(urlPanel);
            }

            // Mostrar dialog
            ContentDialog viewDialog = new ContentDialog()
            {
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await viewDialog.ShowAsync();
        }

        // ═══════════════════════════════════════════════════════════════
        // COPIAR PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private async void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var passwordItem = button?.DataContext as PasswordItem;
            if (passwordItem == null) return;

            // Pedir código TOTP
            bool verified = await VerifyTotpAndPassword();
            if (!verified) return;

            // Desencriptar e copiar
            string decryptedPassword = _encryptor.Decrypt(passwordItem.EncryptedPassword);

            var dataPackage = new DataPackage();
            dataPackage.SetText(decryptedPassword);
            Clipboard.SetContent(dataPackage);

            // Encontrar o ícone dentro do botão e mudar para check
            if (button.Content is FontIcon icon)
            {
                string originalGlyph = icon.Glyph;
                var originalForeground = icon.Foreground;

                // Mudar para check verde
                icon.Glyph = "\uE73E";  // Checkmark
                icon.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 39, 174, 96));

                // Esperar 1.5 segundos
                await Task.Delay(1500);

                // Voltar ao original
                icon.Glyph = originalGlyph;
                icon.Foreground = originalForeground;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EDITAR PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private async void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var passwordItem = button?.DataContext as PasswordItem;
            if (passwordItem == null) return;

            // Pedir código TOTP
            bool verified = await VerifyTotpAndPassword();
            if (!verified) return;

            // Desencriptar password atual
            string decryptedPassword = _encryptor.Decrypt(passwordItem.EncryptedPassword);

            // Criar dialog de edição com dados preenchidos
            var dialogContent = new AddPasswordDialog();
            dialogContent.SiteName = passwordItem.SiteName;
            dialogContent.Url = passwordItem.Url;
            dialogContent.Email = passwordItem.Email;
            dialogContent.Password = decryptedPassword;
            dialogContent.Category = passwordItem.Category;
            dialogContent.Notes = passwordItem.Notes;

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            ContentDialog dialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("EditDialogTitle"),
                Content = dialogContent,
                PrimaryButtonText = resourceLoader.GetString("SaveButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (!dialogContent.IsValid())
                {
                    args.Cancel = true;
                    dialogContent.ShowError();
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Encriptar nova password
                string encryptedPassword = _encryptor.Encrypt(dialogContent.Password);

                // Atualizar objeto
                passwordItem.SiteName = dialogContent.SiteName;
                passwordItem.Url = dialogContent.Url;
                passwordItem.Email = dialogContent.Email;
                passwordItem.EncryptedPassword = encryptedPassword;
                passwordItem.Category = dialogContent.Category;
                passwordItem.Notes = dialogContent.Notes;

                // Atualizar no MongoDB
                bool success = await _passwordQuery.UpdatePasswordAsync(_userEmail, passwordItem);

                if (success)
                {
                    await LoadPasswordsFromDatabase();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // APAGAR PASSWORD
        // ═══════════════════════════════════════════════════════════════
        private async void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var passwordItem = button?.DataContext as PasswordItem;
            if (passwordItem == null) return;

            // Pedir código TOTP diretamente
            bool verified = await VerifyTotpAndPassword();
            if (!verified) return;

            // Apagar do MongoDB
            bool success = await _passwordQuery.DeletePasswordAsync(_userEmail, passwordItem.Id);

            if (success)
            {
                Passwords.Remove(passwordItem);
                await LoadPasswordsFromDatabase();

                // Mostrar mensagem de sucesso rápida
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

                ContentDialog successDialog = new ContentDialog()
                {
                    Content = new StackPanel()
                    {
                        Spacing = 15,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                {
                    new FontIcon()
                    {
                        Glyph = "\uE73E",  // Checkmark
                        FontSize = 40,
                        Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 39, 174, 96))
                    },
                    new TextBlock()
                    {
                        Text = resourceLoader.GetString("DeleteDialog_Success"),
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
                    },
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await successDialog.ShowAsync();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // HOVER ANIMATIONS
        // ═══════════════════════════════════════════════════════════════
        private void AddButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)AddButton.Resources["HoverIn"];
            storyboard.Begin();
        }

        private void AddButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)AddButton.Resources["HoverOut"];
            storyboard.Begin();
        }

        private void LoadPasswords()
        {
            if (Passwords.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                PasswordsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                PasswordsList.Visibility = Visibility.Visible;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogContent = new AddPasswordDialog();
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            ContentDialog dialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("AddDialogTitle"),
                Content = dialogContent,
                PrimaryButtonText = resourceLoader.GetString("SaveButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (!dialogContent.IsValid())
                {
                    args.Cancel = true;
                    dialogContent.ShowError();
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string encryptedPassword = _encryptor.Encrypt(dialogContent.Password);

                var newPassword = new PasswordItem
                {
                    SiteName = dialogContent.SiteName,
                    Url = dialogContent.Url,
                    Email = dialogContent.Email,
                    EncryptedPassword = encryptedPassword,
                    Category = dialogContent.Category,
                    Notes = dialogContent.Notes
                };

                bool success = await _passwordQuery.AddPasswordAsync(_userEmail, newPassword);

                if (success)
                {
                    Passwords.Add(newPassword);
                    await LoadPasswordsFromDatabase();
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implementar pesquisa
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implementar filtro
        }
    }
}