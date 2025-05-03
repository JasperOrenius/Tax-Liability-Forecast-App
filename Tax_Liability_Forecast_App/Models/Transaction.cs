using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Date {  get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string IncomeType { get; set; }
        public TransactionType Type { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public Guid? DeductionTypeId { get; set; }
        public DeductionType? DeductionType { get; set; }

        [NotMapped]
        private bool isEditing;
        [NotMapped]
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}