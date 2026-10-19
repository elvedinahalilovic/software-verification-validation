using ForBooks.Data;
using ForBooks.Enums;
using ForBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Diagnostics; 

namespace ForBooks.Services
{
    public class KnjigaServis
    {
        private readonly DbClass _db;

        public KnjigaServis(DbClass db)
        {
            _db = db;
        }

        private bool IsNullOrWhiteSpace(string s)
        {
            if (s == null) return true;
            for (int i = 0; i < s.Length; i++)
                if (!char.IsWhiteSpace(s[i])) return false;
            return true;
        }

        private bool CaseInsensitiveContains(string haystack, string needle)
        {
            if (needle == null || haystack == null) return false;

            string a = haystack.ToLower();
            string b = needle.ToLower();

            for (int i = 0; i <= a.Length - b.Length; i++)
            {
                int k = 0;
                while (k < b.Length && a[i + k] == b[k]) k++;
                if (k == b.Length) return true;
            }
            return false;
        }


        public string DodajKnjigu(string isbn, string naziv, DateTime datum, VrstaKnjige vrsta, string izdavacNaziv, List<Autor> autori)
        {
            if (IsNullOrWhiteSpace(isbn)) return "GREŠKA: Neispravan ISBN.";
            if (IsNullOrWhiteSpace(naziv)) return "GREŠKA: Neispravan naziv.";
            if (IsNullOrWhiteSpace(izdavacNaziv)) return "GREŠKA: Neispravan izdavač.";
            if (autori == null || autori.Count == 0) return "GREŠKA: Knjiga mora imati barem jednog autora.";

            var izdavac = new Izdavac(_db.knjige.Count + 1, izdavacNaziv);

            foreach (var autor in autori)
            {
                if (!_db.autori.Any(a => a.ImeAutora == autor.ImeAutora && a.PrezimeAutora == autor.PrezimeAutora && a.DatumRodjenja == autor.DatumRodjenja))
                {
                    autor.ID = _db.autori.Count + 1; 
                    _db.AddAutor(autor);
                }
            }

            var nova = new Knjiga(_db.knjige.Count + 1, isbn, naziv, datum, vrsta, izdavac)
            {
                Autori = autori
            };

            _db.AddKnjiga(nova);
            return $"Knjiga '{naziv}' uspješno dodana i sačuvana.";
        }

        public List<Knjiga> PrikaziSveKnjige()
        {
            return _db.knjige.ToList();
        }

        public string AzurirajKnjigu(int id, string noviNaziv, string noviISBN, DateTime? noviDatum, string noviIzdavacNaziv)
        {
            var k = _db.GetKnjiga(id);
            if (k == null) { return "GREŠKA: Knjiga nije pronađena."; }

            if (!string.IsNullOrEmpty(noviNaziv)) k.PromijeniNaziv(noviNaziv);
            if (!string.IsNullOrEmpty(noviISBN)) k.PromijeniISBN(noviISBN);
            if (noviDatum.HasValue) k.DatumObjavljivanja = noviDatum.Value;
            if (!string.IsNullOrEmpty(noviIzdavacNaziv))
                k.Izdavac = new Izdavac(k.Izdavac?.ID ?? 1, noviIzdavacNaziv);

            _db.UpdateKnjige();
            return $"Knjiga ID:{id} uspješno ažurirana i sačuvana.";
        }

        public string ObrisiKnjigu(int id)
        {
            var k = _db.GetKnjiga(id);
            if (k == null) { return "GREŠKA: Knjiga nije pronađena."; }

            int idx = -1;
            for (int x = 0; x < _db.knjige.Count; x++)
                if (_db.knjige[x].ID == id) { idx = x; break; }

            if (idx >= 0)
            {
                string naziv = _db.knjige[idx].NazivKnjige;
                _db.knjige.RemoveAt(idx);
                _db.UpdateKnjige();
                return $"Knjiga '{naziv}' obrisana i JSON ažuriran.";
            }
            return "GREŠKA: Knjiga nije pronađena. (Interna greška)";
        }

        public string ObrisiNeprimjereneRecenzije(int idKnjige, string kljucneRijeci)
        {
            var k = _db.GetKnjiga(idKnjige);
            if (k == null) { return "GREŠKA: Knjiga nije pronađena."; }
            if (k.Recenzije == null || k.Recenzije.Count == 0) { return "Nema recenzija za ovu knjigu."; }

            if (IsNullOrWhiteSpace(kljucneRijeci))
            {
                return "Ključne riječi su prazne ili sadrže samo razmake. Nema brisanja.";
            }
            
            var tokens = kljucneRijeci.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var recenzijeZaBrisanje = new List<Recenzija>();
            
            for (int i = 0; i < k.Recenzije.Count; i++)
            {
                var recenzija = k.Recenzije[i];
                var komentar = recenzija.Komentar ?? "";
                
                for (int t = 0; t < tokens.Length; t++)
                {
                    var token = tokens[t].Trim();
                    if (CaseInsensitiveContains(komentar, token))
                    { 
                        recenzijeZaBrisanje.Add(recenzija); 
                        break; 
                    }
                }
            }
            
            int removed = recenzijeZaBrisanje.Count;
            
            // Uklanjanje..
            foreach (var recenzija in recenzijeZaBrisanje)
            {
                // ..iz liste recenzija knjige
                k.Recenzije.Remove(recenzija);
                
                // ..iz globalne liste baze podataka 
                _db.Recenzije.Remove(recenzija); 
            }
            
            if(removed > 0)
                _db.UpdateKnjige();
                
            return $"Uklonjeno {removed} neprimjerenih recenzija za knjigu '{k.NazivKnjige}'.";
        }

        public string SortirajPoDatumu()
        {
            int n = _db.knjige.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (_db.knjige[j].DatumObjavljivanja > _db.knjige[j + 1].DatumObjavljivanja)
                    {
                        var tmp = _db.knjige[j];
                        _db.knjige[j] = _db.knjige[j + 1];
                        _db.knjige[j + 1] = tmp;
                    }
                }
            }
            _db.UpdateKnjige();
            return "Knjige sortirane po datumu (najstarije-najnovije) i sačuvane.";
        }
        /* private static bool ContainsIgnoreCase(string text, string pattern)
         {
             if (text == null) return false;
             return text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
         }*/
         public List<Knjiga> PretragaKnjiga(string mod, string pojam)
          {
              var rezultati = new List<Knjiga>();
              if (IsNullOrWhiteSpace(pojam)) { return rezultati; }
              if (mod == "1")
              {
                  for (int i = 0; i < _db.knjige.Count; i++)
                  {
                      var k = _db.knjige[i];
                      if (CaseInsensitiveContains(k.NazivKnjige ?? "", pojam))

                      {
                          rezultati.Add(k);
                      }
                  }
              }
              else // Autor

              {
                  for (int i = 0; i < _db.knjige.Count; i++)
                  {
                      var k = _db.knjige[i];
                      if (k.Autori == null) continue;
                      bool match = false;

                      for (int a = 0; a < k.Autori.Count; a++)
                      {
                          var au = k.Autori[a];
                          string punoIme = $"{au.ImeAutora} {au.PrezimeAutora}";
                          if (CaseInsensitiveContains(punoIme, pojam))

                          { match = true; break; }
                      }
                      if (match)
                      {
                          rezultati.Add(k);
                      }
                  }
              }
              return rezultati;
          }
        /*  private static bool ContainsCI(string? text, string pojam)
           {
               return !string.IsNullOrEmpty(text) &&
                      text.IndexOf(pojam, StringComparison.OrdinalIgnoreCase) >= 0;
           }*/
       /* private static bool ContainsIgnoreCase(string text, string pattern)
        {
            if (text == null) return false;
            return text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool AutoriSadrzePojam(List<Autor>? autori, string pojam)
         {
             if (autori == null) return false;

             return autori.Any(autor => ContainsIgnoreCase(autor.ImeAutora, pojam) || ContainsIgnoreCase(autor.PrezimeAutora, pojam));
         }

         public List<Knjiga> PretragaKnjiga(string mod, string pojam)
         {
             if (string.IsNullOrWhiteSpace(pojam))
                 return new List<Knjiga>();

             return mod == "1"
                 ? PretragaPoNazivu(pojam)
                 : PretragaPoAutoru(pojam);
         }

         private List<Knjiga> PretragaPoNazivu(string pojam)
         {
             return _db.knjige
                 .Where(knjiga => ContainsIgnoreCase(knjiga.NazivKnjige, pojam))
                 .ToList();
         }

         private List<Knjiga> PretragaPoAutoru(string pojam)
         {
             return _db.knjige
                 .Where(knjiga => AutoriSadrzePojam(knjiga.Autori, pojam))
                 .ToList();
         }*/


        public Korisnik? PrijaviKorisnika(string email, string sifra)
        {
            if (IsNullOrWhiteSpace(email) || IsNullOrWhiteSpace(sifra))
            {
                return null;
            }

            for (int i = 0; i < _db.korisnici.Count; i++)
            {
                if (_db.korisnici[i].Email == email && _db.korisnici[i].Lozinka == sifra)
                {
                    return _db.korisnici[i];
                }
            }
            return null;
        }

        public List<(Knjiga Knjiga, List<string> Razlozi)> PreporuciKnjige(int korisnikId)
        {
            // Pronađi knjige koje je korisnik pročitao (na osnovu recenzija)
            var procitaneKnjige = new List<Knjiga>();
            for (int i = 0; i < _db.knjige.Count; i++)
            {
                var knjiga = _db.knjige[i];
                if (knjiga.Recenzije != null)
                {
                    for (int j = 0; j < knjiga.Recenzije.Count; j++)
                    {
                        if (knjiga.Recenzije[j].KorisnikID == korisnikId)
                        {
                            procitaneKnjige.Add(knjiga);
                            break;
                        }
                    }
                }
            }

            if (procitaneKnjige.Count == 0)
            {
                return new List<(Knjiga, List<string>)>();
            }

            var preporukeSaSkorom = new List<(Knjiga Knjiga, int Skor, List<string> Razlozi)>();

            for (int i = 0; i < _db.knjige.Count; i++)
            {
                var knjiga = _db.knjige[i];

                if (procitaneKnjige.Any(p => p.ID == knjiga.ID)) continue;

                int skor = 0;
                var razlozi = new List<string>();

                foreach (var procitana_knjiga in procitaneKnjige)
                {
                    // Isti žanr
                    if (knjiga.VrstaKnjige == procitana_knjiga.VrstaKnjige)
                    {
                        skor += 3;
                        string razlog = $"Isti tip: {knjiga.VrstaKnjige}";
                        if (!razlozi.Contains(razlog)) razlozi.Add(razlog);
                    }

                    // Zajednički autor
                    if (knjiga.Autori != null && procitana_knjiga.Autori != null)
                    {
                        foreach (var autorKnjige in knjiga.Autori)
                        {
                            foreach (var autorProcitane in procitana_knjiga.Autori)
                            {
                                if (autorKnjige.ID == autorProcitane.ID)
                                {
                                    skor += 5;
                                    string autor = $"{autorKnjige.ImeAutora} {autorKnjige.PrezimeAutora}";
                                    string razlog = $"Autor: {autor}";
                                    if (!razlozi.Contains(razlog)) razlozi.Add(razlog);
                                }
                            }
                        }
                    }

                    // Slična godina izdavanja 
                    int razlikaGodina = Math.Abs(knjiga.DatumObjavljivanja.Year - procitana_knjiga.DatumObjavljivanja.Year);

                    if (razlikaGodina <= 3)
                    {
                        skor += 2;
                        string razlog = "Slična godina izdavanja (unutar 3 godine)";
                        if (!razlozi.Contains(razlog)) razlozi.Add(razlog);
                    }

                    // Isti izdavač
                    if (knjiga.Izdavac != null && procitana_knjiga.Izdavac != null &&
                        knjiga.Izdavac.NazivIzdavaca == procitana_knjiga.Izdavac.NazivIzdavaca)
                    {
                        skor += 1;
                        string razlog = $"Izdavač: {knjiga.Izdavac.NazivIzdavaca}";
                        if (!razlozi.Contains(razlog)) razlozi.Add(razlog);
                    }
                }

                if (skor > 0)
                {
                    preporukeSaSkorom.Add((knjiga, skor, razlozi));
                }
            }

            // Bubble sort po skoru
            for (int i = 0; i < preporukeSaSkorom.Count - 1; i++)
            {
                for (int j = 0; j < preporukeSaSkorom.Count - i - 1; j++)
                {
                    if (preporukeSaSkorom[j].Skor < preporukeSaSkorom[j + 1].Skor)
                    {
                        var tmp = preporukeSaSkorom[j];
                        preporukeSaSkorom[j] = preporukeSaSkorom[j + 1];
                        preporukeSaSkorom[j + 1] = tmp;
                    }
                }
            }

            return preporukeSaSkorom
                .Take(5) 
                .Select(p => (p.Knjiga, p.Razlozi))
                .ToList();
        }

        public List<(Knjiga Knjiga, int BrojRecenzija)> PreporuciNajpopularnije()
        {
            var popularnost = new List<(Knjiga Knjiga, int BrojRecenzija)>();

            for (int i = 0; i < _db.knjige.Count; i++)
            {
                var knjiga = _db.knjige[i];
                int brojRecenzija = knjiga.Recenzije?.Count ?? 0;
                popularnost.Add((knjiga, brojRecenzija));
            }

            // Bubble sort po broju recenzija (silazno)
            for (int i = 0; i < popularnost.Count - 1; i++)
            {
                for (int j = 0; j < popularnost.Count - i - 1; j++)
                {
                    if (popularnost[j].BrojRecenzija < popularnost[j + 1].BrojRecenzija)
                    {
                        var tmp = popularnost[j];
                        popularnost[j] = popularnost[j + 1];
                        popularnost[j + 1] = tmp;
                    }
                }
            }


            return popularnost
                .Where(p => p.BrojRecenzija > 0)
                .Take(5) 
                .ToList();
        }
    }
}