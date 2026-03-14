namespace SmartQueue.Data.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public Service Service { get; set; }
        public Visitor Visitor { get; set; }
        public enum StatusType
        {
            Waiting = 1,
            Active = 2,
            Completed = 3
        }
        public StatusType Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}
