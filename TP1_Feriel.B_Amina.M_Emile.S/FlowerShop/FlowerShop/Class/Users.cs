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
        //Attributs
        public string Name { get; protected set; }
        public string Email { get; protected set; }
        public string ID { get; protected set; }
        public string Role { get; protected set; }

        private static int IdGenerator = 1; //Génération automatique des IDs

        //Constructeur
        public Users(string name, string email, string? id, string role)
        {
            Name = name;
            Email = email;
            ID = id ?? GenerateUniqueID();
            Role = role;
        }

        //Méthode

        // Génère un ID unique basé sur les utilisateurs existants
        private static string GenerateUniqueID()
        {
            // Charger les utilisateurs existants
            var existingUsers = UserManager.LoadUsers();
            if (existingUsers.Count > 0)
            {
                // Trouver l'ID le plus élevé et l'incrémenter
                var maxId = existingUsers
                    .Select(u => int.TryParse(u.ID.Substring(1), out int num) ? num : 0)
                    .Max();

                IdGenerator = maxId + 1;
            }
            return $"U{IdGenerator++:D3}"; //Génère un ID sous forme U001, U002, U003..Modifier :D pour changer le nombre de décimale(s)
        }
        public abstract void AfficherRole();

    }

    //Employé(Vendeurs et Propriétaires)
    public abstract class Staff : Users
    {
        // Constructeur
        protected Staff(string name, string email, string? id = null, string role = "Staff") : base(name, email, id, role) { }

        // Méthodes communes
        public void CreateBouquet()
        {
            Console.WriteLine($"{Name} crée un bouquet.");
            BouquetManager.CreateBouquet();
        }
    }

    //Fournisseurs
    public class Supplier : Users
    {
        //Attributs
        public string CompanyName { get; set; }
        public List<Flower> AvailableFlowers { get; set; }

        //Constructeur
        public Supplier(string name, string email, string companyName, string? id = null)
            : base(name, email, id, "Supplier")
        {
            CompanyName = companyName;
            AvailableFlowers = new List<Flower>();
        }

        //Méhodes
        public override void AfficherRole()
        {
            Console.WriteLine($"{Name} est un fournisseur ({CompanyName})");
        }

        public void DisplayAvailableFlowers()
        {
            string CsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");

            List<Flower> loadedFlowers = FlowerManager.LoadFlowersFromCSV(CsvPath);

            if (loadedFlowers != null && loadedFlowers.Count > 0)
            {
                Console.WriteLine($"Fleurs disponibles par {CompanyName} :");
                foreach (var flower in loadedFlowers)
                {
                    Console.WriteLine($"- {flower.Name} ({flower.Price:C}$)");
                }
            }
            else
            {
                Console.WriteLine($"Aucune fleur disponible pour {CompanyName}.");
            }
        }

        //Menu
        public void AfficherMenu()
        {
            bool menuActive = true;
            while (menuActive)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Fournisseur : {Name} ({CompanyName}) ===");
                Console.WriteLine("1. Consulter les fleurs disponibles");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        DisplayAvailableFlowers();
                        break;
                    case "0":
                        menuActive = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (menuActive && choix != "0")
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
        public List<Order> Orders { get; set; } = new List<Order>();
        public Client(string name, string email, string? id = null) : base(name, email, id, "Client") { }

        //Méthode
        public override void AfficherRole()
        {
            Console.WriteLine($"Client : {Name}, {Email} {ID}");
        }

        public void CreateBouquet()
        {
            Console.WriteLine($"{Name} crée un bouquet.");
            BouquetManager.CreateBouquet();
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
                Console.WriteLine($"Nombre de fleurs chargées : {availableFlowers.Count}");
                FlowerManager.DisplayFlowers(availableFlowers);

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
                    Console.WriteLine($"{i + 1}. {availableBouquets[i].Name} - {availableBouquets[i].Price}€");
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
            List<Client> clients = UserManager.GetClients(users).Cast<Client>().ToList(); // Convertir  en Client
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
                order.DisplayOrder();
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        public void PayForOrder(Client currentClient)
        {
            string ordersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");
            List<Users> users = UserManager.LoadUsers();
            List<Client> clients = UserManager.GetClients(users);
            List<Seller> sellers = UserManager.GetSellers(users);
            List<Order> orders = OrderManager.LoadOrder(ordersPath, clients, sellers);

            var clientOrders = orders.Where(o => o.Client.ID == currentClient.ID && o.Status == "Complétée").ToList();

            if (clientOrders.Count == 0)
            {
                Console.WriteLine("Aucune commande confirmée disponible pour le paiement.");
                return;
            }

            Console.WriteLine("Liste des commandes confirmées :");
            foreach (var order in clientOrders)
            {
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status} - Montant : {order.CalculateTotal()}$");
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
                // Mettre à jour le statut de la commande
                selectedOrder.UpdateStatus("Payée");
                OrderManager.SaveOrder(orders, ordersPath);
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
            List<Invoice> invoices = InvoiceManager.LoadInvoices(invoicesPath);

            // Récupérer les factures du client
            var clientInvoices = invoices.Where(invoice => invoice.ClientID == client.ID).ToList();

            if (clientInvoices.Count == 0)
            {
                Console.WriteLine($"Aucune facture trouvée pour le client {client.Name}.");
                Console.ReadKey();
                return;
            }

            // Afficher la liste des factures
            Console.WriteLine($"=== Factures de {client.Name} ===");
            foreach (var invoice in clientInvoices)
            {
                Console.WriteLine($"Facture ID: {invoice.InvoiceID},Date:{invoice.IssueDate}, Montant: {invoice.TotalAmount:C} ");
            }
            Console.ReadKey();
        }

        // Méthode pour afficher le menu spécifique au client
        public void AfficherMenu(List<Seller> sellers)
        {
            bool menuActive = true;
            while (menuActive)
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

                var choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        CreateBouquet();
                        break;
                    case "2":
                        BouquetManager.DisplayBouquets();
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
                        menuActive = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (menuActive && choix != "0")
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
        public Seller(string name, string email, string? id = null) : base(name, email, id, "Seller") { }

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

            // Charger les utilisateurs pour récupérer les clients et les vendeurs
            List<Users> users = UserManager.LoadUsers();
            List<Client> clients = UserManager.GetClients(users);
            List<Seller> sellers = UserManager.GetSellers(users);

            // Charger les commandes existantes
            List<Order> orders = OrderManager.LoadOrder(ordersPath, clients, sellers);

            // Récupérer les commandes assignées au vendeur actuel
            var assignedOrders = GetAssignedOrders(orders);

            // Vérifier si le vendeur a des commandes assignées
            if (assignedOrders.Count == 0)
            {
                Console.WriteLine("Aucune commande assignée à ce vendeur.");
                return;
            }

            // Afficher uniquement les IDs des commandes assignées
            Console.WriteLine("Liste des commandes assignées à ce vendeur :");
            foreach (var order in assignedOrders)
            {
                Console.WriteLine($"Commande ID : {order.OrderID} - Statut : {order.Status}");
            }

            // Demander au vendeur de sélectionner une commande à traiter
            Console.Write("\nEntrez l'ID de la commande à traiter (ou '0' pour revenir au menu) : ");
            var selectedOrderId = Console.ReadLine();

            if (selectedOrderId == "0")
            {
                return;
            }

            // Trouver la commande sélectionnée par ID
            var selectedOrder = assignedOrders.FirstOrDefault(o => o.OrderID == selectedOrderId);

            if (selectedOrder != null)
            {
                bool orderMenu = true;
                while (orderMenu)
                {
                    Console.Clear();

                    // Afficher les détails de la commande sélectionnée
                    Console.WriteLine("\n=== Détails de la commande ===");
                    selectedOrder.DisplayOrder();

                    // Afficher les options disponibles
                    Console.WriteLine("\nQue voulez-vous faire ?");
                    Console.WriteLine("1. Valider la commande");
                    Console.WriteLine("2. Annuler la commande");
                    Console.WriteLine("0. Retour à la liste des commandes");

                    Console.Write("\nVotre choix : ");
                    var actionChoice = Console.ReadLine();

                    switch (actionChoice)
                    {
                        case "1":
                            selectedOrder.UpdateStatus("Complétée");
                            OrderManager.SaveOrder(orders, ordersPath);
                            Console.WriteLine("La commande a été validée et les modifications ont été sauvegardées.");
                            Console.WriteLine("Appuyez sur une touche pour continuer...");
                            Console.ReadKey();
                            orderMenu = false;
                            break;

                        case "2":
                            selectedOrder.UpdateStatus("Annulée");
                            OrderManager.SaveOrder(orders, ordersPath);
                            Console.WriteLine("La commande a été annulée et les modifications ont été sauvegardées.");
                            Console.WriteLine("Appuyez sur une touche pour continuer...");
                            Console.ReadKey();
                            orderMenu = false;
                            break;

                        case "0":
                            orderMenu = false; // Retour à la liste des commandes
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
                Console.WriteLine("Commande non trouvée. Appuyez sur une touche pour revenir à la liste.");
                Console.ReadKey();
                ProcessOrder();
            }
        }
        public void AfficherMenu()
        {
            bool menuActive = true;
            while (menuActive)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Vendeur : {Name} ({ID}) ===");
                Console.WriteLine("1. Créer un bouquet");
                Console.WriteLine("2. Traiter une commande");
                Console.WriteLine("3. Consulter la liste des bouquets");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        BouquetManager.CreateBouquet();
                        break;
                    case "2":
                        ProcessOrder();
                        break;
                    case "3":
                        BouquetManager.DisplayBouquets();
                        break;
                    case "0":
                        menuActive = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (menuActive && choix != "0")
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
        public Owner(string name, string email, string? id = null) : base(name, email, id, "Owner") { }

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
        public Seller AddSeller(List<Seller> sellers, string name, string email)
        {
            Seller newSeller = new Seller(name, email);
            sellers.Add(newSeller);
            Console.WriteLine($"Vendeur {name} ajouté par {Name}.");
            UserManager.SaveUser(sellers.Cast<Users>().ToList());  // Change les vendeurs en type Users pour être sauvegardés

            return newSeller;
        }

        // Ajouter un client
        public Client AddClient(List<Client> clients, string name, string email)
        {
            Client newClient = new Client(name, email);
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
        public Owner AddOwner(List<Owner> owners, string name, string email)
        {
            Owner newOwner = new Owner(name, email);
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
        public Supplier AddSupplier(List<Supplier> suppliers, string name, string email, string companyName)
        {
            Supplier newSupplier = new Supplier(name, email, companyName);
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
            bool sousMenuActive = true;
            while (sousMenuActive)
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

                var choix = Console.ReadLine();

                List<Users> users = UserManager.LoadUsers();

                switch (choix)
                {
                    case "1":
                        ManageSellers(users);
                        break;
                    case "2":
                        Console.Write("Nom du vendeur : ");
                        var sellerName = Console.ReadLine();
                        Console.Write("Email du vendeur : ");
                        var sellerEmail = Console.ReadLine();
                        AddSeller(sellers, sellerName, sellerEmail);
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
                        AddClient(clients, clientName, clientEmail);
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
                        AddOwner(owners, ownerName, ownerEmail);
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
                        AddSupplier(suppliers, supplierName, supplierEmail, companyName);
                        break;
                    case "12":
                        Console.Write("ID du fournisseur à supprimer : ");
                        var supplierIdToRemove = Console.ReadLine();
                        RemoveUser(supplierIdToRemove);
                        break;
                    case "0":
                        sousMenuActive = false;
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (sousMenuActive && choix != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu de gestion des utilisateurs.");
                    Console.ReadKey();
                }
            }
        }

        // Méthode pour afficher le menu et gérer les actions
        public void AfficherMenu(List<Seller> sellers, List<Client> clients, List<Owner> owners, List<Supplier> suppliers)
        {
            bool menuActive = true;
            while (menuActive)
            {
                Console.Clear();
                Console.WriteLine($"=== Menu Propriétaire : {Name} ({ID}) ===");
                Console.WriteLine("1. Gérer les utilisateurs");
                Console.WriteLine("2. Consulter la liste des bouquets");
                Console.WriteLine("3. Créer un bouquet");
                Console.WriteLine("0. Se déconnecter");
                Console.Write("Choisissez une option : ");

                var choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        ManageUsers(sellers, clients, owners, suppliers);
                        break;
                    case "2":
                        BouquetManager.DisplayBouquets();
                        break;
                    case "3":
                        BouquetManager.CreateBouquet();
                        break;
                    case "0":
                        menuActive = false;
                        Console.WriteLine("Déconnexion...");
                        break;
                    default:
                        Console.WriteLine("Option invalide, veuillez réessayer.");
                        break;
                }

                if (menuActive && choix != "0")
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu principal.");
                    Console.ReadKey();
                }
            }
        }
    }
}

