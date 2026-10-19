using ForBooks.Data;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTestiranje
{
    [TestClass]
    public class KorisnikServisDataDrivenTests
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
        [DataRow("login@test.com", "123", true)]
        [DataRow("login@test.com", "pogresno", false)]
        [DataRow("nepostojeci@test.com", "123", false)]
        public void Prijava_DataRow(string email, string lozinka, bool expected)
        {
            db.korisnici.Add(new Korisnik(1, "A", "B", "C",
                "login@test.com", "123", "111")
            { Potvrdjen = true });

            var (korisnik, msg) = servis.Prijava(email, lozinka);

            Assert.AreEqual(expected, korisnik != null);
        }

        public static IEnumerable<object[]> RegistracijaData()
        {
            yield return new object[] { -3, true };   // manje od 5 min , potvrđena
            yield return new object[] { -10, false }; // više od 5 min , obrisana
        }

        [TestMethod]
        [DynamicData(nameof(RegistracijaData), DynamicDataSourceType.Method)]
        public void PotvrdaRegistracije_DynamicData(int minutesOffset, bool expectedPotvrdjen)
        {
            var korisnik = new Korisnik(1, "A", "B", "C",
                "test@test.com", "123", "111")
            {
                VrijemeRegistracije = DateTime.Now.AddMinutes(minutesOffset)
            };

            db.korisnici.Add(korisnik);

            string msg = servis.PotvrdaRegistracije("test@test.com");

            if (expectedPotvrdjen)
            {
                Assert.AreEqual(1, db.korisnici.Count);
                Assert.IsTrue(db.korisnici[0].Potvrdjen);
            }
            else
            {
                Assert.AreEqual(0, db.korisnici.Count);
            }
        }

        public static IEnumerable<object[]> PrijavaFromCsv()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrijavaData.csv");

            foreach (var line in File.ReadAllLines(path).Skip(1))
            {
                var parts = line.Split(',');
                yield return new object[]
                {
                    parts[0],                // email
                    parts[1],                // lozinka
                    bool.Parse(parts[2])     // expected
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(PrijavaFromCsv), DynamicDataSourceType.Method)]
        public void Prijava_CSV_LikeDataDriven(string email, string lozinka, bool expected)
        {
            db.korisnici.Add(new Korisnik(1, "A", "B", "C",
                "login@test.com", "123", "111")
            { Potvrdjen = true });

            var (korisnik, msg) = servis.Prijava(email, lozinka);

            Assert.AreEqual(expected, korisnik != null);
        }
    }
}
