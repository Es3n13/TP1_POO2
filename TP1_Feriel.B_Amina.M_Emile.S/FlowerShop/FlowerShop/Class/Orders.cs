﻿using FlowerShop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Class
{
    // Commande
    public class Order
    {
        public string OrderID { get; set; }
        public Client Client { get; set; }  // Référence à l'objet Client
        public Seller AssignedSeller { get; set; }  // Référence à l'objet Seller
        public List<Flower> Flowers { get; set; } = new List<Flower>();
        public List<Bouquet> Bouquets { get; set; } = new List<Bouquet>();
        public string Status { get; set; } // En attente, En cours, Complétée, Annulée
        public DateTime OrderDate { get; set; }
        public Invoice Invoice { get; set; } // Facture associée à la commande

        public Order(Client client, List<Flower> flowers, List<Bouquet> bouquets, Seller seller)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client), "Le client ne peut pas être null.");
            AssignedSeller = seller ?? throw new ArgumentNullException(nameof(seller), "Le vendeur ne peut pas être null.");
            OrderID = Guid.NewGuid().ToString().Substring(0, 5);
            Flowers = flowers ?? new List<Flower>();
            Bouquets = bouquets ?? new List<Bouquet>();
            OrderDate = DateTime.Now;
            Status = "En attente";
        }

        // Récupérer l'ID du client
        public string ClientID => Client.ID;

        // Récupérer l'ID du vendeur
        public string AssignedSellerID => AssignedSeller.ID;

        private Seller AssignSeller(List<Seller> sellers)
        {
            if (sellers == null || !sellers.Any())
            {
                Console.WriteLine("Aucun vendeur disponible pour être assigné !");
                return null;  // Évite un plantage
            }
            return sellers.OrderBy(s => s.AssignedOrders.Count).FirstOrDefault();  // Assigne au vendeur avec le moins de commandes
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            Console.WriteLine($"Commande {OrderID} est maintenant : {Status}");
        }

        public decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var flower in Flowers)
            {
                total += flower.GetPrice();
            }
            foreach (var bouquet in Bouquets)
            {
                total += bouquet.Price;
            }
            return total;
        }

        public void DisplayOrder()
        {
            Console.WriteLine($"Commande ID : {OrderID}");
            Console.WriteLine($"Client : {Client.Name} (ID: {ClientID})");
            Console.WriteLine($"Vendeur ID : {AssignedSellerID}"); 
            Console.WriteLine($"Statut : {Status}");

            Console.WriteLine("Fleurs commandées :");
            foreach (var flower in Flowers)
            {
                Console.WriteLine($"- {flower.Name} ({flower.Price:F2}$)");
            }

            Console.WriteLine("Bouquets commandés :");
            foreach (var bouquet in Bouquets)
            {
                Console.WriteLine($"- {bouquet.Name} ({bouquet.Price:F2}$)");
            }

            Console.WriteLine($"Total : {CalculateTotal()}$");
        }        
    }
}
