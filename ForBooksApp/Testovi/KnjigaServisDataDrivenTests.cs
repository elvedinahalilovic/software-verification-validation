using ForBooks.Data;
using ForBooks.Enums;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis; 
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UnitTestiranje
{
    public class ObrisiRecenzijeTestModel
    {
        public string Description { get; set; } = null!;
        public string Keywords { get; set; } = null!;
        public List<ReviewData> Reviews { get; set; } = null!;
        public int ExpectedRemovedCount { get; set; }

        public class ReviewData
        {
            public string Komentar { get; set; } = null!;
            public double Ocjena { get; set; }
        }
    }

    [TestClass]
    public class KnjigaServisDataDrivenTests
    {
        private FakeDbClass db = null!;
        private KnjigaServis servis = null!;

        [TestInitialize]
        [MemberNotNull(nameof(db), nameof(servis))] 
        public void Setup()
        {
            (db, servis) = CreateServis();
        }

        private (FakeDbClass db, KnjigaServis servis) CreateServis()
        {
            var db = new FakeDbClass();
            var servis = new KnjigaServis(db);
            return (db, servis);
        }

        public static IEnumerable<object[]> ObrisiNeprimjereneRecenzijeTestDataFromJson()
        {
            // JSON string - simulira sadržaj koji bi se čitao iz vanjske JSON datoteke
            string jsonString = @"
            [
              {
                ""Description"": ""Test 1: Uklanjanje 2 od 3 recenzije - Ključne riječi 'uvredljivo, loša'"",
                ""Keywords"": ""uvredljivo, loša"",
                ""Reviews"": [
                  { ""Komentar"": ""Knjiga je uvredljivo loša!"", ""Ocjena"": 1.0 },
                  { ""Komentar"": ""Ovo je dobra knjiga."", ""Ocjena"": 4.0 },
                  { ""Komentar"": ""Nema ništa loša na ovome."", ""Ocjena"": 2.0 }
                ],
                ""ExpectedRemovedCount"": 2
              },
              {
                ""Description"": ""Test 2: Case-insensitive provjera - Ključne riječi 'hate, kritika' (HATE/KRITIKA)"",
                ""Keywords"": ""hate, kritika"",
                ""Reviews"": [
                  { ""Komentar"": ""This is HATE speech."", ""Ocjena"": 1.0 },
                  { ""Komentar"": ""Dobra KRITIKA od autora"", ""Ocjena"": 5.0 }, 
                  { ""Komentar"": ""Samo obican tekst"", ""Ocjena"": 3.0 }
                ],
                ""ExpectedRemovedCount"": 2
              }
            ]";
            
            var data = JsonSerializer.Deserialize<List<ObrisiRecenzijeTestModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return data!
                .Select(testCase => new object[] { testCase })
                .ToList();
        }

        [DataTestMethod]
        [DynamicData(nameof(ObrisiNeprimjereneRecenzijeTestDataFromJson), DynamicDataSourceType.Method)]
        [TestCategory("DataDrivenTest")]
        public void ObrisiNeprimjereneRecenzije_DynamicDataJson_ShouldRemoveCorrectCount(ObrisiRecenzijeTestModel testCase)
        {
            
            int knjigaId = 1;
            int recenzijaIdCounter = 1;

            // Kreiranje knjige i dodavanje u FakeDB
            var knjiga = new Knjiga(knjigaId, "isbn", "Knjiga za test", DateTime.Now, VrstaKnjige.NOVEL, null!)
            {
                Recenzije = new List<Recenzija>()
            };
            db.knjige.Add(knjiga); 

            // Popunjavanje liste recenzija na knjizi na osnovu JSON podataka
            foreach (var rData in testCase.Reviews)
            {
                // Kreiramo korisnika za recenziju
                var korisnik = new Korisnik(recenzijaIdCounter, "Ime", "Prezime", $"kor{recenzijaIdCounter}", $"k{recenzijaIdCounter}@test.com", "loz", "111");
                db.korisnici.Add(korisnik); // Dodavanje korisnika u db
                
                var rec = new Recenzija(recenzijaIdCounter++, korisnik, knjiga, rData.Ocjena, rData.Komentar);
                knjiga.Recenzije.Add(rec);
                db.Recenzije.Add(rec); // Ažuriranje i glavne liste recenzija
            }

            int initialCount = knjiga.Recenzije.Count;
            servis.ObrisiNeprimjereneRecenzije(knjigaId, testCase.Keywords);

            int finalCount = knjiga.Recenzije.Count;
            int actualRemoved = initialCount - finalCount;
            
            Assert.AreEqual(testCase.ExpectedRemovedCount, actualRemoved, 
                $"Test '{testCase.Description}' nije uspio. Očekivano obrisano: {testCase.ExpectedRemovedCount}, Stvarno obrisano: {actualRemoved}.");
        }
        
        [DataTestMethod]
        [DataRow(1, "", 0, "Prazne ključne riječi - ne bi trebalo obrisati ništa")]
        [DataRow(1, "   ", 0, "Samo whitespace ključne riječi - ne bi trebalo obrisati ništa")]
        [DataRow(1, "ne-postojeci-tekst", 0, "Nepostojeće ključne riječi - ne bi trebalo obrisati ništa")]
        [TestCategory("DataDrivenTest")]
        public void ObrisiNeprimjereneRecenzije_DataRow_EdgeCases(int knjigaId, string keywords, int expectedRemovedCount, string description)
        {
            var knjiga = new Knjiga(knjigaId, "isbn", "Knjiga za test", DateTime.Now, VrstaKnjige.NOVEL, null!)
            {
                Recenzije = new List<Recenzija>()
            };
            db.knjige.Add(knjiga);
            
            knjiga.Recenzije.Add(new Recenzija(1, new Korisnik(1,"A","B","c","d","e","f"), knjiga, 5, "ok komentar 1"));
            knjiga.Recenzije.Add(new Recenzija(2, new Korisnik(2,"A","B","c","d","e","f"), knjiga, 4, "ok komentar 2"));
            knjiga.Recenzije.Add(new Recenzija(3, new Korisnik(3,"A","B","c","d","e","f"), knjiga, 3, "ok komentar 3"));
            
            db.Recenzije.AddRange(knjiga.Recenzije);
            db.korisnici.Add(new Korisnik(1,"A","B","c","d","e","f"));
            db.korisnici.Add(new Korisnik(2,"A","B","c","d","e","f"));
            db.korisnici.Add(new Korisnik(3,"A","B","c","d","e","f"));

            int initialCount = knjiga.Recenzije.Count;
            servis.ObrisiNeprimjereneRecenzije(knjigaId, keywords);

            int finalCount = knjiga.Recenzije.Count;
            int actualRemoved = initialCount - finalCount;
            
            Assert.AreEqual(expectedRemovedCount, actualRemoved, 
                $"Test '{description}' nije uspio. Očekivano obrisano: {expectedRemovedCount}, Stvarno obrisano: {actualRemoved}.");
        }
    }
}