using ForBooks.Models;
using ForBooks.Services;
using global::UnitTestiranje;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTestiranje
{
    [TestClass]
    public class IzvjestajServisTests
    {
        private FakeDbClass db;
        private IzvjestajServis servis;

        [TestInitialize]
        public void Setup()
        {
            db = new FakeDbClass();
            servis = new IzvjestajServis(db);

            // korisnici
            var korisnik1 = new Korisnik(1, "Hana", "Hanovic", "hana", "hana@a.com", "pass", "111") { Potvrdjen = true };
            var korisnik2 = new Korisnik(2, "Amina", "Aminovicc", "mina", "mina@m.com", "pass", "222") { Potvrdjen = true };
            db.korisnici.Add(korisnik1);
            db.korisnici.Add(korisnik2);

            // knjige korisnika
            var knjiga1 = new Knjiga(1, "1111111111", "Knjiga1", DateTime.Now, ForBooks.Enums.VrstaKnjige.NOVEL, new Izdavac(1, "Izdavac1"));
            var knjiga2 = new Knjiga(2, "2222222222", "Knjiga2", DateTime.Now, ForBooks.Enums.VrstaKnjige.POETRY, new Izdavac(2, "Izdavac2"));
            korisnik1.Knjige.Add(knjiga1);
            korisnik2.Knjige.Add(knjiga2);
        }

        private void SetConsoleInput(params string[] inputs)
        {
            string joined = string.Join(Environment.NewLine, inputs);
            Console.SetIn(new StringReader(joined));
        }

        private string CaptureConsoleOutput(Action action)
        {
            var originalOut = Console.Out;
            try
            {
                using (var sw = new StringWriter())
                {
                    Console.SetOut(sw);
                    action();
                    return sw.ToString();
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        [TestMethod]
        public void GetIzvjestaj_ValidanKorisnik_PrintaIzvjestaj()
        {
            SetConsoleInput("hana@a.com", "pass");

            string output = CaptureConsoleOutput(() => servis.getIzvjestaj());

            Assert.IsTrue(output.Contains("Sve procitane knjige:"));
            Assert.IsTrue(output.Contains("Knjiga1"));
        }

        [TestMethod]
        public void GetIzvjestaj_InvalidKorisnik_PrintaGresku()
        {
            SetConsoleInput("nepostojeci@a.com", "pass");

            string output = CaptureConsoleOutput(() => servis.getIzvjestaj());

            Assert.IsTrue(output.Contains("Korisnik nije u sistemu."));
        }

        [TestMethod]
        public void GetIzvjestaj_KorisnikBezKnjiga_PrintaNemaUnesenihKnjiga()
        {
            db.korisnici[1].Knjige.Clear();

            SetConsoleInput("mina@m.com", "pass");

            string output = CaptureConsoleOutput(() => servis.getIzvjestaj());

            Assert.IsTrue(output.Contains("Nema unesenih knjiga."));
        }


        [TestMethod]
        public void GetIzvjestaj_PrazanEmail_PrintaGresku()
        {
            SetConsoleInput("", "pass");

            string output = CaptureConsoleOutput(() => servis.getIzvjestaj());

            Assert.IsTrue(output.Contains("GREŠKA: Email ne može biti prazan."));
        }

        [TestMethod]
        public void GetIzvjestaj_PraznaSifra_PrintaGresku()
        {
            SetConsoleInput("hana@a.com", "");

            string output = CaptureConsoleOutput(() => servis.getIzvjestaj());

            Assert.IsTrue(output.Contains("GREŠKA: Šifra ne može biti prazna."));
        }

    }
}
