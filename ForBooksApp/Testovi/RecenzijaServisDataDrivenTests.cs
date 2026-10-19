using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestiranje
{
    [TestClass]
    public class RecenzijaServisDataDrivenTests
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
            var korisnik = new Korisnik
            {
                ID = 1,
                Ime = "Test",
                Prezime = "Korisnik"
            };
            _db.korisnici.Add(korisnik);

            var knjiga = new Knjiga
            {
                ID = 1,
                NazivKnjige = "Test knjiga",
                Recenzije = new List<Recenzija>()
            };
            _db.knjige.Add(knjiga);
        }

        // DATA-DRIVEN TEST - DataRow

        [DataTestMethod]
        [DataRow(0.0)]
        [DataRow(0.5)]
        [DataRow(5.5)]
        [DataRow(10.0)]
        public void DodajRecenziju_NeispravnaOcjena_ThrowArgumentException(double ocjena)
        {
            Assert.ThrowsException<ArgumentException>(() =>
                _servis.DodajRecenziju(
                    korisnikId: 1,
                    knjigaId: 1,
                    ocjena: ocjena,
                    komentar: "Test komentar"
                ));
        }

        // DATA-DRIVEN TEST - CSV + DynamicData

        public static IEnumerable<object[]> ProsjekOcjenaTestData()
        {
            var path = "ProsjecneOcjeneData.csv";

            using var reader = new StreamReader(path);
            string? header = reader.ReadLine();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 4)
                    continue;

                int knjigaId = int.Parse(parts[0], CultureInfo.InvariantCulture);
                double ocjena1 = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double ocjena2 = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double expectedAverage = double.Parse(parts[3], CultureInfo.InvariantCulture);

                yield return new object[] { knjigaId, ocjena1, ocjena2, expectedAverage };
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(ProsjekOcjenaTestData), DynamicDataSourceType.Method)]
        public void IzracunajProsjecnuOcjenuKnjige_DataDrivenCsv_VracaOcekivaniProsjek(
            int knjigaId,
            double ocjena1,
            double ocjena2,
            double expectedAverage)
        {

            var knjiga = new Knjiga
            {
                ID = knjigaId,
                NazivKnjige = $"CSV knjiga {knjigaId}",
                Recenzije = new List<Recenzija>()
            };
            _db.knjige.Add(knjiga);

            var korisnik = _db.korisnici[0];

            var rec1 = new Recenzija
            {
                ID = 100 + knjigaId * 10 + 1,
                KorisnikID = korisnik.ID,
                KnjigaID = knjigaId,
                Korisnik = korisnik,
                Knjiga = knjiga,
                Ocjena = ocjena1,
                Komentar = "csv1"
            };

            var rec2 = new Recenzija
            {
                ID = 100 + knjigaId * 10 + 2,
                KorisnikID = korisnik.ID,
                KnjigaID = knjigaId,
                Korisnik = korisnik,
                Knjiga = knjiga,
                Ocjena = ocjena2,
                Komentar = "csv2"
            };

            _db.Recenzije.Add(rec1);
            _db.Recenzije.Add(rec2);
            knjiga.Recenzije.Add(rec1);
            knjiga.Recenzije.Add(rec2);

            var (prosjek, brojRecenzija) = _servis.IzracunajProsjecnuOcjenuKnjige(knjigaId);

            Assert.AreEqual(2, brojRecenzija, "Broj recenzija mora biti 2 prema CSV datoteci.");
            Assert.AreEqual(expectedAverage, prosjek, 0.001, "Prosječna ocjena nije ispravna.");
        }
    }
}
