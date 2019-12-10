using Microsoft.Xrm.Sdk;
using System;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение функционала классов, реализующих интерфейс <see cref="ITracingService"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ITracingServiceExtensions
    {
        /// <summary>
        /// Запись в трассировку данных об исключении.
        /// </summary>
        /// <param name="service">Экземпляр сервиса трассировки.</param>
        /// <param name="exception">Исключение.</param>
        public static void Trace(this ITracingService service, Exception exception)
        {
            service.Trace($@"{ exception.GetType().FullName}: {exception.Message} (Code {exception.HResult})");
            service.Trace(exception.Source);
            service.Trace(exception.StackTrace);
        }


        //ToDo: нужен NewtonSoft.Json
        ///// <summary>
        ///// Запись в трассировку данных объекта.
        ///// </summary>
        ///// <param name="service">Экземпляр сервиса трассеровки.</param>
        ///// <param name="obj">Объект для записи в трассировку.</param>
        //public static void Trace(this ITracingService service, object obj)
        //{
        //    service.Trace(JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        //    {
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        //        NullValueHandling = NullValueHandling.Ignore,
        //        Formatting = Formatting.Indented,
        //        Error = (s, e) => { e.ErrorContext.Handled = true; }
        //    }));
        //}
    }
}
