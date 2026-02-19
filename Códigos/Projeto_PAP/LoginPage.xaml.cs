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
using System.Net.Http.Json;
using System.Net.Mail;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Projeto_PAP;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
            string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ContentDialog erroNullTextBox = new ContentDialog()
            {
                Title = "Error",
                Content = "Please fill in all fields!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await erroNullTextBox.ShowAsync();
            return; //sai do método se algum campo estiver vazio
        }

        try
        {
            MailAddress mailAddress = new MailAddress(EmailTextBox.Text);

        }
        catch (Exception)
        {
            ContentDialog erroMailAddress = new ContentDialog()
            {
                Title = "Error",
                Content = "Invalid email!",
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
                TextBox num1 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num2 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num3 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num4 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num5 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num6 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num7 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };
                TextBox num8 = new TextBox() { MaxLength = 1, Width = 10, TextAlignment = TextAlignment.Center };

                num1.TextChanged += (s, args) => //TextChanged é um evento que ocorre sempre que o texto de um TextBox é alterado
                //o += serve para adicionar uma função ao evento TextChanged
                //O "s" é o remetente do evento (quem "acionou" o evento) (neste caso, o TextBox num1) e "args" são os argumentos do evento (informações adicionais sobre o evento, que neste caso não são usadas). Neste caso não precisamos de args, mas colocar isso é obrigatório na sintaxe
                {
                    if (num1.Text.Length == 1)
                    {
                        num2.Focus(FocusState.Programmatic); // .Focus muda o foco para o próximo TextBox. FocusState.Programmatic indica que o foco foi alterado por código (programaticamente) e não pelo utilizador
                    }
                };

                //Uma função lambda é uma função sem nome que eu crio ao mesmo tempo que a uso. Serve para poupar tempo e código quando precisas de uma função pequena que só usas uma vez.

                num2.TextChanged += (s, args) =>
                {
                    if (num2.Text.Length == 1)
                    {
                        num3.Focus(FocusState.Programmatic);
                    }
                };

                num3.TextChanged += (s, args) =>
                {
                    if (num3.Text.Length == 1)
                    {
                        num4.Focus(FocusState.Programmatic);
                    }
                };

                num4.TextChanged += (s, args) =>
                {
                    if (num4.Text.Length == 1)
                    {
                        num5.Focus(FocusState.Programmatic);
                    }
                };

                num5.TextChanged += (s, args) =>
                {
                    if (num5.Text.Length == 1)
                    {
                        num6.Focus(FocusState.Programmatic);
                    }
                };

                num6.TextChanged += (s, args) =>
                {
                    if (num6.Text.Length == 1)
                    {
                        num7.Focus(FocusState.Programmatic);
                    }
                };

                num7.TextChanged += (s, args) =>
                {
                    if (num7.Text.Length == 1)
                    {
                        num8.Focus(FocusState.Programmatic);
                    }
                };

                StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 5 };

                panel.Children.Add(num1);
                panel.Children.Add(num2);
                panel.Children.Add(num3);
                panel.Children.Add(num4);
                panel.Children.Add(num5);
                panel.Children.Add(num6);
                panel.Children.Add(num7);
                panel.Children.Add(num8);

                ContentDialog code_dialog = new ContentDialog()
                {
                    Title = "Enter Code",
                    Content = panel,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Verify",
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
                            Title = "Error",
                            Content = "The verification code has expired. Please try again.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await erroTimeCode.ShowAsync();
                        return;
                    }
                    else
                    {
                        string enteredCode = num1.Text + num2.Text + num3.Text + num4.Text + num5.Text + num6.Text + num7.Text + num8.Text;
                        if (enteredCode == code_verification)
                        {
                            var user = new Account
                            {
                                EmailAddress = EmailTextBox.Text,
                            };

                            ActiveProfile.current_user = user;
                            Frame.Navigate(typeof(MainPage));
                        }
                        else
                        {
                            ContentDialog erroCodeVerification = new ContentDialog()
                            {
                                Title = "Error",
                                Content = "Invalid verification code!",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await erroCodeVerification.ShowAsync();
                            return;
                        }
                    }
                }
                else
                {
                    ContentDialog erroemail = new ContentDialog()
                    {
                        Title = "Error",
                        Content = "Failed to send verification email. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await erroemail.ShowAsync();
                    return;
                }


            }
        }
        else if (!result.Success)
        {
            if (result.Message == "Email not found")
            {
                ContentDialog erroMailNotFound = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Email Not Found!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await erroMailNotFound.ShowAsync();
                return;
            }
            else if (result.Message == "Connection to the Data Base was lost. Verify your Wifi connection")
            {
                ContentDialog erroPassword = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Connection to the Data Base was lost. Verify your Wifi connection",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await erroPassword.ShowAsync();
                return;
            }
            else
            {
                ContentDialog erroPassword = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Incorrect Password!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await erroPassword.ShowAsync();
                return;
            }
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(CreateAccount));
    }
}
