## MainWindow.axaml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:notatnik"
        mc:Ignorable="d" d:DesignWidth="850" d:DesignHeight="550"
        Width="850" Height="550"
        MinWidth="600" MinHeight="400"
        WindowStartupLocation="CenterScreen"
        x:Class="notatnik.MainWindow"
        Title="Notatnik">

    <Grid ColumnDefinitions="300, 10, *" Margin="16">
        <!-- Lista zapisanych notatek -->
        <Border Grid.Column="0"
                BorderBrush="{DynamicResource CardStrokeColorDefault}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="12"
                Background="{DynamicResource CardBackgroundFillColorDefault}">
            <Grid RowDefinitions="Auto, *">
                <TextBlock Grid.Row="0"
                           Text="Zapisane notatki"
                           FontSize="16"
                           FontWeight="SemiBold"
                           Margin="0,0,0,10" />

                <ListBox Grid.Row="1"
                         x:Name="NotesListBox"
                         SelectionMode="Single"
                         SelectionChanged="OnNotesListSelectionChanged"
                         Background="Transparent">
                    <ListBox.ItemTemplate>
                        <DataTemplate x:DataType="local:Note">
                            <Border Padding="6,4" Margin="0,2">
                                <StackPanel Spacing="3">
                                    <TextBlock Text="{Binding Title}"
                                               FontWeight="SemiBold"
                                               FontSize="14"
                                               TextTrimming="CharacterEllipsis" />
                                    <TextBlock Text="{Binding FormattedDate}"
                                               FontSize="11"
                                               Opacity="0.6" />
                                    <TextBlock Text="{Binding PreviewSnippet}"
                                               FontSize="12"
                                               Opacity="0.8"
                                               MaxLines="2"
                                               TextWrapping="Wrap"
                                               TextTrimming="CharacterEllipsis" />
                                </StackPanel>
                            </Border>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
            </Grid>
        </Border>

        <!-- Rozdzielacz -->
        <GridSplitter Grid.Column="1"
                      Width="4"
                      Background="Transparent"
                      ResizeDirection="Columns" />

        <!-- Formularz notatki -->
        <Border Grid.Column="2"
                BorderBrush="{DynamicResource CardStrokeColorDefault}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="16"
                Background="{DynamicResource CardBackgroundFillColorDefault}">
            <Grid RowDefinitions="Auto, Auto, Auto, *, Auto">
                <!-- Pole na tytuł -->
                <TextBlock Grid.Row="0"
                           Text="Tytuł:"
                           FontWeight="Medium"
                           FontSize="13"
                           Margin="0,0,0,4" />
                <TextBox Grid.Row="1"
                         x:Name="TitleTextBox"
                         PlaceholderText="Wpisz tytuł notatki..."
                         FontSize="14"
                         Padding="10,8"
                         Margin="0,0,0,12" />

                <!-- Pole na treść -->
                <TextBlock Grid.Row="2"
                           Text="Treść:"
                           FontWeight="Medium"
                           FontSize="13"
                           Margin="0,0,0,4" />
                <TextBox Grid.Row="3"
                         x:Name="ContentTextBox"
                         PlaceholderText="Wpisz treść notatki..."
                         AcceptsReturn="True"
                         TextWrapping="Wrap"
                         VerticalContentAlignment="Top"
                         VerticalAlignment="Stretch"
                         FontSize="14"
                         Padding="10"
                         Margin="0,0,0,12" />

                <!-- Pasek zapisu -->
                <StackPanel Grid.Row="4" Orientation="Horizontal" Spacing="10">
                    <Button x:Name="SaveButton"
                            Content="Zapisz"
                            Classes="accent"
                            Padding="20,8"
                            FontWeight="SemiBold"
                            Click="OnSaveClick" />

                    <TextBlock x:Name="StatusTextBlock"
                               VerticalAlignment="Center"
                               FontSize="13"
                               FontWeight="Medium" />
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</Window>

## MainWindow.axaml.cs
 
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

    // Wzorzec nagłówka wpisu w Markdown: ## RRRR-MM-DD GG:MM — Tytuł notatki
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
    /// Ustala ścieżkę do pliku notatki.md w folderze projektu.
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
                _allNotes.AddRange(loadedNotes.OrderByDescending(n => n.CreatedAt));
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd podczas wczytywania: {ex.Message}", Brushes.Red);
        }

        RefreshNotesList();
    }

    /// <summary>
    /// Parsuje treść pliku Markdown na listę notatek.
    /// </summary>
    private static List<Note> ParseMarkdown(string markdown)
    {
        var notes = new List<Note>();
        if (string.IsNullOrWhiteSpace(markdown)) return notes;

        var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        Note? currentNote = null;
        var contentLines = new List<string>();

        void FlushCurrentNote()
        {
            if (currentNote != null)
            {
                while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[0]))
                    contentLines.RemoveAt(0);
                while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[^1]))
                    contentLines.RemoveAt(contentLines.Count - 1);

                currentNote.Content = string.Join(Environment.NewLine, contentLines);
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

                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
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
    /// Zapisuje wszystkie notatki do jednego pliku notatki.md w folderze projektu.
    /// Najnowsza notatka znajduje się na górze pliku.
    /// Zachowuje dokładny format nagłówka oraz pustą linię między wpisami.
    /// Polskie znaki są zapisywane w kodowaniu UTF-8.
    /// </summary>
    private bool SaveNotesToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var sb = new StringBuilder();
            var sortedNotes = _allNotes.OrderByDescending(n => n.CreatedAt).ToList();

            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var note = sortedNotes[i];
                sb.AppendLine($"## {note.CreatedAt:yyyy-MM-dd HH:mm} — {note.Title}");
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(note.Content))
                {
                    sb.AppendLine(note.Content.TrimEnd());
                }

                // Jedna pusta linia między wpisami
                if (i < sortedNotes.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            File.WriteAllText(_filePath, sb.ToString(), Utf8NoBom);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd zapisu: {ex.Message}", Brushes.Red);
            return false;
        }
    }

    /// <summary>
    /// Obsługa przycisku zapisu notatki.
    /// </summary>
    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text?.Trim() ?? string.Empty;
        var content = ContentTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            SetStatus("Wpisz tytuł notatki!", Brushes.OrangeRed);
            TitleTextBox.Focus();
            return;
        }

        if (_selectedNote != null && _selectedNote.Title == title)
        {
            // Aktualizacja zaznaczonej notatki
            _selectedNote.Content = content;
            _selectedNote.CreatedAt = DateTime.Now;

            _allNotes.Remove(_selectedNote);
            _allNotes.Insert(0, _selectedNote);

            if (SaveNotesToFile())
            {
                SetStatus("Zaktualizowano notatkę!", Brushes.ForestGreen);
            }
        }
        else
        {
            // Utworzenie nowej notatki z zachowaniem wszystkich wcześniejszych
            var newNote = new Note
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _allNotes.Insert(0, newNote);
            _selectedNote = newNote;

            if (SaveNotesToFile())
            {
                SetStatus("Zapisano notatkę!", Brushes.ForestGreen);
            }
        }

        RefreshNotesList();
        NotesListBox.SelectedItem = _selectedNote;
    }

    /// <summary>
    /// Wybór notatki z listy wczytuje jej dane do pól edycji.
    /// </summary>
    private void OnNotesListSelectionChanged(object? sender, SelectionChangedEventArgs e)
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

## notatki.md 

## 2026-10-29 09:04 — test

test

## 2026-09-08 14:32 — Zakupy

mleko

## 2026-09-07 09:15 — Pomysł na projekt

minutnik z historią sesji

## 2026-09-06 18:45 — Ważne sprawy

Spotkanie w Łodzi — przełożyć na piątek.
Pamiętać o książce dla przyjaciółki.
