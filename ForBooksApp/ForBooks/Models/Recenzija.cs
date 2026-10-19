/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Recenzija
    {
        public int ID { get; set; }
        public Korisnik Korisnik { get; set; }
        public Knjiga Knjiga { get; set; }
        public double Ocjena { get; set; }
        public string Komentar { get; set; }

        public Recenzija(int id, Korisnik korisnik, Knjiga knjiga, double ocjena, string komentar)
        {
            ID = id;
            Korisnik = korisnik;
            Knjiga = knjiga;
            Ocjena = ocjena;
            Komentar = komentar;
        }
    }
}*/
using Newtonsoft.Json;
using System;

namespace ForBooks.Models
{
    public class Recenzija
    {
        public int ID { get; set; }

        // ID-jevi koji se čuvaju u JSON-u
        public int KorisnikID { get; set; }
        public int KnjigaID { get; set; }

        // Reference u memoriji (NE idu u JSON)
        [JsonIgnore]
        public Korisnik? Korisnik { get; set; }

        [JsonIgnore]
        public Knjiga? Knjiga { get; set; }

        public double Ocjena { get; set; }
        public string? Komentar { get; set; }

        public Recenzija() { }

        public Recenzija(int id, Korisnik korisnik, Knjiga knjiga, double ocjena, string komentar)
        {
            ID = id;
            Korisnik = korisnik;
            Knjiga = knjiga;
            Ocjena = ocjena;
            Komentar = komentar;

            KorisnikID = korisnik?.ID ?? 0;
            KnjigaID = knjiga?.ID ?? 0;
        }
    }
}


