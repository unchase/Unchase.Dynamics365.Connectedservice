using Unchase.Dynamics365.Shared.Enums;
using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение функционала перечисления <see cref="Stage"/>.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once CheckNamespace
    public static class StageExtensions
    {
        /// <summary>
        /// Получение отображаемого имени значения перечисления.
        /// </summary>
        /// <param name="stage">Значение перечисления.</param>
        /// <returns>
        /// Метод возвращает значение пречисления, которое можно показывать конечным пользователям.
        /// </returns>
        public static string GetDisplayName(this Stage stage)
        {
            var memInfo = (typeof(Stage)).GetMember(stage.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}
