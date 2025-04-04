using FlowerShop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Class
{
    public class Inventory
    {
        public Dictionary<string, int> Stock { get; private set; }

        public Inventory()
        {
            Stock = new Dictionary<string, int>();
        }

        public void AddStock(string flowerName, int quantity)
        {
            if (Stock.ContainsKey(flowerName))
                Stock[flowerName] += quantity;
            else
                Stock[flowerName] = quantity;

            InventoryManager.SaveInventory(this); // Sauvegarde après ajout
        }

        public bool RemoveStock(string flowerName, int quantity)
        {
            if (Stock.ContainsKey(flowerName) && Stock[flowerName] >= quantity)
            {
                Stock[flowerName] -= quantity;
                InventoryManager.SaveInventory(this); // Sauvegarde après suppression
                return true;
            }
            return false; // Stock insuffisant
        }

        public void DisplayStock()
        {
            Console.WriteLine("\n=== Inventaire des fleurs ===");
            foreach (var item in Stock)
            {
                Console.WriteLine($"{item.Key}: {item.Value} en stock");
            }
        }
    }
}
