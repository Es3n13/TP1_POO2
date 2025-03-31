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
        public string InvoiceID { get; set; }
        public string OrderID { get; set; }
        public string ClientID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; }

        public Invoice(string orderID, string clientID, decimal totalAmount)
        {
            InvoiceID = Guid.NewGuid().ToString().Substring(0, 5);
            OrderID = orderID;
            ClientID = clientID;
            TotalAmount = totalAmount;
            IssueDate = DateTime.Now;
        }
    }

    // DTO pour la facture (pour la sérialisation)
    public class InvoiceDTO
    {
        public string InvoiceID { get; set; }
        public string OrderID { get; set; }
        public string ClientID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
