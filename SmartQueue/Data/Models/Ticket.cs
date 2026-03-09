namespace SmartQueue.Data.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public Service Service { get; set; }
        public Visitor Visitor { get; set; }
        public DateTime CreatedAd { get; set; }
    }
}
