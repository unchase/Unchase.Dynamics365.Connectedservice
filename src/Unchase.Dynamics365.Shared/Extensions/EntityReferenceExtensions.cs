using Microsoft.Xrm.Sdk;

namespace Unchase.Dynamics365.Shared.Extensions
{
    ///// <summary>
    ///// Set of extension methods for Microsoft.Xrm.Sdk.EntityReference base class. At the moment just two simple but sometimes useful type conversion methods.
    ///// </summary>

    /// <summary>
    /// Расширение функциональности класса <see cref="EntityReference"/>.
    /// </summary>
    public static class EntityReferenceExtensions
    {
        /// <summary>
        /// Получение сущности из ссылки.
        /// </summary>
        /// <param name="entityRef">Ссылка на сущность.</param>
        /// <param name="withKeys">Copy KeyAttributes collection.</param>
        /// <returns>Метод возвращает сущность с именем и идентификатором, соответствующим ссылке.</returns>
        public static Entity ToEntity(this EntityReference entityRef, bool withKeys)
        {
            var entity = new Entity(entityRef.LogicalName, entityRef.Id);
            if (withKeys)
            {
                entity.KeyAttributes = entityRef.KeyAttributes;
            };
            return entity;
        }


        /// <summary>
        /// Gets the entity based on the EntityReference as the specified type.
        /// </summary>
        /// <param name="withKeys">Copy KeyAttributes collection.</param>
        /// <returns>
        /// 
        /// </returns>
        public static T ToEntity<T>(this EntityReference entityReference, bool withKeys) where T : Entity
        {
            return entityReference.ToEntity(withKeys).ToEntity<T>();
        }
    }
}
