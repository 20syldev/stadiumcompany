using System.Text.Json;

namespace StadiumCompany.Services;

public class LocalizationManager
{
    private static LocalizationManager? _instance;
    public static LocalizationManager Instance => _instance ??= new LocalizationManager();

    private Dictionary<string, string> _strings = new();
    private string _currentLanguage = "fr";

    public string CurrentLanguage => _currentLanguage;

    public event Action? LanguageChanged;

    private LocalizationManager()
    {
        LoadLanguage("fr");
    }

    public void SetLanguage(string language)
    {
        if (_currentLanguage == language) return;
        LoadLanguage(language);
        _currentLanguage = language;
        LanguageChanged?.Invoke();
    }

    public void ToggleLanguage()
    {
        SetLanguage(_currentLanguage == "fr" ? "en" : "fr");
    }

    private void LoadLanguage(string language)
    {
        var resourceName = $"Strings.{language}.json";
        var assembly = typeof(LocalizationManager).Assembly;

        // Try embedded resource first
        var resourcePath = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceName));

        if (resourcePath != null)
        {
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                return;
            }
        }

        // Fallback: try file path relative to assembly
        var basePath = Path.GetDirectoryName(assembly.Location) ?? ".";
        var filePath = Path.Combine(basePath, "Resources", resourceName);

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
    }

    public string T(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
    }

    public string T(string key, params object[] args)
    {
        var template = T(key);
        return string.Format(template, args);
    }

    public string TranslateTheme(string themeName)
    {
        var key = $"theme.{themeName}";
        return _strings.TryGetValue(key, out var value) ? value : themeName;
    }
}
