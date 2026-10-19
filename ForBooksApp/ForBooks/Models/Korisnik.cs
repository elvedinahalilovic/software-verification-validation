using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Models
{
    public class Korisnik
    {
        public int ID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public DateTime DatumPridruzivanja { get; set; } = DateTime.Now;
        public string BrojTelefona { get; set; }
        public bool Potvrdjen { get; set; } = false;
        public DateTime VrijemeRegistracije { get; set; }
        public List<Knjiga> Knjige { get; set; } = new();

        public Korisnik() { }

        public Korisnik(int id, string ime, string prezime, string username, string email, string lozinka, string brojTelefona)
        {
            ID = id;
            Ime = ime;
            Prezime = prezime;
            Username = username;
            Email = email;
            Lozinka = lozinka;
            BrojTelefona = brojTelefona;
            VrijemeRegistracije = DateTime.Now;
        }
    }
}
