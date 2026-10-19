using System.Collections.Generic;
using ForBooks.Models;

namespace ForBooks.Services
{
    public interface IRecenzijaService
    {
        Recenzija DodajRecenziju(int korisnikId, int knjigaId, double ocjena, string komentar);

        List<Recenzija> DohvatiSveRecenzije();

        List<Recenzija> DohvatiRecenzijeZaKnjigu(int knjigaId);

        (double Prosjek, int BrojRecenzija) IzracunajProsjecnuOcjenuKnjige(int knjigaId);

        void ObrisiRecenziju(int recenzijaId);

        Recenzija DohvatiRecenzijuPoId(int recenzijaId);
    }
}
