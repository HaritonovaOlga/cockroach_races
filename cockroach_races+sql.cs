using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;

namespace CockroachRaces
{
    class Program
    {
        // Параметры забега
        private const int TrackLength = 100;      // длина дистанции в условных единицах
        private const int DelayForSecond = 3;    // второй таракан стартует на N шагов позже

        static void Main(string[] args)
        {
            // Инициализация БД и таблицы
            InitializeDatabase();

            Console.WriteLine("=== Тараканьи бега ===");
            Console.WriteLine($"Дистанция: {TrackLength} ед. | Второй таракан стартует позже на {DelayForSecond} шага.");

            var winner = RunRace();
            Console.WriteLine($"\nПобедитель: {winner}");

            // Сохраняем результат
            SaveRaceResult(winner);

            Console.WriteLine("\nРезультаты сохранены в БД.");
            Console.ReadKey();
        }

        /// <summary>
        /// Запускает забег и возвращает имя победителя
        /// </summary>
        private static string RunRace()
        {
            Random rnd = new Random();

            // Скорости: у второго таракана чуть выше средняя, чтобы мог обогнать
            int speedCockroach1 = rnd.Next(3, 8);   // от 3 до 7
            int speedCockroach2 = rnd.Next(5, 10); // от 5 до 9 (чаще быстрее)

            int position1 = 0;
            int position2 = 0;
            int step = 0;

            while (position1 < TrackLength && position2 < TrackLength)
            {
                step++;

                // Таракан 1 бежит каждый шаг
                position1 += speedCockroach1;
                if (position1 > TrackLength) position1 = TrackLength;

                // Таракан 2 стартует с задержкой
                if (step > DelayForSecond)
                {
                    position2 += speedCockroach2;
                    if (position2 > TrackLength) position2 = TrackLength;
                }

                Console.Clear();
                Console.WriteLine($"=== Шаг {step} ===");
                Console.WriteLine($"Таракан 1: [{new string('#', position1 / 2)}{new string('.', (TrackLength - position1) / 2)}] ({position1}/{TrackLength})");
                Console.WriteLine($"Таракан 2: [{new string('#', position2 / 2)}{new string('.', (TrackLength - position2) / 2)}] ({position2}/{TrackLength})");

                Thread.Sleep(150); // небольшая пауза для наглядности

                // Если кто-то добежал — завершаем
                if (position1 >= TrackLength || position2 >= TrackLength) break;
            }

            return position1 >= TrackLength ? "Таракан 1" : "Таракан 2";
        }

        /// <summary>
        /// Создаёт БД и таблицу, если их нет
        /// </summary>
        private static void InitializeDatabase()
        {
            string dbPath = "races.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string createTable = @"
                CREATE TABLE IF NOT EXISTS Races (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Winner TEXT NOT NULL,
                    RaceDate TEXT NOT NULL,
                    TimeTaken INTEGER NOT NULL
                );";

            using var cmd = new SQLiteCommand(createTable, conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Сохраняет результат забега
        /// </summary>
        private static void SaveRaceResult(string winner)
        {
            string dbPath = "races.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string insert = @"
                INSERT INTO Races (Winner, RaceDate, TimeTaken)
                VALUES (@winner, @date, @time);";

            using var cmd = new SQLiteCommand(insert, conn);
            cmd.Parameters.AddWithValue("@winner", winner);
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@time", 0); // можно добавить реальное время забега при желании

            cmd.ExecuteNonQuery();
        }
    }
}
