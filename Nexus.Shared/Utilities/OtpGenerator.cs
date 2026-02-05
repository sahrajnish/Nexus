using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Nexus.Shared.Utilities
{
    public static class OtpGenerator
    {
        // Uses the OS entropy pool (hardware noise) to generate cryptographically secure, non-predictable values.
        // Essential for OTPs/Tokens; standard System.Random is predictable and unsafe for security.
        public static string GenerateSecureOtp()
        {
            // Create a buffer for 4 bytes.
            byte[] bytes = new byte[4];

            // Fill it with cryptographically strong random bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Convert bytes to a positive integer.
            int randomInt = BitConverter.ToInt32(bytes, 0);

            // Ensure its positive.
            if (randomInt < 0) randomInt = -randomInt;

            // Map it to your 6-digit range (100000 to 999999)
            // Modulo (%) ensures it fits in the range, + 100000 ensures it has 6 digits
            int otp = (randomInt % 900000) + 100000;

            return otp.ToString();
        }
    }
}
