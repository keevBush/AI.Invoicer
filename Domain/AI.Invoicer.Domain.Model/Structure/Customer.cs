using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.Structure
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Nif { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

    }
}
