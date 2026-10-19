using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class ViewHistory
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string ViewedItem { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; }
    }
}
