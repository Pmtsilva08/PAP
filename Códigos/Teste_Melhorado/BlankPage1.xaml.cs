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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Teste_Melhorado
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        public BlankPage1()
        {
            InitializeComponent();
        }

        private void Submit_Note(object sender, RoutedEventArgs e)
        {
            var client = new MongoClient("mongodb+srv://Pmtsilva08:008421Apmcjmo!@cluster0.quvshkk.mongodb.net/?appName=Cluster0");
            var database = client.GetDatabase("testes"); 
            var collection = database.GetCollection<Account>("utilizadores");
            //Estou a dizer: "Quando eu inserir ou ler documentos desta coleção, usa a classe Account como molde."
            var note = new Note
            {
                Title = Note_Title.Text,
                Content = Note_Content.Text
            };

            var filter = Builders<Account>.Filter.Eq(a => a.Id, Session.UserLogin.Id);
            //crio um filtro
            //Builders é uma classe do MongoDB.Driver que ajuda a construir filtros, atualizações e outras operações de forma mais fácil e legível.
            //.Filter estou a dizer que vou construir um filtro
            //.Eq significa "equals" (igual a). Estou a criar um filtro que procura o campo ID da base de dados que seja igual ao ID do utilizador que está logado na sessão atual. 
            var update = Builders<Account>.Update.Push(a => a.Note, note);
            collection.UpdateOne(filter, update);

        }

    }
}
