using FlowerShop.Data;
using Json.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Class
{

    //Generic users
    public abstract class Users
    {
        // Attributs
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string ID { get; private set; }
        public string Role { get; private set; }
        public string Password { get; private set; }

        private static int IdGenerator = 1; // Génération automatique des IDs

        // Constructeur
        public Users(string name, string email, string id, string role, string password)
        {
            Name = name;
            Email = email;
            ID = id ?? GenerateUniqueID();
            Role = role;
            Password = password;
        }

        // Génère un ID unique basé sur les utilisateurs existants
        private static string GenerateUniqueID()
        {
            var existingUsers = UserManager.LoadUsers();
            if (existingUsers.Count > 0)
            {
                var maxId = existingUsers
                    .Select(u => int.TryParse(u.ID.Substring(1), out int num) ? num : 0)
                    .Max();

                IdGenerator = maxId + 1;
            }
            return $"U{IdGenerator++:D3}";
        }

        public abstract void AfficherRole();

    }

    //Employé(Vendeurs et Propriétaires)
    public abstract class Staff : Users
    {
        // Constructeur
        protected Staff(string name, string email, string? id, string role, string password): base(name, email, id, role, password) { }

        // Méthodes communes

        //Créer un bouquet
        public static void CreateBouquet()
        {
            Console.Write("Nom du bouquet : ");
            string name = Console.ReadLine();

            //Charge la liste de fleurs
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
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
                Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Color}) - ({availableFlowers[i].Description}) - {availableFlowers[i].Price:F2}$");
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

            BouquetManager.AddBouquet(newBouquet);
            Console.WriteLine($"Bouquet '{name}' ajouté avec succès !");
        }

        public static void DisplayBouquets()
        {
            List<Bouquet> availableBouquets = BouquetManager.LoadBouquets();
            if (availableBouquets.Count == 0)
            {
                Console.WriteLine("Aucun bouquet n'a été créé pour le moment.");
            }
            else
            {
                Console.WriteLine("Bouquets existants :");
                foreach (var bouquet in availableBouquets)
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
        }
    }

    // Fournisseurs
    public class Supplier : Users
    {
        // Attributs
        public string CompanyName { get; private set; }

        // Constructeur
        public Supplier(string name, string email, string? id, string password, string companyName)
            : base(name, email, id, "Supplier", password)
        {
            CompanyName = companyName;
        }

        // Méthodes
        public override void AfficherRole()
        {
            Console.WriteLine($"{Name} est un fournisseur ({CompanyName})");
        }

        public void DisplayAvailableFlowers()
        {

            string CsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
            List<Flower> availableFlowers = FlowerManager.LoadFlowersFromCSV(CsvPath);

            if (availableFlowers != null && availableFlowers.Count > 0)
            {
                Console.WriteLine($"Fleurs disponibles par {CompanyName} :");
                for (int i = 0; i < availableFlowers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Price:F2}$)");
                }
            }
            else
            {
                Console.WriteLine($"Aucune fleur disponible pour {CompanyName}.");
            }
        }

        //méthode pour afficher l'inventaire
        public void DisplayStock(Inventory inventory)
        {
            Console.WriteLine("\n=== Inventaire du magasin ===");
            foreach (var item in inventory.Stock)
            {
                Console.WriteLine($"{item.Key}: {item.Value} en stock");
            }
        }

        //méthode pour ajouter des fleurs à l'inventaire
        public void AddStock(Inventory inventory, List<Flower> availableFlowers)
        {
            Console.WriteLine("\n=== Ajouter du stock ===");
            Console.WriteLine("Voici la liste des fleurs disponibles :");

            // Affiche les fleurs disponibles
            for (int i = 0; i < availableFlowers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Price:F2}$)");
            }

            Console.Write("\nEntrez le numéro de la fleur à ajouter : ");
            if (int.TryParse(Console.ReadLine(), out int flowerIndex) && flowerIndex > 0 && flowerIndex <= availableFlowers.Count)
            {
                Flower selectedFlower = availableFlowers[flowerIndex - 1];
                Console.Write($"Entrez la quantité de {selectedFlower.Name} à ajouter : ");

                if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                {
                    inventory.AddStock(selectedFlower.Name, quantity);
                    Console.WriteLine($"{quantity} unités de {selectedFlower.Name} ont été ajoutées à l'inventaire.");
                }
                else
                {
                    Console.WriteLine("Quantité invalide.");
                }
            }
            else
            {
                Console.WriteLine("Sélection invalide.");
            }
        }

        // Menu
        public void AfficherMenu(Inventory inventory)
        {
            bool activeMenu = true;
            while (activeMenu)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Fournisseur : {Name} ({CompanyName}) ===");
                Console.WriteLine("1. Consulter les fleurs disponibles");
                Console.WriteLine("2. Consulter l'inventaire");
                Console.WriteLine("3. Ajouter des fleurs à l'inventaire");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayAvailableFlowers();
                        break;
                    case "2":
                        DisplayStock(inventory);
                        break;
                    case "3":
                        List<Flower> availableFlowers = FlowerManager.LoadFlowersFromCSV(Path.Combine(Environment.CurrentDirectory, "fleurs_db.csv"));
                        AddStock(inventory, availableFlowers);
                        break;
                    case "0":
                        activeMenu = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (activeMenu && choice != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu.");
                    Console.ReadKey();
                }
            }
        }
    }

    //Clients
    public class Client : Users
    {
        //Attributs
        public Client(string name, string email, string? id, string password) : base(name, email, id, "Client", password) { }

        //Méthode
        public override void AfficherRole()
        {
            Console.WriteLine($"Client : {Name}, {Email} {ID}");
        }

        public static void CreateBouquet()
        {
            Console.Write("Nom du bouquet : ");
            string name = Console.ReadLine();

            //Charge la liste de fleurs
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
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
                Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Color}) - ({availableFlowers[i].Description}) - {availableFlowers[i].Price:F2}$");
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

            BouquetManager.AddBouquet(newBouquet);
            Console.WriteLine($"Bouquet '{name}' ajouté avec succès !");
        }
        public static void DisplayBouquets()
        {
            List<Bouquet> availableBouquets = BouquetManager.LoadBouquets();
            if (availableBouquets.Count == 0)
            {
                Console.WriteLine("Aucun bouquet n'a été créé pour le moment.");
            }
            else
            {
                Console.WriteLine("Bouquets existants :");
                foreach (var bouquet in availableBouquets)
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
        }

        // Fonction pour entrer une commande
        public void PlaceOrder(List<Seller> sellers, string OrdersPath)
        {
            List<Flower> selectedFlowers = new List<Flower>();
            List<Bouquet> selectedBouquets = new List<Bouquet>();

            // Charger les fleurs disponibles
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
            List<Flower> availableFlowers = FlowerManager.LoadFlowersFromCSV(csvPath);

            // Charger les bouquets disponibles
            List<Bouquet> availableBouquets = BouquetManager.LoadBouquets();

            // Sélection des fleurs individuelles
            Console.WriteLine("Voulez-vous ajouter des fleurs individuelles à votre commande ? (o/n)");
            if (Console.ReadLine().ToLower() == "o")
            {
                // Affichage des fleurs disponibles
                Console.WriteLine("\nListe des fleurs disponibles :");
                for (int i = 0; i < availableFlowers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableFlowers[i].Name} ({availableFlowers[i].Color}) - ({availableFlowers[i].Description}) - {availableFlowers[i].Price:F2}$");
                }

                bool addingFlowers = true;
                while (addingFlowers)
                {
                    Console.WriteLine("Entrez le numéro de la fleur à ajouter (ou 0 pour terminer) :");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int flowerIndex) && flowerIndex > 0 && flowerIndex <= availableFlowers.Count)
                    {
                        Flower selectedFlower = availableFlowers[flowerIndex - 1];

                        Console.Write($"Combien de {selectedFlower.Name} souhaitez-vous ajouter ? ");
                        string quantityInput = Console.ReadLine();

                        if (int.TryParse(quantityInput, out int quantity) && quantity > 0)
                        {
                            for (int i = 0; i < quantity; i++)
                            {
                                selectedFlowers.Add(selectedFlower);
                            }
                            Console.WriteLine($"{quantity} fleurs {selectedFlower.Name} ajoutées à la commande.");
                        }
                        else
                        {
                            Console.WriteLine("Quantité invalide, veuillez entrer un nombre positif.");
                        }
                    }
                    else if (flowerIndex == 0)
                    {
                        addingFlowers = false;
                    }
                    else
                    {
                        Console.WriteLine("Sélection invalide, veuillez entrer un numéro valide.");
                    }
                }
            }

            // Sélection des bouquets existants
            Console.WriteLine("Voulez-vous sélectionner un bouquet existant ? (o/n)");
            if (Console.ReadLine().ToLower() == "o")
            {
                Console.WriteLine("Liste des bouquets disponibles :");
                for (int i = 0; i < availableBouquets.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableBouquets[i].Name} - {availableBouquets[i].Price:F2}$");
                }

                bool addingBouquets = true;
                while (addingBouquets)
                {
                    Console.WriteLine("Entrez le numéro du bouquet à ajouter (ou 0 pour terminer) :");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int bouquetIndex) && bouquetIndex > 0 && bouquetIndex <= availableBouquets.Count)
                    {
                        Bouquet selectedBouquet = availableBouquets[bouquetIndex - 1];
                        selectedBouquets.Add(selectedBouquet);
                        Console.WriteLine($"Bouquet {selectedBouquet.Name} ajouté à la commande.");
                    }
                    else if (bouquetIndex == 0)
                    {
                        addingBouquets = false;
                    }
                    else
                    {
                        Console.WriteLine("Sélection invalide, veuillez entrer un numéro valide.");
                    }
                }
            }

            // Vérification si la commande est vide
            if (selectedFlowers.Count == 0 && selectedBouquets.Count == 0)
            {
                Console.WriteLine("Votre commande est vide !");
                return;
            }

            // Charger les utilisateurs pour récupérer les clients et les vendeurs
            List<Users> users = UserManager.LoadUsers();
            List<Client> clients = UserManager.GetClients(users);
            List<Seller> availableSellers = UserManager.GetSellers(users);

            // Vérifier si le client existe
            Client currentClient = clients.FirstOrDefault(c => c.ID == this.ID);
            if (currentClient == null)
            {
                Console.WriteLine("Erreur : Impossible de récupérer le client actuel !");
                return;
            }

            // Vérifier qu'il y a un vendeur disponible
            Seller selectedSeller = availableSellers.FirstOrDefault();
            if (selectedSeller == null)
            {
                Console.WriteLine("Erreur : Aucun vendeur disponible pour traiter la commande !");
                return;
            }

            // Créer la commande avec un seul vendeur et un client valide
            Order order = new Order(currentClient, selectedFlowers, selectedBouquets, selectedSeller);

            // Sauvegarder la commande dans le fichier JSON
            List<Order> allOrders = OrderManager.LoadOrder(OrdersPath, UserManager.GetClients(users), availableSellers);
            OrderManager.AddOrder(order, allOrders, OrdersPath);

            Console.WriteLine($"Commande {order.OrderID} passée et assignée au vendeur {order.AssignedSellerID ?? "Aucun vendeur disponible"}");
        }

        public List<Order> GetClientOrders(Client client, List<Order> orders)
        {
            return orders.Where(order => order.ClientID == client.ID).ToList();
        }

        //Fonction pour que le client puisse voir ses commandes et les annuler si possible
        public void ViewOrders(Client currentClient, List<Client> clients, List<Seller> sellers)
        {
            string ordersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");

            // Charger les commandes existantes
            List<Order> orders = OrderManager.LoadOrder(ordersPath, clients, sellers);

            // Récupérer les commandes du client
            List<Order> clientOrders = GetClientOrders(currentClient, orders);

            if (clientOrders.Count == 0)
            {
                Console.WriteLine("Aucune commande trouvée pour ce client.");
                Console.ReadKey();
                return;
            }

            // Afficher toutes les commandes du client
            Console.WriteLine($"=== Commandes de {currentClient.Name} ===");
            foreach (var order in clientOrders)
            {
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status}");
            }

            // Demander au client de sélectionner une commande
            Console.Write("\nEntrez l'ID de la commande à consulter (ou '0' pour revenir au menu) : ");
            var selectedOrderId = Console.ReadLine();

            if (selectedOrderId == "0")
            {
                return;
            }

            // Trouver la commande sélectionnée par ID
            var selectedOrder = clientOrders.FirstOrDefault(o => o.OrderID == selectedOrderId);

            if (selectedOrder != null)
            {
                bool orderMenu = true;
                while (orderMenu)
                {
                    Console.Clear();

                    // Afficher les détails de la commande
                    Console.WriteLine("\n=== Détails de la commande ===");
                    selectedOrder.DisplayOrder();

                    // Afficher les options disponibles
                    Console.WriteLine("\nQue voulez-vous faire ?");
                    Console.WriteLine("1. Annuler la commande (si possible)");
                    Console.WriteLine("0. Retour à la liste des commandes");

                    Console.Write("\nVotre choix : ");
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            if (selectedOrder.Status == "En attente")
                            {
                                selectedOrder.UpdateStatus("Annulée");
                                OrderManager.SaveOrder(orders, ordersPath);
                                Console.WriteLine("La commande a été annulée.");
                            }
                            else
                            {
                                Console.WriteLine("Impossible d'annuler une commande déjà traitée.");
                            }
                            Console.WriteLine("Appuyez sur une touche pour continuer...");
                            Console.ReadKey();
                            orderMenu = false;
                            break;

                        case "0":
                            orderMenu = false;
                            break;

                        default:
                            Console.WriteLine("Option invalide. Appuyez sur une touche pour réessayer.");
                            Console.ReadKey();
                            break;
                    }
                }

                ViewOrders(currentClient, clients, sellers);
            }
            else
            {
                Console.WriteLine("Commande non trouvée. Appuyez sur une touche pour réessayer.");
                Console.ReadKey();
                ViewOrders(currentClient, clients, sellers);
            }
        }

        public void PayForOrder(Client currentClient)
        {
            string ordersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");
            string invoicesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices.json");

            List<Users> users = UserManager.LoadUsers();
            List<Client> clients = UserManager.GetClients(users);
            List<Seller> sellers = UserManager.GetSellers(users);
            List<Order> orders = OrderManager.LoadOrder(ordersPath, clients, sellers);
            List<Invoice> invoices = InvoiceManager.LoadInvoices();

            var clientOrders = orders.Where(o => o.Client.ID == currentClient.ID && o.Status == "Complétée").ToList();

            if (clientOrders.Count == 0)
            {
                Console.WriteLine("Aucune commande confirmée disponible pour le paiement.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Liste des commandes confirmées :");
            foreach (var order in clientOrders)
            {
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status} - Montant : {order.CalculateTotal():F2}$");
            }

            Console.Write("\nEntrez l'ID de la commande que vous souhaitez payer : ");
            string selectedOrderId = Console.ReadLine();

            var selectedOrder = clientOrders.FirstOrDefault(o => o.OrderID == selectedOrderId);

            if (selectedOrder != null)
            {
                Console.WriteLine("\nChoisissez le mode de paiement :");
                Console.WriteLine("1. Carte de débit");
                Console.WriteLine("2. Carte de crédit");
                Console.WriteLine("3. Espèces");
                Console.Write("\nVotre choix : ");
                var paymentChoice = Console.ReadLine();

                string paymentMethod = paymentChoice switch
                {
                    "1" => "Carte de débit",
                    "2" => "Carte de crédit",
                    "3" => "Espèces",
                    _ => "Inconnu"
                };

                if (paymentMethod == "Inconnu")
                {
                    Console.WriteLine("Méthode de paiement invalide. Paiement annulé.");
                    Console.ReadKey();
                    return;
                }

                // Mettre à jour le statut de la commande
                selectedOrder.UpdateStatus("Payée");
                OrderManager.SaveOrder(orders, ordersPath);

                // Générer une facture
                InvoiceManager.GenerateInvoice(selectedOrder, invoices, invoicesPath);

                Console.WriteLine("\nPaiement réussi !");
                Console.WriteLine($"Une facture a été générée pour la commande {selectedOrder.OrderID}.");
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Commande non trouvée. Appuyez sur une touche pour revenir à la liste des commandes.");
                Console.ReadKey();
            }
        }

        public void DisplayClientInvoices(Client client, string invoicesPath)
        {
            // Charger les factures existantes
            List<Invoice> invoices = InvoiceManager.LoadInvoices();

            // Récupérer les factures du client
            var clientInvoices = invoices.Where(invoice => invoice.ClientID == client.ID).ToList();

            if (clientInvoices.Count == 0)
            {
                Console.WriteLine($"Aucune facture trouvée pour le client {client.Name}.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"=== Factures de {client.Name} ===");

            // Réutilisation de DisplayInvoice pour afficher les factures du client
            Invoice.DisplayInvoice(clientInvoices);
        }

        // Méthode pour afficher le menu spécifique au client
        public void AfficherMenu(List<Seller> sellers)
        {
            bool activeMenu = true;
            while (activeMenu)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Client : {Name} ({ID}) ===");
                Console.WriteLine("1. Créer un bouquet");
                Console.WriteLine("2. Consulter la liste des bouquets");
                Console.WriteLine("3. Passer une commande");
                Console.WriteLine("4. Voir mes commandes");
                Console.WriteLine("5. Payer une commande");
                Console.WriteLine("6. Voir mes factures");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateBouquet();
                        break;
                    case "2":
                        DisplayBouquets();
                        break;
                    case "3":
                        PlaceOrder(sellers, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json"));
                        break;
                    case "4":
                        ViewOrders(this, UserManager.GetClients(UserManager.LoadUsers()), UserManager.GetSellers(UserManager.LoadUsers()));
                        break;
                    case "5":
                        PayForOrder(this);
                        break;
                    case "6":
                        DisplayClientInvoices(this, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices.json"));
                        break;
                    case "0":
                        activeMenu = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (activeMenu && choice != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu.");
                    Console.ReadKey();
                }
            }
        }
    }

    //Vendeurs
    public class Seller : Staff
    {
        //Attributs
        public List<Order> AssignedOrders { get; set; } = new List<Order>();
        public Seller(string name, string email, string? id, string password) : base(name, email, id, "Seller", password) { }

        //Méthodes
        public override void AfficherRole()
        {
            Console.WriteLine($"Vendeur : {Name}, {Email} {ID}");
        }

        //Récupérer les commandes attribuées au vendeur
        public List<Order> GetAssignedOrders(List<Order> orders)
        {
            Console.WriteLine($"Recherche des commandes pour le vendeur ID: {this.ID}");

            var assignedOrders = orders.Where(o => o.AssignedSellerID == this.ID).ToList();

            Console.WriteLine($"Nombre de commandes trouvées: {assignedOrders.Count}");

            return assignedOrders;
        }

        //Fonction pour traiter une commande
        public void ProcessOrder()
        {
            string ordersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");
            string inventoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Inventory.json");
            string CsvPath = Path.Combine(Environment.CurrentDirectory, "fleurs_db.csv");

            // Charger les utilisateurs pour récupérer les clients et les vendeurs
            List<Users> users = UserManager.LoadUsers();
            List<Client> clients = UserManager.GetClients(users);
            List<Seller> sellers = UserManager.GetSellers(users);

            // Charger les commandes existantes
            List<Order> orders = OrderManager.LoadOrder(ordersPath, clients, sellers);

            // Charger l'inventaire
            Inventory inventory = InventoryManager.LoadInventory(FlowerManager.LoadFlowersFromCSV(CsvPath));

            // Récupérer les commandes assignées au vendeur actuel
            var assignedOrders = GetAssignedOrders(orders);

            if (assignedOrders.Count == 0)
            {
                Console.WriteLine("Aucune commande assignée à ce vendeur.");
                return;
            }

            Console.Clear();
            Console.WriteLine("Liste des commandes assignées à ce vendeur :");
            foreach (var order in assignedOrders)
            {
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status}");
            }

            Console.Write("\nEntrez l'ID de la commande à traiter (ou '0' pour revenir au menu) : ");
            var selectedOrderId = Console.ReadLine();

            if (selectedOrderId == "0")
            {
                return;
            }

            var selectedOrder = assignedOrders.FirstOrDefault(o => o.OrderID == selectedOrderId);

            if (selectedOrder != null)
            {
                bool orderMenu = true;
                while (orderMenu)
                {
                    Console.Clear();

                    Console.WriteLine("\n=== Détails de la commande ===");
                    selectedOrder.DisplayOrder();

                    Console.WriteLine("\nQue voulez-vous faire ?");
                    Console.WriteLine("1. Valider la commande");
                    Console.WriteLine("2. Annuler la commande");
                    Console.WriteLine("0. Retour à la liste des commandes");

                    Console.Write("\nVotre choix : ");
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            selectedOrder.UpdateStatus("Complétée");

                            // Regrouper et compter les fleurs individuelles
                            var flowerCounts = selectedOrder.Flowers
                                .GroupBy(f => f.Name)
                                .ToDictionary(g => g.Key, g => g.Count());

                            // Ajouter les fleurs contenues dans les bouquets
                            foreach (var bouquet in selectedOrder.Bouquets)
                            {
                                foreach (var flower in bouquet.Flowers)
                                {
                                    if (flowerCounts.ContainsKey(flower.Name))
                                        flowerCounts[flower.Name] += 1; // Incrémente le nombre de cette fleur
                                    else
                                        flowerCounts[flower.Name] = 1; // Ajoute cette fleur avec une quantité de 1
                                }
                            }

                            // Vérifier et retirer les fleurs du stock
                            foreach (var flower in flowerCounts)
                            {
                                if (!inventory.RemoveStock(flower.Key, flower.Value))
                                {
                                    Console.WriteLine($"Stock insuffisant pour {flower.Key}. Commande non validée.");
                                    Console.ReadKey();
                                    return;
                                }
                            }

                            // Sauvegarder les mises à jour
                            OrderManager.SaveOrder(orders, ordersPath);
                            InventoryManager.SaveInventory(inventory);

                            Console.WriteLine("La commande a été validée et le stock a été mis à jour.");
                            Console.WriteLine("Appuyez sur une touche pour continuer...");
                            Console.ReadKey();
                            orderMenu = false;
                            break;

                        case "2":
                            selectedOrder.UpdateStatus("Annulée");
                            OrderManager.SaveOrder(orders, ordersPath);
                            Console.WriteLine("La commande a été annulée.");
                            Console.WriteLine("Appuyez sur une touche pour continuer...");
                            Console.ReadKey();
                            orderMenu = false;
                            break;

                        case "0":
                            orderMenu = false;
                            break;

                        default:
                            Console.WriteLine("Option invalide. Appuyez sur une touche pour réessayer.");
                            Console.ReadKey();
                            break;
                    }
                }

                ProcessOrder();
            }
            else
            {
                Console.WriteLine("Commande non trouvée. Appuyez sur une touche pour réessayer.");
                Console.ReadKey();
                ProcessOrder();
            }
        }
        public void AfficherMenu()
        {
            bool activeMenu = true;
            while (activeMenu)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Vendeur : {Name} ({ID}) ===");
                Console.WriteLine("1. Créer un bouquet");
                Console.WriteLine("2. Traiter une commande");
                Console.WriteLine("3. Consulter la liste des bouquets");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateBouquet();
                        break;
                    case "2":
                        ProcessOrder();
                        break;
                    case "3":
                        DisplayBouquets();
                        break;
                    case "0":
                        activeMenu = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (activeMenu && choice != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu.");
                    Console.ReadKey();
                }
            }
        }
    }

    // Propriétaires
    public class Owner : Staff
    {
        public Owner(string name, string email, string? id, string password) : base(name, email, id, "Owner", password) { }

        //Méthodes

        // Afficher le rôle
        public override void AfficherRole()
        {
            Console.WriteLine($"Propriétaire : {Name}, {Email}, ID: {ID}");
        }

        // Afficher tous les vendeurs
        public void ManageSellers(List<Users> users)
        {
            var sellers = UserManager.GetSellers(users);
            Console.WriteLine($"Liste des vendeurs gérée par {Name} :");
            foreach (var seller in sellers)
            {
                Console.WriteLine($"- {seller.Name}, {seller.Email} (ID: {seller.ID})");
            }
        }

        // Ajouter un vendeur
        public Seller AddSeller(List<Seller> sellers, string name, string email, string password)
        {
            Seller newSeller = new Seller(name, email, null, password);
            sellers.Add(newSeller);
            Console.WriteLine($"Vendeur {name} ajouté par {Name}.");
            UserManager.SaveUser(sellers.Cast<Users>().ToList());  // Change les vendeurs en type Users pour être sauvegardés

            return newSeller;
        }

        // Ajouter un client
        public Client AddClient(List<Client> clients, string name, string email, string password)
        {
            Client newClient = new Client(name, email, null, password);
            clients.Add(newClient);
            Console.WriteLine($"Client {name} ajouté par {Name}.");
            UserManager.SaveUser(clients.Cast<Users>().ToList());  // Change les clients en type Users pour être sauvegardés

            return newClient;
        }

        // Afficher tous les clients
        public void ManageClients(List<Users> users)
        {
            var clients = UserManager.GetClients(users);
            Console.WriteLine($"Liste des clients gérée par {Name} :");
            foreach (var client in clients)
            {
                Console.WriteLine($"- {client.Name}, {client.Email} (ID: {client.ID})");
            }
        }

        // Ajouter un autre propriétaire
        public Owner AddOwner(List<Owner> owners, string name, string email, string password)
        {
            Owner newOwner = new Owner(name, email, null, password);
            owners.Add(newOwner);
            Console.WriteLine($"Propriétaire {name} ajouté par {Name}.");
            UserManager.SaveUser(owners.Cast<Users>().ToList());  // Change les propriétaires en type Users pour être sauvegardés

            return newOwner;
        }

        // Afficher tous les propriétaires
        public void ManageOwners(List<Users> users)
        {
            var owners = UserManager.GetOwners(users);
            Console.WriteLine($"Liste des propriétaires gérée par {Name} :");
            foreach (var owner in owners)
            {
                Console.WriteLine($"- {owner.Name}, {owner.Email} (ID: {owner.ID})");
            }
        }

        // Afficher tous les fournisseurs
        public void ManageSuppliers(List<Users> users)
        {
            var suppliers = UserManager.GetSuppliers(users);
            Console.WriteLine($"Liste des fournisseurs gérée par {Name} :");
            foreach (var supplier in suppliers)
            {
                Console.WriteLine($"- {supplier.Name}, {supplier.Email} (ID: {supplier.ID}) - Entreprise : {((Supplier)supplier).CompanyName}");
            }
        }

        // Ajouter un fournisseur
        public Supplier AddSupplier(List<Supplier> suppliers, string name, string email, string password, string companyName)
        {
            Supplier newSupplier = new Supplier(name, email, null , password, companyName);
            suppliers.Add(newSupplier);
            Console.WriteLine($"Fournisseur {name} ajouté par {Name}.");
            UserManager.SaveUser(suppliers.Cast<Users>().ToList());

            return newSupplier;
        }

        //Pour retirer un utilsateur
        public static void RemoveUser(string id)
        {
            string UserDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData.json");

            List<Users> users = UserManager.LoadUsers();

            Users? userToRemove = users.Find(u => u.ID == id);
            if (userToRemove != null)
            {
                users.Remove(userToRemove);
                Console.WriteLine($"Utilisateur {userToRemove.Name} ({userToRemove.Role}) supprimé.");

                string json = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(UserDataPath, json);
                Console.WriteLine("Fichier JSON mis à jour avec succès.");
            }
            else
            {
                Console.WriteLine("Aucun utilisateur trouvé avec cet ID.");
            }
        }

        //Menu pour gérer les utilisateurs
        private void ManageUsers(List<Seller> sellers, List<Client> clients, List<Owner> owners, List<Supplier> suppliers)
        {
            bool subMenuActive = true;
            while (subMenuActive)
            {
                Console.Clear();
                Console.WriteLine("=== Gestion des Utilisateurs ===");
                Console.WriteLine("1. Consulter les vendeurs");
                Console.WriteLine("2. Ajouter un vendeur");
                Console.WriteLine("3. Supprimer un vendeur");
                Console.WriteLine("4. Consulter les clients");
                Console.WriteLine("5. Ajouter un client");
                Console.WriteLine("6. Supprimer un client");
                Console.WriteLine("7. Consulter les propriétaires");
                Console.WriteLine("8. Ajouter un propriétaire");
                Console.WriteLine("9. Supprimer un propriétaire");
                Console.WriteLine("10. Consulter les fournisseurs");
                Console.WriteLine("11. Ajouter un fournisseur");
                Console.WriteLine("12. Supprimer un fournisseur");
                Console.WriteLine("0. Retour");
                Console.Write("Choisissez une option : ");

                var choice = Console.ReadLine();

                List<Users> users = UserManager.LoadUsers();

                switch (choice)
                {
                    case "1":
                        ManageSellers(users);
                        break;
                    case "2":
                        Console.Write("Nom du vendeur : ");
                        var sellerName = Console.ReadLine();
                        Console.Write("Email du vendeur : ");
                        var sellerEmail = Console.ReadLine();
                        Console.Write("Sélectionner un mot de passe: ");
                        var sellerPassword = Console.ReadLine();
                        AddSeller(sellers, sellerName, sellerEmail, sellerPassword);
                        break;
                    case "3":
                        Console.Write("ID du vendeur à supprimer : ");
                        var sellerIdToRemove = Console.ReadLine();
                        RemoveUser(sellerIdToRemove);
                        break;
                    case "4":
                        ManageClients(users);
                        break;
                    case "5":
                        Console.Write("Nom du client : ");
                        var clientName = Console.ReadLine();
                        Console.Write("Email du client : ");
                        var clientEmail = Console.ReadLine();
                        Console.Write("Sélectionner un mot de passe: ");
                        var clientPassword = Console.ReadLine();
                        AddClient(clients, clientName, clientEmail, clientPassword);
                        break;
                    case "6":
                        Console.Write("ID du client à supprimer : ");
                        var clientIdToRemove = Console.ReadLine();
                        RemoveUser(clientIdToRemove);
                        break;
                    case "7":
                        ManageOwners(users);
                        break;
                    case "8":
                        Console.Write("Nom du propriétaire : ");
                        var ownerName = Console.ReadLine();
                        Console.Write("Email du propriétaire : ");
                        var ownerEmail = Console.ReadLine();
                        Console.Write("Sélectionner un mot de passe: ");
                        var ownerPassword = Console.ReadLine();
                        AddOwner(owners, ownerName, ownerEmail, ownerPassword);
                        break;
                    case "9":
                        Console.Write("ID du propriétaire à supprimer : ");
                        var ownerIdToRemove = Console.ReadLine();
                        RemoveUser(ownerIdToRemove);
                        break;
                    case "10":
                        ManageSuppliers(users);
                        break;
                    case "11":
                        Console.Write("Nom du fournisseur : ");
                        var supplierName = Console.ReadLine();
                        Console.Write("Email du fournisseur : ");
                        var supplierEmail = Console.ReadLine();
                        Console.Write("Nom de l'entreprise : ");
                        var companyName = Console.ReadLine();
                        Console.Write("Sélectionner un mot de passe: ");
                        var supplierPassword = Console.ReadLine();
                        AddSupplier(suppliers, supplierName, supplierEmail,supplierPassword, companyName);
                        break;
                    case "12":
                        Console.Write("ID du fournisseur à supprimer : ");
                        var supplierIdToRemove = Console.ReadLine();
                        RemoveUser(supplierIdToRemove);
                        break;
                    case "0":
                        subMenuActive = false;
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (subMenuActive && choice != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu de gestion des utilisateurs.");
                    Console.ReadKey();
                }
            }
        }

        // Afficher les commandes
        public void DisplayAllOrders(List<Order> orders)
        {
            Console.Clear();
            Console.WriteLine("=== Liste des Commandes ===");

            if (orders.Count == 0)
            {
                Console.WriteLine("Aucune commande à afficher.");
                Console.ReadKey();
                return;
            }

            // Afficher un résumé des commandes avec leur statut
            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status}");
            }

            // Demander à l'utilisateur de sélectionner une commande
            Console.Write("\nEntrez l'ID de la commande à consulter (ou '0' pour revenir) : ");
            var selectedOrderId = Console.ReadLine();

            if (selectedOrderId == "0")
            {
                return;  // Retour au menu précédent
            }

            // Trouver la commande sélectionnée
            var selectedOrder = orders.FirstOrDefault(o => o.OrderID == selectedOrderId);

            if (selectedOrder != null)
            {
                bool orderMenuActive = true;

                while (orderMenuActive)
                {
                    Console.Clear();

                    // Afficher les détails de la commande sélectionnée
                    Console.WriteLine("\n=== Détails de la commande ===");
                    selectedOrder.DisplayOrder();

                    // Afficher les options disponibles
                    Console.WriteLine("\nQue voulez-vous faire ?");
                    Console.WriteLine("1. Modifier le statut de la commande");
                    Console.WriteLine("0. Retour à la liste des commandes");

                    Console.Write("\nVotre choix : ");
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            // Modifier le statut de la commande
                            Console.WriteLine("Choisissez un nouveau statut :");
                            Console.WriteLine("1. En attente");
                            Console.WriteLine("2. Complétée");
                            Console.WriteLine("3. Payée");
                            Console.WriteLine("4. Annulée");
                            Console.Write("Votre choix : ");
                            var newStatus = Console.ReadLine();

                            // Validation du choix de statut
                            switch (newStatus)
                            {
                                case "1":
                                    selectedOrder.UpdateStatus("En attente");
                                    break;
                                case "2":
                                    selectedOrder.UpdateStatus("Complétée");
                                    break;
                                case "3":
                                    selectedOrder.UpdateStatus("Payée");
                                    break;
                                case "4":
                                    selectedOrder.UpdateStatus("Annulée");
                                    break;
                                default:
                                    Console.WriteLine("Statut invalide, veuillez réessayer.");
                                    continue; // Retourner au menu des options si le statut est invalide
                            }

                            // Sauvegarder les changements dans les commandes
                            string ordersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");
                            OrderManager.SaveOrder(orders, ordersPath);

                            Console.WriteLine("Le statut de la commande a été mis à jour.");
                            Console.ReadKey();
                            break;

                        case "0":
                            orderMenuActive = false;
                            break;

                        default:
                            Console.WriteLine("Option invalide. Appuyez sur une touche pour réessayer.");
                            Console.ReadKey();
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Commande non trouvée. Appuyez sur une touche pour réessayer.");
                Console.ReadKey();
            }
        }

        public void DisplayAllInvoices(List<Invoice> invoices)
        {
            Console.Clear();
            Console.WriteLine("=== Liste des Factures ===");

            if (invoices.Count == 0)
            {
                Console.WriteLine("Aucune facture à afficher.");
                Console.ReadKey();
                return;
            }

            // Afficher un résumé des factures
            for (int i = 0; i < invoices.Count; i++)
            {
                var invoice = invoices[i];
                Console.WriteLine($"Facture ID : {invoice.InvoiceID} - Commande ID : {invoice.OrderID}");
            }

            // Demander à l'utilisateur de sélectionner une facture
            Console.Write("\nEntrez l'ID de la facture à consulter (ou '0' pour revenir) : ");
            var selectedInvoiceId = Console.ReadLine();

            if (selectedInvoiceId == "0")
            {
                return;  // Retour au menu précédent
            }

            // Trouver la facture sélectionnée
            var selectedInvoice = invoices.FirstOrDefault(i => i.InvoiceID == selectedInvoiceId);

            if (selectedInvoice != null)
            {
                bool invoiceMenuActive = true;

                while (invoiceMenuActive)
                {
                    Console.Clear();

                    // Afficher les détails de la facture sélectionnée
                    Console.WriteLine("\n=== Détails de la Facture ===");
                    Invoice.DisplayInvoice(new List<Invoice> { selectedInvoice });

                    // Afficher les options disponibles
                    Console.WriteLine("\nQue voulez-vous faire ?");
                    Console.WriteLine("0. Retour à la liste des factures");

                    Console.Write("\nVotre choix : ");
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "0":
                            invoiceMenuActive = false;
                            break;

                        default:
                            Console.WriteLine("Option invalide. Appuyez sur une touche pour réessayer.");
                            Console.ReadKey();
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Facture non trouvée. Appuyez sur une touche pour réessayer.");
                Console.ReadKey();
            }
        }

        // Méthode pour afficher le menu et gérer les actions
        public void AfficherMenu(Inventory inventory,List<Seller> sellers, List<Client> clients, List<Owner> owners, List<Supplier> suppliers, List<Order> orders, List<Invoice> invoices)
        {
            bool activeMenu = true;
            while (activeMenu)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Propriétaire : {Name} ({ID}) ===");
                Console.WriteLine("1. Gérer les utilisateurs");
                Console.WriteLine("2. Consulter la liste des bouquets");
                Console.WriteLine("3. Créer un bouquet");
                Console.WriteLine("4. Gérer les commandes");
                Console.WriteLine("5. Consulter les factures");
                Console.WriteLine("6. Consulter l'inventaire");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ManageUsers(sellers, clients, owners, suppliers);
                        break;
                    case "2":
                        DisplayBouquets();
                        break;
                    case "3":
                        CreateBouquet();
                        break;
                    case "4":
                        DisplayAllOrders(orders);
                        break;
                    case "5":
                        DisplayAllInvoices(invoices);
                        break;
                    case "6":
                        inventory.DisplayStock();
                        break;
                    case "0":
                        activeMenu = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (activeMenu && choice != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu principal.");
                    Console.ReadKey();
                }
            }
        }
    }
}

