using ForBooks.Data;
using ForBooks.Services;
using System;
using ForBooks.Models; 
using ForBooks.Enums; 
using System.Collections.Generic; 
using System.Linq; 

namespace ForBooks
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new DbClass();            
            var korisnikServis = new KorisnikServis(db); 
            var knjigaServis = new KnjigaServis(db);
            var recenzijaServis = new RecenzijaServis(db);
            var izvjestajServis = new IzvjestajServis(db);
            var wishlistServis = new WishlistService(db);
            var viewHistoryService = new ViewHistoryService(db);
            Console.WriteLine("Želite li pokrenuti benchmark pretrage? (y/n)");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                PokreniBenchmark(knjigaServis, db);
            }
            while (true)
            {
                Console.WriteLine("\n=== GLAVNI MENI ===");
                Console.WriteLine("1. Registracija");
                Console.WriteLine("2. Potvrdi registraciju");
                Console.WriteLine("3. Prijava");
                Console.WriteLine("4. Upravljanje knjigama");
                Console.WriteLine("5. Upravljanje recenzijama");
                Console.WriteLine("6. Predloži mi knjigu");  
                Console.WriteLine("7. Izvjestaj o procitanim knjigama");
                Console.WriteLine("8. Upravljanje wishlistom");
                Console.WriteLine("9. Historija pregledanih knjiga");
                Console.WriteLine("10. Izlaz");
                Console.Write("Odabir: ");
                string izbor = Console.ReadLine()!;

                switch (izbor)
                {
                    case "1": RegistracijaMeni(korisnikServis); break; 
                    case "2": PotvrdaRegistracijeMeni(korisnikServis); break; 
                    case "3": PrijavaMeni(korisnikServis); break; 
                    case "4": PodmeniKnjige(knjigaServis); break;
                    case "5": PodmeniRecenzije(recenzijaServis); break;
                    case "6": PredloziKnjiguMeni(knjigaServis); break;
                    case "7": izvjestajServis.getIzvjestaj(); break;
                    case "8": PodmeniWishlist(wishlistServis); break;
                    case "9": PodmeniViewHistory(viewHistoryService); break;
                    case "10": Console.WriteLine("Doviđenja!"); return;
                    default: Console.WriteLine("Neispravan unos!"); break;
                }
            }
        }
        //korisnikservis
        static void PokreniBenchmark(KnjigaServis s, DbClass db)
        {
            Console.WriteLine("\n--- POKRETANJE BENCHMARK TESTA ---");

            // 1. Priprema podataka: Dodajemo 1000 privremenih knjiga da imamo šta pretraživati
            for (int i = 0; i < 1000; i++)
            {
                var autor = new Autor(i, "Meša", "Selimović", new DateTime(1910, 4, 26));

                var k1 = new Knjiga(i, "123-" + i, "Naslov" + i, DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"))
                {
                    Autori = new List<Autor> { autor }
                };

                db.knjige.Add(k1);
            }

            // 2. Čišćenje memorije prije testa
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 3. Mjerenje
            long memorijaPrije = GC.GetTotalMemory(true);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Pokrećemo metodu 10.000 puta da dobijemo mjerljive rezultate
            for (int i = 0; i < 10000; i++)
            {
                // Testiramo pretragu po autoru (opcija "2") jer je ona najzahtjevnija u tvom kodu
                s.PretragaKnjiga("2", "Prezime500");
            }

            sw.Stop();
            long memorijaPoslije = GC.GetTotalMemory(false);

            // 4. Ispis rezultata
            Console.WriteLine($"Vrijeme (10.000 pokretanja): {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Alocirana memorija: {(memorijaPoslije - memorijaPrije) / 1024.0:F2} KB");
           

            // Očistimo testne podatke da ne kvare stvarni rad aplikacije
            db.knjige.RemoveAll(k => k.NazivKnjige.StartsWith("Knjiga "));
        }
        static void RegistracijaMeni(KorisnikServis s)
        {
            Console.WriteLine("\n--- REGISTRACIJA ---");
            Console.Write("Ime: ");
            string ime = Console.ReadLine()!;
            Console.Write("Prezime: ");
            string prezime = Console.ReadLine()!;
            Console.Write("Korisničko ime (Username): ");
            string username = Console.ReadLine()!;
            Console.Write("Email: ");
            string email = Console.ReadLine()!;
            Console.Write("Telefon: ");
            string telefon = Console.ReadLine()!;
            Console.Write("Lozinka: ");
            string lozinka = Console.ReadLine()!;

            string rezultat = s.Registracija(ime, prezime, username, email, telefon, lozinka);
            Console.WriteLine(rezultat);
        }
        static void PotvrdaRegistracijeMeni(KorisnikServis s)
        {
            Console.WriteLine("\n--- POTVRDA REGISTRACIJE ---");
            Console.Write("Unesite email za potvrdu: ");
            string email = Console.ReadLine()!;

            string rezultat = s.PotvrdaRegistracije(email);
            Console.WriteLine(rezultat);
        }

        static void PrijavaMeni(KorisnikServis s)
        {
            Console.WriteLine("\n--- PRIJAVA ---");
            Console.Write("Email: ");
            string email = Console.ReadLine()!;
            Console.Write("Lozinka: ");
            string lozinka = Console.ReadLine()!;

            var (korisnik, poruka) = s.Prijava(email, lozinka);
            Console.WriteLine(poruka);

            if (korisnik != null)
            {
            }
        }

        // Upravljanje knjigama 
        static void PodmeniKnjige(KnjigaServis s)
        {
            while (true)
            {
                Console.WriteLine("\n--- UPRAVLJANJE KNJIGAMA ---");
                Console.WriteLine("1. Dodaj knjigu");
                Console.WriteLine("2. Prikaži sve knjige");
                Console.WriteLine("3. Ažuriraj knjigu");
                Console.WriteLine("4. Obriši knjigu");
                Console.WriteLine("5. Obriši neprimjerene recenzije");
                Console.WriteLine("6. Sortiraj knjige po datumu");
                Console.WriteLine("7. Pretraži knjige (naslov/autor)");
                Console.WriteLine("8. Nazad");
                Console.Write("Odabir: ");
                string izbor = Console.ReadLine()!;

                switch (izbor)
                {
                    case "1": DodajKnjiguMeni(s); break; 
                    case "2": PrikaziSveKnjigeMeni(s); break; 
                    case "3": AzurirajKnjiguMeni(s); break; 
                    case "4": ObrisiKnjiguMeni(s); break; 
                    case "5": ObrisiNeprimjereneRecenzijeMeni(s); break; 
                    case "6": SortirajPoDatumuMeni(s); break; 
                    case "7": PretragaKnjigaMeni(s); break; 
                    case "8": return;
                    default: Console.WriteLine("Neispravan unos."); break;
                }
            }
        }
        static void DodajKnjiguMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Dodavanje nove knjige ");

            Console.Write("ISBN: ");
            string isbn = Console.ReadLine()!;

            Console.Write("Naziv knjige: ");
            string naziv = Console.ReadLine()!;

            Console.Write("Datum objavljivanja (dd.mm.yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var datum))
            {
                Console.WriteLine(" GREŠKA: Neispravan format datuma. Pokušaj ponovo (npr. 15.03.2021)");
                return;
            }

            Console.WriteLine("Odaberi vrstu knjige (0-MEMOIR, 1-SELF_HELP, 2-POETRY, 3-NOVEL, 4-SHORT_STORIES, 5-NOVELLA): ");
            if (!int.TryParse(Console.ReadLine(), out var vrstaIndex) || vrstaIndex < 0 || vrstaIndex > 5)
            { Console.WriteLine(" GREŠKA: Neispravan izbor vrste."); return; }
            VrstaKnjige vrsta = (VrstaKnjige)vrstaIndex;

            Console.Write("Naziv izdavača: ");
            string izdavacNaziv = Console.ReadLine()!;

            // Unos autora 
            Console.Write("Koliko autora ima knjiga? ");
            if (!int.TryParse(Console.ReadLine(), out var brojAutora) || brojAutora <= 0)
            {
                Console.WriteLine("GREŠKA: Neispravan broj autora.");
                return;
            }

            var autoriKnjige = new List<Autor>();
            for (int i = 0; i < brojAutora; i++)
            {
                Console.WriteLine($"\n Autor {i + 1} ");
                Console.Write("Ime autora: ");
                string imeA = Console.ReadLine()!;
                Console.Write("Prezime autora: ");
                string prezA = Console.ReadLine()!;
                Console.Write("Datum rođenja (dd.mm.yyyy): ");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var datumA))
                {
                    Console.WriteLine("GREŠKA: Neispravan format datuma (npr. 09.10.1892). Autor preskočen.");
                    continue;
                }
                autoriKnjige.Add(new Autor(0, imeA, prezA, datumA));
            }
            string rezultat = s.DodajKnjigu(isbn, naziv, datum, vrsta, izdavacNaziv, autoriKnjige);
            Console.WriteLine(rezultat);
        }

        static void PrikaziSveKnjigeMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Sve knjige");
            var knjige = s.PrikaziSveKnjige();

            if (knjige.Count == 0)
            {
                Console.WriteLine("Nema knjiga.");
                return;
            }

            foreach (var k in knjige)
            {
                Console.WriteLine($"{k.ID}. {k.NazivKnjige} | {k.ISBN} | {k.VrstaKnjige} | {k.DatumObjavljivanja:dd.MM.yyyy} | Izdavač: {k.Izdavac?.NazivIzdavaca}");

                if (k.Autori != null && k.Autori.Count > 0)
                {
                    Console.Write("Autori: ");
                    for (int j = 0; j < k.Autori.Count; j++)
                    {
                        var a = k.Autori[j];
                        Console.Write($"{a.ImeAutora} {a.PrezimeAutora}");
                        if (j < k.Autori.Count - 1)
                            Console.Write(", ");
                    }
                    Console.WriteLine(); 
                }
                Console.WriteLine(); 
            }
        }

        static void AzurirajKnjiguMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Ažuriranje knjige ");
            Console.Write("Unesite ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("GREŠKA: Neispravan ID."); return; }

            Console.Write("Novi naziv (Enter za preskočiti): ");
            string noviNaziv = Console.ReadLine()!;

            Console.Write("Novi ISBN (Enter za preskočiti): ");
            string noviISBN = Console.ReadLine()!;

            DateTime? noviDatum = null;
            Console.Write("Novi datum (dd.mm.yyyy, Enter za preskočiti): ");
            string datumUnos = Console.ReadLine()!;
            if (!string.IsNullOrEmpty(datumUnos) && DateTime.TryParseExact(datumUnos, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var d))
                noviDatum = d;
            
            Console.Write("Novi izdavač (Enter za preskočiti): ");
            string noviIzdavacNaziv = Console.ReadLine()!;

            string rezultat = s.AzurirajKnjigu(id, noviNaziv, noviISBN, noviDatum, noviIzdavacNaziv);
            Console.WriteLine(rezultat);
        }

        static void ObrisiKnjiguMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Brisanje knjige ");
            Console.Write("Unesite ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine(" Neispravan ID."); return; }

            string rezultat = s.ObrisiKnjigu(id);
            Console.WriteLine(rezultat);
        }

        static void ObrisiNeprimjereneRecenzijeMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Brisanje neprimjerenih recenzija ");
            Console.Write("Unesite ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("GREŠKA: Neispravan ID."); return; }

            Console.Write("Unesi ključne riječi (npr. uvredljive), odvojene zarezom: ");
            string kljucne = Console.ReadLine() ?? "";

            string rezultat = s.ObrisiNeprimjereneRecenzije(id, kljucne);
            Console.WriteLine(rezultat);
        }

        static void SortirajPoDatumuMeni(KnjigaServis s)
        {
            string rezultat = s.SortirajPoDatumu();
            Console.WriteLine(rezultat);
            PrikaziSveKnjigeMeni(s);
        }

        static void PretragaKnjigaMeni(KnjigaServis s)
        {
            Console.WriteLine("\n Pretraga knjiga ");
            Console.Write("Traži po (1) Naslovu ili (2) Autoru? ");
            string mod = Console.ReadLine() ?? "1"; 

            Console.Write("Unesi pojam: ");
            string pojam = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(pojam)) { Console.WriteLine("GREŠKA: Prazan pojam."); return; }

            var rezultati = s.PretragaKnjiga(mod, pojam);

            if (rezultati.Count == 0)
            {
                Console.WriteLine("Nema rezultata.");
            }
            else
            {
                Console.WriteLine("Pronađene knjige:");
                foreach (var k in rezultati)
                {
                    Console.WriteLine($"{k.ID}. {k.NazivKnjige} | {k.ISBN}");
                }
            }
        }
        
        static void PredloziKnjiguMeni(KnjigaServis s)
        {
            Console.WriteLine("Predloži mi knjigu\n");
            Console.WriteLine("Za personalizovane preporuke, molimo prijavite se:\n");

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Šifra: ");
            string sifra = Console.ReadLine()!;

            var korisnik = s.PrijaviKorisnika(email, sifra);

            if (korisnik == null)
            {
                Console.WriteLine("\n Pogrešan email ili šifra. Pokušaj ponovo.");
                return;
            }

            Console.WriteLine($"\n Dobro došao/la, {korisnik.Ime} {korisnik.Prezime}!\n");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
            
            var preporuke = s.PreporuciKnjige(korisnik.ID);

            if (preporuke.Count > 0)
            {
                Console.WriteLine(" PERSONALIZOVANE PREPORUKE ZA VAS \n");
                int prikazano = 0;
                // Prikaz top 5 preporučenih knjiga
                foreach (var (knjiga, razlozi) in preporuke.Take(5)) 
                {
                    Console.WriteLine($"───────────────────────────────────────────────────");
                    Console.WriteLine($"{prikazano + 1}.  {knjiga.NazivKnjige}");
                    Console.WriteLine($"   ISBN: {knjiga.ISBN}");
                    Console.WriteLine($"   Tip: {knjiga.VrstaKnjige}");
                    Console.WriteLine($"   Objavljeno: {knjiga.DatumObjavljivanja:dd.MM.yyyy}");

                    if (knjiga.Autori != null && knjiga.Autori.Count > 0)
                    {
                        Console.Write("     Autori: ");
                        for (int a = 0; a < knjiga.Autori.Count; a++)
                        {
                            Console.Write($"{knjiga.Autori[a].ImeAutora} {knjiga.Autori[a].PrezimeAutora}");
                            if (a < knjiga.Autori.Count - 1) Console.Write(", ");
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine($"\n    Zašto preporučujemo:");
                    foreach (var razlog in razlozi)
                        Console.WriteLine($"      • {razlog}");

                    Console.WriteLine();
                    prikazano++;
                }
                Console.WriteLine("═══════════════════════════════════════════════════");
            }
            else
            {
                Console.WriteLine("Nema knjiga sa sličnim karakteristikama u sistemu ili niste pročitali nijednu knjigu.");
                Console.WriteLine("Evo najpopularnijih knjiga:\n");
                PrikaziNajpopularnijeMeni(s);
            }
        }

        static void PrikaziNajpopularnijeMeni(KnjigaServis s)
        {
            var popularnost = s.PreporuciNajpopularnije();
            
            if (popularnost.Count == 0)
            {
                Console.WriteLine("Još nema knjiga sa recenzijama u sistemu.");
                return;
            }

            Console.WriteLine("TOP 5 NAJPOPULARNIJIH KNJIGA\n");

            // Prikaz top 5 najpopularnijih knjiga
            foreach (var item in popularnost.Take(5))
            {
                var (knjiga, brojRecenzija) = item; 
                
                Console.WriteLine($"───────────────────────────────────────────────────");
                // Traženje indeksa unutar popularnost liste da bi se dobio redni broj
                Console.WriteLine($"{popularnost.IndexOf(item) + 1}. {knjiga.NazivKnjige}"); 
                Console.WriteLine($"   ISBN: {knjiga.ISBN}");
                Console.WriteLine($"   Tip: {knjiga.VrstaKnjige}");
                Console.WriteLine($"    Recenzije: {brojRecenzija}");

                if (knjiga.Autori != null && knjiga.Autori.Count > 0)
                {
                    Console.Write("   Autori: ");
                    for (int a = 0; a < knjiga.Autori.Count; a++)
                    {
                        Console.Write($"{knjiga.Autori[a].ImeAutora} {knjiga.Autori[a].PrezimeAutora}");
                        if (a < knjiga.Autori.Count - 1) Console.Write(", ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            Console.WriteLine("═══════════════════════════════════════════════════");
        }

        static void PodmeniRecenzije(RecenzijaServis s)
        {
            while (true)
            {
                Console.WriteLine("\n--- UPRAVLJANJE RECENZIJAMA ---");
                Console.WriteLine("1. Dodaj recenziju");
                Console.WriteLine("2. Prikaži sve recenzije");
                Console.WriteLine("3. Prikaži recenzije za određenu knjigu");
                Console.WriteLine("4. Prikaži prosječnu ocjenu knjige");
                Console.WriteLine("5. Obriši recenziju");
                Console.WriteLine("6. Nazad");
                Console.Write("Odabir: ");
                string izbor = Console.ReadLine()!;

                try
                {
                    switch (izbor)
                    {
                        case "1":
                            DodajRecenzijuMeni(s);
                            break;

                        case "2":
                            PrikaziSveRecenzijeMeni(s);
                            break;

                        case "3":
                            PrikaziRecenzijeZaKnjiguMeni(s);
                            break;

                        case "4":
                            PrikaziProsjecnuOcjenuMeni(s);
                            break;

                        case "5":
                            ObrisiRecenzijuMeni(s);
                            break;

                        case "6":
                            return;

                        default:
                            Console.WriteLine("Neispravan unos.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Greška: {ex.Message}");
                }
            }
        }
        static void DodajRecenzijuMeni(RecenzijaServis s)
        {
            Console.WriteLine("\n--- DODAVANJE RECENZIJE ---");

            Console.Write("ID korisnika: ");
            if (!int.TryParse(Console.ReadLine(), out int korisnikId))
            {
                Console.WriteLine("Neispravan ID korisnika.");
                return;
            }

            Console.Write("ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out int knjigaId))
            {
                Console.WriteLine("Neispravan ID knjige.");
                return;
            }

            Console.Write("Ocjena (1–5): ");
            if (!double.TryParse(Console.ReadLine(), out double ocjena))
            {
                Console.WriteLine("Neispravna ocjena.");
                return;
            }

            Console.Write("Komentar: ");
            string komentar = Console.ReadLine() ?? "";

            var rec = s.DodajRecenziju(korisnikId, knjigaId, ocjena, komentar);
            Console.WriteLine($"Recenzija dodana. ID = {rec.ID}, Ocjena = {rec.Ocjena}, Komentar = {rec.Komentar}");
        }

        static void PrikaziSveRecenzijeMeni(RecenzijaServis s)
        {
            Console.WriteLine("\n--- SVE RECENZIJE ---");
            var recenzije = s.DohvatiSveRecenzije();

            if (recenzije.Count == 0)
            {
                Console.WriteLine("Nema recenzija u bazi.");
                return;
            }

            foreach (var r in recenzije)
            {
                string ime = r.Korisnik?.Ime ?? "?";
                string nazivKnjige = r.Knjiga?.NazivKnjige ?? "?";

                Console.WriteLine($"{r.ID}. {ime} - '{nazivKnjige}' | Ocjena: {r.Ocjena}/5");
                Console.WriteLine($"   Komentar: {r.Komentar}");
            }
        }

        static void PrikaziRecenzijeZaKnjiguMeni(RecenzijaServis s)
        {
            Console.WriteLine("\n--- RECENZIJE ZA KNJIGU ---");
            Console.Write("Unesi ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out int knjigaId))
            {
                Console.WriteLine("Neispravan ID knjige.");
                return;
            }

            var recenzije = s.DohvatiRecenzijeZaKnjigu(knjigaId);

            if (recenzije.Count == 0)
            {
                Console.WriteLine("Nema recenzija za ovu knjigu.");
                return;
            }

            foreach (var r in recenzije)
            {
                string ime = r.Korisnik?.Ime ?? "?";
                Console.WriteLine($"{r.ID}. {ime} | Ocjena: {r.Ocjena}/5");
                Console.WriteLine($"   Komentar: {r.Komentar}");
            }
        }

        static void PrikaziProsjecnuOcjenuMeni(RecenzijaServis s)
        {
            Console.WriteLine("\n--- PROSJEČNA OCJENA KNJIGE ---");
            Console.Write("Unesi ID knjige: ");
            if (!int.TryParse(Console.ReadLine(), out int knjigaId))
            {
                Console.WriteLine("Neispravan ID knjige.");
                return;
            }

            var (prosjek, brojRecenzija) = s.IzracunajProsjecnuOcjenuKnjige(knjigaId);

            if (brojRecenzija == 0)
            {
                Console.WriteLine("Ova knjiga nema recenzija.");
            }
            else
            {
                Console.WriteLine($"Prosječna ocjena: {prosjek:F2}/5 (broj recenzija: {brojRecenzija})");
            }
        }

        static void ObrisiRecenzijuMeni(RecenzijaServis s)
        {
            Console.WriteLine("\n--- BRISANJE RECENZIJE ---");
            Console.Write("Unesi ID recenzije za brisanje: ");
            if (!int.TryParse(Console.ReadLine(), out int recenzijaId))
            {
                Console.WriteLine("Neispravan ID recenzije.");
                return;
            }

            s.ObrisiRecenziju(recenzijaId);
            Console.WriteLine("Recenzija je obrisana.");
        }


        static void PodmeniWishlist(WishlistService s)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine()!;

            Console.Write("Lozinka: ");
            string password = Console.ReadLine()!;

            var (korisnik, loginMsg) = s.Login(username, password);
            Console.WriteLine(loginMsg);

            if (korisnik == null)
                return;

            while (true)
            {
                Console.WriteLine("\n--- WISHLIST ---");
                Console.WriteLine("1. Dodaj knjigu");
                Console.WriteLine("2. Ukloni knjigu");
                Console.WriteLine("3. Prikaži wishlist");
                Console.WriteLine("4. Nazad");
                Console.Write("Odabir: ");
                string izbor = Console.ReadLine()!;

                switch (izbor)
                {
                    case "1":
                        Console.Write("Unesi naziv knjige: ");
                        string nazivDodaj = Console.ReadLine()!;
                        string addMsg = s.AddToWishlist(korisnik, nazivDodaj);
                        Console.WriteLine(addMsg);
                        break;

                    case "2":
                        Console.Write("Unesi naziv knjige: ");
                        string nazivUkloni = Console.ReadLine()!;
                        string removeMsg = s.RemoveFromWishlist(korisnik, nazivUkloni);
                        Console.WriteLine(removeMsg);
                        break;

                    case "3":
                        string wishlist = s.ShowWishlist(korisnik);
                        Console.WriteLine(wishlist);
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("Neispravan izbor!");
                        break;
                }
            }
        }
        
        static void PodmeniViewHistory(ViewHistoryService s)
        {
            while (true)
            {
                Console.WriteLine("\n--- HISTORIJA PREGLEDANIH KNJIGA ---");
                Console.WriteLine("1. Prikaži sve pregledane knjige");
                Console.WriteLine("2. Dodaj pregledanu knjigu");
                Console.WriteLine("3. Nazad");
                Console.Write("Odabir: ");
                string izbor = Console.ReadLine()!;

                switch (izbor)
                {
                    case "1":
                        var views = s.GetAllViews();
                        if (views.Count == 0)
                            Console.WriteLine("Nema pregledanih knjiga.");
                        else
                        {
                            Console.WriteLine("Pregledane knjige:");
                            foreach (var v in views)
                            {
                                Console.WriteLine($"{v.ViewedAt}: {v.User} je pregledao '{v.ViewedItem}'");
                            }
                        }
                        break;

                    case "2":
                        Console.Write("Unesi korisnika: ");
                        string user = Console.ReadLine()!;
                        Console.Write("Unesi naziv knjige: ");
                        string knjiga = Console.ReadLine()!;
                        s.RecordView(user, knjiga);
                        Console.WriteLine("Dodano u historiju.");
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine("Neispravan izbor!");
                        break;
                }
            }
        }

    }
}