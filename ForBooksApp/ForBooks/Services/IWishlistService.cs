using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForBooks.Services
{
    public interface IWishlistService
    {
        void AddToWishlist(int userId, int bookId);
        void RemoveFromWishlist(int userId, int bookId);
        List<int> GetWishlist(int userId);
    }

}
