namespace Unchase.Dynamics365.Shared.Models
{
    /// <summary>
    /// Результат работы плагина.
    /// </summary>
    public interface IPluginResult
    {
        /// <summary>
        /// Метод, вызываемый при завершении работы плагина.
        /// </summary>
        void Result();
    }
}
