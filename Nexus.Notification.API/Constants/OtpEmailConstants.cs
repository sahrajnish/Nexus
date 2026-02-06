namespace Nexus.Notification.API.Constants
{
    public class OtpEmailConstants
    {
        public const string Subject = "Your Nexus One-Time Password (OTP) for Account Verification";
        public const int OtpExpiryMinutes = 15;

        public const string EmailTemplateHTML = """
            <h3>Nexus Email Verification</h3>
            <p>Your OTP code is: <strong>{0}</strong>. It expires in {1} minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>
            """;
        public const string EmailTemplateText = "Your OTP code is: {0}. It expires in {1} minutes. " +
            "If you did not request this code, please ignore this email.";
    }
}
