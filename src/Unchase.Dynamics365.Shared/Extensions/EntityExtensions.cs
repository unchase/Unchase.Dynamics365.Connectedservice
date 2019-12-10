using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение функциональности класса <see cref="Entity"/>.
    /// </summary>
    // ReSharper disable once CheckNamespace
    public static class EntityExtensions
    {
        /// <summary>
        /// Расширение атрибутного состава сущности из других сущностей.
        /// </summary>
        /// <param name="entity">Сущность.</param>
        /// <param name="otherEntities">Набор других сущностей, атрибуты которых последовательно переносятся в исходную сущность.</param>
        /// <returns>
        /// Метод сливает атрибуты нескольких сущностей в одну.
        /// </returns>
        public static void Extend(this Entity entity, params Entity[] otherEntities)
        {
            foreach (var otherEntity in otherEntities)
            {
                foreach (var attribute in otherEntity.Attributes)
                    entity[attribute.Key] = attribute.Value;
            }
        }


        /// <summary>
        /// Копия сущности без значений полей.
        /// </summary>
        /// <param name="entity">Копируемая сущность.</param>
        /// <returns>
        /// Метод возвращет копию сущности, у которой указаны только тип и идентификатор.
        /// </returns>
        public static Entity ToCleanEntity(this Entity entity)
        {
            return new Entity(entity.LogicalName, entity.Id);
        }


        /// <summary>
        /// Simplifies getting values from Entity.FormattedValues collection.
        /// <param name="attributeLogicalName">Attribute name.</param>
        /// <returns>
        /// Attribute formated value.
        /// </returns>
        public static string GetFormatedValue(this Entity entity, string attributeLogicalName)
        {
            CheckParam.CheckForNullOrWhiteSpace(attributeLogicalName, nameof(attributeLogicalName));

            entity.FormattedValues.TryGetValue(attributeLogicalName, out string outValue);
            return outValue;
        }


        /// <summary>
        /// Simplifies getting values from linked entities attributes wrapped in AliasedValue class.
        /// This kind of attributes can be queried by FetchExpression or QueryExpression using Linked Entities .
        /// </summary>
        /// <typeparam name="T">Attribute value type.</typeparam>
        /// <param name="attributeLogicalName">Attribute logical name.</param>
        /// <param name="alias">>Entity alias used in LinkedEntity definition.</param>
        /// <returns>
        /// Attribute value.
        /// </returns>
        public static T GetAliasedValue<T>(this Entity entity, string attributeLogicalName, string alias)
        {
            CheckParam.CheckForNullOrWhiteSpace(attributeLogicalName, nameof(attributeLogicalName));
            CheckParam.CheckForNullOrWhiteSpace(alias, nameof(alias));

            string aliasedAttributeName = alias + "." + attributeLogicalName;
            AliasedValue aliasedValue = entity.GetAttributeValue<AliasedValue>(aliasedAttributeName);

            if (aliasedValue != null)
            {
                return (T)aliasedValue.Value;
            }
            else
            {
                return default;
            }
        }


        /// <summary>
        /// Simplifies getting multiple linked entity attributes by allocating them to separate Entity.
        /// </summary>
        /// <param name="entityLogicalName">Logical name of linked Entity.</param>
        /// <param name="alias">Entity alias used in LinkedEntity definition.</param>
        /// <returns>
        /// Entity with specified logical name that contains all attribute values with specified alias.
        /// </returns>
        public static Entity GetAliasedEntity(this Entity entity, string entityLogicalName, string alias = null)
        {
            CheckParam.CheckForNullOrWhiteSpace(entityLogicalName, nameof(entityLogicalName));

            /// Use LogicalName as alias if it is not specified
            string aliasPrefix = alias ?? entityLogicalName + ".";

            var aliasedAttributes = entity.Attributes.Where(a => a.Key.StartsWith(aliasPrefix))
                .Select(a => a.Value as AliasedValue)
                .Where(a => a != null)
                .Select(a => new KeyValuePair<string, object>(a.AttributeLogicalName, a.Value));

            Entity aliasedEntity = new Entity(entityLogicalName);
            aliasedEntity.Attributes.AddRange(aliasedAttributes);

            return aliasedEntity;
        }


        /// <summary>
        /// Generic version of GetAliasedEntity.
        /// </summary>
        /// <param name="entityLogicalName">Logical name of linked Entity.</param>
        /// <param name="alias">Entity alias used in LinkedEntity definition.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T GetAliasedEntity<T>(this Entity entity, string entityLogicalName, string alias = null) where T : Entity
        {
            CheckParam.CheckForNullOrWhiteSpace(entityLogicalName, nameof(entityLogicalName));

            return GetAliasedEntity(entity, entityLogicalName, alias).ToEntity<T>();
        }


        /// <summary>
        /// Add attributes form source Entity if they don't exist in target Entity.
        /// </summary>
        /// <param name="source">Entity to take attributes form.</param>
        public static void MergeAttributes(this Entity target, Entity source)
        {
            CheckParam.CheckForNull(source, nameof(source));

            if (source != null)
            {
                foreach (var attribute in source.Attributes)
                {
                    if (target.Attributes.ContainsKey(attribute.Key) == false)
                    {
                        target.Attributes.Add(attribute);
                    }
                }
            }
        }


        /// <summary>
        /// Safely sets attribute value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// True if new attribute added.
        /// </returns>
        public static bool SetAttributeValue(this Entity entity, string name, object value)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));

            if (entity.Contains(name))
            {
                entity[name] = value;
                return false;
            }
            else
            {
                entity.Attributes.Add(name, value);
                return true;
            }
        }


        /// <summary>
        /// As it turns out, OOB ToEntityReference is not copying KeyAttributes collection.
        /// New parameter has to be added to be valid override of ToEntityReference.
        /// </summary>
        /// <param name="withKeys">Copy KeyAttributes collection.</param>
        /// <returns>
        /// 
        /// </returns>
        public static EntityReference ToEntityReference(this Entity entity, bool withKeys)
        {
            var reference = entity.ToEntityReference();
            if (withKeys)
            {
                reference.KeyAttributes = entity.KeyAttributes;
            }
            return reference;
        }


        public static string GetOrThrow(this Entity entity, string name, bool isRequired = true)
        {
            string text = entity.Attributes.Keys.FirstOrDefault((string key) => key.ToLower().Equals(name.ToLower()));
            if (text == null)
            {
                if (isRequired)
                {
                    throw new InvalidPluginExecutionException(string.Format("Instance ({0}) of entity '{1}' mot contain field '{2}'.", entity.Id, entity.LogicalName, name));
                }
                return string.Empty;
            }
            else
            {
                return entity[text].ToString();
            }
        }


        public static T GetOrThrow<T>(this Entity entity, string name)
        {
            string text = entity.Attributes.Keys.FirstOrDefault((string key) => key.ToLower().Equals(name.ToLower()));
            if (text == null)
            {
                throw new InvalidPluginExecutionException(string.Format("Instance ({0}) of entity '{1}' mot contain field '{2}'.", entity.Id, entity.LogicalName, name));
            }
            return (T)entity[text];
        }


        public static T GetOrDefault<T>(this Entity entity, string name)
        {
            if (entity == null)
            {
                return default;
            }
            else
            {
                string text = entity.Attributes.Keys.FirstOrDefault((string key) => key.ToLower().Equals(name.ToLower()));
                return (text == null) ? default : ((T)entity[text]);
            }
        }


        public static T GetOrNull<T>(this Entity entity, string name) where T : class
        {
            if (entity == null)
            {
                return default;
            }
            else
            {
                string text = entity.Attributes.Keys.FirstOrDefault((string key) => key.ToLower().Equals(name.ToLower()));
                return (text == null) ? default : ((T)entity[text]);
            }
        }


        public static T? GetOrNullable<T>(this Entity entity, string name) where T : struct
        {
            if (entity == null)
            {
                return null;
            }
            else
            {
                string text = entity.Attributes.Keys.FirstOrDefault((string key) => key.ToLower().Equals(name.ToLower()));
                return ((text == null) ? null : new T?((T)entity[text]));
            }
        }
    }
}
