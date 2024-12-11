using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Npgsql;
using TaskManagerTecnomer.Models;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;


//await connection.OpenAsync();

//SqlCommand command = new SqlCommand(sqlExpression, connection);
//int number = await command.ExecuteNonQueryAsync();

namespace TaskManager
{
    class Program
    {
        private static string connectionString = "Host = localhost; Port = 5432; Username = postgres; Password = s5!sz52x; Database = TaskManager";
        private static Timer _timer;
        private static readonly object logLock = new object();

        static async Task Main(string[] args)
        {
            // Настройка таймера на 5 минут (300000 миллисекунд)
            _timer = new Timer(300000);
            _timer.Elapsed += async (sender, e) => await CheckCompletedTasks(); ;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            var tasks = new List<Task>();

            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Добавить задачу");
                Console.WriteLine("2. Просмотреть все задачи");
                Console.WriteLine("3. Обновить статус задачи");
                Console.WriteLine("4. Удалить задачу");
                Console.WriteLine("5. Выход");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        tasks.Add(AddTask());
                        break;
                    case "2":
                        await ViewTasks();
                        break;
                    case "3":
                        await UpdateTaskStatus();
                        break;
                    case "4":
                        await DeleteTask();
                        break;
                    case "5":
                        await Task.WhenAll(tasks); // Дождаться завершения всех задач
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        private static async Task AddTask()
        {
            Console.Write("Введите название задачи: ");
            var name = Console.ReadLine();
            await ExecuteNonQuery($"INSERT INTO tasks (name, status) VALUES ('{name}', 'Не выполнена')");
            Log($"Добавлена задача: {name}");
            Console.WriteLine("Задача добавлена.");
        }

        private static async Task ViewTasks()
        {
            var tasks = await ExecuteQuery("SELECT * FROM tasks");
            Console.WriteLine("Список задач:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"ID: {task.Id}, Название: {task.Title}, Статус: {task.Status},  Дата создания: {task.datecreation}");
            }
        }

        private static async Task UpdateTaskStatus()
        {
            Console.Write("Введите ID задачи для обновления статуса: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Введите новый статус (Выполнена(1)/Не выполнена(0)): ");
                var status = Console.ReadLine();
                await ExecuteNonQuery($"UPDATE tasks SET status = '{status}' WHERE id = {id}");
                Log($"Обновлен статус задачи ID {id} на {status}");
                Console.WriteLine("Статус задачи обновлен.");
            }
            else
            {
                Console.WriteLine("Неверный ID.");
            }
        }

        private static async Task DeleteTask()
        {
            Console.Write("Введите ID задачи для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                await ExecuteNonQuery($"DELETE FROM tasks WHERE id = {id}");
                Log($"Удалена задача ID {id}");
                Console.WriteLine("Задача удалена.");
            }
            else
            {
                Console.WriteLine("Неверный ID.");
            }
        }

        private static async Task CheckCompletedTasks(object sender, ElapsedEventArgs e)
        {
            var completedTasks = await ExecuteQuery("SELECT * FROM tasks WHERE status = '1'");
            if (completedTasks.Count > 0)
            {
                Console.WriteLine("Найдены выполненные задачи:");
                foreach (var task in completedTasks)
                {
                    Console.WriteLine($"ID: {task.Id}, Название: {task.Title}");
                }

                foreach (var task in completedTasks)
                {
                    await ExecuteNonQuery($"DELETE FROM tasks WHERE id = {task.Id}");
                    Log($"Удалена выполненная задача ID {task.Id}");
                }
            }
        }

        private static async Task<List<TaskModels>> ExecuteQuery(string query)
        {
            var tasks = new List<TaskModels>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tasks.Add(new TaskModels
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            Status = reader.GetInt32(3),
                            datecreation = reader.GetDateTime(4)
                        });
                    }
                }
            }
            return tasks;
        }

        private static async Task ExecuteNonQuery(string query)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                   await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static void Log(string message)
        {
            lock (logLock) // Обеспечиваем потокобезопасность при записи в лог
                using (var writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}

 

