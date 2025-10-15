using ASKUE.Models;

namespace ASKUE.Classes
{
    public static class AppContext
    {
        private static user182_dbEntities _context;
        public static K_Polzovateli CurrentUser { get; set; }

        public static user182_dbEntities GetContext()
        {
            if (_context == null)
                _context = new user182_dbEntities();
            return _context;
        }
    }
}