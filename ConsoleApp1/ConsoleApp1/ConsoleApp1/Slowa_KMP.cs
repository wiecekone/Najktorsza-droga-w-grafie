using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class WyszukiwanieNapisow
    {

        public static int[] BudujFunkcjePrefiksu(string wzorzec)
        {
            int m = wzorzec.Length;
            int[] pi = new int[m];
            int k = 0;
            pi[0] = 0;

            for (int i = 1; i < m; i++)
            {
                while (k > 0 && wzorzec[k] != wzorzec[i])
                {
                    k = pi[k - 1];
                }
                if (wzorzec[k] == wzorzec[i])
                {
                    k++;
                }
                pi[i] = k;
            }
            return pi;
        }

        public static List<int> ZnajdzWszystkieKMP(string tekst, string wzorzec, bool ignorujWielkoscLiter = false)
        {
            var wystapienia = new List<int>();
            if (string.IsNullOrEmpty(wzorzec) || string.IsNullOrEmpty(tekst) || tekst.Length < wzorzec.Length)
                return wystapienia;

            if (ignorujWielkoscLiter)
            {
                tekst = tekst.ToLowerInvariant();
                wzorzec = wzorzec.ToLowerInvariant();
            }

            int n = tekst.Length;
            int m = wzorzec.Length;
            int[] pi = BudujFunkcjePrefiksu(wzorzec);
            int q = 0;

            for (int i = 0; i < n; i++)
            {
                while (q > 0 && wzorzec[q] != tekst[i])
                    q = pi[q - 1];
                if (wzorzec[q] == tekst[i])                    
                    q++;
                if (q == m)
                {
                    wystapienia.Add(i - m + 1);
                    q = pi[q - 1];
                }
            }
            return wystapienia;
        }
    }
}
