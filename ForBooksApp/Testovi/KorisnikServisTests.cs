using ForBooks.Data;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestiranje
{
    [TestClass]
    public class KorisnikServisTests
    {
        private FakeDbClass db;
        private KorisnikServis servis;

        [TestInitialize]
        public void Init()
        {
            db = new FakeDbClass();
            servis = new KorisnikServis(db);
        }

        [TestMethod]
        public void Registracija_DodajeNovogKorisnika()
        {
            string msg = servis.Registracija(
                "Neko",
                "Nekić",
                "neko123",
                "neko@gmail.com",
                "061111111",
                "lozinka123"
            );

            Assert.AreEqual(1, db.korisnici.Count);
            Assert.AreEqual("neko@gmail.com", db.korisnici[0].Email);
            Assert.IsTrue(msg.Contains("Potvrdite registraciju"));
        }

        [TestMethod]
        public void Registracija_NeDozvoljavaDupliEmail()
        {
            db.korisnici.Add(new Korisnik(1, "a", "b", "c", "postoji@gmail.com", "loz", "111"));

            string msg = servis.Registracija(
                "Test",
                "User",
                "test123",
                "postoji@gmail.com",
                "062000000",
                "pass"
            );

            Assert.AreEqual(1, db.korisnici.Count);
            Assert.IsTrue(msg.Contains("već postoji"));
        }

        [TestMethod]
        public void PotvrdaRegistracije_Uspjesna_AkoJeProsloManjeOd5Min()
        {
            db.korisnici.Add(new Korisnik(1, "a", "b", "c", "test@test.com", "loz", "111")
            {
                VrijemeRegistracije = System.DateTime.Now.AddMinutes(-3)
            });

            string msg = servis.PotvrdaRegistracije("test@test.com");

            Assert.IsTrue(db.korisnici[0].Potvrdjen);
            Assert.IsTrue(msg.Contains("uspješno potvrđena"));
        }

        [TestMethod]
        public void PotvrdaRegistracije_BriseKorisnika_AkoJeProsloViseOd5Min()
        {
            db.korisnici.Add(new Korisnik(1, "a", "b", "c", "test@test.com", "loz", "111")
            {
                VrijemeRegistracije = System.DateTime.Now.AddMinutes(-10)
            });

            string msg = servis.PotvrdaRegistracije("test@test.com");

            Assert.AreEqual(0, db.korisnici.Count);
            Assert.IsTrue(msg.Contains("poništena"));
        }

        [TestMethod]
        public void PotvrdaRegistracije_KorisnikNePostoji()
        {
            string msg = servis.PotvrdaRegistracije("nepostojece@test.com");

            Assert.AreEqual(0, db.korisnici.Count);
            Assert.IsTrue(msg.Contains("nije pronađen"));
        }

        [TestMethod]
        public void Prijava_Uspjesna()
        {
            db.korisnici.Add(new Korisnik(1, "A", "B", "C", "login@test.com", "123", "111")
            {
                Potvrdjen = true
            });

            var (korisnik, msg) = servis.Prijava("login@test.com", "123");

            Assert.IsNotNull(korisnik);
            Assert.AreEqual("login@test.com", korisnik.Email);
            Assert.IsTrue(msg.Contains("Dobrodošli"));
        }

        [TestMethod]
        public void Prijava_PogresnaLozinka()
        {
            db.korisnici.Add(new Korisnik(1, "A", "B", "C", "login@test.com", "123", "111")
            {
                Potvrdjen = true
            });

            var (korisnik, msg) = servis.Prijava("login@test.com", "pogresno");

            Assert.IsNull(korisnik);
            Assert.IsTrue(msg.Contains("Pogrešna"));
        }

        [TestMethod]
        public void Prijava_NepotvrdjenKorisnik()
        {
            db.korisnici.Add(new Korisnik(1, "A", "B", "C", "login@test.com", "123", "111")
            {
                Potvrdjen = false
            });

            var (korisnik, msg) = servis.Prijava("login@test.com", "123");

            Assert.IsNull(korisnik);
            Assert.IsTrue(msg.Contains("nije potvrđena"));
        }
    }
}
