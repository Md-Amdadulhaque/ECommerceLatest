using Microsoft.AspNetCore.SignalR;

namespace E_commerce.Models
{
    public class RequestItem
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 0;
    }
}
