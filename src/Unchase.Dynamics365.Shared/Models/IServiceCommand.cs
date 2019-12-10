namespace Unchase.Dynamics365.Shared.Models
{
    /// <summary>
    /// Команда для выполнения.
    /// </summary>
    public interface IServiceCommand
    {
        /// <summary>
        /// Имя команды.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Исполнение команды.
        /// </summary>
        /// <param name="context">Контекст выполнения подключаемого модуля.</param>
        void Execute(Context context);
    }
}
