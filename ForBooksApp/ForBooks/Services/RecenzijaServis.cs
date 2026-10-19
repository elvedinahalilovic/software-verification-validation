using System;
using System.Collections.Generic;
using ForBooks.Data;
using ForBooks.Models;

namespace ForBooks.Services
{
    public class RecenzijaServis : IRecenzijaService
    {
        private readonly DbClass _db;

        public RecenzijaServis(DbClass db)
        {
            _db = db;
        }
        public Recenzija DodajRecenziju(int korisnikId, int knjigaId, double ocjena, string komentar)
        {
            if (ocjena < 1 || ocjena > 5)
                throw new ArgumentException("Ocjena mora biti broj između 1 i 5.", nameof(ocjena));

            if (komentar == null)
                komentar = string.Empty;

            Korisnik? korisnik = null;
            for (int i = 0; i < _db.korisnici.Count; i++)
            {
                if (_db.korisnici[i].ID == korisnikId)
                {
                    korisnik = _db.korisnici[i];
                    break;
                }
            }
            if (korisnik == null)
                throw new InvalidOperationException("Korisnik sa datim ID-jem ne postoji.");

            Knjiga? knjiga = null;
            for (int i = 0; i < _db.knjige.Count; i++)
            {
                if (_db.knjige[i].ID == knjigaId)
                {
                    knjiga = _db.knjige[i];
                    break;
                }
            }
            if (knjiga == null)
                throw new InvalidOperationException("Knjiga sa datim ID-jem ne postoji.");

            int noviId = 1;
            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                if (_db.Recenzije[i].ID >= noviId)
                    noviId = _db.Recenzije[i].ID + 1;
            }

            var rec = new Recenzija
            {
                ID = noviId,
                KorisnikID = korisnikId,
                KnjigaID = knjigaId,
                Korisnik = korisnik,
                Knjiga = knjiga,
                Ocjena = ocjena,
                Komentar = komentar
            };
            _db.Recenzije.Add(rec);

            if (knjiga.Recenzije == null)
                knjiga.Recenzije = new List<Recenzija>();
            knjiga.Recenzije.Add(rec);

            _db.UpdateRecenzije();
            _db.UpdateKnjige();

            return rec;
        }

        public List<Recenzija> DohvatiSveRecenzije()
        {
            var rezultat = new List<Recenzija>();
            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                rezultat.Add(_db.Recenzije[i]);
            }
            return rezultat;
        }

        public List<Recenzija> DohvatiRecenzijeZaKnjigu(int knjigaId)
        {
            Knjiga? knjiga = null;
            for (int i = 0; i < _db.knjige.Count; i++)
            {
                if (_db.knjige[i].ID == knjigaId)
                {
                    knjiga = _db.knjige[i];
                    break;
                }
            }
            if (knjiga == null)
                throw new InvalidOperationException("Knjiga sa datim ID-jem ne postoji.");

            var rezultat = new List<Recenzija>();
            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                if (_db.Recenzije[i].KnjigaID == knjigaId)
                    rezultat.Add(_db.Recenzije[i]);
            }

            return rezultat;
        }

        public (double Prosjek, int BrojRecenzija) IzracunajProsjecnuOcjenuKnjige(int knjigaId)
        {
            if (knjigaId <= 0)
                throw new ArgumentException("Neispravan ID knjige.");

            Knjiga? knjiga = null;
            for (int i = 0; i < _db.knjige.Count; i++)
            {
                if (_db.knjige[i].ID == knjigaId)
                {
                    knjiga = _db.knjige[i];
                    break;
                }
            }
            if (knjiga == null)
                throw new InvalidOperationException("Knjiga sa datim ID-jem ne postoji.");

            double suma = 0;
            int broj = 0;

            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                var r = _db.Recenzije[i];
                if (r.KnjigaID == knjigaId)
                {
                    suma += r.Ocjena;
                    broj++;
                }
            }
            if (broj == 0)
                return (0.0, 0);

            double prosjek = suma / broj;
            return (prosjek, broj);
        }

        public void ObrisiRecenziju(int recenzijaId)
        {
            int index = -1;
            Recenzija? target = null;

            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                if (_db.Recenzije[i].ID == recenzijaId)
                {
                    index = i;
                    target = _db.Recenzije[i];
                    break;
                }
            }

            if (target == null || index == -1)
                throw new InvalidOperationException("Recenzija sa datim ID-jem ne postoji.");

            if (target.Knjiga != null && target.Knjiga.Recenzije != null)
            {
                for (int i = 0; i < target.Knjiga.Recenzije.Count; i++)
                {
                    if (target.Knjiga.Recenzije[i].ID == recenzijaId)
                    {
                        target.Knjiga.Recenzije.RemoveAt(i);
                        break;
                    }
                }
            }

            _db.Recenzije.RemoveAt(index);

            _db.UpdateRecenzije();
            _db.UpdateKnjige();
        }

        public Recenzija DohvatiRecenzijuPoId(int recenzijaId)
        {
            for (int i = 0; i < _db.Recenzije.Count; i++)
            {
                if (_db.Recenzije[i].ID == recenzijaId)
                    return _db.Recenzije[i];
            }

            throw new InvalidOperationException("Recenzija sa datim ID-jem ne postoji.");
        }
    }
}
