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
using Windows.Foundation;
using Windows.Foundation.Collections;
using MongoDB.Driver;
using System.Net.Mail;
using System.Security.Cryptography;
using Windows.Storage;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Projeto_PAP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateAccount : Page
    {
        public CreateAccount()
        {
            InitializeComponent();
        }

        private async void CreateAccountClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
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

            string local_folder = ApplicationData.Current.LocalFolder.Path;
            string path_file = Path.Combine(local_folder, "email_attempts.json"); 
            Dictionary<string, List<DateTime>> emails = new Dictionary<string, List<DateTime>>(); 

            if (File.Exists(path_file))
            {
                string json_content = File.ReadAllText(path_file);
                emails = JsonSerializer.Deserialize<Dictionary<string, List<DateTime>>>(json_content);
            }
            else
            {
                File.Create(path_file).Close();
            }

            if (emails.ContainsKey(EmailTextBox.Text))
            {
                int amount_email_dateTime = emails[EmailTextBox.Text].Count;
                DateTime last_time_email = emails[EmailTextBox.Text].Last();
                if (amount_email_dateTime % 5 == 0 && (DateTime.Now - last_time_email).TotalSeconds < 300)
                {
                    ContentDialog erroAttempts = new ContentDialog()
                    {
                        Title = "Error",
                        Content = "Number of attempts per email exceeded, try again in 5 minutes!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await erroAttempts.ShowAsync();
                    return;
                }
            }
            else
            {
                var time = DateTime.Now;
                emails.Add(EmailTextBox.Text, time);
            }

            if (ConfirmPasswordBox.Password != PasswordBox.Password)
            {
                ContentDialog erroMessage = new ContentDialog()
                {
                    Title = "Error",
                    Content = "The passwords do not match!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await erroMessage.ShowAsync();
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                return; //sai do método se as passwords não coincidirem
            }
            
            string codeVerification = RandomNumberGenerator.GetInt32(10000000, 100000000).ToString(); //Gera um número aleatório de 8 dígitos para verificação por email
            bool result = await EmailService.SendVerificationCodeAsync(EmailTextBox.Text.Trim(), codeVerification); //envia o código de verificação para o email do utilizador

            if (result) 
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

                ContentDialog verificationCode = new ContentDialog()
                {
                    Title = "Enter Code",
                    Content = panel,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Verify",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };
                verificationCode.Resources["ContentDialogMinWidth"] = 600;
                
                ContentDialogResult resultCode = await verificationCode.ShowAsync();
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
                        if (enteredCode == codeVerification)
                        {
                            var hashedPassword = PasswordHash.HashPassword(PasswordBox.Password); 
                            var client = new MongoClient("mongodb+srv://Pmtsilva08:008421Apmcjmo!@cluster0.quvshkk.mongodb.net/?appName=Cluster0");
                            var database = client.GetDatabase("PAP");
                            var collection = database.GetCollection<Account>("Users"); //Quando eu ler documentos ou inserir documentos nesta coleção vou usar a classe Account como molde 
                            var user = new Account
                            {
                                FirstName = FirstNameTextBox.Text.Trim(),
                                Lastname = LastNameTextBox.Text.Trim(),
                                EmailAddress = EmailTextBox.Text.Trim().ToLower(),
                                Password = hashedPassword
                            };

                            try
                            {
                                collection.InsertOne(user);
                                Frame.Navigate(typeof(LoginPage));
                            }
                            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) //Erro de escrita do MongoDB (MongoWriteException) é guardado na variável "ex", mas só quando (when) a categoria do erro for igual a um erro de duplicação de chaves (ex.WriteError.Category == ServerErrorCategory.DuplicateKey). Na minha base de dados é se o email for repetido
                            {
                                ContentDialog erroEmailDuplicate = new ContentDialog()
                                {
                                    Title = "Error",
                                    Content = "This email is already registered!",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.XamlRoot
                                };
                                EmailTextBox.Text = string.Empty;
                                await erroEmailDuplicate.ShowAsync();
                                return;
                            }
                            catch (Exception) //um outro erro qualquer
                            {
                                ContentDialog erroMessage = new ContentDialog()
                                {
                                    Title = "Error",
                                    Content = "Connection error. Please try again.",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.XamlRoot
                                };
                                await erroMessage.ShowAsync();
                                return;
                            }
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
                    return;
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
        

        private void Login_Click(object sender,  RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
