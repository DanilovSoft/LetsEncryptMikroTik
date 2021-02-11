using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LetsEncryptMikroTik.Core
{
    [Serializable]
    public class LetsEncryptMikroTikException : Exception
    {
        public LetsEncryptMikroTikException(string message) : base(message)
        {
        }

        public LetsEncryptMikroTikException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LetsEncryptMikroTikException()
        {
        }

        protected LetsEncryptMikroTikException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            
        }
    }
}
