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

            if(ConfirmPasswordBox.Password != PasswordBox.Password)
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

        private void Login_Click(object sender,  RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
