namespace Y_Gem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscriptions { get; set; }
        public  double AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
       

    }
}
