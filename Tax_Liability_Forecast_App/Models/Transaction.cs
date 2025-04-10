using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; }
        public DateTime Date {  get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string IncomeType { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}