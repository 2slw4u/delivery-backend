namespace deliveryApp.Models.Exceptions
{
    public class Unauthorized : Exception
    {
        public Unauthorized(string message) : base(message) { }
    }
}
