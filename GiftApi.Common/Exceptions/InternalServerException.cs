namespace GiftApi.Common.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string message = "An unexpected error occurred. Please try again later.")
            : base(message) { }
    }
}
