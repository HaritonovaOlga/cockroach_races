using System;
using System.Threading;

namespace RoachRace
{
    class Program
    {
        private static readonly Random Rnd = new Random();
        private static bool raceFinished = false;

        static void Main(string[] args)
        {
            const int trackLength = 50;
            const string roach1Symbol = "🪳";
            const string roach2Symbol = "🐜";

            Console.Clear();
            Console.WriteLine("Тараканьи бега! Старт через 3 секунды...");
            Thread.Sleep(3000);
            Console.Clear();

            // Рисуем дорожку
            for (int i = 0; i < 2; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string('-', trackLength));
            }

            // Запускаем тараканов в отдельных потоках
            var t1 = new Thread(() => RunRoach(0, roach1Symbol, 50, 150)); // чуть быстрее
            var t2 = new Thread(() => RunRoach(1, roach2Symbol, 80, 200)); // чуть медленнее

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.WriteLine("\nГонка завершена!");
            Console.ReadKey();
        }

        static void RunRoach(int row, string symbol, int minDelayMs, int maxDelayMs)
        {
            int position = 0;
            const int trackLength = 50;

            while (position < trackLength && !raceFinished)
            {
                // Если кто-то уже финишировал, останавливаем остальных
                if (raceFinished) return;

                // Рисуем таракана на новой позиции
                Console.SetCursorPosition(position, row);
                Console.Write(" "); // стираем старую позицию
                Console.SetCursorPosition(position + 1, row);
                Console.Write(symbol);

                position++;

                if (position >= trackLength)
                {
                    lock (Console.Out)
                    {
                        Console.SetCursorPosition(0, 3 + row);
                        Console.WriteLine($"Таракан {symbol} пришёл к финишу!");
                    }

                    raceFinished = true;
                    return;
                }

                // Случайная задержка: у первого таракана диапазон меньше, значит он быстрее
                int delay = Rnd.Next(minDelayMs, maxDelayMs + 1);
                Thread.Sleep(delay);
            }
        }
    }
}
