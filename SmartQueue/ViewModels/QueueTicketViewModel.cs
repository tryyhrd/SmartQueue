namespace SmartQueue.ViewModels
{
    public class QueueTicketViewModel
    {
        public Data.Models.Ticket Ticket { get; set; }
        public List<Data.Models.Ticket> ServiceTickets { get; set; }
        public Data.Models.Service Service { get; set; }
    }
}
