using ForBooks.Data;
using ForBooks.Enums;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UnitTestiranje
{
    [TestClass]
    public class KnjigaServisTests
    {
        private FakeDbClass _db = null!;
        private KnjigaServis _servis = null!;
        private Korisnik _testKorisnik = null!;

        [TestInitialize]
        [MemberNotNull(nameof(_db), nameof(_servis), nameof(_testKorisnik))]
        public void Setup()
        {
            _db = new FakeDbClass();
            _servis = new KnjigaServis(_db);

            // Inicijalizacija test korisnika za preporuke
            _testKorisnik = new Korisnik(
                id: 100,
                ime: "Test",
                prezime: "User",
                username: "testuser",
                email: "test@a.com",
                lozinka: "123",
                brojTelefona: "061234567"
            );
            _db.korisnici.Add(_testKorisnik);
            // DODAJ TEST PODATKE ZA WHITE BOX TESTOVE
            //InicijalizujTestPodatke();
        }

        // DODAJ TEST PODATKE ZA WHITE BOX TESTOVE
        private void InicijalizujTestPodatke()
        {
            // Knjige za pretragu po nazivu
            _db.knjige.Add(new Knjiga(1, "111", "Derviš i smrt",
                DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Svjetlost")));

            _db.knjige.Add(new Knjiga(2, "222", "Na Drini ćuprija",
                DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Veselin Masleša")));

            // Knjiga sa autorom
            var autor1 = new Autor(1, "Ivo", "Andrić", new DateTime(1892, 10, 9));
            var knjigaSaAutorom = new Knjiga(3, "333", "Travnička hronika",
                DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(3, "Svjetlost"))
            {
                Autori = new List<Autor> { autor1 }
            };
            _db.knjige.Add(knjigaSaAutorom);

            // Knjiga sa više autora
            var autor2 = new Autor(2, "Meša", "Selimović", new DateTime(1910, 4, 26));
            var autor3 = new Autor(3, "Miroslav", "Krleža", new DateTime(1893, 7, 7));
            var knjigaViseAutora = new Knjiga(4, "444", "Tvrđava",
                DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(4, "Veselin Masleša"))
            {
                Autori = new List<Autor> { autor2, autor3 }
            };
            _db.knjige.Add(knjigaViseAutora);

            // Knjiga bez autora (Autori = null)
            _db.knjige.Add(new Knjiga(5, "555", "Anonimna knjiga",
                DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(5, "Nepoznato")));
        }

        // Pomoćna funkcija za kreiranje autora 
        private Autor CreateAutor(string ime, string prezime, string datumRodjenja)
        {
            var datum = DateTime.ParseExact(datumRodjenja, "dd.MM.yyyy", null);
            return new Autor(0, ime, prezime, datum); 
        }

        // Dodaj knjigu

        [TestMethod]
        public void DodajKnjigu_DodajeKnjiguUSistem_IspravniPodaci()
        {
            var autori = new List<Autor> { CreateAutor("Paja", "Patak", "01.01.2000") };
            var datum = DateTime.ParseExact("01.01.2025", "dd.MM.yyyy", null);

            string rezultat = _servis.DodajKnjigu(
                "1234567890",
                "Test Knjiga",
                datum,
                VrstaKnjige.NOVEL,
                "Izdavac Test",
                autori
            );

            Assert.AreEqual("Knjiga 'Test Knjiga' uspješno dodana i sačuvana.", rezultat);
            Assert.AreEqual(1, _db.knjige.Count);
            var knjiga = _db.knjige[0];
            Assert.AreEqual("Test Knjiga", knjiga.NazivKnjige);
            Assert.AreEqual("1234567890", knjiga.ISBN);
            Assert.AreEqual(VrstaKnjige.NOVEL, knjiga.VrstaKnjige);
            Assert.AreEqual(1, knjiga.Autori.Count);
            Assert.AreEqual(1, _db.autori.Count);
            Assert.IsTrue(_db.AddKnjigaCalled);
            Assert.IsTrue(_db.AddAutorCalled);
        }

        [TestMethod]
        public void DodajKnjigu_PrazanISBN_VracaGresku()
        {
            var autori = new List<Autor> { CreateAutor("Ime", "Prezime", "01.01.1980") };
            string rezultat = _servis.DodajKnjigu("", "Naziv Test", DateTime.Now, VrstaKnjige.MEMOIR, "Izdavac", autori);

            StringAssert.Contains(rezultat, "GREŠKA: Neispravan ISBN.");
            Assert.AreEqual(0, _db.knjige.Count);
        }

        [TestMethod]
        public void DodajKnjigu_PrazanNaziv_VracaGresku()
        {
            var autori = new List<Autor> { CreateAutor("Ime", "Prezime", "01.01.1980") };
            string rezultat = _servis.DodajKnjigu("12345", "", DateTime.Now, VrstaKnjige.MEMOIR, "Izdavac", autori);

            StringAssert.Contains(rezultat, "GREŠKA: Neispravan naziv.");
            Assert.AreEqual(0, _db.knjige.Count);
        }

        [TestMethod]
        public void DodajKnjigu_PrazanIzdavac_VracaGresku()
        {
            var autori = new List<Autor> { CreateAutor("Ime", "Prezime", "01.01.1980") };
            string rezultat = _servis.DodajKnjigu("12345", "Naziv", DateTime.Now, VrstaKnjige.MEMOIR, "", autori);

            StringAssert.Contains(rezultat, "GREŠKA: Neispravan izdavač.");
            Assert.AreEqual(0, _db.knjige.Count);
        }

        [TestMethod]
        public void DodajKnjigu_NemaAutora_VracaGresku()
        {
            var autori = new List<Autor>();
            string rezultat = _servis.DodajKnjigu("12345", "Naziv", DateTime.Now, VrstaKnjige.MEMOIR, "Izdavac", autori);

            StringAssert.Contains(rezultat, "GREŠKA: Knjiga mora imati barem jednog autora.");
            Assert.AreEqual(0, _db.knjige.Count);
        }

        // Prikazi sve knjige 

        [TestMethod]
        public void PrikaziSveKnjige_VracaPraznuListu()
        {
            var rezultat = _servis.PrikaziSveKnjige();
            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PrikaziSveKnjige_VracaKnjige()
        {
            var izdavac = new Izdavac(1, "Test Izdavac");
            var autor1 = CreateAutor("Ime1", "Prezime1", "01.01.1980");
            var autor2 = CreateAutor("Ime2", "Prezime2", "02.02.1990");
            var knjiga = new Knjiga(1, "123-456", "Test Knjiga", new DateTime(2020, 5, 10), VrstaKnjige.NOVEL, izdavac)
            {
                Autori = new List<Autor> { autor1, autor2 }
            };
            _db.knjige.Add(knjiga);

            var rezultat = _servis.PrikaziSveKnjige();

            Assert.AreEqual(1, rezultat.Count);
            Assert.AreEqual("Test Knjiga", rezultat[0].NazivKnjige);
            Assert.AreEqual(2, rezultat[0].Autori.Count);
        }

        // Azuriraj knjigu

        [TestMethod]
        public void AzurirajKnjigu_AzuriraSvePodatke()
        {
            var izd = new Izdavac(1, "Izdavac1");
            var knjiga = new Knjiga(1, "12345", "Stari Naziv", DateTime.Parse("01.01.2020"), VrstaKnjige.NOVEL, izd);
            _db.knjige.Add(knjiga);
            _db.UpdateKnjigeCalled = false;

            var noviDatum = DateTime.ParseExact("15.03.2021", "dd.MM.yyyy", null);

            string rezultat = _servis.AzurirajKnjigu(
                1,                  
                "Novi Naziv",       
                "67890",            
                noviDatum,          
                "Novi Izdavac"     
            );

            Assert.AreEqual("Knjiga ID:1 uspješno ažurirana i sačuvana.", rezultat);
            Assert.AreEqual("Novi Naziv", knjiga.NazivKnjige);
            Assert.AreEqual("67890", knjiga.ISBN);
            Assert.AreEqual(noviDatum, knjiga.DatumObjavljivanja);
            Assert.AreEqual("Novi Izdavac", knjiga.Izdavac!.NazivIzdavaca);
            Assert.IsTrue(_db.UpdateKnjigeCalled);
        }

        [TestMethod]
        public void AzurirajKnjigu_PreskaceNekaPolja()
        {
            var izd = new Izdavac(1, "Izdavac1");
            var stariDatum = DateTime.Parse("01.01.2020");
            var knjiga = new Knjiga(1, "12345", "Stari Naziv", stariDatum, VrstaKnjige.NOVEL, izd);
            _db.knjige.Add(knjiga);
            _db.UpdateKnjigeCalled = false;

            var noviDatum = DateTime.ParseExact("15.03.2021", "dd.MM.yyyy", null);

            string rezultat = _servis.AzurirajKnjigu(
                1,                 
                "",                 
                "",                 
                noviDatum,         
                ""                  
            );

            Assert.AreEqual("Knjiga ID:1 uspješno ažurirana i sačuvana.", rezultat);
            Assert.AreEqual("Stari Naziv", knjiga.NazivKnjige); 
            Assert.AreEqual("12345", knjiga.ISBN); 
            Assert.AreEqual(noviDatum, knjiga.DatumObjavljivanja); 
            Assert.AreEqual("Izdavac1", knjiga.Izdavac!.NazivIzdavaca); 
            Assert.IsTrue(_db.UpdateKnjigeCalled);
        }

        [TestMethod]
        public void AzurirajKnjigu_KnjigaNePostoji_VracaGresku()
        {
            string rezultat = _servis.AzurirajKnjigu(
                999,
                "Novi Naziv",
                "67890",
                new DateTime(2021, 1, 1),
                "Novi Izdavac"
            );

            StringAssert.Contains(rezultat, "GREŠKA: Knjiga nije pronađena.");
            Assert.IsFalse(_db.UpdateKnjigeCalled);
        }

        // obrisi knjigu

        [TestMethod]
        public void ObrisiKnjigu_KnjigaPostoji_BrisanjeUspjesno()
        {
            var knjiga = new Knjiga(1, "12345", "Naziv za brisanje", DateTime.Parse("01.01.2020"), VrstaKnjige.NOVEL, new Izdavac(1, "Izdavac1"));
            _db.knjige.Add(knjiga);
            _db.UpdateKnjigeCalled = false;

            string rezultat = _servis.ObrisiKnjigu(1);

            Assert.AreEqual(0, _db.knjige.Count);
            StringAssert.Contains(rezultat, "Knjiga 'Naziv za brisanje' obrisana i JSON ažuriran.");
            Assert.IsTrue(_db.UpdateKnjigeCalled);
        }

        [TestMethod]
        public void ObrisiKnjigu_KnjigaNePostoji_VracaGresku()
        {
            string rezultat = _servis.ObrisiKnjigu(999);

            StringAssert.Contains(rezultat, "GREŠKA: Knjiga nije pronađena.");
            Assert.IsFalse(_db.UpdateKnjigeCalled);
        }

        // Obrisi neprimjerene recenzije

        [TestMethod]
        public void ObrisiNeprimjereneRecenzije_UklanjaRecenzije()
        {
            var k = new Knjiga(1, "12345", "Naziv", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"));
            var recenzijaZaBrisanje1 = new Recenzija { ID = 1, KnjigaID = 1, Komentar = "Uvredljivo sadrzaj" };
            var recenzijaZaBrisanje2 = new Recenzija { ID = 2, KnjigaID = 1, Komentar = "Drugi Uvredljiv komentar" };
            var recenzijaOK = new Recenzija { ID = 3, KnjigaID = 1, Komentar = "Normalna recenzija" };

            k.Recenzije = new List<Recenzija> { recenzijaZaBrisanje1, recenzijaZaBrisanje2, recenzijaOK };
            _db.Recenzije.AddRange(k.Recenzije); 
            _db.knjige.Add(k);

            string rezultat = _servis.ObrisiNeprimjereneRecenzije(1, "uvredljivo,komentar");

            Assert.AreEqual(1, k.Recenzije.Count);
            Assert.AreEqual(1, _db.Recenzije.Count); 
            Assert.AreEqual("Normalna recenzija", k.Recenzije[0].Komentar);
            StringAssert.Contains(rezultat, "Uklonjeno 2 neprimjerenih recenzija za knjigu 'Naziv'.");
        }

        [TestMethod]
        public void ObrisiNeprimjereneRecenzije_KnjigaNePostoji_VracaGresku()
        {
            string rezultat = _servis.ObrisiNeprimjereneRecenzije(999, "uvredljivo");
            StringAssert.Contains(rezultat, "GREŠKA: Knjiga nije pronađena.");
        }

        [TestMethod]
        public void ObrisiNeprimjereneRecenzije_PrazneKljucneRijeci_VracaPoruku()
        {
            var k = new Knjiga(1, "12345", "Naziv", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"));
            k.Recenzije = new List<Recenzija> { new Recenzija { ID = 1, Komentar = "Neki komentar" } };
            _db.knjige.Add(k);

            string rezultat = _servis.ObrisiNeprimjereneRecenzije(1, " ");
            StringAssert.Contains(rezultat, "Ključne riječi su prazne ili sadrže samo razmake. Nema brisanja.");
            Assert.AreEqual(1, k.Recenzije.Count);
        }

        // Sortiraj po datumu

        [TestMethod]
        public void SortirajPoDatumu_RedoslijedIspravan()
        {
            _db.knjige.Add(new Knjiga(1, "123", "Knjiga1", DateTime.Parse("2022-01-01"), VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            _db.knjige.Add(new Knjiga(2, "124", "Knjiga2", DateTime.Parse("2020-01-01"), VrstaKnjige.NOVEL, new Izdavac(2, "Izd")));
            _db.knjige.Add(new Knjiga(3, "125", "Knjiga3", DateTime.Parse("2021-01-01"), VrstaKnjige.NOVEL, new Izdavac(3, "Izd")));
            _db.UpdateKnjigeCalled = false;

            string rezultat = _servis.SortirajPoDatumu();

            Assert.AreEqual("Knjige sortirane po datumu (najstarije-najnovije) i sačuvane.", rezultat);
            Assert.AreEqual("Knjiga2", _db.knjige[0].NazivKnjige);
            Assert.AreEqual("Knjiga3", _db.knjige[1].NazivKnjige);
            Assert.AreEqual("Knjiga1", _db.knjige[2].NazivKnjige);
            Assert.IsTrue(_db.UpdateKnjigeCalled);
        }

        // Pretraga knjiga

        [TestMethod]
        public void PretragaKnjiga_PrazanPojam_VracaPraznuListu()
        {
            var rezultat = _servis.PretragaKnjiga("1", "");
            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PretragaKnjiga_PretragaPoNaslovu_Pronalazi()
        {
            _db.knjige.Add(new Knjiga(1, "123", "MojNaslov", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            _db.knjige.Add(new Knjiga(2, "124", "DrugiNaslov", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Izd")));

            var rezultat = _servis.PretragaKnjiga("1", "naslov");

            Assert.AreEqual(2, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "MojNaslov"));
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "DrugiNaslov"));
        }

        [TestMethod]
        public void PretragaKnjiga_PretragaPoAutoru_Pronalazi()
        {
            var autor = CreateAutor("Pero", "Peric", "01.01.1980");
            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")) { Autori = new List<Autor> { autor } };
            var k2 = new Knjiga(2, "124", "Naslov2", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Izd")) { Autori = new List<Autor> { autor } };
            var k3 = new Knjiga(3, "125", "DrugiAutor", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(3, "Izd")) { Autori = new List<Autor> { CreateAutor("Mujo", "Mujic", "01.01.1970") } };
            _db.knjige.Add(k1);
            _db.knjige.Add(k2);
            _db.knjige.Add(k3);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(2, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov2"));
        }
        //Pretraga knjiga white box testiranje
        [TestMethod]
        public void PrazanPojam_VracaPraznuListu()
        {
            var rezultat = _servis.PretragaKnjiga("1", "");
            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PretragaPoNazivu_PronadjenaKnjiga()
        {
            InicijalizujTestPodatke();
            var rezultat = _servis.PretragaKnjiga("1", "Derviš");
            Assert.IsTrue(rezultat.Count > 0);
            Assert.IsTrue(rezultat[0].NazivKnjige.Contains("Derviš"));
        }
            [TestMethod]
        public void PretragaPoAutoru_PronadjenAutor()
        {
            var autor = CreateAutor("Pero", "Peric", "01.01.1980");
            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")) { Autori = new List<Autor> { autor } };
            var k2 = new Knjiga(2, "124", "Naslov2", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Izd")) { Autori = new List<Autor> { autor } };
            var k3 = new Knjiga(3, "125", "DrugiAutor", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(3, "Izd")) { Autori = new List<Autor> { CreateAutor("Drugi", "Autor", "01.01.1970") } };
            _db.knjige.Add(k1);
            _db.knjige.Add(k2);
            _db.knjige.Add(k3);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(2, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov2"));
        }
        //Conditional Coverage

        [TestMethod]
        public void PretragaKnjiga_PojamNull_VracaPrazanRezultat()
        {
            var rezultat = _servis.PretragaKnjiga("1", null);
            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PretragaPoNazivu_KnjigaPostoji_VracaKnjigu()
        {
            _db.knjige.Add(new Knjiga(1, "123", "Naslov Knjige", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            _db.knjige.Add(new Knjiga(2, "124", "The Hobbit", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));

            var rezultat = _servis.PretragaKnjiga("1", "naslov");
            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov Knjige"));
        }
        // TC3: Pretraga po nazivu - nije pronađena knjiga
        [TestMethod]
        public void PretragaPoNazivu_KnjigaNePostoji_VracaPrazanRezultat()
        {
            _db.knjige.Add(new Knjiga(1, "123", "Nalov Knjige", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            _db.knjige.Add(new Knjiga(2, "124", "The Hobbit", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));

            var rezultat = _servis.PretragaKnjiga("1", "nepostojeca knjiga");

            Assert.AreEqual(0, rezultat.Count);
            Assert.IsFalse(rezultat.Any(k => k.NazivKnjige == "Nalov Knjige"));
            Assert.IsFalse(rezultat.Any(k => k.NazivKnjige == "The Hobbit"));
        }
        // TC4: Pretraga po nazivu - prazna baza
        // ========================================
        [TestMethod]
        public void PretragaPoNazivu_PraznaBaza_VracaPrazanRezultat()
        {
            // _db.knjige je prazna lista
            var rezultat = _servis.PretragaKnjiga("1", "bilo sta");

            Assert.AreEqual(0, rezultat.Count);
        }
        [TestMethod]
        public void PretragaPoAutoru_AutorPostoji_VracaKnjigu()
        {
            var autor = CreateAutor("Ime", "Prezime", "01.01.1952");
            var knjiga = new Knjiga(1, "123", "Naslov Knjige", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(knjiga);

            var rezultat = _servis.PretragaKnjiga("2", "ime");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov Knjige"));
        }
        // TC6: Pretraga po autoru - autor je null
        [TestMethod]
        public void PretragaPoAutoru_AutoriNull_PreskaciKnjigu()
        {
            var knjiga1 = new Knjiga(1, "123", "Knjiga bez autora", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = null
            };
            var autor2 = CreateAutor("J.K.", "Rowling", "31.07.1965");
            var knjiga2 = new Knjiga(2, "124", "Harry Potter", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor2 }
            };
            _db.knjige.Add(knjiga1);
            _db.knjige.Add(knjiga2);

            var rezultat = _servis.PretragaKnjiga("2", "rowling");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Harry Potter"));
            Assert.IsFalse(rezultat.Any(k => k.NazivKnjige == "Knjiga bez autora"));
        }
        // TC7: Pretraga po autoru - autor nije pronađen
        [TestMethod]
        public void PretragaPoAutoru_AutorNePostoji_VracaPrazanRezultat()
        {
            var autor = CreateAutor("J.K.", "Rowling", "31.07.1965");
            var knjiga = new Knjiga(1, "123", "Harry Potter", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(knjiga);
            var rezultat = _servis.PretragaKnjiga("2", "nepostojeći");
            Assert.AreEqual(0, rezultat.Count);
            Assert.IsFalse(rezultat.Any(k => k.NazivKnjige == "Harry Potter"));
        }
        // TC9: Pretraga po autoru - prazna lista autora
        [TestMethod]
        public void PretragaPoAutoru_PraznaListaAutora_VracaPrazanRezultat()
        {
            var knjiga = new Knjiga(1, "123", "Knjiga bez autora", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor>() // prazna lista
            };
            _db.knjige.Add(knjiga);

            var rezultat = _servis.PretragaKnjiga("2", "bilo koji autor");

            Assert.AreEqual(0, rezultat.Count);
        }


        [TestMethod]
        public void IfContainsNaziv_False_NeDodaje()
        {
            _db.knjige.Add(new Knjiga(1, "123", "Clean Code", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            var rezultat = _servis.PretragaKnjiga("1", "harry");
            Assert.AreEqual(0, rezultat.Count);
        }
        [TestMethod]
        public void IfAutoriNull_True_Continue()
        {
            var k = new Knjiga(1, "123", "Knjiga", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")){Autori = null};
            _db.knjige.Add(k);
            var rezultat = _servis.PretragaKnjiga("2", "knji");
            Assert.AreEqual(0, rezultat.Count);
        }
        [TestMethod]
        public void IfAutoriNull_False_NastavljaDalje()
        {
            var autor = CreateAutor("J.K.", "Rowling", "31.07.1965");
            var k = new Knjiga(1, "123", "Harry Potter", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")){Autori = new List<Autor> { autor }};
            _db.knjige.Add(k);
            var rezultat = _servis.PretragaKnjiga("2", "rowling");
            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(x => x.NazivKnjige == "Harry Potter"));
        }

        [TestMethod]
        public void IfContainsAutor_False_MatchOstajeFalse()
        {
            var autor = CreateAutor("J.K.", "Rowling", "31.07.1965");
            var k = new Knjiga(1, "123", "Harry Potter", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(k);
            var rezultat = _servis.PretragaKnjiga("2", "martin");

            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void IfMatch_True_Dodaje()
        {
            var autor = CreateAutor("J.K.", "Rowling", "31.07.1965");
            var k = new Knjiga(1, "123", "Harry Potter", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(k);

            var rezultat = _servis.PretragaKnjiga("2", "j.k. rowling");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(x => x.NazivKnjige == "Harry Potter"));
        }

        //Loop coverage
        [TestMethod]
        public void Test_Petlja1_PreskociUnutrasnjost()
        {
            _db.knjige.Clear(); 

            var rezultat = _servis.PretragaKnjiga("1", "naslov");

            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void Test_Petlja1_JedanProlaz()
        {
            _db.knjige.Clear();
            _db.knjige.Add(new Knjiga(1, "123", "MojNaslov", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));

            var rezultat = _servis.PretragaKnjiga("1", "naslov");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "MojNaslov"));
        }

        [TestMethod]
        public void Test_Petlja1_DvaProlaza()
        {
            _db.knjige.Clear();
            _db.knjige.Add(new Knjiga(1, "123", "MojNaslov", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd")));
            _db.knjige.Add(new Knjiga(2, "124", "DrugiNaslov", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Izd")));

            var rezultat = _servis.PretragaKnjiga("1", "DrugiNaslov");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "DrugiNaslov"));
        }

        [TestMethod]
        public void Test_Petlja1_MManjeOdN_MatchNa3()
        {
            _db.knjige.Clear();
            //n=5, m=3
            for (int i = 1; i <= 5; i++)
            {
                _db.knjige.Add(new Knjiga(i, $"12{i}", $"Naslov{i}", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(i, "Izd")));
            }

            var rezultat = _servis.PretragaKnjiga("1", "naslov3");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov3"));
         
        }
        public void Test_Petlja1_NMinus1Prolaza()
        {
            _db.knjige.Clear();
            for (int i = 1; i <= 5; i++)
            {
                _db.knjige.Add(new Knjiga(i, $"12{i}", $"Naslov{i}", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(i, "Izd")));
            }

            var rezultat = _servis.PretragaKnjiga("1", "naslov4");
          
            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov4"));
        }
        public void Test_Petlja1_NProlaza()
        {
            _db.knjige.Clear();

            for (int i = 1; i <= 5; i++)
            {
                _db.knjige.Add(new Knjiga(i, $"12{i}", $"Naslov{i}", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(i, "Izd")));
            }
            var rezultat = _servis.PretragaKnjiga("1", "naslov5");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov5"));
        }
        // 2.1) TESTIRANJE UNUTRASNJE PETLJE (vanjska petlja minimalno: 1 knjiga)

        [TestMethod]
        public void Test_Unutrasnja_PreskociUnutrasnjost_AutoriPrazni()
        {
            _db.knjige.Clear();

            // vanjska petlja: 1 prolaz (1 knjiga)
            // unutrasnja petlja: 0 prolaza (Autori = [])
            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor>()
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void Test_Unutrasnja_JedanProlaz_AutorCount1()
        {
            _db.knjige.Clear();
            var autor = CreateAutor("Pero", "Peric", "01.01.1980");
            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }

        [TestMethod]
        public void Test_Unutrasnja_DvaProlaza_AutorCount2()
        {
            _db.knjige.Clear();
            var a1 = CreateAutor("Mujo", "Mujic", "01.01.1970");
            var a2 = CreateAutor("Pero", "Peric", "01.01.1980");

            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { a1, a2 }
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }

        [TestMethod]
        public void Test_Unutrasnja_MManjeOdN_MatchNa3()
        {
            _db.knjige.Clear();
            var autori = new List<Autor>
                {
                    CreateAutor("A1", "B1", "01.01.1970"),
                    CreateAutor("A2", "B2", "01.01.1970"),
                    CreateAutor("Pero", "Peric", "01.01.1980"),
                    CreateAutor("A3", "B3", "01.01.1970"),
                    CreateAutor("A4", "B4", "01.01.1970")
                };

            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = autori
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }
        [TestMethod]
        public void Test_Unutrasnja_NMinus1Prolaza()
        {
            _db.knjige.Clear();
            var autori = new List<Autor>
                {
                    CreateAutor("A1", "B1", "01.01.1970"),
                    CreateAutor("A2", "B2", "01.01.1970"),
                    CreateAutor("A3", "B3", "01.01.1970"),
                    CreateAutor("Pero", "Peric", "01.01.1980"),
                    CreateAutor("A4", "B4", "01.01.1970")
                };

            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = autori
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }
        [TestMethod]
        public void Test_Unutrasnja_NProlaza()
        {
            _db.knjige.Clear();
            var autori = new List<Autor>
                {
                    CreateAutor("A1", "B1", "01.01.1970"),
                    CreateAutor("A2", "B2", "01.01.1970"),
                    CreateAutor("A3", "B3", "01.01.1970"),
                    CreateAutor("A4", "B4", "01.01.1970"),
                    CreateAutor("Pero", "Peric", "01.01.1980")
                };

            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = autori
            };
            _db.knjige.Add(k1);
            var rezultat = _servis.PretragaKnjiga("2", "pero peric");
            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }

        // 2.2) TESTIRANJE VANJSKE PETLJE (unutrasnja petlja minimalno: 1 autor po knjizi)

        [TestMethod]
        public void Test_VanjskaPetlja_PreskociUnutrasnjost()
        {
            _db.knjige.Clear();
            var rezultat = _servis.PretragaKnjiga("2", "pero peric");
            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void Test_VanjskaPetlja_JedanProlaz()
        {
            _db.knjige.Clear();

            // vanjska petlja: 1 prolaz (1 knjiga)
            // unutrasnja: minimalno (1 autor)
            var autor = CreateAutor("Pero", "Peric", "01.01.1980");
            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            _db.knjige.Add(k1);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(1, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
        }

        [TestMethod]
        public void Test_VanjskaPetlja_DvaProlaza()
        {
            _db.knjige.Clear();

            var autor = CreateAutor("Pero", "Peric", "01.01.1980");

            var k1 = new Knjiga(1, "123", "Naslov1", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };
            var k2 = new Knjiga(2, "124", "Naslov2", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(2, "Izd"))
            {
                Autori = new List<Autor> { autor }
            };

            _db.knjige.Add(k1);
            _db.knjige.Add(k2);

            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(2, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov2"));
        }

        [TestMethod]
        public void Test_VanjskaPetlja_MProlaza()
        {
            _db.knjige.Clear();

            var autor = CreateAutor("Pero", "Peric", "01.01.1980");

            for (int i = 1; i <= 5; i++)
            {
                _db.knjige.Add(new Knjiga(i, $"12{i}", $"Naslov{i}", DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(i, "Izd"))
                {
                    Autori = new List<Autor> { autor } 
                });
            }
            var rezultat = _servis.PretragaKnjiga("2", "pero peric");

            Assert.AreEqual(5, rezultat.Count);
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov1"));
            Assert.IsTrue(rezultat.Any(k => k.NazivKnjige == "Naslov5"));
        }


        // Preporuka knjiga

        [TestMethod]
        public void PrijaviKorisnika_UspjesnaPrijava()
        {
            var korisnik = _servis.PrijaviKorisnika("test@a.com", "123");
            Assert.IsNotNull(korisnik);
            Assert.AreEqual("test@a.com", korisnik!.Email);
        }

        [TestMethod]
        public void PrijaviKorisnika_NeuspjesnaPrijava()
        {
            var korisnik = _servis.PrijaviKorisnika("nepostoji@a.com", "999");
            Assert.IsNull(korisnik);
        }

        [TestMethod]
        public void PreporuciKnjige_KorisnikBezProcitanih_VracaPraznuListu()
        {
            _db.knjige.Add(new Knjiga(1, "Knjiga1", "ISBN1", DateTime.Today, VrstaKnjige.NOVEL, new Izdavac(1, "Izdavac1")));
            
            var rezultat = _servis.PreporuciKnjige(100);

            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PreporuciKnjige_NemaSlicnih_VracaPraznuListu()
        {
            var procitana = new Knjiga(1, "Procitana", "ISBN1", DateTime.Today, VrstaKnjige.NOVEL, new Izdavac(1, "Izdavac1"));
            procitana.Recenzije = new List<Recenzija> { new Recenzija { ID = 1, KorisnikID = 100, Komentar = "Ok" } };
            _db.Recenzije.Add(procitana.Recenzije.First());
            _db.knjige.Add(procitana);

            _db.knjige.Add(new Knjiga(2, "Nepovezana", "ISBN2", DateTime.Parse("01.01.1900"), VrstaKnjige.POETRY, new Izdavac(2, "Izdavac2")));

            var rezultat = _servis.PreporuciKnjige(100);

            Assert.AreEqual(0, rezultat.Count);
        }

        [TestMethod]
        public void PreporuciKnjige_ViseSlicnihKnjiga_PrikazujeNajvisePeto()
        {
            var autor = CreateAutor("Marko", "Markovic", "01.01.1980");

            var procitana = new Knjiga(1, "Procitana", "ISBN1", DateTime.Today.AddYears(-1), VrstaKnjige.NOVEL, new Izdavac(1, "Izdavac1"));
            procitana.Autori = new List<Autor> { autor };
            procitana.Recenzije = new List<Recenzija> { new Recenzija { ID = 1, KorisnikID = 100 } };
            _db.Recenzije.Add(procitana.Recenzije.First());
            _db.knjige.Add(procitana);

            for (int i = 2; i <= 7; i++)
            {
                var k = new Knjiga(i, $"Slicna{i}", $"ISBN{i}", DateTime.Today, VrstaKnjige.NOVEL, new Izdavac(i, $"Izdavac{i}"));
                _db.knjige.Add(k);
            }

            var rezultat = _servis.PreporuciKnjige(100);

            Assert.AreEqual(5, rezultat.Count);
        }
    }
}