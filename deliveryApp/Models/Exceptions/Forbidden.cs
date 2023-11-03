namespace deliveryApp.Models.Exceptions
{
    public class Forbidden : Exception
    {
        public Forbidden(string message) : base(message) { }
    }
}
