namespace Nexus.Identity.API.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
}
