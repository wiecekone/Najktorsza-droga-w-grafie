using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public struct Punkt
    {
        public double X, Y;
        public Punkt(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    // Wypukły wielokąt + wartość wydajności jęczmienia na pole
    public class Region
    {
        public List<Punkt> WierzcholkiPoligonu { get; }

        public double PlonNaPole { get; }

        public Region(List<Punkt> wierzcholki, double plonNaPole)
        {
            WierzcholkiPoligonu = wierzcholki;
            PlonNaPole = plonNaPole;
        }
    }

    public class Pole
    {
        public Punkt Lokalizacja { get; }

        public double AktualnyPlon { get; set; }

        public Pole(Punkt lokalizacja)
        {
            Lokalizacja = lokalizacja;
            AktualnyPlon = 0.0;
        }
    }

    public class Browar
    {
        public Punkt Lokalizacja { get; }
        public double PojemnoscJeczmienia { get; }

        public Browar(Punkt lokalizacja, double pojemnoscJeczmienia)
        {
            Lokalizacja = lokalizacja;
            PojemnoscJeczmienia = pojemnoscJeczmienia;
        }
    }

    public class Karczma
    {
        public Punkt Lokalizacja { get; }

        public Karczma(Punkt lokalizacja)
        {
            Lokalizacja = lokalizacja;
        }
    }

    public class Droga
    {
        public Punkt Z { get; }
        public Punkt Do { get; }
        public double PojemnoscJeczmienia { get; } // maksymalna w tonach pojemność dla jęczmienia
        public double PojemnoscPiwa { get; } // podobnie dla piwa
        public double KosztNaprawy { get; }

        public Droga(Punkt z, Punkt doPunkt, double pojemnoscJeczmienia, double pojemnoscPiwa, double kosztNaprawy)
        {
            Z = z;
            Do = doPunkt;
            PojemnoscJeczmienia = pojemnoscJeczmienia;
            PojemnoscPiwa = pojemnoscPiwa;
            KosztNaprawy = kosztNaprawy;
        }
    }
}
