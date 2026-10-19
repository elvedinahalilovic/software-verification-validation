using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Autor
    {
        public int ID { get; set; }
        public string ImeAutora { get; set; }
        public string PrezimeAutora { get; set; }
        public DateTime DatumRodjenja { get; set; }

        public Autor(int id, string ime, string prezime, DateTime datumRodjenja)
        {
            ID = id;
            ImeAutora = ime;
            PrezimeAutora = prezime;
            DatumRodjenja = datumRodjenja;
        }
    }
}
