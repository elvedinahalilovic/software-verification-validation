using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Izdavac
    {
        public int ID { get; set; }
        public string NazivIzdavaca { get; set; }

        public Izdavac(int id, string naziv)
        {
            ID = id;
            NazivIzdavaca = naziv;
        }
    }
}
