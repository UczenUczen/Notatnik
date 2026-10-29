using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly string _filePath;
    private readonly List<Note> _allNotes = new();
    private readonly ObservableCollection<Note> _displayedNotes = new();
    private Note? _selectedNote;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // Wzorzec nagłówka wpisu w Markdown.
    private static readonly Regex HeaderRegex = new(
        @"^##\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2})\s+[—–-]\s*(.*)$",
        RegexOptions.Compiled);

    public MainWindow()
    {
        InitializeComponent();

        _filePath = ResolveStoragePath();
        NotesListBox.ItemsSource = _displayedNotes;

        LoadNotesFromFile();
    }

    /// <summary>
    /// Ustala ścieżkę do pliku notatki.md.
    /// </summary>
    private static string ResolveStoragePath()
    {
        var currentDir = Directory.GetCurrentDirectory();

        var projectFile = Path.Combine(currentDir, "notatnik.csproj");
        if (File.Exists(projectFile) || File.Exists(Path.Combine(currentDir, "notatki.md")))
        {
            return Path.Combine(currentDir, "notatki.md");
        }

        var subDirProject = Path.Combine(currentDir, "notatnik", "notatnik.csproj");
        if (File.Exists(subDirProject))
        {
            return Path.Combine(currentDir, "notatnik", "notatki.md");
        }

        return Path.Combine(AppContext.BaseDirectory, "notatki.md");
    }

    /// <summary>
    /// Wczytuje notatki z pliku Markdown.
    /// </summary>
    private void LoadNotesFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var markdown = File.ReadAllText(_filePath, Encoding.UTF8);
                var loadedNotes = ParseMarkdown(markdown);

                _allNotes.Clear();
                _allNotes.AddRange(
                    loadedNotes.OrderByDescending(n => n.CreatedAt));
            }
        }
        catch (Exception ex)
        {
            SetStatus(
                $"Błąd podczas wczytywania: {ex.Message}",
                Brushes.Red);
        }

        RefreshNotesList();
    }

    /// <summary>
    /// Parsuje treść pliku Markdown na listę notatek.
    /// </summary>
    private static List<Note> ParseMarkdown(string markdown)
    {
        var notes = new List<Note>();

        if (string.IsNullOrWhiteSpace(markdown))
            return notes;

        var lines = markdown
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        Note? currentNote = null;
        var contentLines = new List<string>();

        void FlushCurrentNote()
        {
            if (currentNote != null)
            {
                while (contentLines.Count > 0 &&
                       string.IsNullOrWhiteSpace(contentLines[0]))
                {
                    contentLines.RemoveAt(0);
                }

                while (contentLines.Count > 0 &&
                       string.IsNullOrWhiteSpace(contentLines[^1]))
                {
                    contentLines.RemoveAt(contentLines.Count - 1);
                }

                currentNote.Content =
                    string.Join(Environment.NewLine, contentLines);

                notes.Add(currentNote);
                currentNote = null;
                contentLines.Clear();
            }
        }

        foreach (var line in lines)
        {
            var match = HeaderRegex.Match(line);

            if (match.Success)
            {
                FlushCurrentNote();

                var dateStr = match.Groups[1].Value.Trim();
                var titleStr = match.Groups[2].Value.Trim();

                if (!DateTime.TryParseExact(
                        dateStr,
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDate))
                {
                    parsedDate = DateTime.Now;
                }

                currentNote = new Note
                {
                    Id = Guid.NewGuid(),
                    Title = titleStr,
                    CreatedAt = parsedDate
                };
            }
            else if (currentNote != null)
            {
                contentLines.Add(line);
            }
        }

        FlushCurrentNote();
        return notes;
    }

    /// <summary>
    /// Zapisuje wszystkie notatki do jednego pliku Markdown.
    /// Najnowsza notatka znajduje się na górze.
    /// Kodowanie: UTF-8 bez BOM.
    /// </summary>
    private bool SaveNotesToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(dir) &&
                !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var sb = new StringBuilder();
            var sortedNotes = _allNotes
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var note = sortedNotes[i];

                sb.AppendLine(
                    $"## {note.CreatedAt:yyyy-MM-dd HH:mm} — {note.Title}");
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(note.Content))
                {
                    sb.AppendLine(note.Content.TrimEnd());
                }

                // Jedna pusta linia między wpisami.
                if (i < sortedNotes.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            File.WriteAllText(
                _filePath,
                sb.ToString(),
                Utf8NoBom);

            return true;
        }
        catch (Exception ex)
        {
            SetStatus(
                $"Błąd zapisu: {ex.Message}",
                Brushes.Red);

            return false;
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text?.Trim() ?? string.Empty;
        var content = ContentTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            SetStatus(
                "Wpisz tytuł notatki!",
                Brushes.OrangeRed);

            TitleTextBox.Focus();
            return;
        }

        var newNote = new Note
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            CreatedAt = DateTime.Now
        };

        // Dodaj notatkę tymczasowo do listy.
        _allNotes.Insert(0, newNote);

        // Jeśli zapis się nie uda, wycofaj zmianę w pamięci.
        // Nie czyść formularza — użytkownik zachowa swoją treść.
        if (!SaveNotesToFile())
        {
            _allNotes.Remove(newNote);
            RefreshNotesList();
            return;
        }

        // Formularz czyścimy dopiero po udanym zapisie.
        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        _selectedNote = null;
        NotesListBox.SelectedItem = null;

        RefreshNotesList();

        SetStatus(
            "Zapisano notatkę!",
            Brushes.ForestGreen);
    }

    /// <summary>
    /// Wybór notatki z listy wczytuje jej dane do pól edycji.
    /// </summary>
    private void OnNotesListSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (NotesListBox.SelectedItem is Note note)
        {
            _selectedNote = note;
            TitleTextBox.Text = note.Title;
            ContentTextBox.Text = note.Content;

            SetStatus(string.Empty, Brushes.Transparent);
        }
    }

    private void RefreshNotesList()
    {
        _displayedNotes.Clear();

        foreach (var note in _allNotes)
        {
            _displayedNotes.Add(note);
        }
    }

    private void SetStatus(string message, IBrush brush)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = brush;
    }
}
