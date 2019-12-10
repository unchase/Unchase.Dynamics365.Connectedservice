using System;
using Unchase.Dynamics365.Shared.Enums;
using Unchase.Dynamics365.Shared.Models;

namespace Unchase.Dynamics365.Shared.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PluginRegisteredEventAttribute : Attribute, IPluginEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public Stage Stage { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public Message Message { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string PrimaryEntity { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string CustomMessage { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        /// <param name="customMessage"></param>
        public PluginRegisteredEventAttribute(string entityName, Stage stage = Stage.PostOperation, Message message = Message.Custom, string customMessage = null)
        {
            PrimaryEntity = entityName;
            Stage = stage;
            Message = message;
            CustomMessage = customMessage;
        }
    }
}
