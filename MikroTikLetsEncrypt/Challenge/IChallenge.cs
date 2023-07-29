namespace LetsEncryptMikroTik.Core;

internal interface IChallenge
{
    void Start();
    Task Completion { get; }
    int PublicPort { get; }
    int ListenPort { get; }
}
