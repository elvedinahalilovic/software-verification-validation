using ForBooks.Data;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestiranje
{
    [TestClass]
    public class RecenzijeServisTests
    {
        private FakeDbClass _db;
        private RecenzijaServis _servis;

        [TestInitialize]
        public void TestInitialize()
        {
            _db = new FakeDbClass();
            InicijalizirajTestnePodatke();
            _servis = new RecenzijaServis(_db);
        }

        private void InicijalizirajTestnePodatke()
        {
            var korisnik1 = new Korisnik { ID = 1, Ime = "Nejla", Prezime = "Test" };
            var korisnik2 = new Korisnik { ID = 2, Ime = "Amir", Prezime = "Test" };
            _db.korisnici.Add(korisnik1);
            _db.korisnici.Add(korisnik2);

            var knjiga1 = new Knjiga { ID = 1, NazivKnjige = "Knjiga 1", Recenzije = new List<Recenzija>() };
            var knjiga2 = new Knjiga
            {
                ID = 2,
                NazivKnjige = "Knjiga 2",
                Recenzije = new List<Recenzija>()
            };

            _db.knjige.Add(knjiga1);
            _db.knjige.Add(knjiga2);


            var rec1 = new Recenzija { ID = 1, KorisnikID = 1, KnjigaID = 1, Korisnik = korisnik1, Knjiga = knjiga1, Ocjena = 5, Komentar = "Odlično" };
            var rec2 = new Recenzija { ID = 2, KorisnikID = 2, KnjigaID = 1, Korisnik = korisnik2, Knjiga = knjiga1, Ocjena = 3, Komentar = "Može proći" };

            _db.Recenzije.Add(rec1);
            _db.Recenzije.Add(rec2);
            knjiga1.Recenzije.Add(rec1);
            knjiga1.Recenzije.Add(rec2);
        }


        [TestMethod]
        public void DodajRecenziju_ValidniPodaci_DodajeRecenziju()
        {
            int pocetniBroj = _db.Recenzije.Count;

            var rec = _servis.DodajRecenziju(
                korisnikId: 1,
                knjigaId: 2,
                ocjena: 4.0,
                komentar: "Super knjiga"
            );

            Assert.IsNotNull(rec);
            Assert.AreEqual(pocetniBroj + 1, _db.Recenzije.Count, "Treba biti dodana jedna recenzija više.");
            Assert.AreEqual(1, _db.knjige[1].Recenzije.Count, "Druga knjiga treba imati jednu recenziju.");
            Assert.AreEqual(2, rec.KnjigaID);
            Assert.AreEqual(1, rec.KorisnikID);
            Assert.AreEqual(4.0, rec.Ocjena);
        }

        [TestMethod]
        public void DohvatiSveRecenzije_VracaSveIzBaze()
        {
            var rezultat = _servis.DohvatiSveRecenzije();

            Assert.AreEqual(_db.Recenzije.Count, rezultat.Count);
            Assert.AreEqual(1, rezultat[0].ID);
            Assert.AreEqual(2, rezultat[1].ID);
        }

        [TestMethod]
        public void DohvatiRecenzijeZaKnjigu_PostojecaKnjiga_VracaTacneRecenzije()
        {
            var rezultat = _servis.DohvatiRecenzijeZaKnjigu(1);

            Assert.AreEqual(2, rezultat.Count);
            Assert.IsTrue(rezultat.TrueForAll(r => r.KnjigaID == 1));
        }

        [TestMethod]
        public void IzracunajProsjecnuOcjenuKnjige_SaRecenzijama_VracaIspravanProsjek()
        {
            var (prosjek, broj) = _servis.IzracunajProsjecnuOcjenuKnjige(1);

            Assert.AreEqual(2, broj, "Očekuju se 2 recenzije za knjigu 1.");
            Assert.AreEqual(4.0, prosjek, 0.001, "Prosjek ocjena trebao bi biti (5+3)/2 = 4.0");
        }

        [TestMethod]
        public void IzracunajProsjecnuOcjenuKnjige_BezRecenzija_VracaNula()
        {
            var (prosjek, broj) = _servis.IzracunajProsjecnuOcjenuKnjige(2);

            Assert.AreEqual(0, broj);
            Assert.AreEqual(0.0, prosjek, 0.0);
        }

        [TestMethod]
        public void ObrisiRecenziju_PostojeciId_BriseIzBazeIKnjige()
        {
            int pocetniBroj = _db.Recenzije.Count;
            int pocetniBrojKnjigaRec = _db.knjige[0].Recenzije.Count;

            _servis.ObrisiRecenziju(1);

            Assert.AreEqual(pocetniBroj - 1, _db.Recenzije.Count, "Treba biti jedna recenzija manje.");
            Assert.AreEqual(pocetniBrojKnjigaRec - 1, _db.knjige[0].Recenzije.Count, "Treba biti jedna recenzija manje i na knjizi.");
        }

        [TestMethod]
        public void DohvatiRecenzijuPoId_PostojeciId_VracaRecenziju()
        {
            var rec = _servis.DohvatiRecenzijuPoId(1);

            Assert.IsNotNull(rec);
            Assert.AreEqual(1, rec.ID);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DodajRecenziju_NepostojeciKorisnik_ThrowInvalidOperationException()
        {
            _servis.DodajRecenziju(
                korisnikId: 999,
                knjigaId: 1,
                ocjena: 4,
                komentar: "Komentar"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DodajRecenziju_NepostojecaKnjiga_ThrowInvalidOperationException()
        {
            _servis.DodajRecenziju(
                korisnikId: 1,
                knjigaId: 999,
                ocjena: 4,
                komentar: "Komentar"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DohvatiRecenzijeZaKnjigu_NepostojecaKnjiga_ThrowInvalidOperationException()
        {
            _servis.DohvatiRecenzijeZaKnjigu(999);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IzracunajProsjecnuOcjenuKnjige_NepostojecaKnjiga_ThrowInvalidOperationException()
        {
            _servis.IzracunajProsjecnuOcjenuKnjige(999);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ObrisiRecenziju_NepostojeciId_ThrowInvalidOperationException()
        {
            _servis.ObrisiRecenziju(999);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DohvatiRecenzijuPoId_NepostojeciId_ThrowInvalidOperationException()
        {
            _servis.DohvatiRecenzijuPoId(999);
        }
    }
}
