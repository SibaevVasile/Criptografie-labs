/*
 * Lucrare de laborator nr. 3
 * Cifrul Playfair pentru mesaje in limba romana (alfabet de 30 de litere,
 * fara litera Q).
 *
 * Alfabet (30): A Ă Â B C D E F G H I Î J K L M N O P R S Ș T Ț U V W X Y Z
 *
 * Matrice: 6 x 5 = 30 celule = cele 30 de litere ale alfabetului (fara Q),
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace LaboratorCriptografie3
{
    internal static class Playfair
    {
        private const int Rows = 6;
        private const int Cols = 5;
        private const int NSym = Rows * Cols; 
        private const int NLet = NSym;        
        private const int MinKey = 7;

        private static readonly char[] Sym =
        {
            'A', 'Ă', 'Â', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'Î', 'J', 'K',
            'L', 'M', 'N', 'O', 'P', 'R', 'S', 'Ș', 'T', 'Ț', 'U', 'V', 'W',
            'X', 'Y', 'Z'
        };

        private static readonly Dictionary<char, char> Alt = new Dictionary<char, char>
        {
            ['a'] = 'A', ['ă'] = 'Ă', ['â'] = 'Â', ['b'] = 'B', ['c'] = 'C',
            ['d'] = 'D', ['e'] = 'E', ['f'] = 'F', ['g'] = 'G', ['h'] = 'H',
            ['i'] = 'I', ['î'] = 'Î', ['j'] = 'J', ['k'] = 'K', ['l'] = 'L',
            ['m'] = 'M', ['n'] = 'N', ['o'] = 'O', ['p'] = 'P',
            ['r'] = 'R', ['s'] = 'S', ['ș'] = 'Ș', ['ş'] = 'Ș', ['Ş'] = 'Ș',
            ['t'] = 'T', ['ț'] = 'Ț', ['ţ'] = 'Ț', ['Ţ'] = 'Ț',
            ['u'] = 'U', ['v'] = 'V', ['w'] = 'W', ['x'] = 'X', ['y'] = 'Y',
            ['z'] = 'Z'
        };

        private static readonly int[,] Mat = new int[Rows, Cols];
        private static readonly int[] PosR = new int[NSym];
        private static readonly int[] PosC = new int[NSym];
        private static int _idX, _idAlt, _idZ; 

        // ------------------------------------------------------------------
        // Utilitare
        // ------------------------------------------------------------------

        /* Aduce caracterul c la forma canonica (majuscula "oficiala" din Sym),
         * folosind tabelul Alt pentru minuscule/sedila. Literele deja mari
         * (A-Z si Ă Â Î Ș Ț) si cifrele raman neschimbate. */
        private static char Canonical(char c)
        {
            return Alt.TryGetValue(c, out char up) ? up : c;
        }

        /* Cauta pozitia (0..35) a caracterului c in tabelul Sym, dupa aducerea
         * lui la forma canonica. Returneaza -1 daca nu apartine matricei. */
        private static int ParseSymbol(char c)
        {
            char canon = Canonical(c);
            for (int i = 0; i < NSym; i++)
                if (Sym[i] == canon) return i;
            return -1;
        }

        private static string ReadLinePrompt(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }

        /* Verifica un text format din litere ale alfabetului romanesc */
        private static List<int>? CollectAlpha(string s)
        {
            var result = new List<int>();
            foreach (char ch in s)
            {
                if (char.IsWhiteSpace(ch)) continue;

                int id = ParseSymbol(ch);
                if (id < 0 || id >= NLet)
                {
                    Console.WriteLine($"  Eroare: caracterul '{ch}' nu este permis.");
                    Console.WriteLine("  Diapazon corect: litere de la 'A' la 'Z' (fara 'Q'), de la " +
                                       "'a' la 'z' (fara 'q') si literele românești Ă Â Î Ș Ț (ă â î ș ț).");
                    return null;
                }
                result.Add(id);
            }
            return result;
        }

        /* Afiseaza o lista de coduri sub forma de text, eventual grupat cate
         * 2 (perechi), separate prin spatiu. */
        private static void PrintSyms(List<int> syms, bool group)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < syms.Count; i++)
            {
                sb.Append(Sym[syms[i]]);
                if (group && i % 2 == 1 && i + 1 < syms.Count) sb.Append(' ');
            }
            Console.WriteLine(sb.ToString());
        }

        // ------------------------------------------------------------------
        // Matricea Playfair
        // ------------------------------------------------------------------

        private static void Place(int k, int sym)
        {
            int r = k / Cols, c = k % Cols;
            Mat[r, c] = sym;
            PosR[sym] = r;
            PosC[sym] = c;
        }

        private static void BuildMatrix(List<int> key)
        {
            var used = new bool[NSym];
            int k = 0;

            foreach (int sym in key)           
                if (!used[sym])
                {
                    used[sym] = true;
                    Place(k++, sym);
                }
            for (int s = 0; s < NSym; s++)     
                if (!used[s])
                {
                    used[s] = true;
                    Place(k++, s);
                }
        }

        private static int DistinctCount(List<int> key)
        {
            var used = new bool[NSym];
            int d = 0;
            foreach (int sym in key)
                if (!used[sym])
                {
                    used[sym] = true;
                    d++;
                }
            return d;
        }

        private static void PrintMatrix()
        {
            Console.WriteLine();
            Console.WriteLine($"Matricea de criptare ({Rows}x{Cols}):");
            for (int i = 0; i < Rows; i++)
            {
                Console.Write("  ");
                for (int j = 0; j < Cols; j++)
                    Console.Write(Sym[Mat[i, j]] + " ");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // Criptare / decriptare
        // ------------------------------------------------------------------

        /* dir = +1 la criptare, dir = -1 la decriptare. */
        private static void Transform(int a, int b, int dir, out int x, out int y)
        {
            int r1 = PosR[a], c1 = PosC[a];
            int r2 = PosR[b], c2 = PosC[b];

            if (r1 == r2)                      /* aceeasi linie   */
            {
                x = Mat[r1, Mod(c1 + dir, Cols)];
                y = Mat[r2, Mod(c2 + dir, Cols)];
            }
            else if (c1 == c2)                 /* aceeasi coloana */
            {
                x = Mat[Mod(r1 + dir, Rows), c1];
                y = Mat[Mod(r2 + dir, Rows), c2];
            }
            else                                /* dreptunghi      */
            {
                x = Mat[r1, c2];
                y = Mat[r2, c1];
            }
        }

        /* Modulo "matematic" (intotdeauna >= 0), util cand dir = -1. */
        private static int Mod(int a, int n) => ((a % n) + n) % n;

        /* Imparte textul in perechi, inserand o litera de completare acolo
         * unde doua litere identice ar forma o pereche, sau la finalul unui
         * text cu numar impar de litere. */
        private static List<int> Prepare(List<int> t)
        {
            var p = new List<int>();
            int i = 0, n = t.Count;

            while (i < n)
            {
                int a = t[i];
                if (i + 1 < n)
                {
                    int b = t[i + 1];
                    if (a == b)
                    {
                        p.Add(a);
                        p.Add(a == _idX ? _idAlt : _idX);
                        i += 1;
                    }
                    else
                    {
                        p.Add(a);
                        p.Add(b);
                        i += 2;
                    }
                }
                else
                {
                    p.Add(a);
                    p.Add(a == _idX ? _idZ : _idX);
                    i += 1;
                }
            }
            return p;
        }

        // ------------------------------------------------------------------

        private static void Main()
        {
            // Pentru afisarea corecta a diacriticelor in consola.
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding = new UTF8Encoding(false);

            _idX = ParseSymbol('X');
            _idAlt = ParseSymbol('W');
            _idZ = ParseSymbol('Z');

            while (true)
            {
                Console.WriteLine("=== Cifrul Playfair (alfabet romanesc, 30 litere, fara Q) ===");
                Console.WriteLine("  1 - Criptare");
                Console.WriteLine("  2 - Decriptare");
                Console.WriteLine("  0 - Iesire");

                string opLine = ReadLinePrompt("Alegeti operatia: ");
                if (opLine == null) break;                 

                if (!int.TryParse(opLine.Trim(), out int op))
                {
                    Console.WriteLine("  Optiune invalida. Introduceti 0, 1 sau 2.\n");
                    continue;
                }
                if (op == 0) break;
                if (op != 1 && op != 2)
                {
                    Console.WriteLine("  Optiune invalida. Introduceti 0, 1 sau 2.\n");
                    continue;
                }
                /* --- cheia --- */
                List<int>? key;
                while (true)
                {
                    string keyLine = ReadLinePrompt("Cheia (minim 7 litere distincte): ");
                    if (keyLine == null) return;
                    key = CollectAlpha(keyLine);
                    if (key == null) continue;
                    if (key.Count < MinKey)
                    {
                        Console.WriteLine($"  Eroare: cheia are {key.Count} litere; minim {MinKey}.");
                        continue;
                    }
                    int d = DistinctCount(key);
                    if (d < MinKey)
                    {
                        Console.WriteLine($"  Eroare: cheia are {d} litere distincte; dupa " +
                                           $"eliminarea literelor repetate trebuie sa ramana minim {MinKey}.");
                        continue;
                    }
                    break;
                }
                BuildMatrix(key);
                PrintMatrix();

                if (op == 1)
                {
                    /* --- CRIPTARE --- */
                    string msgLine = ReadLinePrompt("Mesajul: ");
                    if (msgLine == null) return;
                    List<int>? n = CollectAlpha(msgLine);
                    if (n == null) continue;
                    if (n.Count == 0)
                    {
                        Console.WriteLine("  Eroare: mesajul este gol.\n");
                        continue;
                    }

                    List<int> prep = Prepare(n);
                    Console.Write("\nText pregatit (perechi): ");
                    PrintSyms(prep, true);

                    var outp = new List<int>(new int[prep.Count]);
                    for (int i = 0; i < prep.Count; i += 2)
                    {
                        Transform(prep[i], prep[i + 1], +1, out int x, out int y);
                        outp[i] = x;
                        outp[i + 1] = y;
                    }

                    Console.Write("Criptograma (perechi)  : ");
                    PrintSyms(outp, true);
                    Console.Write("Criptograma            : ");
                    PrintSyms(outp, false);
                }
                else
                {
                    /* --- DECRIPTARE --- */
                    string cipherLine = ReadLinePrompt("Criptograma: ");
                    if (cipherLine == null) return;
                    List<int>? n = CollectAlpha(cipherLine);
                    if (n == null) continue;
                    if (n.Count == 0)
                    {
                        Console.WriteLine("  Eroare: criptograma este goala.\n");
                        continue;
                    }
                    if (n.Count % 2 != 0)
                    {
                        Console.WriteLine($"  Eroare: criptograma trebuie sa aiba un numar par de " +
                                           $"simboluri (are {n.Count}).\n");
                        continue;
                    }

                    bool bad = false;
                    for (int i = 0; i < n.Count; i += 2)
                        if (n[i] == n[i + 1])
                        {
                            Console.WriteLine($"  Eroare: perechea {i / 2 + 1} contine doua simboluri " +
                                               $"identice ({Sym[n[i]]}{Sym[n[i]]}) - nu poate proveni din Playfair.\n");
                            bad = true;
                            break;
                        }
                    if (bad) continue;

                    var outp = new List<int>(new int[n.Count]);
                    for (int i = 0; i < n.Count; i += 2)
                    {
                        Transform(n[i], n[i + 1], -1, out int x, out int y);
                        outp[i] = x;
                        outp[i + 1] = y;
                    }

                    Console.Write("\nText decriptat (perechi): ");
                    PrintSyms(outp, true);
                    Console.Write("Text decriptat          : ");
                    PrintSyms(outp, false);
                    Console.WriteLine("(Eliminati manual literele de completare X/W/Z si adaugati spatiile.)");
                }
                Console.WriteLine();
            }

            Console.WriteLine("La revedere!");
        }
    }
}
