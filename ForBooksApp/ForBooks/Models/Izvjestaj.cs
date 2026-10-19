using ForBooks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Izvjestaj
    {
        public int ID { get; set; }
        public Korisnik Korisnik { get; set; }
        public List<int> BrojStranicaNedovrsenihKnjiga { get; set; } = new(); //za kasnije nadogradnje
        public string NajcitanijiZanr { get; set; } = string.Empty; //takodjer za kasnije nadogradnje
        public List<Knjiga> PrijedloziNovihKnjiga { get; set; } = new(); //implementirano negdje drugo

        public Izvjestaj(int id, Korisnik korisnik)
        {
            ID = id;
            Korisnik = korisnik;
        }
    }
}
