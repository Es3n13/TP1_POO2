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
        public string Name { get; private set; }
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
}