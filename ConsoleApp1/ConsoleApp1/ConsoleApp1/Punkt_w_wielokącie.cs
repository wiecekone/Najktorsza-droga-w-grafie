using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class Geometria
    {
        public static bool CzyPunktWPolygonie_RegulaParzystosci(Punkt p, List<Punkt> wielokat)
        {
            if (wielokat.Count < 3) return false;

            double minX = wielokat.Min(punkt => punkt.X);
            double maxX = wielokat.Max(punkt => punkt.X);

            if (p.X < minX || p.X > maxX)
                return false;

            int przeciecia = 0;
            int n = wielokat.Count;

            for (int i = 0; i < n; i++)
            {
                Punkt a = wielokat[i];
                Punkt b = wielokat[(i + 1) % n];

                if (CzyPrzecinaPoziomaPolprosta(p, a, b))
                    przeciecia++;
            }

            return przeciecia % 2 == 1;
        }

        private static bool CzyPrzecinaPoziomaPolprosta(Punkt p, Punkt a, Punkt b)
        {
            if ((a.Y > p.Y) == (b.Y > p.Y))
                return false;

            double xPrzeciecia = a.X + (p.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y);

            return xPrzeciecia > p.X;
        }
    }
}
