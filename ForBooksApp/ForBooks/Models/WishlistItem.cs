using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class WishlistItem
    {
        public string Username { get; set; } = "";
        public List<string> BookTitles { get; set; } = new();
    }
}
