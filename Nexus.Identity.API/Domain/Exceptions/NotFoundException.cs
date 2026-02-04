namespace Nexus.Identity.API.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message) { }
    }
}
