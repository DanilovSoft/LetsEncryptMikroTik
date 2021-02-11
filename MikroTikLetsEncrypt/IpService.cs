using DanilovSoft.MikroApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace LetsEncryptMikroTik.Core
{
    internal struct IpService
    {
        [MikroTikProperty("port")]
        public int Port { get; set; }

        [MikroTikProperty("disabled")]
        public bool Disabled { get; set; }

        [MikroTikProperty("address")]
        public string Address { get; set; }

        public string[] Addresses { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _)
        {
            Addresses = Address?.Split(',');
        }
    }
}
