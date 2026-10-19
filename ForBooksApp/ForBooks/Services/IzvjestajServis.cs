using ForBooks.Data;
using ForBooks.Models;
using ForBooks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Services
{
    public class IzvjestajServis
    {
        private readonly DbClass _db;
        public IzvjestajServis(DbClass db)
        {
            _db = db;
        }
        private bool IsNullOrWhite(string s)
        {
            if (s == null) return true;
            for (int i = 0; i < s.Length; i++)
                if (!char.IsWhiteSpace(s[i])) return false;
            return true;
        }
        public void getIzvjestaj()
        {
            Console.WriteLine("Za Vaš izvještaj pročitanih knjiga, molimo prijavite se:\n");

            Console.Write("Email: ");
            string email = Console.ReadLine()!;
            if (IsNullOrWhite(email))
            {
                Console.WriteLine("GREŠKA: Email ne može biti prazan.");
                return;
            }

            Console.Write("Šifra: ");
            string sifra = Console.ReadLine()!;
            if (IsNullOrWhite(sifra))
            {
                Console.WriteLine("GREŠKA: Šifra ne može biti prazna.");
                return;
            }
            // Pronađi korisnika
            Korisnik? korisnik = null;
            for (int i = 0; i < _db.korisnici.Count; i++)
            {
                if (_db.korisnici[i].Email == email && _db.korisnici[i].Lozinka == sifra)
                {
                    korisnik = _db.korisnici[i];
                    break;
                }
            }
            if (korisnik == null)
            {
                Console.WriteLine("Korisnik nije u sistemu.");
                return;
            }
            List<Knjiga> knjigeKorisnika = korisnik.Knjige;

            if (knjigeKorisnika.Count == 0)
            {
                Console.WriteLine("Nema unesenih knjiga.");
                return;
            }
            //dodala sam inicijalizaciju Izvjestaja radi vece pokrivenosti modela Izvjestaj
            Izvjestaj izvjestaj = new Izvjestaj(1, korisnik);

            Console.WriteLine("\n Sve procitane knjige: ");
            foreach (var knjiga in knjigeKorisnika)
            {
                Console.WriteLine($"- {knjiga.NazivKnjige} ({knjiga.VrstaKnjige})");
            }

        }

        
    }
}
