using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using FlowerShop.Class;
using Newtonsoft.Json.Linq;

namespace FlowerShop.Data
{
    public class UserManager
    {
        //Chemin vers le fihcier UserDate.JSON
        private static string UserDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData.json");

        //Sauvegarde des utilisateurs dans un fichier JSON
        public static void SaveUser(List<Users> newUsers)
         {
             List<Users> users = LoadUsers(); // Charger les utilisateurs existants

             // Fusionner les nouveaux utilisateurs en évitant les doublons
             foreach (var newUser in newUsers)
             {
                 if (!users.Any(u => u.ID == newUser.ID))
                 {
                     users.Add(newUser);
                 }
             }

             string json = JsonConvert.SerializeObject(users, Formatting.Indented);
             File.WriteAllText(UserDataPath, json);
             Console.WriteLine("Données sauvegardées !");
         }

        // Chargement des utilisateurs depuis un fichier JSON
        public static List<Users> LoadUsers()
        {
            try
            {
                // Vérifier si le fichier JSON existe
                if (!File.Exists(UserDataPath))
                {

                    // Créer un utilisateur Owner par défaut
                    List<Users> defaultUsers = new List<Users>
                    {
                        new Owner("Owner1", "owner@1", "U001","12345")
                    };

                    // Sauvegarder cet utilisateur dans un fichier JSON
                    string defaultJson = JsonConvert.SerializeObject(defaultUsers, Formatting.Indented);
                    File.WriteAllText(UserDataPath, defaultJson);

                    return defaultUsers;
                }

                // Charger les utilisateurs à partir du fichier JSON
                string json = File.ReadAllText(UserDataPath);

                // Désérialiser en utilisant le JsonConverter si nécessaire
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new UserJsonConverter()); // Utilisation d'un convertisseur personnalisé

                List<Users> users = JsonConvert.DeserializeObject<List<Users>>(json, settings);

                return users ?? new List<Users>(); // Retourne une liste vide si la désérialisation échoue
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du chargement des utilisateurs : " + ex.Message);
                return new List<Users>(); // Retourne une liste vide en cas d'erreur
            }
        }


        //Authentification des utilisateurs
        public static Users? AuthentifyUser(string userId, string password)
        {
            var users = LoadUsers();
            var foundUser = users.Find(u => u.ID == userId);

            if (foundUser != null && foundUser.Password == password)
            {
                Console.WriteLine($"Authentification réussie !");
                return foundUser;
            }
            else
            {
                Console.WriteLine("Échec de l'authentification. Vérifiez votre ID et votre mot de passe.");
                return null;
            }
        }
        // Filtrer les utilisateurs par rôle
        public static List<Seller> GetSellers(List<Users> users)
        {
            return users.Where(u => u.Role == "Seller")
                        .Select(u => new Seller(u.Name, u.Email, u.ID, u.Password))
                        .ToList();
        }

        public static List<Client> GetClients(List<Users> users)
        {
            return users.OfType<Client>().ToList();
        }

        public static List<Owner> GetOwners(List<Users> users)
        {
            return users.OfType<Owner>().ToList();
        }

        public static List<Supplier> GetSuppliers(List<Users> users)
        {
            return users.OfType<Supplier>().ToList();
        }
    }
    
}
