using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowerShop.Class;
using FlowerShop.Class;

namespace FlowerShop.Data
{
    public static class InvoiceManager
    {
        // Méthode pour générer une facture
        public static void GenerateInvoice(Order order, List<Invoice> invoices, string InvoicesPath)
        {
            if (order.Status == "Complétée") // Générer une facture seulement si la commande est complétée
            {
                // Créer la facture
                var invoice = new Invoice(order.OrderID, order.ClientID, order.CalculateTotal());

                // Ajouter la facture à la liste
                invoices.Add(invoice);

                // Sauvegarder les factures dans le fichier JSON
                SaveInvoices(invoices, InvoicesPath);
            }
        }

        // Méthode pour charger les factures depuis un fichier JSON
        public static List<Invoice> LoadInvoices(string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<Invoice>>(json) ?? new List<Invoice>();
            }
            return new List<Invoice>();
        }

        // Méthode pour sauvegarder les factures dans un fichier JSON
        public static void SaveInvoices(List<Invoice> invoices, string path)
        {
            var json = JsonConvert.SerializeObject(invoices, Formatting.Indented);
            File.WriteAllText(path, json);
            Console.WriteLine("Factures sauvegardées !");
        }
    }
}
