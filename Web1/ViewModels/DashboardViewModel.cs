
using Web1.Models;

namespace Web1.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEnquiries { get; set; }

        public int TotalDocuments { get; set; }

        public List<ContactMessage> RecentEnquiries { get; set; }
            = new List<ContactMessage>();

        public List<Document> RecentDocuments { get; set; }
            = new List<Document>();
    }
}
