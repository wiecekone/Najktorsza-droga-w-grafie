using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class FordFulkerson
    {
        private readonly SiecPrzeplywu _siec;
        private const long NIESKONCZONOSC = long.MaxValue;

        public FordFulkerson(SiecPrzeplywu siec)
        {
            _siec = siec;
        }

        public long MaksymalnyPrzeplyw(int zrodlo, int ujscie)
        {
            long maxFlow = 0;
            bool[] odwiedzone = new bool[_siec.LiczbaWezlow];

            while (true)
            {
                Array.Fill(odwiedzone, false);
                long przeslane = DfsSciezka(zrodlo, ujscie, NIESKONCZONOSC, odwiedzone);
                if (przeslane == 0)
                    break;
                maxFlow += przeslane;
            }

            return maxFlow;
        }

        private long DfsSciezka(int u, int t, long dopuszczony, bool[] odwiedzone)
        {
            if (u == t)
                return dopuszczony;

            odwiedzone[u] = true;

            foreach (var kraw in _siec.Sasiedzi[u])
            {
                int v = kraw.Cel;
                if (!odwiedzone[v] && kraw.RezidualnaPojemnosc > 0)
                {
                    long moznaPrzeslac = Math.Min(dopuszczony, kraw.RezidualnaPojemnosc);
                    long przeslane = DfsSciezka(v, t, moznaPrzeslac, odwiedzone);

                    if (przeslane > 0)
                    {
                        kraw.Przeplyw += przeslane;
                        _siec.Sasiedzi[v][kraw.OdwrotnaIdx].Przeplyw -= przeslane;
                        return przeslane;
                    }
                }
            }

            return 0;
        }
    }
}