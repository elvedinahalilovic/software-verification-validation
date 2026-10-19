using ForBooks.Data;
using ForBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForBooks.Services
{
    public class WishlistService
    {
        private readonly DbClass _db;

        public WishlistService(DbClass db)
        {
            _db = db;

            if (_db.Wishlists == null)
                _db.Wishlists = new List<WishlistItem>();
        }

        // Login sada PRIMA username i password, ne čita sa konzole
        public (Korisnik? user, string message) Login(string username, string password)
        {
            var user = _db.korisnici
                .FirstOrDefault(u => u.Username == username && u.Lozinka == password);

            if (user == null)
                return (null, "Neispravan username ili lozinka.");

            return (user, "Uspješan login.");
        }

        // Dodaj knjigu u wishlist
        public string AddToWishlist(Korisnik korisnik, string nazivKnjige)
        {
            var knjiga = _db.knjige
                .FirstOrDefault(k => k.NazivKnjige.Equals(nazivKnjige, StringComparison.OrdinalIgnoreCase));

            if (knjiga == null)
                return "Knjiga ne postoji.";

            var wish = _db.Wishlists.FirstOrDefault(w => w.Username == korisnik.Username);

            if (wish == null)
            {
                wish = new WishlistItem { Username = korisnik.Username };
                _db.Wishlists.Add(wish);
            }

            if (!wish.BookTitles.Contains(knjiga.NazivKnjige))
            {
                wish.BookTitles.Add(knjiga.NazivKnjige);
                _db.UpdateWishlist();
                return "Knjiga dodana u wishlist.";
            }

            return "Knjiga već postoji u wishlisti.";
        }

        // Ukloni knjigu
        public string RemoveFromWishlist(Korisnik korisnik, string nazivKnjige)
        {
            var wish = _db.Wishlists.FirstOrDefault(w => w.Username == korisnik.Username);

            if (wish == null || !wish.BookTitles.Contains(nazivKnjige))
                return "Knjiga nije u wishlisti.";

            wish.BookTitles.Remove(nazivKnjige);
            _db.UpdateWishlist();
            return "Knjiga uklonjena iz wish liste.";
        }

        // Vrati wishlist kao string (za prikaz)
        public string ShowWishlist(Korisnik korisnik)
        {
            var wish = _db.Wishlists.FirstOrDefault(w => w.Username == korisnik.Username);

            if (wish == null || wish.BookTitles.Count == 0)
                return "Wishlist je prazan.";

            return "Wishlist:\n" + string.Join("\n", wish.BookTitles.Select(t => "- " + t));
        }

        // Vrati wishlist kao listu (za testove ili API)
        public List<string> GetWishlist(string username)
        {
            var wish = _db.Wishlists.FirstOrDefault(w => w.Username == username);
            return wish?.BookTitles ?? new List<string>();
        }
    }
}
