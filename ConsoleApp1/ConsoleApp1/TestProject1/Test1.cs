using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConsoleApp1;
using System.Security.Cryptography;

namespace ConsoleApp1.Tests
{
    [TestClass]
    public class AlgorithmTests
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void FordFulkerson_MalyGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 10000;
            long lacznyCzasMs = 0;
            int oczekiwanyPrzeplyw = 5;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 4;
                var siec = new SiecPrzeplywu(n);
                siec.DodajKrawedzSkierowana(0, 1, 3);
                siec.DodajKrawedzSkierowana(0, 2, 2);
                siec.DodajKrawedzSkierowana(1, 2, 1);
                siec.DodajKrawedzSkierowana(1, 3, 2);
                siec.DodajKrawedzSkierowana(2, 3, 3);

                var fordfulk = new FordFulkerson(siec);

                var stoper = Stopwatch.StartNew();
                long przeplyw = fordfulk.MaksymalnyPrzeplyw(0, 3);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny MaxFlow (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {przeplyw}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[FF mały] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }
        [TestMethod]
        public void FordFulkerson_SredniGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 1000;
            long lacznyCzasMs = 0;
            int rozmiarL = 10, rozmiarR = 10;
            int oczekiwanyPrzeplyw = rozmiarL * rozmiarR;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 2 + rozmiarL + rozmiarR;
                int zrodlo = 0;
                int ujscie = n - 1;
                var siec = new SiecPrzeplywu(n);

                for (int u = 1; u <= rozmiarL; u++)
                {
                    siec.DodajKrawedzSkierowana(zrodlo, u, rozmiarL);
                }

                for (int u = 1; u <= rozmiarL; u++)
                {
                    for (int indeksR = 0; indeksR < rozmiarR; indeksR++)
                    {
                        int v = 1 + rozmiarL + indeksR;
                        siec.DodajKrawedzSkierowana(u, v, 1);
                    }
                }

                for (int v = 1 + rozmiarL; v < ujscie; v++)
                {
                    siec.DodajKrawedzSkierowana(v, ujscie, rozmiarR);
                }

                var fordfulk = new FordFulkerson(siec);

                var stoper = Stopwatch.StartNew();
                long przeplyw = fordfulk.MaksymalnyPrzeplyw(zrodlo, ujscie);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny MaxFlow (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {przeplyw}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[FF Średni] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }

        [TestMethod]
        public void FordFulkerson_DuzyGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 500;
            long lacznyCzasMs = 0;
            int rozmiarL = 50, rozmiarR = 50;
            int oczekiwanyPrzeplyw = rozmiarL * rozmiarR;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 2 + rozmiarL + rozmiarR;
                int zrodlo = 0;
                int ujscie = n - 1;
                var siec = new SiecPrzeplywu(n);

                for (int u = 1; u <= rozmiarL; u++)
                {
                    siec.DodajKrawedzSkierowana(zrodlo, u, rozmiarL);
                }

                for (int u = 1; u <= rozmiarL; u++)
                {
                    for (int indeksR = 0; indeksR < rozmiarR; indeksR++)
                    {
                        int v = 1 + rozmiarL + indeksR;
                        siec.DodajKrawedzSkierowana(u, v, 1);
                    }
                }

                for (int v = 1 + rozmiarL; v < ujscie; v++)
                {
                    siec.DodajKrawedzSkierowana(v, ujscie, rozmiarR);
                }

                var fordfulk = new FordFulkerson(siec);

                var stoper = Stopwatch.StartNew();
                long przeplyw = fordfulk.MaksymalnyPrzeplyw(zrodlo, ujscie);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny MaxFlow (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {przeplyw}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[FF Duży] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }

        [TestMethod]
        public void MinKosztMaxPrzeplyw_MalyGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 10000;
            long lacznyCzasMs = 0;
            int oczekiwanyPrzeplyw = 2;
            int oczekiwanyKoszt = 4;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 3;
                var siec = new SiecPrzeplywu(n);
                siec.DodajKrawedzSkierowana(0, 1, 2, koszt: 1);
                siec.DodajKrawedzSkierowana(1, 2, 2, koszt: 1);

                var mcmf = new MinKosztMaxPrzeplyw(siec);

                var stoper = Stopwatch.StartNew();
                var wynik = mcmf.PobierzMaxPrzeplywMinKoszt(0, 2);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, wynik.Przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny przepływ (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {wynik.Przeplyw}).");
                Assert.AreEqual(oczekiwanyKoszt, wynik.Koszt,
                    $"Iteracja {iteracja + 1}: niepoprawny koszt (oczekiwano {oczekiwanyKoszt}, otrzymano {wynik.Koszt}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[BF mały] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }

        [TestMethod]
        public void MinKosztMaxPrzeplyw_SredniGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 1000;
            long lacznyCzasMs = 0;
            int rozmiarL = 10, rozmiarR = 10;
            int oczekiwanyPrzeplyw = rozmiarL * rozmiarR;
            int oczekiwanyKoszt = oczekiwanyPrzeplyw * 1;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 2 + rozmiarL + rozmiarR;
                int zrodlo = 0;
                int ujscie = n - 1;
                var siec = new SiecPrzeplywu(n);

                for (int u = 1; u <= rozmiarL; u++)
                {
                    siec.DodajKrawedzSkierowana(zrodlo, u, rozmiarL, koszt: 0);
                }

                for (int u = 1; u <= rozmiarL; u++)
                {
                    for (int indeksR = 0; indeksR < rozmiarR; indeksR++)
                    {
                        int v = 1 + rozmiarL + indeksR;
                        siec.DodajKrawedzSkierowana(u, v, 1, koszt: 1);
                    }
                }

                for (int v = 1 + rozmiarL; v < ujscie; v++)
                {
                    siec.DodajKrawedzSkierowana(v, ujscie, rozmiarR, koszt: 0);
                }

                var mcmf = new MinKosztMaxPrzeplyw(siec);

                var stoper = Stopwatch.StartNew();
                var wynik = mcmf.PobierzMaxPrzeplywMinKoszt(zrodlo, ujscie);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, wynik.Przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny przepływ (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {wynik.Przeplyw}).");
                Assert.AreEqual(oczekiwanyKoszt, wynik.Koszt,
                    $"Iteracja {iteracja + 1}: niepoprawny koszt (oczekiwano {oczekiwanyKoszt}, otrzymano {wynik.Koszt}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[BF średni] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }

        [TestMethod]
        public void MinKosztMaxPrzeplyw_DuzyGraf_SredniCzas()
        {
            const int liczbaPowtorzen = 500;
            long lacznyCzasMs = 0;
            int rozmiarL = 50, rozmiarR = 50;
            int oczekiwanyPrzeplyw = rozmiarL * rozmiarR;
            int oczekiwanyKoszt = oczekiwanyPrzeplyw * 1;

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                int n = 2 + rozmiarL + rozmiarR;
                int zrodlo = 0;
                int ujscie = n - 1;
                var siec = new SiecPrzeplywu(n);

                for (int u = 1; u <= rozmiarL; u++)
                {
                    siec.DodajKrawedzSkierowana(zrodlo, u, rozmiarL, koszt: 0);
                }

                for (int u = 1; u <= rozmiarL; u++)
                {
                    for (int indeksR = 0; indeksR < rozmiarR; indeksR++)
                    {
                        int v = 1 + rozmiarL + indeksR;
                        siec.DodajKrawedzSkierowana(u, v, 1, koszt: 1);
                    }
                }

                for (int v = 1 + rozmiarL; v < ujscie; v++)
                {
                    siec.DodajKrawedzSkierowana(v, ujscie, rozmiarR, koszt: 0);
                }

                var mcmf = new MinKosztMaxPrzeplyw(siec);

                var stoper = Stopwatch.StartNew();
                var wynik = mcmf.PobierzMaxPrzeplywMinKoszt(zrodlo, ujscie);
                stoper.Stop();

                Assert.AreEqual(oczekiwanyPrzeplyw, wynik.Przeplyw,
                    $"Iteracja {iteracja + 1}: niepoprawny przepływ (oczekiwano {oczekiwanyPrzeplyw}, otrzymano {wynik.Przeplyw}).");
                Assert.AreEqual(oczekiwanyKoszt, wynik.Koszt,
                    $"Iteracja {iteracja + 1}: niepoprawny koszt (oczekiwano {oczekiwanyKoszt}, otrzymano {wynik.Koszt}).");
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"[BF duży] Średni czas na {liczbaPowtorzen} powtórzeniach: {sredniCzasMs:F2} ms");
        }

        [TestMethod]
        public void Geometria_PunktWKoniunkcyjnymWielokaciu_ProstokatTesty()
        {
            var square = new List<Punkt>
            {
                new Punkt(0, 0),
                new Punkt(0, 1),
                new Punkt(1, 1),
                new Punkt(1, 0)
            };

            var pInside = new Punkt(0.5, 0.5);
            bool isInside = Geometria.CzyPunktWPolygonie_RegulaParzystosci(pInside, square);
            Assert.IsTrue(isInside, "Punkt (0.5,0.5) powinien być wewnątrz.");

            var pOnEdge = new Punkt(1, 0.5);
            bool isOnEdge = Geometria.CzyPunktWPolygonie_RegulaParzystosci(pOnEdge, square);
            Assert.IsFalse(isOnEdge, "Punkt (1,0.5) na krawędzi powinien być traktowany jako wewnątrz.");

            var pOutside = new Punkt(1.5, 0.5);
            bool isOutside = Geometria.CzyPunktWPolygonie_RegulaParzystosci(pOutside, square);
            Assert.IsFalse(isOutside, "Punkt (1.5,0.5) powinien być na zewnątrz.");

            var pVertex = new Punkt(0, 0);
            bool isVertex = Geometria.CzyPunktWPolygonie_RegulaParzystosci(pVertex, square);
            Assert.IsTrue(isVertex, "Punkt (0,0) w wierzchołku powinien być traktowany jako wewnątrz.");
        }

        [TestMethod]
        public void Geometria_MierzSredniCzas_PunktWKoniunkcyjnymWielokaciu()
        {
            const int liczbaWierzcholkow = 100;
            var convexPolygon = new List<Punkt>();
            for (int i = 0; i < liczbaWierzcholkow; i++)
            {
                double kat = 2 * Math.PI * i / liczbaWierzcholkow;
                convexPolygon.Add(new Punkt(Math.Cos(kat), Math.Sin(kat)));
            }

            const int liczbaPowtorzen = 1000;
            long lacznyCzasTickow = 0;
            var rnd = new Random(42);

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                var p = new Punkt(rnd.NextDouble() * 4 - 2, rnd.NextDouble() * 4 - 2);
                var stoper = Stopwatch.StartNew();
                bool _ = Geometria.CzyPunktWPolygonie_RegulaParzystosci(p, convexPolygon);
                stoper.Stop();
                lacznyCzasTickow += stoper.ElapsedTicks;
            }

            double sredniaTickow = lacznyCzasTickow / (double)liczbaPowtorzen;
            double sredniCzasMs = (sredniaTickow / Stopwatch.Frequency) * 1000;
            TestContext.WriteLine($"Średni czas PunktWKoniukcyjnymWielokącie (na {liczbaPowtorzen} wywołaniach): {sredniCzasMs:F4} ms");
        }

        [TestMethod]
        public void WyszukiwanieNapisow_KMP_ZnajdzWszystkie_ZwracaPoprawneIndeksy()
        {
            string text = "ababcabcab";
            string pattern = "abc";
            var occurrences = WyszukiwanieNapisow.ZnajdzWszystkieKMP(text, pattern, ignorujWielkoscLiter: false);

            CollectionAssert.AreEqual(new List<int> { 2, 5 }, occurrences,
                "Niepoprawne indeksy wystąpień wzorca 'abc'.");
        }

        [TestMethod]
        public void WyszukiwanieNapisow_KMP_MierzSredniCzas_ZnajdzWszystkieKMP()
        {
            int dlugoscTekstu = 500000;
            var budowniczyTekstu = new System.Text.StringBuilder(dlugoscTekstu);
            for (int i = 0; i < dlugoscTekstu; i++)
                budowniczyTekstu.Append((char)('a' + (i % 26)));
            string text = budowniczyTekstu.ToString();

            const int liczbaPowtorzen = 10000;
            long lacznyCzasMs = 0;
            var rnd = new Random(123);

            for (int iteracja = 0; iteracja < liczbaPowtorzen; iteracja++)
            {
                var budowniczyWzorca = new System.Text.StringBuilder(5);
                for (int k = 0; k < 5; k++)
                    budowniczyWzorca.Append((char)('a' + rnd.Next(0, 26)));
                string pattern = budowniczyWzorca.ToString();

                var stoper = Stopwatch.StartNew();
                var occurrences = WyszukiwanieNapisow.ZnajdzWszystkieKMP(text, pattern, ignorujWielkoscLiter: false);
                stoper.Stop();
                lacznyCzasMs += stoper.ElapsedMilliseconds;
            }

            double sredniCzasMs = lacznyCzasMs / (double)liczbaPowtorzen;
            TestContext.WriteLine($"Średni czas ZnajdzSłowaKMP (na {liczbaPowtorzen} losowych wzorcach): {sredniCzasMs:F2} ms");
        }

    }
}
