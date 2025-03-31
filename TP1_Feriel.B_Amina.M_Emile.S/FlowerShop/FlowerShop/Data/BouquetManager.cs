using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FlowerShop.Class;

namespace FlowerShop.Data
{
    public static class BouquetManager
    {
        //Chemin relatif vers les donneés des bouquets
        private static string BouquetDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BouquetData.json");

        //Vérification que le fichier existe
        private static void EnsureFileExists()
        {
            if (!File.Exists(BouquetDataPath))
            {
                File.WriteAllText(BouquetDataPath, "[]");
            }
        }

        //Sauvegarder un bouqet
        public static void SaveBouquets(List<Bouquet> bouquets)
        {
            string json = JsonConvert.SerializeObject(bouquets, Formatting.Indented);
            File.WriteAllText(BouquetDataPath, json);
        }

        //Cahrger les bouquets
        public static List<Bouquet> LoadBouquets()
        {
            EnsureFileExists();
            string json = File.ReadAllText(BouquetDataPath);
            return JsonConvert.DeserializeObject<List<Bouquet>>(json) ?? new List<Bouquet>();
        }

        //Ajouter un bouquet
        public static void AddBouquet(Bouquet bouquet)
        {
            List<Bouquet> bouquets = LoadBouquets();
            bouquets.Add(bouquet);
            SaveBouquets(bouquets);
        }

        //Créer un bouquet
        public static void CreateBouquet()
        {
            Console.Write("Nom du bouquet : ");
            string name = Console.ReadLine();

            List<Flower> flowers = new List<Flower>();
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
            List<Flower> availableFlowers = FlowerManager.LoadFlowersFromCSV(csvPath);

            if (availableFlowers.Count == 0)
            {
                Console.WriteLine("Aucune fleur disponible pour créer un bouquet.");
                return;
            }

            if (flowers.Count == 0)
            {
                Console.WriteLine("Aucune fleur sélectionnée. Annulation de la création du bouquet.");
                return;
            }

            Bouquet newBouquet = new Bouquet(name) { Flowers = flowers };

            AddBouquet(newBouquet);
            Console.WriteLine($"Bouquet '{name}' ajouté avec succès !");
        }

        //Afficher les bouquets existants
        public static void DisplayBouquets()
        {
            List<Bouquet> bouquets = LoadBouquets();
            if (bouquets.Count == 0)
            {
                Console.WriteLine("Aucun bouquet n'a été créé pour le moment.");
            }
            else
            {
                Console.WriteLine("Bouquets existants :");
                foreach (var bouquet in bouquets)
                {
                    Console.WriteLine($"Nom : {bouquet.Name} - Prix : {bouquet.CalculatePrice()}");
                    Console.WriteLine("Fleurs dans le bouquet :");
                    foreach (var flower in bouquet.Flowers)
                    {
                        Console.WriteLine($"- {flower.Name} ({flower.Color})");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
