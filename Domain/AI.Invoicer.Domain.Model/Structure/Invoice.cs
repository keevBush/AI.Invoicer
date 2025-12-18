using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.Structure
{
    [Table("Invoices")]
    public class Invoice : BaseEntity
    {
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        [Indexed]
        public string CustomerId { get; set; }
        [Ignore]
        public Customer? Customer { get; set; }
        [Indexed]
        public string CompanyId { get; set; }
        [Ignore]
        public Company? Company { get; set; }

        [Ignore]
        
        public IEnumerable<InvoiceItem> Items { get; set; } 
        public decimal TaxRate { get; set; }

    }
}
