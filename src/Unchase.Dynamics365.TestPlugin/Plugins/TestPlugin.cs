using Unchase.Dynamics365.TestPlugin.Commands;
using Unchase.Dynamics365.Shared;
using Unchase.Dynamics365.Shared.Enums;
using Unchase.Dynamics365.Shared.Extensions;
using Unchase.Dynamics365.Shared.Models;
using System;
using Unchase.Dynamics365.Shared.Attributes;
using System.Reflection;

namespace Unchase.Dynamics365.TestPlugin.Plugins
{
    /// <summary>
    /// Тестовый подключаемый модуль для проверки работоспособности.
    /// </summary>
    /// <remarks>
    /// Нужен для тестирования взаимодействия с MS Dynamics 365.
    /// </remarks>
    [PluginRegisteredEvent("Account", Stage.PreOperation, Message.Create)]
    [PluginRegisteredEvent("Account", Stage.PreOperation, Message.Update)]
    [PluginRegisteredEvent("Account", Stage.PreOperation, Message.Custom, "custom_message")]
    public class TestPlugin : PluginBase
    {
        /// <summary>
        /// Стандартный конструктор класса.
        /// </summary>
        /// <remarks>
        /// <param name="unsecureConfig">Не защищенная конфигурация.</param>
        /// <param name="secureConfig">Защищенная конфигурация.</param>
        /// Здесь регистрируются все события, при возникновении одного из которых будет вызвано выполнение подключаемого модуля.
        /// </remarks>
        public TestPlugin(string unsecureConfig = null, string secureConfig = null) : base(typeof(TestPlugin), unsecureConfig, secureConfig)
        {
            // регистрируем исполняемые события (будут исполнены до команд)
            base.RegisterEvents(this.GetType().GetCustomAttributes<PluginRegisteredEventAttribute>(), new Func<Context, IPluginResult>(this.Execute));

            // регистрируем исполняемые после событий команды
            base.RegisterCommands(new TestServiceCommand("testString"), new TestServiceCommand2(1));
        }


        /// <inheritdoc/>
        public override IPluginResult Execute(Context context)
        {
            // check and get input parameter "ContactId"
            var contactId = context.CheckAndGetInputParameter<string>(Parameter.ContactId, true, this.ChildClassType);

            //ToDo: put your plugin logic there

            // fill output parameter "errorCode"
            context.SetOutputParameter("errorCode", 1, true, this.ChildClassType);

            // fill output parameter "errorMessage"
            context.SetOutputParameter("errorMessage", "Error message string", true, this.ChildClassType);

            return Ok("Plugin execution successed.");
        }
    }
}
