using SyncVerse.Application.Interfaces.Identity;
using System.Security.Cryptography;

namespace SyncVerse.Application.Services.Identity
{
    public class OtpService : IOtpService
    {
        private const int OtpLength = 4;
        private const int MinOtpValue = 1000;
        private const int MaxOtpValue = 9999;
        private const int IterationCount = 10000;
        private const int KeySize = 32;

        public string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[4];
                rng.GetBytes(buffer);
                int randomNumber = Math.Abs(BitConverter.ToInt32(buffer, 0));
                int otp = (randomNumber % (MaxOtpValue - MinOtpValue + 1)) + MinOtpValue;
                return otp.ToString();
            }
        }

        public string HashOtp(string otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException("OTP cannot be null or empty.", nameof(otp));

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(otp, salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(KeySize);
                    byte[] hashWithSalt = new byte[salt.Length + hash.Length];
                    Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
                    Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);
                    return Convert.ToBase64String(hashWithSalt);
                }
            }
        }

        public bool VerifyOtp(string otp, string hash)
        {
            if (string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                byte[] hashWithSalt = Convert.FromBase64String(hash);
                byte[] salt = new byte[16];
                Array.Copy(hashWithSalt, 0, salt, 0, salt.Length);
                byte[] storedHash = new byte[hashWithSalt.Length - salt.Length];
                Array.Copy(hashWithSalt, salt.Length, storedHash, 0, storedHash.Length);

                using (var pbkdf2 = new Rfc2898DeriveBytes(otp, salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(KeySize);
                    return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
