using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MongoDB.Bson;
using MongoDB.Driver;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Teste1
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void SubmitClick(object sender, RoutedEventArgs e)
         /* | `private` | Só pode ser usada dentro desta classe |
            | `void` | Não retorna nada |
            | `SubmitClick` | Nome da função(igual ao que está no XAML) |
            | `object sender` | O elemento que disparou o evento(o botão) |
            | `RoutedEventArgs e` | Informações sobre o evento | */
        {
            var client = new MongoClient("mongodb+srv://Pmtsilva08:008421Apmcjmo!@cluster0.quvshkk.mongodb.net/?appName=Cluster0");
            var database = client.GetDatabase("testes");
            var collection = database.GetCollection<Mongo>("utilizadores");
            var user = new Mongo
            {
                User = UserName.Text,
                Password = Password.Text
            };
            collection.InsertOne(user);
        }

    }
}
