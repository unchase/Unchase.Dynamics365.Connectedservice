using Microsoft.Xrm.Sdk;
using System;
using System.Data.SqlClient;
using System.Reflection;

namespace Unchase.Dynamics365.Shared.Extensions
{
    ///// <summary>
    ///// Set of extension methods for Microsoft.Xrm.Sdk.IServiceProvider base class. Just shortcut methods to save you few lines of code during plugin development
    ///// </summary>

    /// <summary>
    /// Расширение стандартного функционала классов, реализующих интерфейс <see cref="IServiceProvider"/>.
    /// </summary>
    // ReSharper disable once CheckNamespace
    // ReSharper disable once InconsistentNaming
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Получение контекста плагина.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <returns>
        /// Метод возвращает объект, представляющий собой контекст выполнени¤ плагина.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var context = serviceProvider.GetContext();
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static IPluginExecutionContext GetContext(this IServiceProvider serviceProvider)
        {
            return (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        }


        /// <summary>
        /// Получение экземплпра CRM-сервиса.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <returns>
        /// Метод возвращает ссылку на экземпляр CRM-сервиса, запусщенного от имени пользователя, указанного при регистрации плагина.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var service = serviceProvider.GetService();
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static IOrganizationService GetService(this IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetContext();
            return serviceProvider.GetService(context.UserId);
        }


        /// <summary>
        /// Получение экземпляра CRM-сервиса.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <param name="userId">
        /// <para>Идентификатор пользователя, от имени которого будет выполняться сервис.</para>
        /// <para>Идентификатор можно взять из контекста плагина (см. <see cref="IPluginExecutionContext"/>). Метод <c>InitiatingUserId</c> контекста позволяет получить идентификатор 
        /// пользователя, инициировавщего запуск плагина, а метод <c>UserId</c> - идентификатор пользователя, указанный в настройках плагина при регистрации (по умолчанию 
        /// равно <c>InitiatingUserId</c>).</para>
        /// </param>
        /// <returns>
        /// Метод возвращает ссылку на экземпляр CRM-сервиса, запусщенного от имени указанного пользователя.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var context = serviceProvider.GetContext();
        ///     var service = serviceProvider.GetService(context.UserId);
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static IOrganizationService GetService(this IServiceProvider serviceProvider, Guid userId)
        {
            CheckParam.CheckForNotEmpty(userId, nameof(userId));

            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            return factory?.CreateOrganizationService(userId);
        }


        /// <summary>
        /// Получение экземпляра сервиса трассировки.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <returns>
        /// Метод возвращает ссылку на экземпляр сервиса трассировки.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var tracingService = serviceProvider.GetTracingService();
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static ITracingService GetTracingService(this IServiceProvider serviceProvider)
        {
            return (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }


        /// <summary>
        /// Получение экземпляра <see cref="SqlTransaction"/> текущего плагина.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <returns>
        /// Метод возвращает ссылку на экземпляр <see cref="SqlTransaction"/> текущего плагина.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var tracingService = serviceProvider.GetCurrentSqlTransaction();
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static SqlTransaction GetSqlTransaction(this IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetContext();
            var platformContext = context.GetType().InvokeMember("PlatformContext", BindingFlags.GetProperty, null, context, null);
            return (SqlTransaction)platformContext.GetType().InvokeMember("SqlTransaction", BindingFlags.GetProperty, null, platformContext, null);
        }


        ///// <summary>
        ///// Получение экземпляра <see cref="SqlTransaction"/> текущего плагина.
        ///// </summary>
        ///// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        ///// <returns>
        ///// Метод возвращает ссылку на экземпляр <see cref="SqlTransaction"/> текущего плагина.
        ///// </returns>
        ///// <example>
        ///// <code>
        ///// public void Execute(IServiceProvider serviceProvider)
        ///// {
        /////     ...
        /////     var tracingService = serviceProvider.GetCurrentSqlTransaction();
        /////     ...
        ///// }
        ///// </code>
        ///// </example>
        //public static SqlTransaction GetCurrentSqlTransaction(this IServiceProvider serviceProvider)
        //{
        //    var context = serviceProvider.GetContext();
        //    return (SqlTransaction)context.GetType().InvokeMember("SqlTransaction", BindingFlags.GetProperty, null, context, null);
        //}


        /// <summary>
        /// Получение ConnectionString текущего плагина.
        /// </summary>
        /// <param name="serviceProvider">Экземпляр класса <see cref="IServiceProvider"/>.</param>
        /// <returns>
        /// Метод возвращает ConnectionString текущего плагина.
        /// </returns>
        /// <example>
        /// <code>
        /// public void Execute(IServiceProvider serviceProvider)
        /// {
        ///     ...
        ///     var tracingService = serviceProvider.GetSqlConnectionString();
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static string GetSqlConnectionString(this IServiceProvider serviceProvider)
        {
            var transaction = serviceProvider.GetSqlTransaction();
            return transaction.Connection.ConnectionString;
        }


        /// <summary>
        /// Gets IPluginExecutionContext from service provider.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static IPluginExecutionContext GetPluginExecutionContext(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
        }


        /// <summary>
        /// Gets IOrganizationServiceFactory from service provider.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static IOrganizationServiceFactory GetOrganizationServiceFactory(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;
        }
    }
}
