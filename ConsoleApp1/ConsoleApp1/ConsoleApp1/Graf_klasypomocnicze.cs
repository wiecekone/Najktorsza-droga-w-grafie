using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Krawedz
    {
        public int Zrodlo, Cel;
        public long Pojemnosc;
        public long Przeplyw;
        public long Koszt;
        public int OdwrotnaIdx;

        public Krawedz(int zrodlo, int cel, long pojemnosc, long koszt, int odwrotnaIdx)
        {
            Zrodlo = zrodlo;
            Cel = cel;
            Pojemnosc = pojemnosc;
            Koszt = koszt;
            Przeplyw = 0;
            OdwrotnaIdx = odwrotnaIdx;
        }

        public long RezidualnaPojemnosc => Pojemnosc - Przeplyw;
    }

    public class SiecPrzeplywu
    {
        public int LiczbaWezlow;
        public List<Krawedz>[] Sasiedzi;

        public SiecPrzeplywu(int liczbaWezlow)
        {
            LiczbaWezlow = liczbaWezlow;
            Sasiedzi = new List<Krawedz>[LiczbaWezlow];
            for (int i = 0; i < LiczbaWezlow; i++)
                Sasiedzi[i] = new List<Krawedz>();
        }

        // Do tworzenia krawędzi odwrotnej o zerowej pojemności i koszcie ujemnym.
        public void DodajKrawedzSkierowana(int u, int v, long pojemnosc, long koszt = 0)
        {
            var a = new Krawedz(u, v, pojemnosc, koszt, Sasiedzi[v].Count);
            var b = new Krawedz(v, u, 0, -koszt, Sasiedzi[u].Count);
            Sasiedzi[u].Add(a);
            Sasiedzi[v].Add(b);
        }
    }
}
