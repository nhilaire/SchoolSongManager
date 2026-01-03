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

        // Pour Azure Web App, le répertoire data est à la racine
        var rootPath = configuration["DataPath"] ?? "/home/data";

        // En développement local, utiliser un répertoire dans le projet
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            rootPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        }

        _dataDirectory = rootPath;

        // Configuration de QuestPDF (licence community)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateNurseryRhymesPdf(List<NurseryRhyme> nurseryRhymes, string schoolYear, string period)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Format paysage A4
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20); // Marge minimale

                // Contenu uniquement - pas d'en-tête ni de pied de page
                page.Content().Column(column =>
                {
                    // Une page par comptine, avec 2 fois la même image
                    foreach (var nurseryRhyme in nurseryRhymes)
                    {
                        column.Item().Row(row =>
                        {
                            row.Spacing(20);

                            // Première instance de la comptine (côté gauche)
                            row.RelativeItem(1).Column(leftColumn =>
                            {
                                RenderNurseryRhymeImageOnly(leftColumn, nurseryRhyme);
                            });

                            // Deuxième instance de la même comptine (côté droit)
                            row.RelativeItem(1).Column(rightColumn =>
                            {
                                RenderNurseryRhymeImageOnly(rightColumn, nurseryRhyme);
                            });
                        });

                        // Nouvelle page pour la prochaine comptine (sauf pour la dernière)
                        if (nurseryRhyme != nurseryRhymes.Last())
                        {
                            column.Item().PageBreak();
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private void RenderNurseryRhymeImageOnly(ColumnDescriptor column, NurseryRhyme nurseryRhyme)
    {
        // Image uniquement, taille adaptée à l'espace disponible
        column.Item().AlignCenter().Column(imageColumn =>
        {
            try
            {
                if (!string.IsNullOrEmpty(nurseryRhyme.ImageFileName))
                {
                    // Chemin vers l'image dans le dossier data/images
                    var imagePath = Path.Combine(_dataDirectory, "images", nurseryRhyme.ImageFileName);

                    if (File.Exists(imagePath))
                    {
                        imageColumn.Item()
                            .MaxWidth(380)    // Largeur max adaptée à l'espace (391px - marge)
                            .MaxHeight(540)   // Hauteur max adaptée à l'espace (555px - marge)
                            .Image(imagePath)
                            .FitArea();    // Redimensionne en gardant les proportions
                    }
                    else
                    {
                        // Placeholder si l'image n'existe pas
                        RenderImagePlaceholder(imageColumn, "📷", "Image non trouvée");
                    }
                }
                else
                {
                    // Pas d'image spécifiée
                    RenderImagePlaceholder(imageColumn, "🎵", "Pas d'image");
                }
            }
            catch (Exception ex)
            {
                // Placeholder en cas d'erreur
                RenderImagePlaceholder(imageColumn, "❌", "Erreur de chargement");
                Console.WriteLine($"Erreur chargement image {nurseryRhyme.ImageFileName}: {ex.Message}");
            }
        });
    }

    private void RenderNurseryRhymeImage(ColumnDescriptor column, NurseryRhyme nurseryRhyme)
    {
        column.Spacing(15);

        // Titre seulement (pas de numérotation)
        column.Item().Text(nurseryRhyme.Title)
            .FontSize(18)
            .Bold()
            .FontColor(Colors.Blue.Darken2)
            .AlignCenter();

        // Image principale
        column.Item().AlignCenter().Column(imageColumn =>
        {
            try
            {
                if (!string.IsNullOrEmpty(nurseryRhyme.ImageFileName))
                {
                    // Chemin vers l'image dans le dossier data/images
                    var imagePath = Path.Combine(_dataDirectory, "images", nurseryRhyme.ImageFileName);

                    if (File.Exists(imagePath))
                    {
                        imageColumn.Item()
                            .Width(300)    // Grande image pour format paysage
                            .Height(300)   // Format carré
                            .Border(2)
                            .BorderColor(Colors.Blue.Medium)
                            .Image(imagePath)
                            .FitArea();
                    }
                    else
                    {
                        // Placeholder si l'image n'existe pas
                        RenderImagePlaceholder(imageColumn, "📷", "Image non trouvée");
                    }
                }
                else
                {
                    // Pas d'image spécifiée
                    RenderImagePlaceholder(imageColumn, "🎵", "Pas d'image");
                }
            }
            catch (Exception ex)
            {
                // Placeholder en cas d'erreur
                RenderImagePlaceholder(imageColumn, "❌", "Erreur de chargement");
                Console.WriteLine($"Erreur chargement image {nurseryRhyme.ImageFileName}: {ex.Message}");
            }
        });
    }

    private void RenderImagePlaceholder(ColumnDescriptor column, string emoji, string message)
    {
        column.Item()
            .MaxWidth(380)
            .MaxHeight(540)
            .Border(2)
            .BorderColor(Colors.Grey.Medium)
            .Background(Colors.Grey.Lighten3)
            .AlignCenter()
            .AlignMiddle()
            .Column(placeholderColumn =>
            {
                placeholderColumn.Item().AlignCenter().Text(emoji)
                    .FontSize(80);  // Plus grande taille pour l'emoji
                    
                placeholderColumn.Item().AlignCenter().Text(message)
                    .FontSize(18)   // Plus grande taille pour le texte
                    .FontColor(Colors.Grey.Darken1);
            });
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
}