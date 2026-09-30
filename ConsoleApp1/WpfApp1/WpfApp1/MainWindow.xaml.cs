using System;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using ST = ConsoleApp1;
using ConsoleApp1;
using System.Text;
using System.Text.RegularExpressions;

namespace ShireTransportFrontend
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ObliczPrzeplyw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTekst.Text = "Parsowanie danych i wyznaczanie pól w ćwiartkach…";

                List<ST.Region> regiony = ParsujRegiony(RegionyTextBox.Text);
                List<ST.Pole> pola = ParsujPola(PolaTextBox.Text);
                List<ST.Browar> browary = ParsujBrowary(BrowaryTextBox.Text);
                List<ST.Karczma> karczmy = ParsujKarczmy(KarczmyTextBox.Text);
                List<ST.Droga> drogi = ParsujDrogi(DrogiTextBox.Text);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("-- Ćwiartki i pola, które w nich leżą --");
                for (int i = 0; i < regiony.Count; i++)
                {
                    var region = regiony[i];
                    sb.AppendLine($"Ćwiartka {i + 1} (ilość jęczmienia na pole = {region.PlonNaPole}):");
                    bool anyPole = false;

                    for (int j = 0; j < pola.Count; j++)
                    {
                        var fld = pola[j];
                        if (ST.Geometria.CzyPunktWPolygonie_RegulaParzystosci(fld.Lokalizacja, region.WierzcholkiPoligonu))
                        {
                            sb.AppendLine($"   • Pole {j + 1} : ({fld.Lokalizacja.X.ToString(CultureInfo.InvariantCulture)},{fld.Lokalizacja.Y.ToString(CultureInfo.InvariantCulture)})");
                            anyPole = true;
                        }
                    }

                    if (!anyPole)
                    {
                        sb.AppendLine("   (brak pól w tej ćwiartce)");
                    }
                    sb.AppendLine();
                }

                WynikiTextBox.Text = sb.ToString();

                bool uwzgledniKoszty = UwzglednijKosztyCheckBox.IsChecked == true;

                var budowniczy = new ST.BudowniczySieciPrzeplywowej(regiony, pola, browary, karczmy, drogi, uwzgledniKoszty);
                var siec = budowniczy.Siec;
                int S = 0;
                int T = siec.LiczbaWezlow - 1;

                long koncowyPrzeplyw = 0;
                long koncowyKoszt = 0;
                bool pokazujUzyteDrogi = true;

                if (!uwzgledniKoszty)
                {
                    var fordfulk = new ST.FordFulkerson(siec);
                    koncowyPrzeplyw = fordfulk.MaksymalnyPrzeplyw(S, T);
                    WynikiTextBox.AppendText($"\nMaksymalna ilość piwa (bez kosztów): {koncowyPrzeplyw}\n\n");

                    var wykorzystane = budowniczy.PobierzWykorzystanePrzeplywyDrog();

                    WynikiTextBox.AppendText("-- Użyte drogi (warstwa jęczmienia) --\n");
                    foreach (var (droga, j, _) in wykorzystane)
                    {
                        if (j > 0)
                            WynikiTextBox.AppendText($"  {droga.Z.X},{droga.Z.Y} → {droga.Do.X},{droga.Do.Y} : Przepływ jęczmienia = {j}\n");
                    }

                    WynikiTextBox.AppendText("\n-- Użyte drogi (warstwa piwa) --\n");
                    foreach (var (droga, _, p) in wykorzystane)
                    {
                        if (p > 0)
                            WynikiTextBox.AppendText($"  {droga.Z.X},{droga.Z.Y} → {droga.Do.X},{droga.Do.Y} : Przepływ piwa = {p}\n");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(BudzetTextBox.Text))
                    {
                        var mcmfFull = new ST.MinKosztMaxPrzeplyw(siec);
                        (koncowyPrzeplyw, koncowyKoszt) = mcmfFull.PobierzMaxPrzeplywMinKoszt(S, T);
                        WynikiTextBox.AppendText($"Maksymalna ilość piwa (min-cost): {koncowyPrzeplyw}\n" + $"Minimalny koszt napraw: {koncowyKoszt}\n\n");
                    }
                    else
                    {
                        if (!long.TryParse(BudzetTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out long budzet))
                        {
                            MessageBox.Show("Niepoprawny format budżetu. Upewnij się, że wpisałeś liczbę całkowitą.", "Błąd formatu", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var mcmf = new ST.MinKosztMaxPrzeplyw(siec);
                        (long flowAll, long costAll) = mcmf.PobierzMaxPrzeplywMinKoszt(S, T);

                        if (costAll <= budzet)
                        {
                            koncowyPrzeplyw = flowAll;
                            koncowyKoszt = costAll;
                            WynikiTextBox.AppendText($"Maksymalna ilość piwa (min-cost): {koncowyPrzeplyw}\n" + $"Minimalny koszt napraw: {koncowyKoszt}\n" + $"(Całość mieści się w budżecie {budzet}.)\n\n");
                        }
                        else
                        {
                            long lewo = 0, prawo = flowAll;
                            long najlepszyFlow = 0, najlepszyKoszt = 0;

                            mcmf.ResetujWszystkiePrzeplywy();
                            while (lewo <= prawo)
                            {
                                long srodek = (lewo + prawo) / 2;
                                mcmf.ResetujWszystkiePrzeplywy();
                                (long uzyskanyFlow, long kosztNaFlow) = mcmf.PobierzMinKosztDokladnegoPrzeplywu(S, T, srodek);

                                if (uzyskanyFlow < srodek)
                                {
                                    prawo = srodek - 1;
                                }
                                else
                                {
                                    if (kosztNaFlow <= budzet)
                                    {
                                        najlepszyFlow = srodek;
                                        najlepszyKoszt = kosztNaFlow;
                                        lewo = srodek + 1;
                                    }
                                    else
                                    {
                                        prawo = srodek - 1;
                                    }
                                }
                            }

                            if (najlepszyFlow == 0)
                            {
                                WynikiTextBox.AppendText($"Nie da się dostarczyć żadnej ilości piwa w ramach budżetu {budzet}.\n" + $"Najtańszy koszt dostarczenia choćby jednej jednostki jest wyższy niż {budzet}.\n\n");
                                pokazujUzyteDrogi = false;
                            }
                            else
                            {
                                koncowyPrzeplyw = najlepszyFlow;
                                koncowyKoszt = najlepszyKoszt;
                                WynikiTextBox.AppendText($"W zadanym budżecie {budzet} największy możliwy przepływ piwa: {koncowyPrzeplyw}\n" + $"Koszt dla tego przepływu: {koncowyKoszt}\n\n");
                                mcmf.ResetujWszystkiePrzeplywy();
                                mcmf.PobierzMinKosztDokladnegoPrzeplywu(S, T, najlepszyFlow);
                            }



                            StatusTekst.Text = "Obliczono.";
                        }
                    }
                    if (pokazujUzyteDrogi && koncowyPrzeplyw > 0)
                    {
                        var uzyteDrogi = budowniczy.PobierzWykorzystanePrzeplywyDrog();

                        WynikiTextBox.AppendText("-- Użyte drogi (warstwa jęczmienia) --\n");
                        foreach (var (droga, przeplywJeczmien, przeplywPiwa) in uzyteDrogi)
                        {
                            if (przeplywJeczmien > 0)
                            {
                                string from = $"{droga.Z.X.ToString(CultureInfo.InvariantCulture)},{droga.Z.Y.ToString(CultureInfo.InvariantCulture)}";
                                string to = $"{droga.Do.X.ToString(CultureInfo.InvariantCulture)},{droga.Do.Y.ToString(CultureInfo.InvariantCulture)}";
                                WynikiTextBox.AppendText($"  {from} → {to} : Przepływ jęczmienia = {przeplywJeczmien}\n");
                            }
                        }

                        WynikiTextBox.AppendText("\n-- Użyte drogi (warstwa piwa) --\n");
                        foreach (var (droga, przeplywJeczmien, przeplywPiwa) in uzyteDrogi)
                        {
                            if (przeplywPiwa > 0)
                            {
                                string from = $"{droga.Z.X.ToString(CultureInfo.InvariantCulture)},{droga.Z.Y.ToString(CultureInfo.InvariantCulture)}";
                                string to = $"{droga.Do.X.ToString(CultureInfo.InvariantCulture)},{droga.Do.Y.ToString(CultureInfo.InvariantCulture)}";
                                WynikiTextBox.AppendText($"  {from} → {to} : Przepływ piwa = {przeplywPiwa}\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WynikiTextBox.Text = "Błąd: " + ex.Message;
                StatusTekst.Text = "Wystąpił błąd podczas obliczeń.";
            }
        }

        private List<ST.Region> ParsujRegiony(string text)
        {
            var regiony = new List<ST.Region>();

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => !l.StartsWith("#") && l.Contains(":"));

            foreach (var rawLine in lines)
            {
                string line = rawLine.Replace(" ", "");
                int idxDwukropek = line.LastIndexOf(':');
                if (idxDwukropek < 0) continue;

                string czescWierzcholkow = line.Substring(0, idxDwukropek);
                string czescPlonu = line.Substring(idxDwukropek + 1);

                if (!double.TryParse(czescPlonu, NumberStyles.Any, CultureInfo.InvariantCulture, out double plonNaPole))
                {
                    throw new FormatException($"Niepoprawny plon w regionie: '{rawLine}'");
                }

                var vertTokens = czescWierzcholkow.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).Where(v => v.Contains(",")).ToList();

                var wierzcholki = new List<ST.Punkt>();
                foreach (var vt in vertTokens)
                {
                    var coords = vt.Split(',');
                    if (coords.Length != 2)
                        throw new FormatException($"Niepoprawny wierzchołek w regionie: '{vt}'");

                    double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                    double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                    wierzcholki.Add(new ST.Punkt(x, y));
                }

                foreach (var p in wierzcholki)
                    Debug.WriteLine($"     ({p.X},{p.Y})");

                regiony.Add(new ST.Region(wierzcholki, plonNaPole));
            }

            return regiony;
        }

        private List<ST.Pole> ParsujPola(string text)
        {
            var pola = new List<ST.Pole>();

            var tokens = text.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !s.StartsWith("#") && s.Contains(","));

            foreach (var token in tokens)
            {
                var coords = token.Split(',');
                if (coords.Length != 2) continue;
                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                pola.Add(new ST.Pole(new ST.Punkt(x, y)));
            }

            return pola;
        }

        private List<ST.Browar> ParsujBrowary(string text)
        {
            var browary = new List<ST.Browar>();

            var tokens = text.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !s.StartsWith("#") && s.Contains(":"));

            foreach (var token in tokens)
            {
                var parts = token.Split(':');
                if (parts.Length != 2) continue;

                var coordPart = parts[0];
                var capPart = parts[1];

                var coords = coordPart.Split(',');
                if (coords.Length != 2) continue;
                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                double pojemnosc = double.Parse(capPart, CultureInfo.InvariantCulture);

                browary.Add(new ST.Browar(new ST.Punkt(x, y), pojemnosc));
            }

            return browary;
        }

        private List<ST.Karczma> ParsujKarczmy(string text)
        {
            var karczmy = new List<ST.Karczma>();

            var tokens = text.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !s.StartsWith("#") && s.Contains(","));

            foreach (var token in tokens)
            {
                var coords = token.Split(',');
                if (coords.Length != 2) continue;
                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                karczmy.Add(new ST.Karczma(new ST.Punkt(x, y)));
            }

            return karczmy;
        }

        private List<ST.Droga> ParsujDrogi(string text)
        {
            var drogi = new List<ST.Droga>();

            var tokens = text.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !s.StartsWith("#") && s.Contains(":") && s.Contains("-"));

            foreach (var token in tokens)
            {
                var mainParts = token.Split(':');
                if (mainParts.Length != 2) continue;

                var endpoints = mainParts[0];
                var values = mainParts[1];

                var coords = endpoints.Split('-');
                if (coords.Length != 2) continue;

                var fromCoords = coords[0].Split(',');
                var toCoords = coords[1].Split(',');
                if (fromCoords.Length != 2 || toCoords.Length != 2) continue;

                double x1 = double.Parse(fromCoords[0], CultureInfo.InvariantCulture);
                double y1 = double.Parse(fromCoords[1], CultureInfo.InvariantCulture);
                double x2 = double.Parse(toCoords[0], CultureInfo.InvariantCulture);
                double y2 = double.Parse(toCoords[1], CultureInfo.InvariantCulture);

                var vals = values.Split(',');
                if (vals.Length != 3) continue;

                double pojemnoscJeczmienia = double.Parse(vals[0], CultureInfo.InvariantCulture);
                double pojemnoscPiwa = double.Parse(vals[1], CultureInfo.InvariantCulture);
                double kosztNaprawy = double.Parse(vals[2], CultureInfo.InvariantCulture);

                drogi.Add(new ST.Droga(new ST.Punkt(x1, y1), new ST.Punkt(x2, y2), pojemnoscJeczmienia, pojemnoscPiwa, kosztNaprawy));
            }

            return drogi;
        }

        private void OtworzPlik_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Wskaż plik tekstowy",
                Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*"
            };

            bool? wynik = dlg.ShowDialog(this);
            if (wynik == true)
            {
                try
                {
                    string sciezka = dlg.FileName;
                    string zawartosc = File.ReadAllText(sciezka);
                    TekstPlikuTextBox.Document.Blocks.Clear();
                    TekstPlikuTextBox.Document.Blocks.Add(new Paragraph(new Run(zawartosc)));
                    StatusPlikuText.Text = $"Wczytano plik: {System.IO.Path.GetFileName(sciezka)}";
                    WynikWyszukiwaniaText.Text = "";
                }
                catch (Exception ex)
                {
                    StatusPlikuText.Text = $"Błąd wczytywania pliku: {ex.Message}";
                }
            }
        }

        private void SzukajWPliku_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(TekstPlikuTextBox.Document.ContentStart, TekstPlikuTextBox.Document.ContentEnd);
            string dokument = textRange.Text;
            string wzorzec = WzorzecTextBox.Text;

            if (string.IsNullOrWhiteSpace(dokument))
            {
                WynikWyszukiwaniaText.Text = "Najpierw wczytaj plik.";
                return;
            }
            if (string.IsNullOrWhiteSpace(wzorzec))
            {
                WynikWyszukiwaniaText.Text = "Podaj wzorzec do wyszukania.";
                return;
            }

            List<int> pozycje =
                ConsoleApp1.WyszukiwanieNapisow.ZnajdzWszystkieKMP(
                    dokument,
                    wzorzec,
                    ignorujWielkoscLiter: true
                );

            if (pozycje.Count == 0)
            {
                WynikWyszukiwaniaText.Text = $"Nie znaleziono '{wzorzec}' w pliku.";
            }
            else
            {
                WynikWyszukiwaniaText.Text =
                    $"Znaleziono '{wzorzec}' {pozycje.Count} razy. Pozycje: {string.Join(", ", pozycje)}";
            }


            TekstPlikuTextBox.Document.Blocks.Clear();

            var paragraph = new Paragraph();
            int currentIndex = 0;

            foreach (int matchIndex in pozycje)
            {
                if (matchIndex > currentIndex)
                {
                    string beforeMatch = dokument.Substring(currentIndex, matchIndex - currentIndex);
                    paragraph.Inlines.Add(new Run(beforeMatch));
                }

                string matchText = dokument.Substring(matchIndex, wzorzec.Length);
                var highlightedRun = new Run(matchText)
                {
                    Background = Brushes.MediumSpringGreen,
                    Foreground = Brushes.Black
                };
                paragraph.Inlines.Add(highlightedRun);

                currentIndex = matchIndex + wzorzec.Length;
            }

            if (currentIndex < dokument.Length)
            {
                paragraph.Inlines.Add(new Run(dokument.Substring(currentIndex)));
            }

            TekstPlikuTextBox.Document.Blocks.Add(paragraph);

        }

        private void ZapiszPlik_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*",
                Title = "Zapisz dane do pliku"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("REGIONY");
                    sb.AppendLine(RegionyTextBox.Text.Trim());
                    sb.AppendLine("POLA");
                    sb.AppendLine(PolaTextBox.Text.Trim());
                    sb.AppendLine("BROWARY");
                    sb.AppendLine(BrowaryTextBox.Text.Trim());
                    sb.AppendLine("KARCZMY");
                    sb.AppendLine(KarczmyTextBox.Text.Trim());
                    sb.AppendLine("DROGI");
                    sb.AppendLine(DrogiTextBox.Text.Trim());

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    MessageBox.Show("Zapisano dane.", "Gotowe", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd zapisu: " + ex.Message);
                }
            }
        }

        private void WczytajPlik_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*",
                Title = "Wczytaj dane z pliku"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var text = File.ReadAllText(openFileDialog.FileName);

                    var sections = PodzielNaSekcje(text);

                    RegionyTextBox.Text = sections.TryGetValue("REGIONY", out var regiony) ? regiony : "";
                    PolaTextBox.Text = sections.TryGetValue("POLA", out var pola) ? pola : "";
                    BrowaryTextBox.Text = sections.TryGetValue("BROWARY", out var browary) ? browary : "";
                    KarczmyTextBox.Text = sections.TryGetValue("KARCZMY", out var karczmy) ? karczmy : "";
                    DrogiTextBox.Text = sections.TryGetValue("DROGI", out var drogi) ? drogi : "";

                    MessageBox.Show("Wczytano dane.", "Gotowe", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd odczytu: " + ex.Message);
                }
            }
        }

        private Dictionary<string, string> PodzielNaSekcje(string input)
        {
            var result = new Dictionary<string, string>();
            var pattern = @"^(REGIONY|POLA|BROWARY|KARCZMY|DROGI)\r?\n";
            var matches = Regex.Matches(input, pattern, RegexOptions.Multiline);

            for (int i = 0; i < matches.Count; i++)
            {
                var start = matches[i].Index + matches[i].Length;
                var end = (i + 1 < matches.Count) ? matches[i + 1].Index : input.Length;

                var header = matches[i].Groups[1].Value;
                var content = input.Substring(start, end - start).Trim();

                result[header] = content;
            }

            return result;
        }

    }
}
