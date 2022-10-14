using System;

namespace LetsEncryptMikroTik.Core
{
    internal readonly struct LetsEncryptCert
    {
        public DateTime ExpiresAfter { get; }
        public string CertPem { get; }
        public string KeyPem { get; }
        public string Thumbprint { get; }
        public string CommonName { get; }

        public LetsEncryptCert(DateTime notAfter, string certPem, string keyPem, string commonName, string thumbprint)
        {
            ExpiresAfter = notAfter;
            CertPem = certPem;
            KeyPem = keyPem;
            CommonName = commonName;
            Thumbprint = thumbprint;
        }
    }
}
