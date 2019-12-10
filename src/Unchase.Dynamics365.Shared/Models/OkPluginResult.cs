namespace Unchase.Dynamics365.Shared.Models
{
    /// <summary>
    /// Плагин выполнен без ошибок.
    /// </summary>
    public class OkPluginResult : IPluginResult
    {
        /// <summary>
        /// Причина завершения работы плагина.
        /// </summary>
        public string Reason { get; }


        /// <summary>
        /// Конструор плагина.
        /// </summary>
        /// <param name="reason">Причина завершения работы плагина.</param>
        public OkPluginResult(string reason = null)
        {
            Reason = string.IsNullOrWhiteSpace(reason) ? "Common completion." : reason;
        }


        /// <inheritdoc />
        public virtual void Result()
        {
        }
    }
}
