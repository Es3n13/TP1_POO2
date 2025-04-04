using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Xml.Linq;

namespace FlowerShop.Class
{

    //Fleur
    public class Flower
    {
        public string Name { get; init; }
        public string Price { get; init; }
        public string Color { get; init; }
        public string Description { get; init; }

        //Retourne le prix en décimal pour être utilisable
        public decimal GetPrice()
        {
            if (decimal.TryParse(Price, out decimal result))
            {
                return result;
            }
            else
            {
                Console.WriteLine($"Erreur de conversion du prix pour {Name} : {Price}");
                return 0m; // Retourne 0 en cas d'erreur
            }
        }
    }
    //Mappage des colonnes du fichier fleurs_db.csv pour correspondre avec les attributs de la classe Flower
    public class FlowerMap : ClassMap<Flower>
    {
        public FlowerMap()
        {
            Map(m => m.Name).Name("Nom");
            Map(m => m.Price).Name("Prix Unitaire (CAD)");
            Map(m => m.Color).Name("Couleur");
            Map(m => m.Description).Name("Caractéristiques");
        }
    }

    //Gestionnaire de fleurs
    public static class FlowerManager
    {
        public static List<Flower> LoadFlowersFromCSV(string CsvPath)
        {
            if (!File.Exists(CsvPath))
            {
                Console.WriteLine($"Erreur : Le fichier CSV '{CsvPath}' n'existe pas.");
                return new List<Flower>();
            }

            try
            {
                var config = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
                {
                    Delimiter = ",",
                    Encoding = Encoding.UTF8,
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    DetectColumnCountChanges = true
                };

                using var reader = new StreamReader(CsvPath, Encoding.UTF8);
                using var csv = new CsvReader(reader, config);

                // Enregistrement EXPLICITE du mapping
                csv.Context.RegisterClassMap<FlowerMap>();

                // Lecture des données
                var flowers = csv.GetRecords<Flower>().ToList();

                return flowers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture du fichier CSV : {ex.Message}");
                return new List<Flower>();
            }
        }
    }
}


