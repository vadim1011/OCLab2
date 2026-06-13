using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using OCLab2.Helpers;

namespace OCLab2.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private List<int> _numbers = new();
    public List<int> Numbers
    {
        get => _numbers;
        set
        {
            _numbers = value;
            OnPropertyChanged();
        }
    }

    private Dictionary<int, BigInteger> _results = new();
    public Dictionary<int, BigInteger> Results
    {
        get => _results;
        set
        {
            _results = value;
            OnPropertyChanged();
        }
    }

    private string _status = "Выберите файл с числами";
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadFileCommand { get; }
    public ICommand CalculateCommand { get; }

    public MainViewModel()
    {
        LoadFileCommand = new RelayCommand(_ => LoadFile());
        CalculateCommand = new RelayCommand(_ => CalculateFactorials(), _ => Numbers.Count > 0);
    }

    private void LoadFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;
            var lines = File.ReadAllLines(FilePath);

            Numbers = lines
                .SelectMany(line => line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(s => int.TryParse(s, out int n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .Where(n => n >= 0) 
                .ToList();

            Status = $"Загружено чисел: {Numbers.Count}";
            Results = new Dictionary<int, BigInteger>();
        }
    }

    private void CalculateFactorials()
    {
        Status = "Вычисление факториалов в параллельном режиме...";

        Task.Run(() =>
        {
            var results = FactorialCalculator.CalculateParallel(Numbers);

            var sorted = results
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            App.Current.Dispatcher.Invoke(() =>
            {
                Results = sorted;
                Status = $"Вычислено факториалов: {Results.Count} (потоков: {Environment.ProcessorCount})";
            });
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}