namespace Nexus.Identity.API.Domain.Exceptions
{
    public class BadRequestException : DomainException
    {
        public BadRequestException(string message) : base(message) { }
    }
}
