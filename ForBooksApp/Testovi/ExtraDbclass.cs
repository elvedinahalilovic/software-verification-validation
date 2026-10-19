using ForBooks.Data;
using ForBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestiranje
{
    public class ExtraDbClass : DbClass
    {
        public ExtraDbClass()
        {
            korisnici = new List<Korisnik>
        {
            new Korisnik { Username="user1", Lozinka="pass1" },
            new Korisnik { Username="user2", Lozinka="pass2" } 
        };

            knjige = new List<Knjiga>
        {
            new Knjiga { NazivKnjige="Knjiga1" },
            new Knjiga { NazivKnjige="Knjiga2" }
        };
        }

        public bool UpdateWishlistCalled { get; set; } = false;

        public override void UpdateWishlist() => UpdateWishlistCalled = true;
    }



}
