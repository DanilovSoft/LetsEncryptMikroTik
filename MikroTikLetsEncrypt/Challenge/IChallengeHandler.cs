namespace LetsEncryptMikroTik.Core;

internal interface IChallengeHandler
{
    void Start();
    
    /// <summary>
    /// Устанавливается в RunToComplete когда был успешно обработан хотя-бы один запрос.
    /// </summary>
    Task RequestHandled { get; }
    
    /// <summary>
    /// Может иметь значения: 80, 443, 53.
    /// </summary>
    int PublicPort { get; }

    int ListenPort { get; }
}
