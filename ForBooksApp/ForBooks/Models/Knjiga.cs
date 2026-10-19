using ForBooks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Knjiga
    {
        public int ID { get; set; }
        public string ISBN { get; set; }
        public string NazivKnjige { get; set; }
        public DateTime DatumObjavljivanja { get; set; }
        public VrstaKnjige VrstaKnjige { get; set; }
        public Izdavac Izdavac { get; set; }
        public List<Autor> Autori { get; set; } = new();
        public List<Recenzija> Recenzije { get; set; } = new();

        public Knjiga() { }

        public Knjiga(int id, string isbn, string naziv, DateTime datum, VrstaKnjige vrsta, Izdavac izdavac)
        {
            ID = id;
            ISBN = isbn;
            NazivKnjige = naziv;
            DatumObjavljivanja = datum;
            VrstaKnjige = vrsta;
            Izdavac = izdavac;
        }

        public void PromijeniNaziv(string noviNaziv) => NazivKnjige = noviNaziv;
        public void PromijeniISBN(string noviISBN) => ISBN = noviISBN;
    }
}
