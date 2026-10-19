using ForBooks.Models;
using System.Collections.Generic;
using System.Linq;

namespace ForBooks.Data
{
    public class DbClass
    {
        public List<Korisnik> korisnici { get; set; }
        public List<Knjiga> knjige { get; set; }
        public List<Autor> autori { get; set; }
        public List<Recenzija> Recenzije { get; set; }
        public List<WishlistItem> Wishlists { get; set; }
        public List<ViewHistory> ViewHistories { get; set; }

        public DbClass()
        {
            korisnici = FileHelper.LoadFromFile<Korisnik>("korisnici");
            knjige = FileHelper.LoadFromFile<Knjiga>("knjige");
            autori = FileHelper.LoadFromFile<Autor>("autori");
            Recenzije = FileHelper.LoadFromFile<Recenzija>("recenzije");
            Wishlists = FileHelper.LoadFromFile<WishlistItem>("wishlist");
            ViewHistories = FileHelper.LoadFromFile<ViewHistory>("viewHistories");

            foreach (var r in Recenzije)
            {
                r.Knjiga = knjige.FirstOrDefault(k => k.ID == r.KnjigaID);
                r.Korisnik = korisnici.FirstOrDefault(u => u.ID == r.KorisnikID);
            }

            foreach (var knjiga in knjige)
            {
                if (knjiga.Recenzije == null)
                    knjiga.Recenzije = new List<Recenzija>();

                foreach (var recenzija in Recenzije)
                {
                    if (recenzija.KnjigaID == knjiga.ID)
                    {
                        knjiga.Recenzije.Add(recenzija);
                    }
                }
            }
        }


        // Korisnici
        public void AddKorisnik(Korisnik korisnik)
        {
            korisnici.Add(korisnik);
            FileHelper.SaveToFile("korisnici", korisnici);
        }

        public Korisnik? GetKorisnik(int korisnikId)
        {
            return korisnici.FirstOrDefault(u => u.ID == korisnikId);
        }

        public virtual void UpdateKorisnici()
        {
            FileHelper.SaveToFile("korisnici", korisnici);
        }

        // Knjige
        public virtual void AddKnjiga(Knjiga knjiga) 
        {
            knjige.Add(knjiga);
            FileHelper.SaveToFile("knjige", knjige);
        }

        public Knjiga? GetKnjiga(int knjigaId)
        {
            return knjige.FirstOrDefault(b => b.ID == knjigaId);
        }

        public virtual void UpdateKnjige() 
        {
            FileHelper.SaveToFile("knjige", knjige);
        }

        // Autori
        public virtual void AddAutor(Autor autor) 
        {
            autori.Add(autor);
            FileHelper.SaveToFile("autori", autori);
        }
       
        public virtual void UpdateRecenzije()
        {
            FileHelper.SaveToFile("recenzije", Recenzije);
        }

        public virtual void UpdateWishlist()
        {
            FileHelper.SaveToFile("wishlist", Wishlists);
        }
        public void AddView(ViewHistory vh)
        {
            ViewHistories.Add(vh);
            UpdateViewHistory();
        }

        public virtual void UpdateViewHistory() 
        {
            FileHelper.SaveToFile("viewHistories", ViewHistories);
        }

    }
}