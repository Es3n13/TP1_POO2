using FlowerShop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Class
{
    // Classe représentant une facture
    public class Invoice
    {
        public string InvoiceID { get; private set; }
        public string OrderID { get; private set; }
        public string ClientID { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime IssueDate { get; private set; }
        public List<InvoiceItem> Items { get; private set; }

        public Invoice(string orderID, string clientID, decimal totalAmount, List<InvoiceItem> items)
        {
            InvoiceID = Guid.NewGuid().ToString().Substring(0, 5);
            OrderID = orderID;
            ClientID = clientID;
            TotalAmount = totalAmount;
            IssueDate = DateTime.Now;
            Items = Items ?? new List<InvoiceItem>();
        }

        public static void DisplayInvoice(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
            {
                Console.WriteLine("Aucune facture disponible.");
                return;
            }

            foreach (var invoice in invoices)
            {
                Console.WriteLine($"Facture ID : {invoice.InvoiceID}");
                Console.WriteLine($"Commande ID : {invoice.OrderID}");
                Console.WriteLine($"Client ID : {invoice.ClientID}");
                Console.WriteLine($"Date d'émission : {invoice.IssueDate}");
                Console.WriteLine($"Montant total : {invoice.TotalAmount:F2}$");
                //Console.WriteLine("\n** Articles de la Facture **");

                /*foreach (var item in invoice.Items)
                {
                    Console.WriteLine($"- {item.Name}: {item.Price:F2}$");
                }*/

                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }

    public class InvoiceItem
    {
        public string Name { get; private set; }  // Nom de la fleur ou du bouquet
        public float Price { get; private set; }

        public InvoiceItem(string name, float price)
        {
            Name = name;
            Price = price;
        }
    }
}