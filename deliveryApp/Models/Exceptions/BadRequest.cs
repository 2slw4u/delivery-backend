namespace deliveryApp.Models.Exceptions
{
    public class BadRequest : Exception
    {
        public BadRequest(string message) : base(message) { }
    }
}
