using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LetsEncryptMikroTik.Core
{
    internal static class ExtensionMethods
    {
        public static string GetSha2Thumbprint(this X509Certificate2 cert)
        {
            byte[] hashBytes;
            using (var hasher = new SHA256Managed())
            {
                hashBytes = hasher.ComputeHash(cert.RawData);
            }
            return hashBytes.Aggregate(string.Empty, (str, hashByte) => str + hashByte.ToString("x2", CultureInfo.InvariantCulture));
        }

        public static string GetCommonName(this X509Certificate2 cert)
        {
            return cert.GetNameInfo(X509NameType.SimpleName, false);
        }
    }
}
