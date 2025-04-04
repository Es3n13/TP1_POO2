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
        //Chemin relatif vers le fichier de sauvegarde des factures
        private static string invoicesPath = Path.Combine(Environment.CurrentDirectory, "Invoices.json");
        // Méthode pour générer une facture
        public static void GenerateInvoice(Order order, List<Invoice> invoices, string invoicesPath)
        {
            try
            {
                // Vérifier si la facture existe déjà pour éviter les doublons
                if (invoices.Any(inv => inv.OrderID == order.OrderID))
                {
                    Console.WriteLine("Une facture existe déjà pour cette commande.");
                    return;
                }

                // Construire la liste des articles achetés
                List<InvoiceItem> invoiceItems = new List<InvoiceItem>();

                // Ajouter les fleurs commandées
                foreach (var flower in order.Flowers)
                {
                    float price = float.Parse(flower.Price);
                    invoiceItems.Add(new InvoiceItem(flower.Name, price));
                }

                // Ajouter les bouquets commandés
                foreach (var bouquet in order.Bouquets)
                {
                    float price = (float)bouquet.Price;
                    invoiceItems.Add(new InvoiceItem(bouquet.Name, price));
                }

                // Créer une nouvelle facture avec les articles
                Invoice newInvoice = new Invoice(order.OrderID, order.Client.ID, order.CalculateTotal(), invoiceItems);
                invoices.Add(newInvoice);

                // Sérialiser et sauvegarder
                string json = JsonConvert.SerializeObject(invoices, Formatting.Indented);
                File.WriteAllText(invoicesPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de la facture : {ex.Message}");
            }
        }

        // Méthode pour charger les factures depuis un fichier JSON
        public static List<Invoice> LoadInvoices()
        {
            if (!File.Exists(invoicesPath))
            {
                Console.WriteLine("Le fichier Invoices.json n'existe pas. Création...");
                File.WriteAllText(invoicesPath, "[]"); // Création d'un fichier JSON vide
                return new List<Invoice>();
            }

            try
            {
                string json = File.ReadAllText(invoicesPath);
                return JsonConvert.DeserializeObject<List<Invoice>>(json) ?? new List<Invoice>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des factures : {ex.Message}");
                return new List<Invoice>();
            }
        }

    }
}
