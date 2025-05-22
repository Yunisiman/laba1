using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Text.Json;

namespace WPF_SymPy
{
    public partial class MainWindow : Window
    {
        private string pythonInterpreterPath, pythonScriptPath, jsonFilePath;

        public MainWindow()
        {
            InitializeDirectories();
            InitializeComponent(); // Убедитесь, что этот метод вызывается после InitializeDirectories
        }

        private void InitializeDirectories()
        {
            try
            {
                // Путь к папке с Python скриптом
                string pythonProjectName = @"C:\Users\\source\repos\WPF_SymPy\WPF_SymPy\PythonApplication";
                string pythonVenvName = ".venv"; // Имя виртуального окружения

                // Имя Python скрипта
                pythonScriptPath = Path.Combine(pythonProjectName, "parser.py");

                // Указываем путь к виртуальному окружению
                string venvPath = Path.Combine(pythonProjectName, pythonVenvName);
                // Указываем путь к интерпретатору Python
                pythonInterpreterPath = Path.Combine(venvPath, "Scripts", "python.exe");

                // Путь к файлу JSON
                jsonFilePath = @"C:\Users\\source\repos\WPF_SymPy\WPF_SymPy\PythonApplication\currencies.json";

                // Проверка существования файлов
                if (!File.Exists(pythonScriptPath))
                {
                    MessageBox.Show($"Python script wasn't found: {pythonScriptPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (!File.Exists(pythonInterpreterPath))
                {
                    MessageBox.Show($"Interpreter Python wasn't found: {pythonInterpreterPath}\n" +
                                    $"Make sure the virt env '{pythonVenvName}' was made.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while initializing path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Запуск Python-скрипта
                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonInterpreterPath,
                    Arguments = pythonScriptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    // Проверка на ошибки
                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        MessageBox.Show($"Error with script:\n{error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Чтение данных из JSON файла
                var currencies = LoadCurrenciesFromJson();
                Dispatcher.Invoke(() => CurrencyList.ItemsSource = currencies);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Currency> LoadCurrenciesFromJson()
        {
            try
            {
                string json = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<List<Currency>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while loading currencies from JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Currency>(); // Возвращаем пустой список в случае ошибки
            }
        }
    }

    // Класс для представления валюты
    public class Currency
    {
        public string Code { get; set; }
        public string CodeWithLetters { get; set; } // Переименовано для соответствия стилю C#
        public string Denomination { get; set; }
        public string Name { get; set; }
        public string Rate { get; set; }
    }
}
