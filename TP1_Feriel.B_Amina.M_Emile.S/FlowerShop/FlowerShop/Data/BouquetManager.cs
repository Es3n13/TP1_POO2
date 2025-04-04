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
        private static string BouquetDataPath = Path.Combine(Environment.CurrentDirectory, "BouquetData.json");

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
       /* public static void CreateBouquet()
        {
            Console.Write("Nom du bouquet : ");
            string name = Console.ReadLine();

            string csvPath = Path.Combine(Environment.CurrentDirectory, "fleurs_db.csv");
            List<Flower> availableFlowers = FlowerManager.LoadFlowersFromCSV(csvPath);

            if (availableFlowers.Count == 0)
            {
                Console.WriteLine("Aucune fleur disponible pour créer un bouquet.");
                return;
            }

            List<(Flower Flower, int Quantity)> flowers = new List<(Flower, int)>();
            bool addingFlowers = true;

            // Affichage les fleurs disponibles
            Console.WriteLine("\nListe des fleurs disponibles :");
            for (int i = 0; i < availableFlowers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Color}) - ({availableFlowers[i].Description}) - {availableFlowers[i].Price:F2}");
            }

            //Boucle pour ajouter des fleurs au bouquet
            while (addingFlowers)
            {
                Console.Write("\nSélectionner une fleur en entrant son numéro (ou 0 pour terminer) : ");
                if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 0 || selection > availableFlowers.Count)
                {
                    Console.WriteLine("Numéro invalide, essayez encore.");
                    continue;
                }

                if (selection == 0)
                {
                    addingFlowers = false;
                    break;
                }

                Flower selectedFlower = availableFlowers[selection - 1];

                Console.Write($"Combien de {selectedFlower.Name} voulez-vous ajouter ? ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Quantité invalide. Essayez encore.");
                    continue;
                }

                flowers.Add((selectedFlower, quantity));
                Console.WriteLine($"{quantity} x {selectedFlower.Name} ajoutée(s) au bouquet.");
            }

            if (flowers.Count == 0)
            {
                Console.WriteLine("Aucune fleur sélectionnée. Annulation de la création du bouquet.");
                return;
            }

            // Créer le bouquet avec les fleurs sélectionnées et leurs quantités
            Bouquet newBouquet = new Bouquet(name);
            foreach (var (flower, quantity) in flowers)
            {
                for (int i = 0; i < quantity; i++)
                {
                    newBouquet.Flowers.Add(flower);
                }
            }

            AddBouquet(newBouquet);
            Console.WriteLine($"Bouquet '{name}' ajouté avec succès !");
        }*/

        //Afficher les bouquets existants
       /* public static void DisplayBouquets()
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
                    Console.WriteLine($"Nom : {bouquet.Name} - Prix : {bouquet.CalculatePrice()}0$");
                    Console.WriteLine("Fleurs dans le bouquet :");
                    foreach (var flower in bouquet.Flowers)
                    {
                        Console.WriteLine($"- {flower.Name} ({flower.Color})");
                    }
                    Console.WriteLine();
                }
            }
        }*/
    }
}
