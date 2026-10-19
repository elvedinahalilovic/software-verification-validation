using ForBooks.Data;
using ForBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestiranje
{
    public class FakeDbClass : DbClass
    {
        public bool UpdateKnjigeCalled { get; set; } = false;
        public bool AddKnjigaCalled { get; set; } = false;
        public bool AddAutorCalled { get; set; } = false;
        public bool UpdateWishlistCalled { get; set; } = false;
        public bool UpdateViewHistoryCalled { get; set; } = false;      
        public FakeDbClass()
        {
            korisnici = new List<Korisnik>();
            knjige = new List<Knjiga>();
            autori = new List<Autor>();
            Recenzije = new List<Recenzija>();
            Wishlists = new List<WishlistItem>();
            ViewHistories = new List<ViewHistory>();
        }
        public override void UpdateKorisnici(){}
        public override void UpdateRecenzije() { }     
        public override void UpdateWishlist() 
        { 
            UpdateWishlistCalled = true;
        }
        
        public override void UpdateViewHistory() 
        { 
            UpdateViewHistoryCalled = true;
        }

        public override void AddKnjiga(Knjiga knjiga)
        {
            AddKnjigaCalled = true;
            if (knjiga.ID <= 0)
                knjiga.ID = this.knjige.Count > 0 ? this.knjige.Max(k => k.ID) + 1 : 1;

            knjige.Add(knjiga);
        }

        public override void UpdateKnjige()
        {
            UpdateKnjigeCalled = true;
        }

        public override void AddAutor(Autor autor)
        {
            AddAutorCalled = true;
            if (autor.ID <= 0)
                autor.ID = this.autori.Count > 0 ? this.autori.Max(a => a.ID) + 1 : 1;
            
            autori.Add(autor);
        }
    }
}