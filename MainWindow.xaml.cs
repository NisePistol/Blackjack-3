using System.Threading;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.IO;

namespace Blackjack_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int spelarePoäng = 0;
        static int datorPoäng = 0;
        static int runda = 1;
        static bool spelareHarEss = false;
        static bool datorHarEss = false;
        static bool harSatsat = false;
        static bool rundaÄrÖver = false;
        static int pengar = 100;
        static string datornsFörstaKort;
        static string spelarensNamn;
        static List<Spelare> sparadeAnvändare = new List<Spelare>();
        static string filNamn = "Inloggningar.json";

        static double mainGridHöjd;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void KlickSatsa(object sender, RoutedEventArgs e)
        {
            if (!harSatsat)
            {
                if (int.TryParse(rutaSatsning.Text, out int intSatsning))
                {
                    if (intSatsning <= pengar && intSatsning > 0)
                    {
                        //Skriver ut hur mycket användaren har efter satsningen
                        rutaPengar.Text = $"${pengar - intSatsning}";

                        //Tar bort det spelaren satsat från dens totala pengar
                        pengar -= intSatsning;

                        //Skriver ut att spelaren har satsat
                        MessageBox.Show($"Du har satsat ${intSatsning}");
                        harSatsat = true;
                    }
                    else
                    {
                        MessageBox.Show($"Din satsning måste vara mellan 1 - {pengar}");
                    }
                }
                else
                {
                    MessageBox.Show("Du måste skriva ett heltal!");
                    rutaSatsning.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Du har redan satsat denna runda!");
            }

        }

        private void KlickTaKort(object sender, RoutedEventArgs e)
        {
            if (rutaSatsning.Text == "")
            {
                MessageBox.Show("Du måste satsa först!");
            }
            else
            {
                if (!rundaÄrÖver)
                {
                    if (spelarePoäng <= 21)
                    {
                        //Slumpar kort åt spelaren och datorn
                        spelareKort.Text += SlumpaKort("spelare") + ",\n";
                    }

                    //Första rundan visas datorns kort
                    if (runda == 1)
                    {
                        datorKort.Text += SlumpaKort("dator") + ",\n";
                        datornsFörstaKort = datorKort.Text;
                    }
                    //Andra rundan visas inte datorns kort
                    else if (runda == 2)
                    {
                        datorKort.Text += "GÖMT KORT,\n";
                    }

                    //Om spelaren eller datorn har ett ess och får över 21 så ändras esset till en 1a
                    if (spelarePoäng > 21 && spelareHarEss)
                    {
                        spelarePoäng -= 10;
                        spelareHarEss = false;
                    }
                    if (datorPoäng > 21 && datorHarEss)
                    {
                        datorPoäng -= 10;
                        datorHarEss = false;
                    }

                    //Detta är endast för debugging
                    spelare.Text = spelarePoäng.ToString();
                    dator.Text = datorPoäng.ToString();

                    runda++;

                    if (spelarePoäng > 21)
                    {
                        MessageBox.Show("Du har över 21, du förlorade!");

                        //Skriver ut spelarens pengar till csv filen
                        UppdateraSpelarensPengar();
                        rundaÄrÖver = true;
                    }
                }
            }
        }

        private string SlumpaKort(string vem)
        {
            string kort = "";
            Random generator = new Random();
            int kortNummer = generator.Next(2, 14);

            if (kortNummer < 10)
            {
                for (var i = 2; i < 10; i++)
                {
                    if (i == kortNummer)
                    {
                        kort = kortNummer.ToString();
                        AdderaPoäng(vem, kortNummer);
                    }
                }
            }
            else if (kortNummer == 10)
            {
                kort = "Knäckt (10)";
                AdderaPoäng(vem, 10);
            }
            else if (kortNummer == 11)
            {
                kort = "Dam (10)";
                AdderaPoäng(vem, 10);
            }
            else if (kortNummer == 12)
            {
                kort = "Kung (10)";
                AdderaPoäng(vem, 10);
            }
            else if (kortNummer == 13)
            {
                kort = "Ess (1 elr 11)";
                AdderaPoäng(vem, 11);
                if (vem == "spelare")
                {
                    spelareHarEss = true;
                }
                else
                {
                    datorHarEss = true;
                }
            }
            return kort;
        }

        private void AdderaPoäng(string vem, int kortNummer)
        {
            if (vem == "spelare")
            {
                spelarePoäng += kortNummer;
            }
            else
            {
                datorPoäng += kortNummer;
            }
        }

        private void KlickStå(object sender, RoutedEventArgs e)
        {
            if (rutaSatsning.Text == "")
            {
                MessageBox.Show("Du måste satsa först!");
            }
            else
            {
                if (!rundaÄrÖver)
                {
                    //Tar bort texten "GÖMT KORT" från datorns kort
                    datorKort.Text = datornsFörstaKort;

                    //Datorn tar kort tills den har 17 poäng eller högre
                    while (datorPoäng < 17)
                    {
                        //Datorn tar kort och räknar ut hur mycket poäng den har
                        datorKort.Text += SlumpaKort("dator") + ",\n";

                        //Om Datorn har ett ess och får över 21 poäng så ändras esset till en 1a
                        if (datorHarEss && datorPoäng > 21)
                        {
                            datorPoäng -= 10;
                            datorHarEss = false;
                        }
                    }

                    //Om spelaren har mer poäng än datorn eller datorn får över 21 poäng så vinner spelaren
                    if (spelarePoäng > datorPoäng || datorPoäng > 21)
                    {
                        //Skriver ut att spelaren vann
                        MessageBox.Show("Du vann!");

                        //Multiplicerar spelarens satsning med 1.5 och ger tillbaka det till spelarens pengar
                        pengar += int.Parse(rutaSatsning.Text) * 2;
                        rutaPengar.Text = "$" + pengar.ToString();
                        rundaÄrÖver = true;

                        //Skriver ut spelarens nya pengar till json filen
                        UppdateraSpelarensPengar();
                    }

                    //Om spelaren och datorn får lika mycket poäng så får spelaren tillbaka det den satsade
                    else if (spelarePoäng == datorPoäng)
                    {
                        MessageBox.Show("Det blev lika!");
                        pengar += int.Parse(rutaSatsning.Text);
                        rutaPengar.Text = "$" + pengar.ToString();
                        rundaÄrÖver = true;
                    }
                    else
                    {
                        //Skriver ut spelarens pengar till csv filen
                        UppdateraSpelarensPengar();

                        MessageBox.Show("Datorn vann!");
                        rundaÄrÖver = true;
                    }
                }
            }
        }

        private void KlickBörjaOm(object sender, RoutedEventArgs e)
        {
            if (rundaÄrÖver)
            {
                dator.Text = "";
                spelare.Text = "";
                spelarePoäng = 0;
                datorPoäng = 0;
                datorKort.Text = "";
                spelareKort.Text = "";
                rutaSatsning.Text = "";
                harSatsat = false;
                runda = 1;
                datornsFörstaKort = "";
                rundaÄrÖver = false;
                datorHarEss = false;
                spelareHarEss = false;
            }
            else
            {
                MessageBox.Show("Rundan är inte över än!");
            }
        }

        private void KlickLoggaIn(object sender, RoutedEventArgs e)
        {
            //Gömmer inloggningsgriden
            gridInlogg.Visibility = Visibility.Hidden;
            gridInlogg.Height = 0;

            //Loggar in eller skapar nytt konto om spelaren inte har ett
            LoggaInEllerSkapaKonto();

            gridMain.Visibility = Visibility.Visible;
        }

        private void LoggaInEllerSkapaKonto()
        {
            spelarensNamn = rutaInloggNamn.Text.ToLower();

            bool användarenHittades = false;

            //Skapar intenderins inställningar för json filen
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            if (File.Exists(filNamn))
            {
                string jsonIn = File.ReadAllText(filNamn);

                sparadeAnvändare = JsonSerializer.Deserialize<List<Spelare>>(jsonIn);

                foreach (var användare in sparadeAnvändare)
                {
                    //Om spelarens användarnamn finns i json filen
                    if (spelarensNamn == användare.namn)
                    {
                        //Läser av spelarens sparade pengar
                        pengar = användare.pengar;

                        //Uppdaterar spelarens pengar i fönstret
                        rutaPengar.Text = "$" + pengar.ToString();

                        MessageBox.Show($"Användaren hittades\nNamn: {spelarensNamn}\nPengar: {pengar}");
                        användarenHittades = true;
                        break;
                    }
                }

                if (!användarenHittades)
                {
                    //Skapar ny spelare
                    Spelare nySpelare = new Spelare
                    {
                        namn = spelarensNamn,
                        pengar = 100
                    };

                    //Lägger till nya spelaren i listan av spelare
                    sparadeAnvändare.Add(nySpelare);

                    //Gör om listan av spelare till json format
                    string json = JsonSerializer.Serialize(sparadeAnvändare, options);

                    //Skriver ut listan av användare till json filen
                    File.WriteAllText(filNamn, json);

                    //Uppdaterar spelarens pengar i fönstret
                    rutaPengar.Text = "$" + pengar.ToString();

                    MessageBox.Show($"Nytt konto skapat...\nNamn: {spelarensNamn}\nPengar: 100 ");
                }
            }

            //Om filen inte hittades
            else
            {
                //Skapar ny spelare
                Spelare nySpelare = new Spelare
                {
                    namn = spelarensNamn,
                    pengar = 100
                };

                //Lägger till nya spelaren i listan av spelare
                sparadeAnvändare.Add(nySpelare);

                //Gör om listan av spelare till json format
                string json = JsonSerializer.Serialize(sparadeAnvändare, options);

                //Skriver ut listan av användare till json filen
                File.WriteAllText(filNamn, json);

                //Uppdaterar spelarens pengar i fönstret
                rutaPengar.Text = "$" + pengar.ToString();

                MessageBox.Show("Filen hittades inte... Ny fil skapad");
            }
        }

        public static void UppdateraSpelarensPengar()
        {
            //loopar igenom alla användare i json filen
            for (var i = 0; i < sparadeAnvändare.Count; i++)
            {
                //Lägger in namnet för profilen med index 'i' i variabeln "namn"
                string namn = sparadeAnvändare[i].namn;

                //Kollar om det finns en profil med användarnamnet som användaren skrev in
                if (namn == spelarensNamn)
                {
                    //Uppdaterar hur muycket pengar spelaren har
                    sparadeAnvändare[i].pengar = pengar;

                    //Skapar intenderins inställningar för json filen
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    //Gör om listan av spelare till json format
                    string json = JsonSerializer.Serialize(sparadeAnvändare, options);

                    //Skriver ut listan av användare till json filen
                    File.WriteAllText(filNamn, json);
                    break;
                }
            }
        }
    }

    class Spelare
    {
        public string namn { get; set; }
        public int pengar { get; set; }
    }
}