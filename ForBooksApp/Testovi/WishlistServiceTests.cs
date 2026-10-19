using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForBooks.Services;
using ForBooks.Models;
using System.Linq;

namespace UnitTestiranje
{
    [TestClass]
    public class WishlistServiceTests
    {
        private ExtraDbClass _db;
        private WishlistService _service;
        private Korisnik _user;

        [TestInitialize]
        public void Setup()
        {
            _db = new ExtraDbClass();
            _db.Wishlists = null; // Testira inicijalizaciju
            _service = new WishlistService(_db);
            _user = _db.korisnici.First();
        }

        // --- KONSTRUKTOR ---
        [TestMethod]
        public void Constructor_ShouldInitializeWishlist()
        {
            var db = new ExtraDbClass();
            db.Wishlists = null;

            var service = new WishlistService(db);

            Assert.IsNotNull(db.Wishlists);
            Assert.AreEqual(0, db.Wishlists.Count);
        }

        // --- LOGIN ---
        [TestMethod]
        public void Login_Success()
        {
            var (user, message) = _service.Login(_user.Username, _user.Lozinka);

            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Username, user!.Username);
            Assert.AreEqual("Uspješan login.", message);
        }

        [TestMethod]
        public void Login_Fail()
        {
            var (user, message) = _service.Login("pogresan", "pogresan");

            Assert.IsNull(user);
            Assert.AreEqual("Neispravan username ili lozinka.", message);
        }

        // --- ADD ---
        [TestMethod]
        public void AddToWishlist_ShouldAdd()
        {
            string msg = _service.AddToWishlist(_user, "Knjiga1");

            Assert.AreEqual("Knjiga dodana u wishlist.", msg);

            var wl = _db.Wishlists.First(w => w.Username == _user.Username);
            Assert.IsTrue(wl.BookTitles.Contains("Knjiga1"));
            Assert.IsTrue(_db.UpdateWishlistCalled);
        }

        [TestMethod]
        public void AddToWishlist_NonexistentBook()
        {
            string msg = _service.AddToWishlist(_user, "NEPOSTOJI");

            Assert.AreEqual("Knjiga ne postoji.", msg);
            Assert.IsFalse(_db.UpdateWishlistCalled);
        }

        [TestMethod]
        public void AddToWishlist_AlreadyExists()
        {
            _service.AddToWishlist(_user, "Knjiga1");
            _db.UpdateWishlistCalled = false;

            string msg = _service.AddToWishlist(_user, "Knjiga1");

            Assert.AreEqual("Knjiga već postoji u wishlisti.", msg);
            Assert.IsFalse(_db.UpdateWishlistCalled);
        }

        // --- REMOVE ---
        [TestMethod]
        public void RemoveFromWishlist_ShouldRemove()
        {
            _service.AddToWishlist(_user, "Knjiga1");
            _db.UpdateWishlistCalled = false;

            string msg = _service.RemoveFromWishlist(_user, "Knjiga1");

            Assert.AreEqual("Knjiga uklonjena iz wish liste.", msg);
            Assert.IsTrue(_db.UpdateWishlistCalled);

            var wl = _service.GetWishlist(_user.Username);
            Assert.AreEqual(0, wl.Count);
        }

        [TestMethod]
        public void RemoveFromWishlist_NotExisting()
        {
            string msg = _service.RemoveFromWishlist(_user, "Nista");

            Assert.AreEqual("Knjiga nije u wishlisti.", msg);
        }

        // --- SHOW ---
        [TestMethod]
        public void ShowWishlist_Empty()
        {
            string result = _service.ShowWishlist(_user);

            Assert.AreEqual("Wishlist je prazan.", result);
        }

        [TestMethod]
        public void ShowWishlist_WithItems()
        {
            _service.AddToWishlist(_user, "Knjiga1");
            _service.AddToWishlist(_user, "Knjiga2");

            string result = _service.ShowWishlist(_user);

            StringAssert.Contains(result, "Wishlist:");
            StringAssert.Contains(result, "- Knjiga1");
            StringAssert.Contains(result, "- Knjiga2");
        }

        // --- GET ---
        [TestMethod]
        public void GetWishlist_ShouldReturnItems()
        {
            _service.AddToWishlist(_user, "Knjiga1");
            _service.AddToWishlist(_user, "Knjiga2");

            var result = _service.GetWishlist(_user.Username);

            CollectionAssert.AreEquivalent(
                new[] { "Knjiga1", "Knjiga2" },
                result
            );
        }

        // --- MULTIUSER ---
        [TestMethod]
        public void MultipleUsers_ShouldHaveSeparateWishlists()
        {
            var u2 = _db.korisnici[1];

            _service.AddToWishlist(_user, "Knjiga1");
            _service.AddToWishlist(u2, "Knjiga2");

            CollectionAssert.AreEquivalent(
                new[] { "Knjiga1" },
                _service.GetWishlist(_user.Username)
            );
            CollectionAssert.AreEquivalent(
                new[] { "Knjiga2" },
                _service.GetWishlist(u2.Username)
            );
        }
    }
}
