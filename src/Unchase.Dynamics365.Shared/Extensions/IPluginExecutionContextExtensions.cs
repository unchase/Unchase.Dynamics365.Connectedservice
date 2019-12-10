using Microsoft.Xrm.Sdk;
using Unchase.Dynamics365.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение стандартного функционала класса <see cref="IPluginExecutionContext"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IPluginExecutionContextExtensions
    {
        /// <summary>
        /// Название снимка состояния по умолчанию.
        /// </summary>
        public const string DefaultImageName = "Image";


        /// <summary> 
        /// Получение сущности из контекста подключаемого модуля.
        /// </summary>
        /// <param name="context">Контекст подключаемого модуля.</param>
        /// <exception cref="System.Exception">
        /// Сущность не доступна в контексте данного события.
        /// </exception>
        /// <returns>
        /// Метод возвращает сущность из контекста плагина (если событие предусматривает передачу сущности).
        /// </returns>
        /// <remarks>
        /// Сущность в контекст передается, например, при событиях <see cref="Microsoft.Xrm.Sdk.Messages.CreateRequest"/>, <see cref="Microsoft.Xrm.Sdk.Messages.UpdateRequest"/> и др.
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyPlugin : IPlugin
        /// {
        ///     public void Execute(IServiceProvider serviceProvider)
        ///     {
        ///         ...
        ///         var context = serviceProvider.GetContext();
        ///         var entity = context.GetContextEntity();
        ///         ...
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IServiceProviderExtensions"/>
        public static Entity GetContextEntity(this IPluginExecutionContext context)
        {
            if (context.InputParameters.Contains(Parameter.Target) && context.InputParameters[Parameter.Target] is Entity)
                return (Entity)context.InputParameters[Parameter.Target];
            const string ERROR_MESSAGE = "Entity is not accessible in a context of {0} {1} event.";
            throw new InvalidPluginExecutionException(string.Format(ERROR_MESSAGE, ((Stage)context.Stage).GetDisplayName(), context.MessageName));
        }


        ///// <summary> 
        ///// Получение сущности из контекста подключаемого модуля.
        ///// </summary>
        ///// <param name="context">Контекст подключаемого модуля.</param>
        ///// <exception cref="System.Exception">
        ///// Сущность не доступна в контексте данного события.
        ///// </exception>
        ///// <returns>
        ///// Метод возвращает сущность из контекста плагина (если событие предусматривает передачу сущности).
        ///// </returns>
        ///// <remarks>
        ///// Сущность в контекст передается, например, при событиях <see cref="Microsoft.Xrm.Sdk.Messages.CreateRequest"/>, <see cref="Microsoft.Xrm.Sdk.Messages.UpdateRequest"/> и др.
        ///// </remarks>
        ///// <example>
        ///// <code>
        ///// public class MyPlugin : IPlugin
        ///// {
        /////     public void Execute(IServiceProvider serviceProvider)
        /////     {
        /////         ...
        /////         var context = serviceProvider.GetContext();
        /////         var entity = context.GetContextEntity<Account>();
        /////         ...
        /////     }
        ///// }
        ///// </code>
        ///// </example>
        ///// <seealso cref="IServiceProviderExtensions"/>
        //public static TResult GetContextEntity<TResult>(this IPluginExecutionContext context) where TResult : Entity
        //{
        //    if (context.InputParameters.Contains(Parameter.Target) && context.InputParameters[Parameter.Target] is Entity)
        //        return (TResult)context.InputParameters[Parameter.Target];
        //    const string ERROR_MESSAGE = "Entity is not accessible in a context of {0} {1} event.";
        //    throw new InvalidPluginExecutionException(string.Format(ERROR_MESSAGE, ((Stage)context.Stage).GetDisplayName(), context.MessageName));
        //}


        /// <summary>    
        /// Получение ссылки на сущность из контекста плагина.
        /// </summary>
        /// <param name="context">Контекст плагина.</param>
        /// <exception cref="System.Exception">
        /// Ссылка на сущность не доступна в контексте данного события.
        /// </exception>
        /// <returns>
        /// Метод возвращает ссылку на сущность из контекста плагина, если событие предусматривает передачу ссылки.
        /// </returns>
        /// <remarks>
        /// Ссылки передаются, например, при событиях <see cref="Microsoft.Xrm.Sdk.Messages.DeleteRequest"/>, 
        /// <see cref="Microsoft.Xrm.Sdk.Messages.AssociateRequest"/>, <see cref="Microsoft.Xrm.Sdk.Messages.DisassociateRequest"/> и др.
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyPlugin : IPlugin
        /// {
        ///     public void Execute(IServiceProvider serviceProvider)
        ///     {
        ///         ...
        ///         var context = serviceProvider.GetContext();
        ///         var entityRef = context.GetContextEntityReference();
        ///         ...
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IServiceProviderExtensions"/>
        public static EntityReference GetContextEntityReference(this IPluginExecutionContext context)
        {
            // Delete message
            if (context.InputParameters.Contains(Parameter.Target) && context.InputParameters[Parameter.Target] is EntityReference)
                return (EntityReference)context.InputParameters[Parameter.Target];
            // QualifyLead message
            if (context.InputParameters.Contains(Parameter.LeadIn) && context.InputParameters[Parameter.LeadIn] is EntityReference)
                return (EntityReference)context.InputParameters[Parameter.LeadIn];
            // SetStateDynamicEntity message
            if (context.InputParameters.Contains(Parameter.EntityMoniker) && context.InputParameters[Parameter.EntityMoniker] is EntityReference)
                return (EntityReference)context.InputParameters[Parameter.EntityMoniker];
            // Opportunity Win message
            if (context.InputParameters.Contains(Parameter.OpportunityClose) && context.InputParameters[Parameter.OpportunityClose] is Entity)
                return ((Entity)context.InputParameters[Parameter.OpportunityClose]).GetAttributeValue<EntityReference>("opportunityid");

            const string ERROR_MESSAGE = "EntityReference is not accessible in a context of {0} {1} event.";
            throw new InvalidPluginExecutionException(string.Format(ERROR_MESSAGE, ((Stage)context.Stage).GetDisplayName(), context.MessageName));
        }


        /// <summary>
        /// Получение Pre Image сущности с именем по умолчанию.
        /// </summary>
        /// <param name="context">Контекст плагина.</param>
        /// <returns>Метод возвращает Pre Image сущности с именем по умолчанию. Имя по умолчанию - "Image".</returns>
        /// <exception cref="System.Exception">В шаге плагина не определен Image с именем по умолчанию.</exception>
        public static Entity GetDefaultPreEntityImage(this IPluginExecutionContext context)
        {
            const string DEFAILT_IMAGE_NAME = DefaultImageName;
            if (context.PreEntityImages.ContainsKey(DEFAILT_IMAGE_NAME))
                return context.PreEntityImages[DEFAILT_IMAGE_NAME];
            var entityName = context.PrimaryEntityName;
            var stageName = ((Stage)context.Stage).GetDisplayName();
            var message = $@"Для сущности {entityName} на шаге {stageName} не определен Image с именем ""{DEFAILT_IMAGE_NAME}"".";
            throw new InvalidPluginExecutionException(message);
        }


        /// <summary>
        /// Получение Post Image сущности с именем по умолчанию.
        /// </summary>
        /// <param name="context">Контекст плагина.</param>
        /// <returns>Метод возвращает Post Image сущности с именем по умолчанию. Имя по умолчанию - "Image".</returns>
        /// <exception cref="System.Exception">В шаге плагина не определен Image с именем по умолчанию.</exception>
        public static Entity GetDefaultPostEntityImage(this IPluginExecutionContext context)
        {
            const string DEFAILT_IMAGE_NAME = DefaultImageName;
            if (context.PostEntityImages.ContainsKey(DEFAILT_IMAGE_NAME))
                return context.PostEntityImages[DEFAILT_IMAGE_NAME];
            var entityName = context.PrimaryEntityName;
            var stageName = ((Stage)context.Stage).GetDisplayName();
            var message = $@"Для сущности {entityName} на шаге {stageName} не определен Image с именем ""{DEFAILT_IMAGE_NAME}"".";
            throw new InvalidPluginExecutionException(message);
        }


        /// <summary>
        /// Return OrganizationId and OrganizationName fields as single EntityReference.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference GetOrganization(this IPluginExecutionContext context)
        {
            return new EntityReference()
            {
                Id = context.PrimaryEntityId,
                Name = context.OrganizationName,
                LogicalName = "organization"
            };
        }


        /// <summary>
        /// Return PrimaryEntityId and PrimaryEntityName fields as single EntityReference.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference GetPrimaryEntity(this IPluginExecutionContext context)
        {
            if (Guid.Empty.Equals(context.PrimaryEntityId))
            {
                return null;
            }

            return new EntityReference()
            {
                Id = context.PrimaryEntityId,
                LogicalName = context.PrimaryEntityName
            };
        }


        /// <summary>
        /// Return UserId field as EntityReference.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference GetUser(this IPluginExecutionContext context)
        {
            return new EntityReference()
            {
                Id = context.UserId,
                LogicalName = "systemuser"
            };
        }


        /// <summary>
        /// Return InitiatingUserId field as EntityReference.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference GetInitiatingUser(this IPluginExecutionContext context)
        {
            return new EntityReference()
            {
                Id = context.InitiatingUserId,
                LogicalName = "systemuser"
            };
        }


        /// <summary>
        /// Return BusinessUnitId field as EntityReference.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference GetBusinessUnit(this IPluginExecutionContext context)
        {
            return new EntityReference()
            {
                Id = context.BusinessUnitId,
                LogicalName = "businessunit"
            };
        }


        /// <summary>
        /// Gets input parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetInputParameter<T>(this IPluginExecutionContext context, string name)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return (T)context.InputParameters[name];
        }


        /// <summary>
        /// Gets output parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetOutputParameter<T>(this IPluginExecutionContext context, string name)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return (T)context.OutputParameters[name];
        }


        /// <summary>
        /// Gets pre image.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Entity GetPreImage(this IPluginExecutionContext context, string name)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return context.PreEntityImages[name];
        }


        /// <summary>
        /// Gets pre image as the specified type.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetPreImage<T>(this IPluginExecutionContext context, string name) where T : Entity
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return context.PreEntityImages[name]?.ToEntity<T>();
        }


        /// <summary>
        /// Gets post image.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Entity GetPostImage(this IPluginExecutionContext context, string name)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return context.PostEntityImages[name];
        }


        /// <summary>
        /// Gets post image as the specified type.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetPostImage<T>(this IPluginExecutionContext context, string name) where T : Entity
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return context.PostEntityImages[name]?.ToEntity<T>();
        }


        /// <summary>
        /// Shortcut for getting "Target" input parameter of type Entity.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static Entity GetTarget(this IPluginExecutionContext context)
        {
            return GetInputParameter<Entity>(context, "Target");
        }


        /// <summary>
        /// Shortcut for getting "Target" input parameter  as the specified type.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static T GetTarget<T>(this IPluginExecutionContext context) where T : Entity
        {
            return GetTarget(context)?.ToEntity<T>();
        }


        /// <summary>
        /// Get "Target" entity parameter merged with specified pre image.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetPreTarget<T>(this IPluginExecutionContext context, string name) where T : Entity
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            T target = GetTarget<T>(context);
            T image = GetPreImage<T>(context, name);

            target?.MergeAttributes(image);

            return target;
        }


        /// <summary>
        /// Get "Target" entity parameter merged with specified post image.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetPostTarget<T>(this IPluginExecutionContext context, string name) where T : Entity
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            T target = GetTarget<T>(context);
            T image = GetPostImage<T>(context, name);

            target?.MergeAttributes(image);

            return target;
        }


        /// <summary>
        /// Gets shared Variable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetSharedVariable<T>(this IPluginExecutionContext context, string name)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            return (T)context.SharedVariables[name];
        }


        /// <summary>
        /// Simplifies handling of Associate and Disassociate messages. This messages can't be filtered by entity type, furthermore
        /// two options possible: when "A" entity is associated with array of "B", or "B" is associated with array of "A".
        /// 
        /// This method generates universal dictionary of arguments which is suitable in all cases.
        /// </summary>
        /// <param name="keyEntity">Key entity schema name.</param>
        /// <param name="valueEntity">Secondary entity schema name.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Dictionary<EntityReference, EntityReferenceCollection> GetRelatedEntitiesByTarget(this IPluginExecutionContext pluginContext,
            string keyEntity,
            string valueEntity)
        {
            CheckParam.CheckForNullOrWhiteSpace(keyEntity, nameof(keyEntity));
            CheckParam.CheckForNullOrWhiteSpace(valueEntity, nameof(valueEntity));

            // Check that we handling appropriate message
            if (pluginContext.MessageName != "Associate" && pluginContext.MessageName != "Disassociate")
            {
                throw new InvalidOperationException($"This method is not supported for { pluginContext.MessageName } message");
            }

            // Get InputParameters for Associate и Disassociate 
            EntityReference target = pluginContext.InputParameters["Target"] as EntityReference;
            EntityReferenceCollection relatedEntities = pluginContext.InputParameters["RelatedEntities"] as EntityReferenceCollection;

            // Get schema names for this participating entities
            string targetName = target.LogicalName;
            string relatedName = relatedEntities.First().LogicalName;

            // Generate result dictionary
            Dictionary<EntityReference, EntityReferenceCollection> dictionary = new Dictionary<EntityReference, EntityReferenceCollection>(relatedEntities.Count);

            if (targetName == keyEntity && relatedName == valueEntity)
            {
                dictionary.Add(target, relatedEntities);
            }
            else if (relatedName == keyEntity && targetName == valueEntity)
            {
                foreach (EntityReference key in relatedEntities)
                {
                    dictionary.Add(key, new EntityReferenceCollection() { target });
                }
            }

            return dictionary;
        }
    }
}
