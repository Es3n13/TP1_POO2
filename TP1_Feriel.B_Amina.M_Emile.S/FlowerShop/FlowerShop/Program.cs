using FlowerShop.Class;
using FlowerShop.Data;
using Json.Net;
namespace FlowerShop
{
    class Program
    {
        static void Main()
        {
            bool applicationRunning = true;

            while (applicationRunning)
            {
                // Chargement des données
                string CsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fleurs_db.csv");
                string OrdersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders.json");

                var flowers = FlowerManager.LoadFlowersFromCSV(CsvPath);
                List<Users> users = UserManager.LoadUsers();

                Console.Clear();
                Console.WriteLine("=== Bienvenue au Magasin de Fleurs ===");
                Console.WriteLine("1. Se connecter");
                Console.WriteLine("2. Créer un compte client");
                Console.WriteLine("0. Quitter");

                Console.Write("Choisissez une option : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        Connexion();
                        break;
                    case "2":
                        CreerCompteClient();
                        break;
                    case "0":
                        applicationRunning = false;
                        Console.WriteLine("Au revoir !");
                        break;
                    default:
                        Console.WriteLine("Option invalide ! Appuyez sur une touche pour continuer.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void Connexion()
        {
            Console.Clear();
            Console.WriteLine("=== Connexion ===");
            Console.Write("Entrez votre ID utilisateur (ou 0 pour revenir) : ");
            string userId = Console.ReadLine();

            if (userId == "0")
                return;

            // Authentifier l'utilisateur avec son ID
            Users user = UserManager.AuthentifyUser(userId);

            if (user != null)
            {
                Console.WriteLine($"Bienvenue {user.Name}!");
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();

                // Rediriger vers le bon menu en fonction du rôle
                if (user is Owner owner)
                {
                    List<Users> allUsers = UserManager.LoadUsers();
                    List<Seller> sellers = UserManager.GetSellers(allUsers);
                    List<Client> clients = UserManager.GetClients(allUsers);
                    List<Owner> owners = UserManager.GetOwners(allUsers);
                    List<Supplier> suppliers = UserManager.GetSuppliers(allUsers);
                    owner.AfficherMenu(sellers, clients, owners, suppliers);
                }
                else if (user is Seller seller)
                {
                    seller.AfficherMenu();
                }
                else if (user is Client client)
                {
                    List<Seller> sellers = UserManager.GetSellers(UserManager.LoadUsers());
                    client.AfficherMenu(sellers);
                }
                else if (user is Supplier supplier)
                {
                    supplier.AfficherMenu();
                }
                else
                {
                    Console.WriteLine("Utilisateur introuvable ! Appuyez sur une touche pour continuer.");
                    Console.ReadKey();
                }
            }
        }
        static void CreerCompteClient()
        {
            Console.Clear();
            Console.WriteLine("=== Création d'un compte client ===");
            Console.Write("Nom : ");
            string nom = Console.ReadLine();
            Console.Write("Email : ");
            string email = Console.ReadLine();

            // Création du client
            Client newClient = new Client(nom, email);

            // Chargement des utilisateurs et des clients existants
            List<Users> users = UserManager.LoadUsers();

            // Ajout du nouveau client
            users.Add(newClient);

            // Enregistrement des utilisateurs mis à jour
            UserManager.SaveUser(users);

            // Affichage de l'ID du nouveau client
            Console.WriteLine($"Compte client créé avec succès ! Votre ID : {newClient.ID}");

            // Demande à l'utilisateur s'il veut accéder au menu client
            Console.Write("\nVoulez-vous accéder au menu client ? (O/N) : ");
            string reponse = Console.ReadLine()?.Trim().ToUpper();

            if (reponse == "O")
            {
                List<Seller> sellers = UserManager.GetSellers(users);
                newClient.AfficherMenu(sellers);
            }
        }

    }
}
