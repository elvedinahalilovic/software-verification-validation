using ForBooks.Data;
using ForBooks.Models;
using System;

namespace ForBooks.Services
{
    public class KorisnikServis
    {
        private readonly DbClass _db;

        public KorisnikServis(DbClass db)
        {
            _db = db;
        }

        // REGISTRACIJA
        public string Registracija(string ime, string prezime, string username,
                                   string email, string telefon, string lozinka)
        {
            // Provjeri da li email već postoji
            foreach (var k in _db.korisnici)
            {
                if (k.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    return "Korisnik sa ovim emailom već postoji!";
            }

            var korisnik = new Korisnik(
                id: _db.korisnici.Count + 1,
                ime: ime,
                prezime: prezime,
                username: username,
                email: email,
                lozinka: lozinka,
                brojTelefona: telefon
            );

            _db.AddKorisnik(korisnik);

            return $"Email za potvrdu poslan na {email}. Potvrdite registraciju.";
        }

        // POTVRDA 
        public string PotvrdaRegistracije(string email)
        {
            var korisnik = _db.korisnici
                .Find(k => k.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (korisnik == null)
                return "Korisnik nije pronađen.";

            TimeSpan proslo = DateTime.Now - korisnik.VrijemeRegistracije;

            if (proslo.TotalMinutes <= 5)
            {
                korisnik.Potvrdjen = true;
                _db.UpdateKorisnici();
                return "Registracija uspješno potvrđena!";
            }
            else
            {
                _db.korisnici.Remove(korisnik);
                _db.UpdateKorisnici();
                return "Vrijeme za potvrdu isteklo. Registracija poništena.";
            }
        }

        // PRIJAVA
        public (Korisnik? user, string message) Prijava(string email, string lozinka)
        {
            var korisnik = _db.korisnici
                .Find(k => k.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (korisnik == null)
                return (null, "Korisnik ne postoji.");

            if (!korisnik.Potvrdjen)
                return (null, "Registracija nije potvrđena.");

            if (korisnik.Lozinka != lozinka)
                return (null, "Pogrešna lozinka.");

            return (korisnik, $"Dobrodošli, {korisnik.Ime} {korisnik.Prezime}!");
        }
    }
}
