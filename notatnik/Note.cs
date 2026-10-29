using System;
using System.Text.Json.Serialization;

namespace notatnik;

public class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ModifiedAt { get; set; }

    [JsonIgnore]
    public string FormattedDate => (ModifiedAt ?? CreatedAt).ToString("dd.MM.yyyy HH:mm");

    [JsonIgnore]
    public string PreviewSnippet
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Content))
                return "(Brak treści)";

            var singleLine = Content.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
            return singleLine.Length > 60 ? singleLine[..57] + "..." : singleLine;
        }
    }
}
