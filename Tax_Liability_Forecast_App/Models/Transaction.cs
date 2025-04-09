using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime Date {  get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}