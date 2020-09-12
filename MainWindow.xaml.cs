using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace ASYNCapp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void exectueSync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            RunDownloadSync();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total excetution time: {elapsedMs}";
        }
        private List<string> PrepData() {
            List<string> output = new List<string>();
            resultsWindow.Text = "";
            output.Add("https://www.yahoo.com");
            output.Add("https://www.google.com");
            output.Add("https://www.microsoft.com");
            output.Add("https://www.cnn.com");
            output.Add("https://www.codeproject.com");
            output.Add("https://www.stackoverflow.com");
            return output;
        }

        private void RunDownloadSync()
        {
            List<string> websites = PrepData();
            foreach (var site in websites)
            {
                WebsiteDataModel result = DownloadWebsite(site);
                ReportWebsiteInfo(result);
            }
        }

        private void ReportWebsiteInfo(WebsiteDataModel data)
        {
            resultsWindow.Text += $"{data.WebsiteUrl} downloaded: {data.WebsiteData.Length} characters long.{Environment.NewLine}";
        }

        private WebsiteDataModel DownloadWebsite(string websiteURL)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();
            output.WebsiteUrl = websiteURL;
            output.WebsiteData = client.DownloadString(websiteURL);
            return output;
            
        }
        /// <summary>
        /// ASYNC part
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void exectueAsync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await RunDownloadAsync();
            //bez tego słowa await  metodka pojdzie dalej tj. zatzryma stopwatcha i wyswietli czas zanim strony zostana pobrnae, dla tego
            //wzywamy await aby poczekał na to co sie dzieje w metodzie
            //pamietam o zmianie w zwiazkuz  tym metody na async, ALE nie zmieniam typu zwracanego na TASK bo w przypadku eventow ma zostac void mimo ze jest async
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total excetution time: {elapsedMs}";
        }
        private async Task RunDownloadAsync()
        {
            List<string> websites = PrepData();
            foreach (var site in websites)
            {
                WebsiteDataModel result = await Task.Run(() => DownloadWebsite(site)); //Task obudowuje asynchronicznie WebsiteDataModel to co ma zwrócić
                //slowko await mowi ze ma poczekac na to co ma byc zwrocone, wiec wykonaj to asnychronicznie ale poczekaj -> robie tak bo Report potrzebuje resulta wiec musi poczekac az go zrobi
                //zeby await działał musi być metoda asynchroniczna stad dopisane do metody słowo async
                //Natomaist TaskRun obudowuje dodatkowo cała funkcje downloadWebsite ktora sama w sobie nie jest asynchroniczna
                ReportWebsiteInfo(result);

                //nie zwracaj nigdy voida z metody async, jezeli nie masz nic do zwrócenia zwróć Task, jezeli masz zwrocic string to zwroc to ale jako Task<string>
                //dobrze jest do kazdej metodki ktora dziala asynchronicznie dopisac ASYNC na nkoniec
            }
        }
        /// <summary>
        /// Async część ale bez awaita który blokuje pobieranie wielu stron na raz
        /// </summary>
        /// <returns></returns>
        /// 

        private async void exectueParallelAsync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await RunDownloadParallelAsync();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total excetution time: {elapsedMs}";
        }
        private async Task RunDownloadParallelAsync()
        {
            List<string> websites = PrepData();
            List<Task<WebsiteDataModel>> tasks = new List<Task<WebsiteDataModel>>();
            foreach (var site in websites)
            {
                //tasks.Add( Task.Run(() => DownloadWebsite(site)));
                //wczesniejsza wersja  miała awaiata i Task.Run - task run zwraca nam nasz oczekiawny typ czyli tam było WebsiteDataModel
                //ale dodatkowo obudowuje go czyli teoerytcnzie zwraca Task<WebsiteDataModel>, my tu stworzylismy liste tego typu dokładniej
                //lista tego typu jak działa jak praca do wykonania  w tym samym czasie, i wykonuje je no
                tasks.Add(DownloadWebsiteAsync(site));
                //to działa tak samo jak to na górze oproócz tego ze nie musi obudowaywac metody asynchronicznie bo jest ona juz asynchroniczna
            }
            var results = await Task.WhenAll(tasks);
            //whenAll znaczy ze chce dostarczyc zestaw taskow, 1 albo setki a ty poczekaj tak dlugo az  wszystkie beda gotowe,
            // gdy wszystkie beda to przypisz result do zmiennej, u nas resulatatem bedzie arrayka WebsiteDataModel[]
            foreach (var item in results)
            {
                ReportWebsiteInfo(item);
            }
        }

        ///Teraz Async ale DownloadWebsite bd ez asynchroniczne (czyli nie  musi być obudowane przez Task.Run
        ///

        private async Task<WebsiteDataModel> DownloadWebsiteAsync(string websiteURL)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();
            output.WebsiteUrl = websiteURL;
            output.WebsiteData = await client.DownloadStringTaskAsync(websiteURL);
            // client ma ze soba od razu przyniesiona async wersje funkcji Download
            return output;

        }


    }
}
