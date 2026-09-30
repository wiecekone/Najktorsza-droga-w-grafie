using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MinKosztMaxPrzeplyw
    {
        private readonly SiecPrzeplywu _siec;
        private const long NIESKONCZONOSC = long.MaxValue / 4;

        public MinKosztMaxPrzeplyw(SiecPrzeplywu siec)
        {
            _siec = siec;
        }

        private bool BellmanFord(int zrodlo, int ujscie, long[] dystans, int[] wierzcholekRodzic, int[] krawedzRodzic)
        {
            int n = _siec.LiczbaWezlow;

            for (int i = 0; i < n; i++)
            {
                dystans[i] = NIESKONCZONOSC;
                wierzcholekRodzic[i] = -1;
                krawedzRodzic[i] = -1;
            }
            dystans[zrodlo] = 0;

            for (int i = 0; i < n - 1; i++)
            {
                bool zmiana = false;

                for (int u = 0; u < n; u++)
                {
                    if (dystans[u] == NIESKONCZONOSC) continue;

                    for (int j = 0; j < _siec.Sasiedzi[u].Count; j++)
                    {
                        var kraw = _siec.Sasiedzi[u][j];
                        if (kraw.RezidualnaPojemnosc > 0 && dystans[u] + kraw.Koszt < dystans[kraw.Cel])
                        {
                            dystans[kraw.Cel] = dystans[u] + kraw.Koszt;
                            wierzcholekRodzic[kraw.Cel] = u;
                            krawedzRodzic[kraw.Cel] = j;
                            zmiana = true;
                        }
                    }
                }

                if (!zmiana) break;
            }

            for (int u = 0; u < n; u++)
            {
                if (dystans[u] == NIESKONCZONOSC) continue;

                for (int i = 0; i < _siec.Sasiedzi[u].Count; i++)
                {
                    var kraw = _siec.Sasiedzi[u][i];
                    if (kraw.RezidualnaPojemnosc > 0 && dystans[u] + kraw.Koszt < dystans[kraw.Cel])
                    {
                        throw new InvalidOperationException(
                            $"Wykryto ujemny cykl w grafie rezydualnym między węzłami {u} i {kraw.Cel}. " +
                            "Graf może mieć nieograniczony koszt minimalny.");
                    }
                }
            }

            return wierzcholekRodzic[ujscie] != -1;
        }

        public (long Przeplyw, long Koszt) PobierzMaxPrzeplywMinKoszt(int zrodlo, int ujscie)
        {
            int n = _siec.LiczbaWezlow;
            long przeplyw = 0, koszt = 0;

            long[] dystans = new long[n];
            int[] wierzcholekRodzic = new int[n];
            int[] krawedzRodzic = new int[n];

            while (true)
            {
                if (!BellmanFord(zrodlo, ujscie, dystans, wierzcholekRodzic, krawedzRodzic))
                    break;

                long push = NIESKONCZONOSC;
                int v = ujscie;
                while (v != zrodlo)
                {
                    int u = wierzcholekRodzic[v];
                    int idx = krawedzRodzic[v];
                    var kraw = _siec.Sasiedzi[u][idx];
                    push = Math.Min(push, kraw.RezidualnaPojemnosc);
                    v = u;
                }

                if (push == 0 || push == NIESKONCZONOSC)
                    break;

                v = ujscie;
                while (v != zrodlo)
                {
                    int u = wierzcholekRodzic[v];
                    int idx = krawedzRodzic[v];
                    var kraw = _siec.Sasiedzi[u][idx];
                    kraw.Przeplyw += push;
                    _siec.Sasiedzi[v][kraw.OdwrotnaIdx].Przeplyw -= push;
                    koszt += push * kraw.Koszt;
                    v = u;
                }

                przeplyw += push;
            }

            return (przeplyw, koszt);
        }
        
        // funkcja w celu przeszukiwania binarnego w mainwindow.xaml.cs dla budżetu
        public (long Przeplyw, long MinimalnyKoszt) PobierzMinKosztDokladnegoPrzeplywu(int zrodlo, int ujscie, long wymaganyPrzeplyw)
        {
            int n = _siec.LiczbaWezlow;
            long przeplyw = 0, koszt = 0;
            long[] dystans = new long[n];
            int[] wierzcholekRodzic = new int[n];
            int[] krawedzRodzic = new int[n];

            while (przeplyw < wymaganyPrzeplyw)
            {
                if (!BellmanFord(zrodlo, ujscie, dystans, wierzcholekRodzic, krawedzRodzic))
                    break;

                long push = wymaganyPrzeplyw - przeplyw;
                int v = ujscie;
                while (v != zrodlo)
                {
                    int u = wierzcholekRodzic[v];
                    int idx = krawedzRodzic[v];
                    var kraw = _siec.Sasiedzi[u][idx];
                    push = Math.Min(push, kraw.RezidualnaPojemnosc);
                    v = u;
                }

                if (push == 0)
                    break;

                v = ujscie;
                while (v != zrodlo)
                {
                    int u = wierzcholekRodzic[v];
                    int idx = krawedzRodzic[v];
                    var kraw = _siec.Sasiedzi[u][idx];
                    kraw.Przeplyw += push;
                    _siec.Sasiedzi[v][kraw.OdwrotnaIdx].Przeplyw -= push;
                    koszt += push * kraw.Koszt;
                    v = u;
                }
                przeplyw += push;
            }

            return (przeplyw, koszt);
        }

        public void ResetujWszystkiePrzeplywy()
        {
            for (int u = 0; u < _siec.LiczbaWezlow; u++)
            {
                foreach (var kraw in _siec.Sasiedzi[u])
                {
                    kraw.Przeplyw = 0;
                }
            }
        }
    }
}