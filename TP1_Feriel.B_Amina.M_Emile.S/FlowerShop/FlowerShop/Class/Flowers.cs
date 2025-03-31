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

        public Flower() { }

        public Flower(string name, string price, string color, string description)
        {
            Name = name;
            Price = price;
            Color = color;
            Description = description;
        }

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
                var flowers = new List<Flower>();

                while (csv.Read())
                {
                    try
                    {
                        var flower = csv.GetRecord<Flower>();
                        if (flower != null) // Vérifier si la lecture a réussi
                        {
                            flowers.Add(flower);
                            /*Console.WriteLine($"Lue: {flower.Name} {flower.Price} {flower.Color} {flower.Description}");*/
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur ligne {csv.Context.Parser.Row}: {ex.Message}");
                    }
                }

                //Console.WriteLine($"Nombre total de fleurs chargées : {flowers.Count}");
                return flowers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de lecture du fichier CSV : {ex.Message}");
                return new List<Flower>(); // Retourne une liste vide en cas d'erreur
            }
        }
        // Afficher les fleurs disponibles
        public static void DisplayFlowers(List<Flower> flowers)
        {
            if (flowers == null || flowers.Count == 0)
            {
                Console.WriteLine("Aucune fleur disponible.");
                return;
            }

            Console.WriteLine("Fleurs disponibles :");
            for (int i = 0; i < flowers.Count; i++)
            {
                var flower = flowers[i];
                Console.WriteLine($"{i + 1}. {flower.Name} - {flower.Color} - {flower.Price:C}$ - {flower.Description}");
            }
        }
    }
}

