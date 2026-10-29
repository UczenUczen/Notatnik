## MainWindow.axaml - zmiany
     <Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:notatnik"
        mc:Ignorable="d" d:DesignWidth="920" d:DesignHeight="600"
        Width="920" Height="620"
        MinWidth="680" MinHeight="440"
        WindowStartupLocation="CenterScreen"
        x:Class="notatnik.MainWindow"
        Title="Notatnik"> 

    <Grid ColumnDefinitions="320, 8, *" Margin="16">
        <!-- Lewy panel: Lista notatek -->
        <Border Grid.Column="0"
                BorderBrush="{DynamicResource CardStrokeColorDefault}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="12"
                Background="{DynamicResource CardBackgroundFillColorDefault}">
            <Grid RowDefinitions="Auto, Auto, Auto, *, Auto">
                <!-- Nagłówek listy -->
                <Grid Grid.Row="0" ColumnDefinitions="*, Auto" Margin="0,0,0,10">
                    <TextBlock Text="Zapisane notatki"
                               FontSize="18"
                               FontWeight="SemiBold"
                               VerticalAlignment="Center" />
                    <Border Grid.Column="1"
                            Background="{DynamicResource AccentButtonBackground}"
                            CornerRadius="10"
                            Padding="8,2"
                            VerticalAlignment="Center">
                        <TextBlock x:Name="NotesCountTextBlock"
                                   Text="0"
                                   FontSize="12"
                                   FontWeight="Bold"
                                   Foreground="{DynamicResource AccentButtonForeground}" />
                    </Border>
                </Grid>

                <!-- Przycisk nowej notatki -->
                <Button Grid.Row="1"
                        x:Name="NewNoteBtn"
                        Content="➕ Nowa notatka"
                        Classes="accent"
                        HorizontalAlignment="Stretch"
                        HorizontalContentAlignment="Center"
                        Click="OnNewNoteClick"
                        Margin="0,0,0,10" />

                <!-- Pole wyszukiwania -->
                <TextBox Grid.Row="2"
                         x:Name="SearchTextBox"
                         PlaceholderText="🔍 Szukaj notatki..."
                         TextChanged="OnSearchTextChanged"
                         Margin="0,0,0,10" />

                <!-- Lista notatek -->
                <ListBox Grid.Row="3"
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

                <!-- Komunikat gdy brak notatek -->
                <TextBlock Grid.Row="3"
                           x:Name="EmptyNotesPlaceholder"
                           Text="Brak zapisanych notatek.&#x0a;Wypełnij formularz po prawej i zapisz swoją pierwszą notatkę!"
                           TextWrapping="Wrap"
                           TextAlignment="Center"
                           Opacity="0.6"
                           VerticalAlignment="Center"
                           HorizontalAlignment="Center"
                           Margin="12"
                           IsHitTestVisible="False" />

                <!-- Informacja o pliku w stopce listy -->
                <Border Grid.Row="4"
                        Margin="0,8,0,0"
                        Padding="6"
                        Background="{DynamicResource LayerOnAcrylicAccentFillColorDefault}"
                        CornerRadius="4">
                    <TextBlock x:Name="StorageInfoTextBlock"
                               Text="Plik: notatki.json"
                               FontSize="11"
                               Opacity="0.7"
                               TextTrimming="CharacterEllipsis" />
                </Border>
            </Grid>
        </Border>

        <!-- Rozdzielacz paneli -->
        <GridSplitter Grid.Column="1"
                      Width="4"
                      Background="Transparent"
                      ResizeDirection="Columns" />

        <!-- Prawy panel: Edytor notatki -->
        <Border Grid.Column="2"
                BorderBrush="{DynamicResource CardStrokeColorDefault}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="16"
                Background="{DynamicResource CardBackgroundFillColorDefault}">
            <Grid RowDefinitions="Auto, Auto, Auto, Auto, *, Auto">
                <!-- Tytuł sekcji edytora -->
                <TextBlock Grid.Row="0"
                           x:Name="EditorHeaderTextBlock"
                           Text="Nowa notatka"
                           FontSize="18"
                           FontWeight="SemiBold"
                           Margin="0,0,0,12" />

                <!-- Etykieta i pole na tytuł -->
                <TextBlock Grid.Row="1"
                           Text="Tytuł:"
                           FontWeight="Medium"
                           FontSize="13"
                           Margin="0,0,0,4" />
                <TextBox Grid.Row="2"
                         x:Name="TitleTextBox"
                         PlaceholderText="Wpisz tytuł notatki..."
                         FontSize="14"
                         Padding="10,8"
                         Margin="0,0,0,12" />

                <!-- Etykieta na treść -->
                <TextBlock Grid.Row="3"
                           Text="Treść:"
                           FontWeight="Medium"
                           FontSize="13"
                           Margin="0,0,0,4" />

                <!-- Pole na treść -->
                <TextBox Grid.Row="4"
                         x:Name="ContentTextBox"
                         PlaceholderText="Wpisz treść notatki..."
                         AcceptsReturn="True"
                         TextWrapping="Wrap"
                         VerticalContentAlignment="Top"
                         VerticalAlignment="Stretch"
                         FontSize="14"
                         Padding="10"
                         Margin="0,0,0,12" />

                <!-- Pasek akcji -->
                <Grid Grid.Row="5" ColumnDefinitions="Auto, Auto, Auto, *">
                    <!-- Przycisk zapisu -->
                    <Button Grid.Column="0"
                            x:Name="SaveButton"
                            Content="💾 Zapisz notatkę"
                            Classes="accent"
                            Padding="16,8"
                            FontWeight="SemiBold"
                            Click="OnSaveClick"
                            HotKey="Ctrl+S"
                            Margin="0,0,8,0" />

                    <!-- Przycisk wyczyszczenia -->
                    <Button Grid.Column="1"
                            x:Name="ClearButton"
                            Content="Wyczyść formularz"
                            Padding="12,8"
                            Click="OnClearClick"
                            Margin="0,0,8,0" />

                    <!-- Przycisk usunięcia -->
                    <Button Grid.Column="2"
                            x:Name="DeleteButton"
                            Content="🗑️ Usuń"
                            Padding="12,8"
                            Click="OnDeleteClick"
                            IsVisible="False"
                            Margin="0,0,8,0" />

                    <!-- Komunikat o statusie zapisu / błędu -->
                    <TextBlock Grid.Column="3"
                               x:Name="StatusTextBlock"
                               VerticalAlignment="Center"
                               Margin="8,0,0,0"
                               FontSize="13"
                               FontWeight="Medium"
                               TextTrimming="CharacterEllipsis" />
                </Grid>
            </Grid>
        </Border>
    </Grid>
    </Window> 


## MainWindow.axaml.cs


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly string _filePath;
    private readonly List<Note> _allNotes = new();
    private readonly ObservableCollection<Note> _displayedNotes = new();
    private Note? _selectedNote;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public MainWindow()
    {
        InitializeComponent();

        _filePath = ResolveStoragePath();

        NotesListBox.ItemsSource = _displayedNotes;

        StorageInfoTextBlock.Text = $"Plik: {Path.GetFileName(_filePath)}";
        ToolTip.SetTip(StorageInfoTextBlock, $"Pełna ścieżka: {_filePath}");

        LoadNotesFromFile();
    }

    private static string ResolveStoragePath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var projectFile = Path.Combine(currentDir, "notatnik.csproj");
        if (File.Exists(projectFile) || File.Exists(Path.Combine(currentDir, "notatki.json")))
        {
            return Path.Combine(currentDir, "notatki.json");
        }

        return Path.Combine(AppContext.BaseDirectory, "notatki.json");
    }

    /// <summary>
    /// Wczytuje notatki z pliku JSON.
    /// </summary>
    private void LoadNotesFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loadedNotes = JsonSerializer.Deserialize<List<Note>>(json, JsonOptions);

                _allNotes.Clear();
                if (loadedNotes != null)
                {
                    _allNotes.AddRange(loadedNotes.OrderByDescending(n => n.ModifiedAt ?? n.CreatedAt));
                }
            }
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Błąd podczas wczytywania notatek: {ex.Message}", Brushes.Red);
        }

        ApplyFilter();
    }

    /// <summary>
    /// Zapisuje aktualną listę notatek do pliku JSON.
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

            var json = JsonSerializer.Serialize(_allNotes, JsonOptions);
            File.WriteAllText(_filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Błąd zapisu do pliku: {ex.Message}", Brushes.Red);
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

        // Walidacja - tytuł jest wymagany
        if (string.IsNullOrWhiteSpace(title))
        {
            SetStatus("⚠️ Wpisz tytuł notatki przed zapisaniem!", Brushes.OrangeRed);
            TitleTextBox.Focus();
            return;
        }

        if (_selectedNote != null)
        {
            // Aktualizacja istniejącej notatki
            _selectedNote.Title = title;
            _selectedNote.Content = content;
            _selectedNote.ModifiedAt = DateTime.Now;

            // Sortowanie notatek wg najnowszej edycji
            _allNotes.Remove(_selectedNote);
            _allNotes.Insert(0, _selectedNote);

            if (SaveNotesToFile())
            {
                SetStatus("✅ Zaktualizowano notatkę i zapisano do pliku!", Brushes.ForestGreen);
            }
        }
        else
        {
            // Utworzenie nowej notatki
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
                SetStatus("✅ Notatka zapisana do pliku!", Brushes.ForestGreen);
            }

            EditorHeaderTextBlock.Text = "Edycja notatki";
            DeleteButton.IsVisible = true;
            ClearButton.Content = "Nowa notatka";
        }

        ApplyFilter();
        NotesListBox.SelectedItem = _selectedNote;
    }

    /// <summary>
    /// Obsługa przycisku nowej notatki / wyczyszczenia formularza.
    /// </summary>
    private void OnNewNoteClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        TitleTextBox.Focus();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    /// <summary>
    /// Czyści formularz i przełącza w tryb tworzenia nowej notatki.
    /// </summary>
    private void ClearForm()
    {
        _selectedNote = null;
        NotesListBox.SelectedItem = null;

        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        EditorHeaderTextBlock.Text = "Nowa notatka";
        DeleteButton.IsVisible = false;
        ClearButton.Content = "Wyczyść formularz";
        SetStatus("Formularz wyczyszczony. Gotowy na nową notatkę.", Brushes.Gray);
    }

    /// <summary>
    /// Obsługa usuwania zaznaczonej notatki.
    /// </summary>
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedNote == null) return;

        var noteToRemove = _selectedNote;
        _allNotes.Remove(noteToRemove);

        if (SaveNotesToFile())
        {
            SetStatus($"🗑️ Usunięto notatkę \"{noteToRemove.Title}\".", Brushes.DodgerBlue);
        }

        ClearForm();
        ApplyFilter();
    }

    /// <summary>
    /// Obsługa zmiany wyboru notatki na liście.
    /// </summary>
    private void OnNotesListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NotesListBox.SelectedItem is Note note)
        {
            _selectedNote = note;
            TitleTextBox.Text = note.Title;
            ContentTextBox.Text = note.Content;

            EditorHeaderTextBlock.Text = "Edycja notatki";
            DeleteButton.IsVisible = true;
            ClearButton.Content = "Nowa notatka";
            SetStatus($"Wczytano notatkę: {note.Title}", Brushes.Gray);
        }
    }

    /// <summary>
    /// Filtrowanie listy notatek po wpisaniu tekstu w wyszukiwarkę.
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filter = SearchTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;

        var matchingNotes = string.IsNullOrEmpty(filter)
            ? _allNotes
            : _allNotes.Where(n =>
                n.Title.ToLowerInvariant().Contains(filter) ||
                n.Content.ToLowerInvariant().Contains(filter)).ToList();

        _displayedNotes.Clear();
        foreach (var note in matchingNotes)
        {
            _displayedNotes.Add(note);
        }

        NotesCountTextBlock.Text = _allNotes.Count.ToString();
        EmptyNotesPlaceholder.IsVisible = _displayedNotes.Count == 0;
    }

    /// <summary>
    /// Ustawia komunikat o statusie.
    /// </summary>
    private void SetStatus(string message, IBrush brush)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = brush;
    }
}




