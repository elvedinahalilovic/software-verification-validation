using System.Collections.Generic;
using ForBooks.Data;
using ForBooks.Models;

namespace ForBooks.Services
{
    public class ViewHistoryService
    {
        private readonly DbClass _db;

        public ViewHistoryService(DbClass db)
        {
            _db = db;
        }

        public void RecordView(string user, string item)
        {
            var vh = new ViewHistory
            {
                Id = _db.ViewHistories.Count + 1,
                User = user,
                ViewedItem = item,
                ViewedAt = DateTime.Now
            };
            _db.AddView(vh);
        }

        public List<ViewHistory> GetAllViews()
        {
            return _db.ViewHistories;
        }
    }
}
