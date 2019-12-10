using Microsoft.Xrm.Sdk;
using System;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение функционала класса <see cref="Context"/>.
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Проверка параметра метода <see cref="PluginBase.Execute(Context)"/>.
        /// </summary>
        /// <param name="context">Экземпляр класса <see cref="Context"/>.</param>
        /// <param name="pluginName">Имя класса плагина (не обязательный).</param>
        /// <param name="maxDepthOfExecution">Максимальная глубина вызова плагина (не обязательный).</param>
        /// <example>
        /// <code>
        /// public void Execute(Context context)
        /// {
        ///     context.Validate(typeof(MyPlugin));
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static void Validate(this Context context, string pluginName = null, uint maxDepthOfExecution = 0)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), $"{pluginName ?? "Unnamed plugin"}: Failed to Creating the plugin internal context.");
            }
            if (context.SourceContext == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"{pluginName ?? "Unnamed plugin"}: Failed to Receiving the plugin execution context.");
            }
            if (context.Service == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"{pluginName ?? "Unnamed plugin"}: Failed to Receiving the organization service.");
            }
            if (context.TracingService == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"{pluginName ?? "Unnamed plugin"}: Failed to Receiving the tracing service.");
            }
            if (maxDepthOfExecution > 0 && context.SourceContext.Depth > maxDepthOfExecution)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"{pluginName ?? "Unnamed plugin"}: Maximun depth ({maxDepthOfExecution}) of execution was reached.");
            }
        }


        /// <summary>
        /// Получение значения входного параметра, если он задан.
        /// </summary>
        /// <typeparam name="T">Тип входного параметра.</typeparam>
        /// <param name="context">Экземпляр класса <see cref="Context"/>.</param>
        /// <param name="parameterName">Имя входного параметра.</param>
        /// <param name="withTracing">Использовать трассировку (не обязательный).</param>
        /// <param name="pluginType">Тип класса плагина (не обязательный).</param>
        /// <example>
        /// <code>
        /// public void Execute(Context context)
        /// {
        ///     ...
        ///     context.CheckAndGetInputParameter<string>(Parameter.ContactId, typeof(MyPlugin));
        ///     ...
        /// }
        /// </code>
        /// </example>
        /// <returns>
        /// Метод возвращает значение входного параметра, если он задан.
        /// </returns>
        public static T CheckAndGetInputParameter<T>(this Context context, string parameterName, bool withTracing = false, Type pluginType = null)
        {
            CheckParam.CheckForNullOrWhiteSpace(parameterName, nameof(parameterName));

            if (!context.SourceContext.InputParameters.Contains(parameterName))
            {
                throw new InvalidPluginExecutionException($"{pluginType.ToString() ?? "Unnamed plugin"}: LocalContext not contain key '{parameterName}'. Keys ({context.SourceContext.InputParameters.Count}): {string.Join(", ", context.SourceContext.InputParameters.Keys)}.");
            }

            if (!(context.SourceContext.InputParameters[parameterName] is T inputParameter))
            {
                if (withTracing)
                {
                    context.TraceMessage($"{pluginType.ToString() ?? "Unnamed plugin"}: input parameter \"{parameterName}\" ({typeof(T).ToString()}) was not passed.");
                }
                throw new InvalidPluginExecutionException($"{pluginType.ToString() ?? "Unnamed plugin"}: LocalContext contain key '{parameterName}' with other type ({context.SourceContext.InputParameters[parameterName].GetType().ToString()}) than {typeof(T).ToString()}.");
            }
            else
            {
                if (withTracing)
                {
                    context.TraceMessage($"{pluginType.ToString() ?? "Unnamed plugin"}: input parameter \"{parameterName}\" ({typeof(T).ToString()}) was getted.");
                }
                return inputParameter;
            }
        }


        /// <summary>
        /// Выставление значения выходного параметра.
        /// </summary>
        /// <param name="context">Экземпляр класса <see cref="Context"/>.</param>
        /// <param name="parameterName">Имя выходного параметра.</param>
        /// <param name="parameterValue">Значение выходного параметра.</param>
        /// <param name="withTracing">Использовать трассировку (не обязательный).</param>
        /// <param name="pluginType">Тип класса плагина (не обязательный).</param>
        /// <example>
        /// <code>
        /// public void Execute(Context context)
        /// {
        ///     ...
        ///     context.SetOutputParameter(Parameter.ContactId, Guid.NewGuid());
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static void SetOutputParameter(this Context context, string parameterName, object parameterValue, bool withTracing = false, Type pluginType = null)
        {
            CheckParam.CheckForNullOrWhiteSpace(parameterName, nameof(parameterName));

            if (context.SourceContext.OutputParameters.Contains(parameterName))
            {
                context.SourceContext.OutputParameters[parameterName] = parameterValue;
            }
            else
            {
                context.SourceContext.OutputParameters.Add(parameterName, parameterValue);
            }

            if (withTracing)
            {
                context.TraceMessage($"{pluginType.ToString() ?? "Unnamed plugin"}: output parameter \"{parameterName}\" ({parameterValue.GetType().ToString()}) was added.");
            }
        }
    }
}
