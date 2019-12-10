using Microsoft.Xrm.Sdk;
using Unchase.Dynamics365.Shared.Enums;
using Unchase.Dynamics365.Shared.Extensions;
using Unchase.Dynamics365.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;

namespace Unchase.Dynamics365.Shared
{
    /// <summary>
    /// Plugin base class.
    /// </summary>
    public abstract class PluginBase : PluginBaseWithResolvedAssemblies
    {
        private bool _firstExecute = true;
        private Collection<Tuple<Stage, string, string, Func<Context, IPluginResult>>> _registeredEvents;
        private Collection<IServiceCommand> _registeredComands;
        private protected string ChildClassName { get; private set; }
        internal protected Type ChildClassType { get; private set; }


        /// <summary>
        /// Unsecure configuration.
        /// </summary>
        public string UnsecureConfiguration { get; }


        /// <summary>
        /// Secure configuration.
        /// </summary>
        public string SecureConfiguration { get; }


        /// <summary>
        /// Collection of plugin registered events.
        /// </summary>
        protected Collection<Tuple<Stage, string, string, Func<Context, IPluginResult>>> RegisteredEvents
        {
            get
            {
                if (this._registeredEvents == null)
                {
                    this._registeredEvents = new Collection<Tuple<Stage, string, string, Func<Context, IPluginResult>>>();
                }
                return this._registeredEvents;
            }
        }


        /// <summary>
        /// Collection of plugin registered executed commands.
        /// </summary>
        /// <remarks>
        /// <see cref="IServiceCommand"/>.
        /// </remarks>
        protected Collection<IServiceCommand> RegisteredCommands
        {
            get
            {
                if (this._registeredComands == null)
                {
                    this._registeredComands = new Collection<IServiceCommand>();
                }
                return this._registeredComands;
            }
        }


        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="childClassName">Child class type.</param>
        /// <param name="unsecureConfig">Unsecure configuration.</param>
        /// <param name="secureConfig">Secure configuration.</param>
        internal PluginBase(Type childClassName, string unsecureConfig = null, string secureConfig = null) : base()
        {
            this.ChildClassType = childClassName;
            this.ChildClassName = childClassName.ToString();
            this.UnsecureConfiguration = unsecureConfig;
            this.SecureConfiguration = secureConfig;
        }


        /// <summary>
        /// Регистрация события, на возникновение которого будет вызван подключаемый модуль.
        /// </summary>
        /// <param name="stage">Стадия выполнения подключаемого модуля.</param>
        /// <param name="message">Событие подключаемого модуля.</param>
        /// <param name="entityName">Имя сущности, для которого возникло событие.</param>
        /// <param name="func">Выполняемый на событие метод.</param>
        protected void RegisterEvent(Stage stage, Message message, string entityName, Func<Context, IPluginResult> func, string customMessage = null)
        {
            this.RegisteredEvents.Add(new Tuple<Stage, string, string, Func<Context, IPluginResult>>(stage, message == Message.Custom ? customMessage : message.ToString(), entityName, func));
        }


        /// <summary>
        /// Регистрация события, на возникновение которого будет вызван подключаемый модуль.
        /// </summary>
        /// <param name="pluginEvent">Атрибут с метаданными для регистрации события.</param>
        /// <param name="func">Выполняемый на событие метод.</param>
        protected void RegisterEvent(IPluginEvent pluginEvent, Func<Context, IPluginResult> func)
        {
            this.RegisteredEvents.Add(new Tuple<Stage, string, string, Func<Context, IPluginResult>>(pluginEvent.Stage, pluginEvent.Message == Message.Custom ? pluginEvent.CustomMessage : pluginEvent.Message.ToString(), pluginEvent.PrimaryEntity, func));
        }


        /// <summary>
        /// Регистрация коллекции событий, на возникновение которых будет вызван подключаемый модуль.
        /// </summary>
        /// <param name="pluginEvents">Атрибуты с метаданными для регистрации событий.</param>
        /// <param name="func">Выполняемый на событие метод.</param>
        protected void RegisterEvents(IEnumerable<IPluginEvent> pluginEvents, Func<Context, IPluginResult> func)
        {
            if (pluginEvents == null)
                throw new InvalidPluginExecutionException($"{this.ChildClassName}: There are no registered events.");

            foreach (var pluginEvent in pluginEvents)
            {
                this.RegisteredEvents.Add(new Tuple<Stage, string, string, Func<Context, IPluginResult>>(pluginEvent.Stage, pluginEvent.Message == Message.Custom ? pluginEvent.CustomMessage : pluginEvent.Message.ToString(), pluginEvent.PrimaryEntity, func));
            }
        }


        /// <summary>
        /// Регистрация коллекции команд.
        /// </summary>
        /// <param name="serviceCommands">Коллекция регистрируемых команд.</param>
        protected void RegisterCommands(params IServiceCommand[] serviceCommands)
        {
            foreach (var serviceCommand in serviceCommands)
            {
                this.RegisteredCommands.Add(serviceCommand);
            }
        }


        /// <summary>
        /// Стандартный метод запуска основной бизнес-логики подключаемого модуля.
        /// </summary>
        /// <param name="serviceProvider">Провайдер контекста выполенения подключаемого модуля.</param>
        /// <exception cref="InvalidPluginExecutionException">Ошибка настройки компонента системы.</exception>
        /// <exception cref="InvalidPluginExecutionException">Произошла не ожидаемая ошибка системы.</exception>
        public override void Execute(IServiceProvider serviceProvider)
        {
            var context = new Context(serviceProvider);
            context.TraceMessage($"Entered {this.ChildClassName} internal context Validation");
            context.Validate(this.ChildClassName);
            context.TraceMessage($"Exiting {this.ChildClassName} internal context Validation");
            context.TraceMessage($"Entered {this.ChildClassName}.{nameof(Execute)}()");
            try
            {
                if (_firstExecute)
                {
                    try
                    {
                        Configuring(context);
                        _firstExecute = false;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidPluginExecutionException($"{this.ChildClassName}: System component setting error.\n Please contact support.", ex);
                    }
                }
                //Execute(context).Result(); // заменили на RegisteredEvents
                //------------------------------------------------------------------------------------------------------------------------------------------------
                var registeredEvent = this.RegisteredEvents
                    .Where(re => re.Item1 == context.Stage
                        && (re.Item2 == context.Message.ToString()
                            || (re.Item2 == Message.Custom.ToString() && string.Equals(re.Item2, context.MessageName, StringComparison.OrdinalIgnoreCase)))
                        && (string.IsNullOrWhiteSpace(re.Item3)
                            || re.Item3 == context.SourceContext.PrimaryEntityName))
                    .Select(re => re.Item4).FirstOrDefault();

                // call registered event (first founded)
                if (registeredEvent != null)
                {
                    context.TraceMessage(string.Format(CultureInfo.InvariantCulture, "{0} is firing for Entity: {1}, Message: {2}", new object[]
                    {
                        this.ChildClassName,
                        context.SourceContext.PrimaryEntityName,
                        context.SourceContext.MessageName
                    }));
                    registeredEvent(context).Result();

                    // call registered commands
                    foreach (var registeredCommand in this.RegisteredCommands)
                    {
                        context.TraceMessage($"Entered {registeredCommand.CommandName}.{nameof(IServiceCommand.Execute)}({nameof(Context)} context)");
                        registeredCommand.Execute(context);
                        context.TraceMessage($"Exiting {registeredCommand.CommandName}.{nameof(IServiceCommand.Execute)}({nameof(Context)} context)");
                    }
                }
                //------------------------------------------------------------------------------------------------------------------------------------------------
            }
            catch (FaultException<OrganizationServiceFault> faultException)
            {
                TraceException(context, faultException, new object[] 
                {
                    typeof(FaultException<OrganizationServiceFault>).ToString(),
                    faultException.Detail.Timestamp, 
                    faultException.Detail.ErrorCode, 
                    faultException.Detail.Message,
                    faultException.Detail.InnerFault == null ? "No Inner Fault" : "Has Inner Fault"
                });
                throw;
            }
            catch (TimeoutException ex)
            {
                TraceException(context, ex, typeof(TimeoutException).ToString());
                throw;
            }
            catch (InvalidPluginExecutionException ex)
            {
                TraceException(context, ex, typeof(InvalidPluginExecutionException).ToString());
                throw;
            }
            catch (Exception ex)
            {
                TraceException(context, ex);
                throw new InvalidPluginExecutionException($"{this.ChildClassName}: An unexpected system error.\n Please contact support.", ex);
            }
            finally
            {
                context.TraceMessage($"Exiting {this.ChildClassName}.{nameof(Execute)}()");
            }
        }


        /// <summary>
        /// Записывание данных исключения в сервис трассировки.
        /// </summary>
        /// <param name="context">Конекст выполнения.</param>
        /// <param name="exception">Исключение.</param>
        protected virtual void TraceException(Context context, Exception exception, params object[] data)
        {
            context.TracingService.Trace("=== Plug-in Config ===");
            context.TracingService.Trace(this.UnsecureConfiguration);
            context.TracingService.Trace("=== Plug-in Name ===");
            context.TracingService.Trace(this.ChildClassName);
            context.TracingService.Trace("=== Context ===");
            context.TracingService.Trace("{0}", new
            {
                context.SourceContext.MessageName,
                context.SourceContext.Stage,
                context.SourceContext.PrimaryEntityId,
                context.SourceContext.PrimaryEntityName,
                context.SourceContext.SecondaryEntityName,
                context.SourceContext.UserId,
                context.SourceContext.InitiatingUserId,
                context.SourceContext.InputParameters,
                context.SourceContext.OutputParameters,
                context.SourceContext.SharedVariables,
                context.SourceContext.PreEntityImages,
                context.SourceContext.PostEntityImages,
                context.SourceContext.BusinessUnitId,
                context.SourceContext.CorrelationId,
                context.SourceContext.OperationId,
                context.SourceContext.RequestId,
                context.SourceContext.OrganizationId,
                context.SourceContext.OrganizationName,
                context.SourceContext.Depth,
                context.SourceContext.Mode,
                context.SourceContext.IsExecutingOffline,
                context.SourceContext.IsInTransaction,
                context.SourceContext.IsOfflinePlayback,
                context.SourceContext.IsolationMode,
                context.SourceContext.OperationCreatedOn,
                context.SourceContext.OwningExtension
            });
            context.TracingService.Trace("=== Exception ===");
            var ex = exception;
            while (ex != null)
            {
                context.TracingService.Trace("{0}", ex);
                ex = ex.InnerException;
            }
            context.TracingService.Trace("=== Additional data ===");
            context.TracingService.Trace("{0}", data);
        }


        /// <summary>
        /// Метод, содержащий основную бизнес-логику.
        /// </summary>
        /// <param name="context">Контекст выполнения подключаемого модуля.</param>
        public abstract IPluginResult Execute(Context context);


        /// <summary>
        /// Метод конфигурирования подключаемого модуля, выполняющийся один раз при первом выполнении.
        /// </summary>
        /// <param name="context">Контекст выполнения подключаемого модуля.</param>
        /// <remarks>
        /// В случае возникновения ошибки в процессе конфигурирования метод будет вызван повторно при следующем запуске модуля.
        /// </remarks>
        public virtual void Configuring(Context context)
        {
        }


        /// <summary>
        /// История изменения версий плагина.
        /// </summary>
        /// <returns>
        /// Метод возвращает спиок содержащий историю изменения версии плагина. Элемент истории содержит номер, дату и описание версии.
        /// </returns>
        public virtual List<Tuple<string, DateTime?, string>> VersionHistory()
        {
            return new List<Tuple<string, DateTime?, string>>
            {
                new Tuple<string, DateTime?, string>(null, null, "History not defined")
            };
        }


        /// <summary>
        /// Плагин завершен.
        /// </summary>
        /// <returns>
        /// Метод завершает работу плагина.
        /// </returns>
        protected IPluginResult Ok(string reason = null)
        {
            return new OkPluginResult(reason);
        }


        /// <summary>
        /// Плагин завершен с ошибкой.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <returns>
        /// Метод завершает плагин с ошибкой.
        /// </returns>
        protected IPluginResult Error(string message)
        {
            return new ErrorPluginResult(message);
        }


        /// <summary>
        /// Плагин завершен с ошибкой.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="exception">Содержимое исключения.</param>
        /// <returns>
        /// Метод завершает плагин с ошибкой.
        /// </returns>
        protected IPluginResult Error(string message, Exception exception)
        {
            return new ErrorPluginResult(message, exception);
        }
    }
}
