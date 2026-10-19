using ForBooks.Enums;
using ForBooks.Models;
using ForBooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnitTestiranje
{
    [TestClass]
    public class KnjigaServisPerformanceTests
    {
        private FakeDbClass _db = null!;
        private KnjigaServis _servis = null!;

        [TestInitialize]
        public void Setup()
        {
            _db = new FakeDbClass();
            _servis = new KnjigaServis(_db);

            // napuni bazu velikim brojem knjiga (npr. 50k)
            Seed(50000);
        }

        private Autor CreateAutor(string ime, string prezime, string datumRodjenja)
        {
            var datum = DateTime.ParseExact(datumRodjenja, "dd.MM.yyyy", null);
            return new Autor(0, ime, prezime, datum);
        }

        private void Seed(int n)
        {
            _db.knjige.Clear();

            var a1 = CreateAutor("Meša", "Selimović", "01.01.1910");
            var a2 = CreateAutor("Ivo", "Andrić", "01.01.1892");
            var a3 = CreateAutor("Branko", "Ćopić", "01.01.1915");

            for (int i = 1; i <= n; i++)
            {
                var naziv = (i % 3 == 0) ? "Derviš i smrt" : $"Naslov{i}";
                var k = new Knjiga(i, $"{1000 + i}", naziv, DateTime.Now, VrstaKnjige.NOVEL, new Izdavac(1, "Izd"));

                // dio knjiga bez autora, dio sa autorima
                if (i % 10 == 0)
                    k.Autori = null;
                else if (i % 5 == 0)
                    k.Autori = new List<Autor> { a1, a2 };
                else
                    k.Autori = new List<Autor> { a3 };

                _db.knjige.Add(k);
            }
        }

        private static (long ms, long bytes) Measure(string label, Action action, int warmup = 20, int runs = 200)
        {
            // warmup (JIT + cache efekti)
            for (int i = 0; i < warmup; i++) action();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long beforeBytes = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < runs; i++) action();

            sw.Stop();
            long afterBytes = GC.GetAllocatedBytesForCurrentThread();

            long elapsedMs = sw.ElapsedMilliseconds;
            long allocated = afterBytes - beforeBytes;

            Console.WriteLine($"{label} -> Vrijeme: {elapsedMs} ms | Alocirano: {allocated} B");
            return (elapsedMs, allocated);
        }

        [TestMethod]
        public void Perf_Baseline_Mod1_Naziv()
        {
            Measure("BASELINE mod=1", () => _servis.PretragaKnjiga("1", "derviš"));
        }

        [TestMethod]
        public void Perf_Baseline_Mod2_Autor()
        {
            Measure("BASELINE mod=2", () => _servis.PretragaKnjiga("2", "selimović"));
        }
    }
}
