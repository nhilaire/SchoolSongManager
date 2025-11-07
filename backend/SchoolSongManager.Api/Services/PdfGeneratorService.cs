using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolSongManager.Api.Models;
using System.Text.Json;

namespace SchoolSongManager.Api.Services;

public interface IPdfGeneratorService
{
    byte[] GenerateNurseryRhymesPdf(List<NurseryRhyme> nurseryRhymes, string schoolYear, string period);
}

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _dataDirectory;

    public PdfGeneratorService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _dataDirectory = _configuration.GetValue<string>("DataDirectory") ?? "data";
        
        // Configuration de QuestPDF (licence community)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateNurseryRhymesPdf(List<NurseryRhyme> nurseryRhymes, string schoolYear, string period)
    {
        // Charger les noms des thèmes une seule fois
        var themeMap = LoadThemeNames();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                // En-tête
                page.Header()
                    .Height(120)  // Augmenté de 100 à 120
                    .Background(Colors.Blue.Lighten3)
                    .Padding(15)  // Réduit de 20 à 15 pour plus d'espace
                    .Column(column =>
                    {
                        column.Spacing(5);  // Espacement réduit entre les lignes
                        
                        column.Item().Text("Comptines Sélectionnées")
                            .FontSize(20)  // Réduit de 24 à 20
                            .FontColor(Colors.White)
                            .Bold();

                        column.Item().Text($"Année scolaire: {schoolYear}")
                            .FontSize(14)  // Réduit de 16 à 14
                            .FontColor(Colors.White);

                        column.Item().Text($"Période: {GetPeriodLabel(period)}")
                            .FontSize(12)  // Réduit de 14 à 12
                            .FontColor(Colors.White);
                    });

                // Contenu principal
                page.Content().Padding(15).Column(column =>  // Réduit le padding
                {
                    column.Spacing(10);  // Réduit l'espacement

                    // Informations générales
                    column.Item().Text($"Date de génération: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10)  // Réduit de 12 à 10
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().Text($"Nombre total de comptines: {nurseryRhymes.Count}")
                        .FontSize(12)  // Réduit de 14 à 12
                        .Bold();

                    column.Item().LineHorizontal(1).LineColor(Colors.Blue.Medium);  // Ligne plus fine

                    // Liste des comptines
                    foreach (var (nurseryRhyme, index) in nurseryRhymes.Select((nr, i) => (nr, i + 1)))
                    {
                        column.Item().Row(row =>
                        {
                            // Image (si disponible)
                            if (!string.IsNullOrEmpty(nurseryRhyme.ImageFileName))
                            {
                                row.RelativeItem(1).Column(imageColumn =>
                                {
                                    try
                                    {
                                        // Chemin correct vers les images dans wwwroot
                                        var imagePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "wwwroot", "images", "nursery-rhymes", nurseryRhyme.ImageFileName);
                                        
                                        // Alternative: essayer aussi le chemin direct dans le dossier data
                                        if (!File.Exists(imagePath))
                                        {
                                            imagePath = Path.Combine(_dataDirectory, "images", nurseryRhyme.ImageFileName);
                                        }
                                        
                                        if (File.Exists(imagePath))
                                        {
                                            imageColumn.Item()
                                                .Width(60)   // Réduit de 80 à 60
                                                .Height(60)  // Réduit de 80 à 60
                                                .Image(imagePath)
                                                .FitArea();
                                        }
                                        else
                                        {
                                            // Placeholder si l'image n'existe pas
                                            imageColumn.Item()
                                                .Width(60)
                                                .Height(60)
                                                .Background(Colors.Grey.Lighten2)
                                                .AlignCenter()
                                                .AlignMiddle()
                                                .Text("📷")
                                                .FontSize(20);  // Réduit de 24 à 20
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Placeholder en cas d'erreur avec info de debug
                                        imageColumn.Item()
                                            .Width(60)
                                            .Height(60)
                                            .Background(Colors.Grey.Lighten2)
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Text("❌")
                                            .FontSize(20);
                                        
                                        // Log de l'erreur pour le debug
                                        Console.WriteLine($"Erreur chargement image {nurseryRhyme.ImageFileName}: {ex.Message}");
                                    }
                                });
                            }
                            else
                            {
                                // Pas d'image
                                row.RelativeItem(1).Column(imageColumn =>
                                {
                                    imageColumn.Item()
                                        .Width(60)
                                        .Height(60)
                                        .Background(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("🎵")
                                        .FontSize(20);
                                });
                            }

                            // Informations de la comptine
                            row.RelativeItem(3).Column(textColumn =>
                            {
                                textColumn.Item().Text($"{index}. {nurseryRhyme.Title}")
                                    .FontSize(14)  // Réduit de 16 à 14
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                if (!string.IsNullOrEmpty(nurseryRhyme.Url))
                                {
                                    textColumn.Item().Text($"URL: {nurseryRhyme.Url}")
                                        .FontSize(9)   // Réduit de 10 à 9
                                        .FontColor(Colors.Grey.Darken1);
                                }

                                // Utiliser les noms des thèmes au lieu des IDs
                                if (nurseryRhyme.ThemeIds?.Any() == true)
                                {
                                    var themeNames = GetThemeNames(nurseryRhyme.ThemeIds, themeMap);
                                    if (!string.IsNullOrEmpty(themeNames))
                                    {
                                        textColumn.Item().Text($"Thèmes: {themeNames}")
                                            .FontSize(9)   // Réduit de 10 à 9
                                            .FontColor(Colors.Grey.Darken1);
                                    }
                                }
                            });
                        });

                        // Ligne de séparation entre les comptines (plus petite)
                        if (index < nurseryRhymes.Count)
                        {
                            column.Item().Padding(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);  // Ligne plus fine
                        }
                    }
                });

                // Pied de page
                page.Footer()
                    .Height(50)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text =>
                    {
                        text.Span("Généré par ").FontSize(10);
                        text.Span("School Song Manager").FontSize(10).Bold();
                        text.Span(" - Page ");
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" sur ");
                        text.TotalPages().FontSize(10);
                    });
            });
        });

        return document.GeneratePdf();
    }

    private string GetPeriodLabel(string period)
    {
        return period switch
        {
            "P1" => "P1 - Septembre - Octobre",
            "P2" => "P2 - Novembre - Décembre", 
            "P3" => "P3 - Janvier - Février",
            "P4" => "P4 - Mars - Avril",
            "P5" => "P5 - Mai - Juin",
            _ => period
        };
    }

    private Dictionary<string, string> LoadThemeNames()
    {
        try
        {
            var themesFilePath = Path.Combine(_dataDirectory, "themes.json");
            Console.WriteLine($"Recherche des thèmes dans: {themesFilePath}");
            
            if (!File.Exists(themesFilePath))
            {
                Console.WriteLine($"Fichier themes.json non trouvé à: {themesFilePath}");
                return new Dictionary<string, string>();
            }

            var json = File.ReadAllText(themesFilePath);
            Console.WriteLine($"Contenu JSON des thèmes: {json.Substring(0, Math.Min(200, json.Length))}...");
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var themes = JsonSerializer.Deserialize<List<Theme>>(json, options) ?? new List<Theme>();
            Console.WriteLine($"Nombre de thèmes chargés: {themes.Count}");
            
            var themeMap = themes.ToDictionary(t => t.Id, t => t.Name);
            foreach (var kvp in themeMap)
            {
                Console.WriteLine($"Thème mappé: {kvp.Key} -> {kvp.Value}");
            }
            
            return themeMap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement des thèmes: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return new Dictionary<string, string>();
        }
    }

    private string GetThemeNames(List<string> themeIds, Dictionary<string, string> themeMap)
    {
        if (themeIds == null || themeIds.Count == 0)
        {
            Console.WriteLine("Aucun ID de thème fourni");
            return "";
        }

        Console.WriteLine($"IDs de thèmes à résoudre: {string.Join(", ", themeIds)}");
        
        var themeNames = new List<string>();
        foreach (var id in themeIds)
        {
            if (themeMap.TryGetValue(id, out var name))
            {
                Console.WriteLine($"Thème trouvé: {id} -> {name}");
                themeNames.Add(name);
            }
            else
            {
                Console.WriteLine($"Thème non trouvé pour ID: {id}");
                themeNames.Add(id); // Fallback sur l'ID
            }
        }
            
        var result = string.Join(", ", themeNames);
        Console.WriteLine($"Noms de thèmes finaux: {result}");
        return result;
    }
}