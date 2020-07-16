using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// Program klasycznego algorytmy genetycznego v.1.0
// ostatnia aktualizacja 2018-08-25
// CopyLeft Feliks Kurp 2018
// Praca nad projektem - Damian Narodzonek
namespace AlgorytmGenetyczny
{
    struct Parametry //parametry symulacji
    { 
        public const int lp = 80;      //liczba pokoleń w eksperymencie
        public const double a = 0.2;    //wartość początkowa przestrzeni poszukiwań
        public const double b = 1.2;    //wartość końcowa przestrzeni poszukiwań 
        public const int N = 10;        //liczba genów w pojedynczym chromosomie
        public const int pula = 60;     //liczba osobników  w populacji (liczba parzysta)
        public const double pk = 0.7;     //prawdopodobieństwo krzyżowania
        public const double pm = 0.01;  //prawdopodobieństwo mutacji
    }

    class LiczbyBazowe
    {
        static int[] tablicaBazowa = new int[100 * Parametry.lp];
        public static int indexBazowy = 0;
        //tablica i indeks bazowych liczb losowych dla generatora Random

        public void LosujBaze()
        {
            Random Generator = new Random();
            for (int i = 0; i < 100 * Parametry.lp; i++)
                tablicaBazowa[i] = Generator.Next(256);
        }
        public static int PobierzBazowa()
        {
            return tablicaBazowa[indexBazowy++];
        }
        public static int Power()
        {   // liczy n-tą nieujemną potegę dwójki
            int power = 1;
            for (int i = 1; i <= Parametry.N; i++)
                power = power * 2;
            return power;
        }

    }

    class Populacja
    {
        Byte[,] populacja;
        //dwuwymiarowa tablica aktualnych chromosomów populacji,
        //rozmieszczonych w kolejnych wierszach tablicy

        public Populacja()
        //konstruktor populacji - przydziela pamięć dla bitowej
        //reprezentacji populacji chromosomów
        { populacja = new Byte[Parametry.pula, Parametry.N]; }

        double[] tablicaFenotypow = new double[Parametry.pula];
        //tablica wartości fenotypów dla populacji chromosomów

        int power = LiczbyBazowe.Power(); // liczba 2 do potęgi liczby genów

        double[] tablicaDostosowanie = new double[Parametry.pula];
        //tablica wartości funkcji dostosowania dla populacji chromosomw

        public void LosujPopulacje()
        {   //metoda losuje nową populację chromosomów
            //i umieszcza je w lokalnej tablicy 'populacja'
            for (int pozycja = 0; pozycja < Parametry.pula; pozycja++)
            {
                Random Generator = new Random(LiczbyBazowe.PobierzBazowa());
                for (int j = 0; j < Parametry.N; j++)
                    populacja[pozycja, j] = (Byte)Generator.Next(2);
            }
        }

        int ObliczFenotypChromosomu(int pozycjaChromosomu)
        {   //metoda liczy reprezentację dziesiętną wskazanego chromosomu
            int fenotyp = 0, rat = 1;
            for (int j = 0; j < Parametry.N; j++)
            { fenotyp = fenotyp + populacja[pozycjaChromosomu, j] * rat; rat = rat * 2; }
            return fenotyp;
        }

        public void ObliczFenotypy()
        {   //metoda liczy wartości fenotypów chromosomów populacji
            //w liniowej przestrzeni poszukiwań <a,b>
            //i umieszcza je w tablicy 'tablicaFenotypów'
            for (int pozycja = 0; pozycja < Parametry.pula; pozycja++)
                tablicaFenotypow[pozycja] = Parametry.a + (Parametry.b - Parametry.a)
                * ObliczFenotypChromosomu(pozycja) / power;
        }

        public void ObliczDostosowanie()
        {   //metoda oblicza wartości funkcji dostosowania,
            //umieszcza je w tablicy 'tablicaDostosowanie'
            //a następnie normalizuje
            double x;
            for (int i = 0; i < Parametry.pula; i++)
            {
                x = tablicaFenotypow[i];
                tablicaDostosowanie[i] = 0.9f - Math.Abs(Math.Pow(x-1f, 2) - Math.Pow(x-0.5f, 3));
                
            }
        }

        public void Ruletka()
        {   //selekcja chromosomów w populacji metodą koła ruletki

            Byte[,] nowePokolenie = new Byte[Parametry.pula, Parametry.N];
            //tablica pomocnicza chromosomów dla ruletki

            double[] tablicaNI = new double[Parametry.pula];
            //tablica pomocnicza ruletki

            double sumaDostosowanie = 0;
            foreach (double dostosowanie in tablicaDostosowanie)
                sumaDostosowanie += dostosowanie;

            for (int i = 0; i < Parametry.pula; i++)
            {
                tablicaNI[i] = tablicaDostosowanie[i]
                               / sumaDostosowanie * power;
            }

            int[] losowe = new int[Parametry.pula];
            //tabela 'losowe' przechowuje liczby losowe z przedziału 0...Power()
            Random Generator = new Random(LiczbyBazowe.PobierzBazowa());
            for (int i = 0; i < Parametry.pula; i++)
                losowe[i] = Generator.Next(power);

            double[] ruletka = new double[Parametry.pula];
            //tablica pozycji wycinków ruletki

            double pozycja = 0;
            for (int i = 0; i < Parametry.pula; i++)
            {
                pozycja += tablicaNI[i];
                ruletka[i] = pozycja;
            }

            for (int i = 0; i < Parametry.pula; i++)
            {
                int j = 0;
                while (losowe[i] > ruletka[j]) j++;
                for (int k = 0; k < Parametry.N; k++)
                    nowePokolenie[i, k] = populacja[j, k];
            }

            populacja = nowePokolenie;
        }



        //Selekcji chromosomów metodą turnieju
        public void Turniej(int liczOs = 2)
        {
            Byte[,] nowaPopulacja = new Byte[Parametry.pula, Parametry.N];
            // Tablica, w której umieszczone zostaną wybrane osobniki

            double[] grupaA = new double[liczOs];
            double[] grupaB = new double[liczOs];
            //Grupy z których wybierany jest najlepszy osobnik do puli populacji

            int iPop = 0;
            for (int w = 0; w < Parametry.pula; w++)
            {
                //dodawanie osobników do grupy A
                for (int a = 0; a < grupaA.Length; a++)
                {
                    grupaA[a] = tablicaDostosowanie[iPop];
                    iPop++;
                    if (iPop == Parametry.pula) iPop = 0;
                }

                // dodawanie osobników do grupy B
                for (int b = 0; b < grupaB.Length; b++)
                {
                    grupaB[b] = tablicaDostosowanie[iPop];
                    iPop++;
                    if (iPop == Parametry.pula) iPop = 0;
                }

                // wybór najlepszego osobnika z grupyA
                double maxA = grupaA[0];
                foreach (double a in grupaA)
                {
                    if (maxA < a) maxA = a;
                }

                //wybór najlepszego osobnika z grupyB
                double maxB = grupaB[0];
                foreach (double b in grupaB)
                {
                    if (maxB < b) maxB = b;
                }

                //pozycja (index) wybranych osobników z populacji 
                int indexOfA = Array.IndexOf(tablicaDostosowanie, maxA);
                int indexOfB = Array.IndexOf(tablicaDostosowanie, maxB);
                
                // dodanie wybranych osobników do nowej populacji
                for(int x = 0; x < Parametry.N; x++)
                {
                    nowaPopulacja[w, x] = populacja[indexOfA, x];
                }
                for (int x = 0; x < Parametry.N; x++)
                {
                    nowaPopulacja[w, x] = populacja[indexOfB, x];
                }
            }
            populacja = nowaPopulacja;
        }

        void Krzyzowanie()
        {
            Random Generator = new Random(LiczbyBazowe.PobierzBazowa());
            //tworzy generator liczb losowych oparty o kolejną
            //liczbę bazową

            //losowanie par osobników do krzyżowania
            int liczbaPar = Parametry.pula / 2;
            int[] losowePary = new int[liczbaPar];
            for (int i = 0; i < liczbaPar; i++)
                losowePary[i] = Generator.Next(100);

            //losowanie miejsc krzyżowania dla par
            int[] losoweMiejsca = new int[liczbaPar];
            for (int i = 0; i < liczbaPar; i++)
                losoweMiejsca[i] = Generator.Next(Parametry.N - 2);

            //proces krzyżowania genów w parach
            int pierwszy = 0; //indeks pierwszego osobnika w każdej parze
            byte bufor;
            for (int para = 0; para < liczbaPar; para++)
            {
                if (losowePary[para] < Parametry.pk * 100)
                    for (int i = losoweMiejsca[para]; i < Parametry.N; i++)
                    {
                        bufor = populacja[pierwszy, i];
                        populacja[pierwszy, i] = populacja[pierwszy + 1, i];
                        populacja[pierwszy + 1, i] = bufor;
                    }
                pierwszy += 2;
            }
        }


        void KrzyzowanieNieLosowe()
        {
            Random Generator = new Random(LiczbyBazowe.PobierzBazowa());
            //tworzy generator liczb losowych oparty o kolejną
            //liczbę bazową

            //losowanie par osobników do krzyżowania
            int liczbaPar = Parametry.pula / 2;
            int[] wybranePary = new int[liczbaPar];

            //losowanie miejsc krzyżowania dla par
            int[] losoweMiejsca = new int[liczbaPar];
            for (int i = 0; i < liczbaPar; i++)
                losoweMiejsca[i] = Generator.Next(Parametry.N - 2);

            //proces krzyżowania genów w parach
            int pierwszy = 0; //indeks pierwszego osobnika w każdej parze
            byte bufor;
            for (int para = 0; para < liczbaPar; para++)
            {
                if (wybranePary[para] < Parametry.pk * 100)
                    for (int i = losoweMiejsca[para]; i < Parametry.N; i++)
                    {
                        bufor = populacja[pierwszy, i];
                        populacja[pierwszy, i] = populacja[pierwszy + 1, i];
                        populacja[pierwszy + 1, i] = bufor;
                    }
                pierwszy += 2;
            }
        }

        public void PokazFenotypyPopulacji()
        {   //wyświetla fenotypy aktualnej populacji
            foreach (double fenotyp in tablicaFenotypow)
                Console.Write("{0:#.###}\n ", fenotyp);
        }

        public void Mutacje()
        {  //metoda losuje chromosomy do mutacji 
           //i mutuje losowe geny w wylosowanych chromosomach

            Random Generator = new Random(LiczbyBazowe.PobierzBazowa());
            //tworzy generator liczb losowych oparty o kolejną
            //liczbę bazową

            double[] losowe = new double[Parametry.pula];
            for (int i = 0; i < Parametry.pula; i++)
                losowe[i] = Generator.Next(100) / 100.0;

            //proces krzyżowania genów w parach
            int miejsceMutacji;
            for (int i = 0; i < Parametry.pula; i++)
                if (losowe[i] < Parametry.pm)
                {
                    miejsceMutacji = Generator.Next(Parametry.N);
                    if (populacja[i, miejsceMutacji] == 0)
                        populacja[i, miejsceMutacji] = 1;
                    else populacja[i, miejsceMutacji] = 0;
                }
        }

        public void PokazDostosowaniePopulacji()
        {   //wyświetla wartości funkcji dostosowania 
            //chromosomów aktualnej populacji
            foreach (double dostosowanie in tablicaDostosowanie)
                Console.Write("{0:#.###}\n ", dostosowanie);
        }

        void PokazChromosomyPopulacji()
        {
            //wyświetla wszystkie chromosomy populacji
            for (int i = 0; i < populacja.GetLength(0); i++)
            {
                for (int j = 0; j < populacja.GetLength(1); j++)
                {
                    Console.Write("{0} ", populacja[i, j]);
                }
                Console.WriteLine();
            }
        }

        public double ObliczDostosowanieSrednie()
        {
            double srednia = 0;
            foreach (double dostosowanie in tablicaDostosowanie)
                srednia += dostosowanie;
            return srednia / Parametry.pula;
        }

        public void PokazDostosowanieSrednie()
        {   //wyświetla wartość średnią funkcji dostosowania 
            //wszystkich chromosomów aktualnej populacji 
            Console.WriteLine("{0:#.###}", ObliczDostosowanieSrednie());
        }

        class Program
        {
            static void Main(string[] args)
            {
                int nrPokolenia = 0;
                LiczbyBazowe liczbyBazowe = new LiczbyBazowe();
                liczbyBazowe.LosujBaze();

                Populacja populacja = new Populacja();
                populacja.LosujPopulacje();
                //wylosowanie populacji rodzicielskiej

                populacja.ObliczFenotypy();
                populacja.ObliczDostosowanie();

                Console.WriteLine("Nr pokolenia Srednia wartosc funkcji dostosowania");
                Console.Write("{0, 3}          ", nrPokolenia);
                populacja.PokazDostosowanieSrednie();
                while (nrPokolenia < Parametry.lp)
                {
                    nrPokolenia++;
                    populacja.Turniej(3);
                    populacja.KrzyzowanieNieLosowe();
                    populacja.Mutacje();
                    populacja.ObliczFenotypy();
                    populacja.ObliczDostosowanie();
                    Console.Write("{0, 3}          ", nrPokolenia);
                    populacja.PokazDostosowanieSrednie();
                }
                populacja.PokazChromosomyPopulacji();
                populacja.PokazFenotypyPopulacji();
                populacja.PokazDostosowaniePopulacji();

                Console.ReadKey();
            }
        }
    }
}
