using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nomes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window //Classe public que pode ser acessada pelo projeto todo e que é parcial (partial) porque parte dela é gerada automaticamente pelo XAML. A classe está dividida em 2 ficheiros. O Visual Studio (ou o compilador do WPF) lê o ficheiro XAML e gera código C# baseado nele. 
      //A classe chama-se MainWindow e herda (:) da classe Window (uma janela do WPF)
      //A classe Window é uma classe do WPF que representa uma janela do Windows. O molde de uma janela normal do windows.
    {
        public MainWindow() //É o construtor da classe MainWindow. É chamado quando a janela é criada. Está tudo no ficheiro App.xaml
        {
            InitializeComponent(); // Lê o ficheiro XAML e cria todos os controlos (botões, caixas de texto, etc.).
            // Eu vou estar a chamar a função InitializeComponent do código c# gerado automáticamente com base no meu xml. Daí partial
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e) //private = só pode ser usada dentro desta classe
//void = não devolve nenhum valor
//sender = o botão que foi clicado
//e = informações extra sobre o clique
        {
            if (!string.IsNullOrWhiteSpace(txtNome.Text) && !lstNomes.Items.Contains(txtNome.Text))
            { // Se a string que o utilizador insere (txtNome.Text) não estiver vazia ou composta apenas por espaços em branco. E se a lista (lstNomes.Items) não contiver já o nome inserido

                // O .Text obtém o texto inserido na caixa de texto txtNome
                //Já o string é uma classe do c# que tem métodos úteis para trabalhar com strings (cadeias de caracteres)

                lstNomes.Items.Add(txtNome.Text); // Adiciona o nome à lista
                txtNome.Clear(); // Limpa a caixa de texto, para de seguida inseririr um novo nome
            }
        }
    }
}