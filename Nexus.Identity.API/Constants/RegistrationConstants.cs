namespace Nexus.Identity.API.Constants
{
    public class RegistrationConstants
    {
        public const int EmailVerificationExpiryMinutes = 15;

        public class ExceptionStrings
        {
            public const string EmailAlreadyExists = "Email already exists.";
            public const string RegistrationInProgress = "A registration process is already in progress for this email.";
            public const string EmailValidationMessage = "A valid email address is required.";
        }
    }
}
