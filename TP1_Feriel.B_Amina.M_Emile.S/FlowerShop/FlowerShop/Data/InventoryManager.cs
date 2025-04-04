using FlowerShop.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Data
{
    public static class InventoryManager
    {
        private static string inventoryPath = Path.Combine(Environment.CurrentDirectory, "Inventory.json");

        public static Inventory LoadInventory(List<Flower> availableFlowers)
        {
            if (!File.Exists(inventoryPath))
            {
                Console.WriteLine("Le fichier Inventory.json n'existe pas. Initialisation du stock...");

                Inventory newInventory = new Inventory();

                // Ajouter 500 unités de chaque fleur disponible
                foreach (var flower in availableFlowers)
                {
                    newInventory.AddStock(flower.Name, 500);
                }

                SaveInventory(newInventory);
                return newInventory;
            }

            try
            {
                string json = File.ReadAllText(inventoryPath);
                return JsonConvert.DeserializeObject<Inventory>(json) ?? new Inventory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'inventaire : {ex.Message}");
                return new Inventory();
            }
        }

        public static void SaveInventory(Inventory inventory)
        {
            try
            {
                string json = JsonConvert.SerializeObject(inventory, Formatting.Indented);
                File.WriteAllText(inventoryPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de l'inventaire : {ex.Message}");
            }
        }
    }
}
