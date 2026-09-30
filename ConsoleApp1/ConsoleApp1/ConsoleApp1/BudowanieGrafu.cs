using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    public class BudowniczySieciPrzeplywowej
    {
        private const long NIESKONCZONOSC = long.MaxValue / 4;

        public List<Region> Regiony { get; }
        public List<Pole> Pola { get; }
        public List<Browar> Browary { get; }
        public List<Karczma> Karczmy { get; }
        public List<Droga> Drogi { get; }

        private int zrodlo, ujscie;
        private int pierwszyWezelPola;
        private int pierwszeSkrzyzowanieJeczmien, liczbaSkrzyzowanJeczmien;
        private int pierwszyBrowarJeczmien, pierwszyBrowarPiwo;
        private int pierwszeSkrzyzowaniePiwo, liczbaSkrzyzowanPiwo;
        private int pierwszyWezelKarczma;
        private int lacznaLiczbaWezlow;

        // Mapa z indeksami w warstwie jęczmienia i piwa
        private Dictionary<Punkt, int> indeksSkrzyzowaniaJeczmien;
        private Dictionary<Punkt, int> indeksSkrzyzowaniaPiwo;

        public SiecPrzeplywu Siec { get; private set; }

        private const double EPS = 1e-6;

        private bool CzyRowne(Punkt a, Punkt b)
        {
            return Math.Abs(a.X - b.X) < EPS && Math.Abs(a.Y - b.Y) < EPS;
        }

        private bool CzyPolaczenieJestPoprawneJeczmien(Punkt z, Punkt do_)
        {
            bool zPoleLubBrowar = Pola.Any(p => CzyRowne(p.Lokalizacja, z)) || Browary.Any(b => CzyRowne(b.Lokalizacja, z));
            bool doBrowar = Browary.Any(b => CzyRowne(b.Lokalizacja, do_));

            return zPoleLubBrowar && doBrowar;
        }


        private bool CzyPolaczenieJestPoprawnePiwo(Punkt z, Punkt do_)
        {
            bool zBrowar = Browary.Any(b => CzyRowne(b.Lokalizacja, z));
            bool doKarczmaLubBrowar = Karczmy.Any(k => CzyRowne(k.Lokalizacja, do_)) || Browary.Any(b => CzyRowne(b.Lokalizacja, do_));

            return zBrowar && doKarczmaLubBrowar;
        }


        public BudowniczySieciPrzeplywowej(
            List<Region> regiony,
            List<Pole> pola,
            List<Browar> browary,
            List<Karczma> karczmy,
            List<Droga> drogi,
            bool uzywajKosztow /* czy uwzględniamy koszty, false - bez kosztów czyli maksymalny przepływ, 
                                true - z kosztami jednostkowymi czyli maksymalny przepływ o minimalnym koszcie */
            )
        {
            Regiony = regiony;
            Pola = pola;
            Browary = browary;
            Karczmy = karczmy;
            Drogi = drogi;
            BudujIndeksowanieSkrzyzowan();
            ObliczPlonyPol();

            BudujPrzesunieciaWezlow(uzywajKosztow);
            Siec = new SiecPrzeplywu(lacznaLiczbaWezlow);
            BudujGraf(uzywajKosztow);
        }

        // Indeksujemy wszystkie unikalne skrzyżowania (punkty)
        private void BudujIndeksowanieSkrzyzowan()
        {
            indeksSkrzyzowaniaJeczmien = new Dictionary<Punkt, int>();
            indeksSkrzyzowaniaPiwo = new Dictionary<Punkt, int>();

            HashSet<Punkt> unikalne = new HashSet<Punkt>();

            foreach (var d in Drogi)
            {
                if (!CzyPolaczenieJestPoprawnePiwo(d.Z, d.Do))
                {
                    Console.WriteLine($"[DEBUG - PIWO] Pomijam: {d.Z.X},{d.Z.Y} → {d.Do.X},{d.Do.Y}");
                    continue;
                }
                unikalne.Add(d.Z);
                unikalne.Add(d.Do);
            }

            foreach (var b in Browary)
                unikalne.Add(b.Lokalizacja);
            foreach (var k in Karczmy)
                unikalne.Add(k.Lokalizacja);
            foreach (var p in Pola)
                unikalne.Add(p.Lokalizacja);

            int idx = 0;
            foreach (var p in unikalne)
            {
                indeksSkrzyzowaniaJeczmien[p] = idx++;
            }
            liczbaSkrzyzowanJeczmien = idx;

            idx = 0;
            foreach (var p in unikalne)
            {
                indeksSkrzyzowaniaPiwo[p] = idx++;
            }
            liczbaSkrzyzowanPiwo = idx;
        }

        // W jakim regionie znajduje się pole, ustawienie plonu na pole z danego regionu
        private void ObliczPlonyPol()
        {
            foreach (var p in Pola)
            {
                p.AktualnyPlon = 0;
                foreach (var r in Regiony)
                {
                    if (Geometria.CzyPunktWPolygonie_RegulaParzystosci(p.Lokalizacja, r.WierzcholkiPoligonu))
                    {
                        p.AktualnyPlon = r.PlonNaPole;
                        break;
                    }
                }
            }
        }

        // Oblicza przesunięcia węzłów w grafie.
        private void BudujPrzesunieciaWezlow(bool uzywajKosztow)
        {
            zrodlo = 0;

            pierwszyWezelPola = 1;
            int liczbaPol = Pola.Count;

            pierwszeSkrzyzowanieJeczmien = pierwszyWezelPola + liczbaPol;
            int liczbaJeczmienInter = liczbaSkrzyzowanJeczmien;

            pierwszyBrowarJeczmien = pierwszeSkrzyzowanieJeczmien + liczbaJeczmienInter;
            int liczbaBrowarow = Browary.Count;

            pierwszyBrowarPiwo = pierwszyBrowarJeczmien + liczbaBrowarow;
            int liczbaPiwoBrowarow = liczbaBrowarow;

            pierwszeSkrzyzowaniePiwo = pierwszyBrowarPiwo + liczbaPiwoBrowarow;
            int liczbaPiwoInter = liczbaSkrzyzowanPiwo;

            pierwszyWezelKarczma = pierwszeSkrzyzowaniePiwo + liczbaPiwoInter;
            int liczbaKarczm = Karczmy.Count;

            ujscie = pierwszyWezelKarczma + liczbaKarczm;

            lacznaLiczbaWezlow = ujscie + 1;
        }

        private void BudujGraf(bool uzywajKosztow)
        {
            var g = Siec;

            for (int i = 0; i < Pola.Count; i++)
            {
                int wezelPola = pierwszyWezelPola + i;
                long pojemnosc = (long)Math.Round(Pola[i].AktualnyPlon);
                if (pojemnosc < 0) pojemnosc = 0;

                g.DodajKrawedzSkierowana(zrodlo, wezelPola, pojemnosc, koszt: 0);

                Punkt punkt = Pola[i].Lokalizacja;
                int idxSkr = indeksSkrzyzowaniaJeczmien[punkt];
                int wezelJeczmien = pierwszeSkrzyzowanieJeczmien + idxSkr;

                g.DodajKrawedzSkierowana(wezelPola, wezelJeczmien, pojemnosc, koszt: 0);
            }

            foreach (var d in Drogi)
            {
                int u = pierwszeSkrzyzowanieJeczmien + indeksSkrzyzowaniaJeczmien[d.Z];
                int v = pierwszeSkrzyzowanieJeczmien + indeksSkrzyzowaniaJeczmien[d.Do];
                long pojemnoscJeczmien = (long)Math.Round(d.PojemnoscJeczmienia);
                long kosztJeczmien = uzywajKosztow ? (long)Math.Round(d.KosztNaprawy) : 0;
                if (pojemnoscJeczmien > 0)
                {
                    g.DodajKrawedzSkierowana(u, v, pojemnoscJeczmien, kosztJeczmien);
                    g.DodajKrawedzSkierowana(v, u, pojemnoscJeczmien, kosztJeczmien);
                    Debug.WriteLine($"[DEBUG-BE] Krawędź (jęczmień): ({d.Z.X},{d.Z.Y})→({d.Do.X},{d.Do.Y}) pojJeczmien={pojemnoscJeczmien} koszt={kosztJeczmien}");
                }
            }

            for (int i = 0; i < Browary.Count; i++)
            {
                Punkt punkt = Browary[i].Lokalizacja;
                int idxSkr = indeksSkrzyzowaniaJeczmien[punkt];
                int wezelJeczmien = pierwszeSkrzyzowanieJeczmien + idxSkr;
                int wezelBrowarJeczmien = pierwszyBrowarJeczmien + i;
                long pojemnoscBrowar = (long)Math.Round(Browary[i].PojemnoscJeczmienia);

                g.DodajKrawedzSkierowana(wezelJeczmien, wezelBrowarJeczmien, pojemnoscBrowar, koszt: 0);
            }

            for (int i = 0; i < Browary.Count; i++)
            {
                int wezelBrowarJeczmien = pierwszyBrowarJeczmien + i;
                int wezelBrowarPiwo = pierwszyBrowarPiwo + i;
                long pojemnoscBrowar = (long)Math.Round(Browary[i].PojemnoscJeczmienia);

                g.DodajKrawedzSkierowana(wezelBrowarJeczmien, wezelBrowarPiwo, pojemnoscBrowar, koszt: 0);
            }

            for (int i = 0; i < Browary.Count; i++)
            {
                Punkt punkt = Browary[i].Lokalizacja;
                int idxSkr = indeksSkrzyzowaniaPiwo[punkt];
                int wezelPiwo = pierwszeSkrzyzowaniePiwo + idxSkr;
                int wezelBrowarPiwo = pierwszyBrowarPiwo + i;
                long pojemnoscBrowar = (long)Math.Round(Browary[i].PojemnoscJeczmienia);

                g.DodajKrawedzSkierowana(wezelBrowarPiwo, wezelPiwo, pojemnoscBrowar, koszt: 0);
            }

            foreach (var d in Drogi)
            {
                int u = pierwszeSkrzyzowaniePiwo + indeksSkrzyzowaniaPiwo[d.Z];
                int v = pierwszeSkrzyzowaniePiwo + indeksSkrzyzowaniaPiwo[d.Do];
                long pojemnoscPiwo = (long)Math.Round(d.PojemnoscPiwa);
                long kosztPiwo = uzywajKosztow ? (long)Math.Round(d.KosztNaprawy) : 0;
                if (pojemnoscPiwo > 0)
                {
                    g.DodajKrawedzSkierowana(u, v, pojemnoscPiwo, kosztPiwo);
                    g.DodajKrawedzSkierowana(v, u, pojemnoscPiwo, kosztPiwo);
                }
            }

            for (int i = 0; i < Karczmy.Count; i++)
            {
                Punkt punkt = Karczmy[i].Lokalizacja;
                int idxSkr = indeksSkrzyzowaniaPiwo[punkt];
                int wezelPiwo = pierwszeSkrzyzowaniePiwo + idxSkr;
                int wezelKarczma = pierwszyWezelKarczma + i;

                g.DodajKrawedzSkierowana(wezelPiwo, wezelKarczma, NIESKONCZONOSC, koszt: 0);

                g.DodajKrawedzSkierowana(wezelKarczma, ujscie, NIESKONCZONOSC, koszt: 0);
            }
        }

        // Do wyświetlania jakimi drogami poszła produkcja jęczmienia i piwa.

        public List<(Droga droga, long przeplywJeczmien, long przeplywPiwo)> PobierzWykorzystanePrzeplywyDrog()
        {
            var wynik = new List<(Droga, long, long)>();

            foreach (var d in Drogi)
            {
                bool czyDrogaDlaJeczmienia = CzyPolaczenieJestPoprawneJeczmien(d.Z, d.Do);
                bool czyDrogaDlaPiwa = CzyPolaczenieJestPoprawnePiwo(d.Z, d.Do);

                long fJeczmien = 0, fPiwo = 0;

                if (czyDrogaDlaJeczmienia)
                {
                    int uJeczmien = pierwszeSkrzyzowanieJeczmien + indeksSkrzyzowaniaJeczmien[d.Z];
                    int vJeczmien = pierwszeSkrzyzowanieJeczmien + indeksSkrzyzowaniaJeczmien[d.Do];

                    foreach (var kraw in Siec.Sasiedzi[uJeczmien])
                    {
                        if (kraw.Cel == vJeczmien && kraw.Pojemnosc > 0)
                        {
                            fJeczmien = kraw.Przeplyw;
                            break;
                        }
                    }
                }

                if (czyDrogaDlaPiwa)
                {
                    int uPiwo = pierwszeSkrzyzowaniePiwo + indeksSkrzyzowaniaPiwo[d.Z];
                    int vPiwo = pierwszeSkrzyzowaniePiwo + indeksSkrzyzowaniaPiwo[d.Do];

                    foreach (var kraw in Siec.Sasiedzi[uPiwo])
                    {
                        if (kraw.Cel == vPiwo && kraw.Pojemnosc > 0)
                        {
                            fPiwo = kraw.Przeplyw;
                            break;
                        }
                    }
                }

                if (fJeczmien > 0 || fPiwo > 0)
                    wynik.Add((d, fJeczmien, fPiwo));
            }

            return wynik;
        }

    }
}
