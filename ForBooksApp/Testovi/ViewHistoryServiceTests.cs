using ForBooks.Data;
using ForBooks.Models;
using ForBooks.Services;
using global::ForBooks.Data;
using global::ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Testovi
{
    [TestClass]
    public class ViewHistoryServiceTests
    {
        private DbClass _db;
        private ViewHistoryService _service;

        [TestInitialize]
        public void PostaviOkruzenje()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "DataFiles", "viewHistories.json");
            if (File.Exists(path))
                File.Delete(path); // remove old data

            _db = new DbClass();
            _service = new ViewHistoryService(_db);
        }


        [TestMethod]
        public void RecordView_PregledanaKnjiga_IspravanBrojZapisanihPregleda()
        {
            
            string korisnik = "testKorisnik";
            string knjiga = "Test Knjiga";

            
            _service.RecordView(korisnik, knjiga);

            
            Assert.AreEqual(1, _db.ViewHistories.Count);
            var vh = _db.ViewHistories.First();
            Assert.AreEqual(korisnik, vh.User);
            Assert.AreEqual(knjiga, vh.ViewedItem);
            Assert.IsTrue((DateTime.Now - vh.ViewedAt).TotalSeconds < 5); 
        }

        [TestMethod]
        public void RecordView_UneseniPreglediKorisnika_SviZapisaniPregledi()
        {
            
            _service.RecordView("korisnik1", "Knjiga1");
            _service.RecordView("korisnik2", "Knjiga2");

            var sviPregledi = _service.GetAllViews();

            
            Assert.AreEqual(2, sviPregledi.Count);
            Assert.IsTrue(sviPregledi.Any(v => v.User == "korisnik1" && v.ViewedItem == "Knjiga1"));
            Assert.IsTrue(sviPregledi.Any(v => v.User == "korisnik2" && v.ViewedItem == "Knjiga2"));
        }

        [TestMethod]
        public void RecordView_UneseniPreglediKorisnika_PovecanjeIda()
        {
            
            _service.RecordView("korisnikA", "KnjigaA");
            _service.RecordView("korisnikB", "KnjigaB");

            
            var sviPregledi = _service.GetAllViews();

           
            Assert.AreEqual(2, sviPregledi.Count);
            Assert.AreEqual(1, sviPregledi[0].Id);
            Assert.AreEqual(2, sviPregledi[1].Id);
        }
    }
}

