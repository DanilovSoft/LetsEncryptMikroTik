using System;
using System.Threading.Tasks;

namespace LetsEncryptMikroTik.Core
{
    internal interface IChallenge
    {
        void Start();
        [Obsolete]
        Task RunAsync();
        Task Completion { get; }
        int PublicPort { get; }
        int ListenPort { get; }
        //Task<bool> StopAsync(TimeSpan timeout);
    }
}
