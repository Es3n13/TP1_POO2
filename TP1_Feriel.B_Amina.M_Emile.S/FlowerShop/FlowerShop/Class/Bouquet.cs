using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Class
{
    //Bouquet
    public class Bouquet
    {
        public List<Flower> Flowers { get; set; } = new List<Flower>();
        public string Name { get; set; }
        private decimal LaborFee = 2.0m;
        private decimal CardFee = 1.0m;
        public decimal Price => CalculatePrice();

        public Bouquet(string name)
        {
            Name = name;
        }


        //Méthodes
        public decimal CalculatePrice()
        {
            // Vérifier si des fleurs ont été ajoutées avant de calculer
            if (!Flowers.Any()) return 0m;

            // Calculer le prix en utilisant la méthode de conversion de Price dans Flower
            return Flowers.Sum(flower => ConvertToDecimal(flower.Price)) + LaborFee + CardFee;
        }

        public void AddFlower(Flower flower)
        {
            if (flower != null) // Validation de la fleur avant ajout
            {
                Flowers.Add(flower);
            }
            else
            {
                Console.WriteLine("La fleur est invalide et n'a pas été ajoutée.");
            }
        }

        public void RemoveFlower(Flower flower)
        {
            if (flower != null && Flowers.Contains(flower))
            {
                Flowers.Remove(flower);
            }
            else
            {
                Console.WriteLine("La fleur à supprimer n'existe pas dans ce bouquet.");
            }
        }

        // Méthode pour convertir le prix en string ou autre format vers un decimal
        private decimal ConvertToDecimal(string priceString)
        {
            if (decimal.TryParse(priceString, out decimal parsedPrice))
            {
                return parsedPrice;
            }
            else
            {
                Console.WriteLine($"Erreur de conversion du prix : {priceString}. Valeur par défaut 0m utilisée.");
                return 0m; // Retourne 0 si la conversion échoue
            }
        }
    }

    public class BouquetCreator
    {
        public static Bouquet CreateBouquet(List<Flower> availableFlowers)
        {
            Console.Write("Nom du bouquet : ");
            string bouquetName = Console.ReadLine();
            Bouquet bouquet = new Bouquet(bouquetName);
            bool addingFlowers = true;

            // Afficher les fleurs disponibles
            FlowerManager.DisplayFlowers(availableFlowers);

            while (addingFlowers)
            {
                Console.WriteLine("Entrez le numéro de la fleur à ajouter au bouquet (ou 0 pour finir) :");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int flowerIndex) && flowerIndex > 0 && flowerIndex <= availableFlowers.Count)
                {
                    var flower = availableFlowers[flowerIndex - 1]; // Index base 1

                    // Demander le nombre de fleurs de ce type à ajouter
                    Console.Write($"Combien de {flower.Name} souhaitez-vous ajouter au bouquet ? ");
                    string quantityInput = Console.ReadLine();

                    if (int.TryParse(quantityInput, out int quantity) && quantity > 0)
                    {
                        for (int i = 0; i < quantity; i++)
                        {
                            bouquet.AddFlower(flower);  // Ajouter chaque fleur du bouquet
                        }
                        Console.WriteLine($"{quantity} fleurs {flower.Name} ajoutées au bouquet.");
                    }
                    else
                    {
                        Console.WriteLine("Quantité invalide, veuillez entrer un nombre positif.");
                    }
                }
                else if (flowerIndex == 0)
                {
                    addingFlowers = false;
                    Console.WriteLine("Création du bouquet terminée.");
                }
                else
                {
                    Console.WriteLine("Sélection invalide, veuillez entrer un numéro valide.");
                }
            }

            return bouquet;
        }
    }
}
