using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowerShop.Class;
using Json.Net;
using Newtonsoft.Json;

namespace FlowerShop.Data
{
    public static class OrderManager
    {
        // Sauvegarde des commandes dans un fichier JSON
        public static void SaveOrder(List<Order> orders, string OrdersPath)
        {
            List<OrderDTO> orderDTOs = orders.Select(order => new OrderDTO
            {
                OrderID = order.OrderID,
                ClientID = order.ClientID,
                AssignedSellerID = order.AssignedSellerID,
                Total = order.CalculateTotal(),
                OrderDate = order.OrderDate,
                Status = order.Status,
                Flowers = order.Flowers,
                Bouquets = order.Bouquets
            }).ToList();

            string json = JsonConvert.SerializeObject(orderDTOs, Formatting.Indented);

            // Vérification si le fichier existe, sinon, le créer
            if (!File.Exists(OrdersPath))
            {
                using (File.Create(OrdersPath)) { }
            }

            // Écriture du JSON dans un fichier
            File.WriteAllText(OrdersPath, json);
            Console.WriteLine("Données sauvegardées !");
        }

        // DTO pour la commande
        public class OrderDTO
        {
            public string OrderID { get; set; }
            public string ClientID { get; set; }
            public string AssignedSellerID { get; set; }
            public decimal Total { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }  // Ajout du statut
            public List<Flower> Flowers { get; set; } = new List<Flower>();
            public List<Bouquet> Bouquets { get; set; } = new List<Bouquet>();
        }

        // Chargement des commandes depuis un fichier JSON
        public static List<Order> LoadOrder(string OrdersPath, List<Client> clients, List<Seller> sellers)
        {
            if (!File.Exists(OrdersPath))
            {
                return new List<Order>();  // Si le fichier n'existe pas, on retourne une liste vide
            }

            var json = File.ReadAllText(OrdersPath);
            var orderDTOs = JsonConvert.DeserializeObject<List<OrderDTO>>(json) ?? new List<OrderDTO>();

            var orders = new List<Order>();



            foreach (var orderDTO in orderDTOs)
            {
                var client = clients.FirstOrDefault(c => c.ID == orderDTO.ClientID);
                var seller = sellers.FirstOrDefault(s => s.ID == orderDTO.AssignedSellerID);



                if (client == null)
                {
                    Console.WriteLine($"Client avec ID {orderDTO.ClientID} non trouvé pour la commande {orderDTO.OrderID}. La commande sera ignorée.");
                    continue;
                }

                if (seller == null)
                {
                    Console.WriteLine($"Vendeur avec ID {orderDTO.AssignedSellerID} non trouvé pour la commande {orderDTO.OrderID}. La commande sera ignorée.");
                    continue;
                }

                var order = new Order(client, orderDTO.Flowers, orderDTO.Bouquets, seller)
                {
                    OrderID = orderDTO.OrderID,
                    Status = orderDTO.Status,
                    OrderDate = orderDTO.OrderDate
                };

                orders.Add(order);
            }

            return orders;
        }

        public static void AddOrder(Order order, List<Order> orders, string OrdersPath)
        {
            // Ajouter la commande à la liste
            orders.Add(order);

            // Sauvegarder les commandes dans le fichier
            SaveOrder(orders, OrdersPath);
        }
    }
}
