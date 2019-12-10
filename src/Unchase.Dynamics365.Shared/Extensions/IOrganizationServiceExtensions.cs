using Microsoft.Crm.Sdk.Messages;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using Unchase.Dynamics365.Shared.Enums;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Net;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata.Query;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using Unchase.Dynamics365.Shared.Models;

namespace Unchase.Dynamics365.Shared.Extensions
{
    /// <summary>
    /// Расширение функционала классов, реализующих интерфейс <see cref="IOrganizationService"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IOrganizationServiceExtensions
    {
        #region CreateService

        //ToDo: доработать и проверить
        /// <summary>
        /// Создать экземпляр класса, реализующего <see cref="IOrganizationService"/>.
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <param name="domain"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IOrganizationService CreateService(Uri serviceUri, string domain, string userName, string password)
        {
            IOrganizationService organizationService = null;
            try
            {
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.Windows.ClientCredential.Domain = domain;
                clientCredentials.Windows.ClientCredential.UserName = userName;
                clientCredentials.Windows.ClientCredential.Password = password;

                clientCredentials.UserName.UserName = userName;
                clientCredentials.UserName.Password = password;

                // For Dynamics 365 Customer Engagement V9.X, set Security Protocol as TLS12
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                organizationService = new OrganizationServiceProxy(serviceUri, null, clientCredentials, null);
                if (organizationService != null)
                {
                    var gOrgId = organizationService.Execute<WhoAmIResponse>(new WhoAmIRequest()).OrganizationId;
                    if (gOrgId != Guid.Empty)
                    {
                        Console.WriteLine("Connection Established Successfully...");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to Established Connection!!!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - " + ex.Message);
            }
            return organizationService;
        }

        #endregion


        #region Retrieve

        /// <summary>
        /// Получение отображаемого в CRM имени атрибута.
        /// </summary>
        /// <param name="entitySchemaName"></param>
        /// <param name="attributeSchemaName"></param>
        /// <returns>
        /// Метод возвращает отображаемое в CRM имя атрибута.
        /// </returns>
        public static string RetrieveAttributeDisplayName(this IOrganizationService service, string entitySchemaName, string attributeSchemaName)
        {
            CheckParam.CheckForNullOrWhiteSpace(entitySchemaName, nameof(entitySchemaName));
            CheckParam.CheckForNullOrWhiteSpace(attributeSchemaName, nameof(attributeSchemaName));

            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entitySchemaName,
                LogicalName = attributeSchemaName,
                RetrieveAsIfPublished = true
            };
            var retrievedAttributeMetadata = service.Execute<RetrieveAttributeResponse>(retrieveAttributeRequest).AttributeMetadata;

            //Get Length
            // Int32 length = (Int32)((Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata)retrievedAttributeMetadata).MaxLength;
            return retrievedAttributeMetadata.DisplayName.UserLocalizedLabel.Label;
        }


        ///// <summary>
        ///// Retrieve method override. Takes EntityReference as input parameter.
        ///// </summary>
        ///// <param name="reference">Entity to retrieve.</param>

        /// <summary>
        /// Получение записи по ссылке на нее.
        /// </summary>
        /// <param name="service">Экземпляр сервиса CRM.</param>
        /// <param name="entityRef">Ссылка на сущность.</param>
        /// <param name="columnSet">Набор атрибутов сущности.</param>
        /// <returns>
        /// Метод возвращает одну запись по ссылке на нее.
        /// </returns>
        public static Entity Retrieve(this IOrganizationService service, EntityReference reference, ColumnSet columnSet)
        {
            CheckParam.CheckForNull(reference, nameof(reference));

            /// Retrieve by id if possible as is supposed to be faster
            if (reference.Id != Guid.Empty)
            {
                return service.Retrieve(reference.LogicalName, reference.Id, columnSet);
            }
            /// Use alternative key
            else
            {
                return service.Retrieve(reference.LogicalName, reference.KeyAttributes, columnSet);
            }
        }


        /// <summary>
        /// Retrieve method override. Takes EntityReference as input parameter and array of attribute names to retrieve.
        /// </summary>
        /// <param name="reference">Entity to retrieve.</param>
        public static Entity Retrieve(this IOrganizationService service, EntityReference reference, params string[] columns)
        {
            return service.Retrieve(reference, new ColumnSet(columns));
        }


        /// <summary>
        /// Retrieve method override. Takes EntityReference as input parameter and return strongly typed entity object.
        /// </summary>
        /// <param name="reference">Entity to retrieve.</param>
        public static T Retrieve<T>(this IOrganizationService service, EntityReference reference, ColumnSet columnSet) where T : Entity
        {
            Entity entity = service.Retrieve(reference, columnSet);

            return entity.ToEntity<T>();
        }


        /// <summary>
        /// Retrieve method override. Takes EntityReference as input parameter and return strongly typed entity object.
        /// </summary>
        /// <param name="reference">Entity to retrieve.</param>
        public static T Retrieve<T>(this IOrganizationService service, EntityReference reference, params string[] columns) where T : Entity
        {
            return service.Retrieve<T>(reference, new ColumnSet(columns));
        }


        /// <summary>
        /// Retrieve method override. Retrieves by Alternative key.
        /// </summary>
        /// <param name="keyName">Name of alternative key.</param>
        /// <param name="keyValue">Key value.</param>
        public static Entity Retrieve(this IOrganizationService service, string logicalName, KeyAttributeCollection keyAttributeCollection, ColumnSet columnSet)
        {
            CheckParam.CheckForNullOrWhiteSpace(logicalName, nameof(logicalName));
            CheckParam.CheckForNull(keyAttributeCollection, nameof(keyAttributeCollection));

            RetrieveResponse response = service.Execute(new RetrieveRequest()
            {
                Target = new EntityReference(logicalName, keyAttributeCollection),
                ColumnSet = columnSet
            }) as RetrieveResponse;

            return response.Entity;
        }


        /// <summary>
        /// Retrieve method override. Retrieves by Alternative key.
        /// </summary>
        /// <param name="keyName">Name of alternative key.</param>
        /// <param name="keyValue">Key value.</param>
        public static Entity Retrieve(this IOrganizationService service, string logicalName, string keyName, object keyValue, ColumnSet columnSet)
        {
            CheckParam.CheckForNullOrWhiteSpace(keyName, nameof(keyName));
            CheckParam.CheckForNull(keyValue, nameof(keyValue));

            KeyAttributeCollection keys = new KeyAttributeCollection();
            keys.Add(keyName, keyValue);

            return service.Retrieve(logicalName, keys, columnSet);
        }


        /// <summary>
        /// Retrieve method override. Retrieves by Alternative key.
        /// </summary>
        /// <param name="keyName">Name of alternative key.</param>
        /// <param name="keyValue">Key value.</param>
        public static Entity Retrieve(this IOrganizationService service, string logicalName, string keyName, object keyValue, params string[] columns)
        {
            return service.Retrieve(logicalName, keyName, keyValue, new ColumnSet(columns));
        }


        /// <summary>
        /// Retrieve method override. Retrieves by Alternative key and returns strongly typed entity object.
        /// </summary>
        /// <param name="keyName">Name of alternative key.</param>
        /// <param name="keyValue">Key value.</param>
        public static T Retrieve<T>(this IOrganizationService service, string logicalName, string keyName, string keyValue, ColumnSet columnSet) where T : Entity
        {
            Entity entity = service.Retrieve(logicalName, keyName, keyValue, columnSet);

            return entity.ToEntity<T>();
        }


        /// <summary>
        /// Retrieve method override. Retrieves by Alternative key and returns strongly typed entity object.
        /// </summary>
        /// <param name="keyName">Name of alternative key.</param>
        /// <param name="keyValue">Key value.</param>
        public static T Retrieve<T>(this IOrganizationService service, string logicalName, string keyName, string keyValue, params String[] columns) where T : Entity
        {
            return service.Retrieve<T>(logicalName, keyName, keyValue, new ColumnSet(columns));
        }

        #endregion


        #region RetrieveMultiple

        /// <summary>
        /// Получение списка записей по запросу FetchXML.
        /// </summary>
        /// <param name="service">Экземпляр сервиса CRM.</param>
        /// <param name="fetchXml">Текст запроса.</param>
        /// <returns>
        /// Метод возвращает список найденных записей.
        /// </returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService service, string fetchXml)
        {
            CheckParam.CheckForNullOrWhiteSpace(fetchXml, nameof(fetchXml));

            return service.RetrieveMultiple(new FetchExpression(fetchXml));
        }


        /// <summary>
        /// Universal RetrieveMultiple method override. Returns all pages using callback or 'yield' iterator.
        /// </summary>
        /// <param name="query">A query that determines the set of record.</param>
        /// <param name="callback">Optional function to be called for each record page.</param>
        /// <returns>
        /// Entity set as 'yield' iterator.
        /// </returns>
        public static IEnumerable<Entity> RetrieveMultiple(this IOrganizationService service, QueryBase query, Action<EntityCollection> callback = null)
        {
            CheckParam.CheckForNull(query, nameof(query));

            EntityCollection collection = new EntityCollection
            {
                MoreRecords = true
            };

            while (collection.MoreRecords)
            {
                /// Paging start working if Page > 1
                query.NextPage(collection.PagingCookie);

                collection = service.RetrieveMultiple(query);
                callback?.Invoke(collection);

                foreach (Entity entity in collection.Entities)
                {
                    yield return entity;
                }
            }
        }


        /// <summary>
        /// RetrieveMultiple method override optimized for FetchExpression. Returns all pages using callback or 'yield' iterator.
        /// </summary>
        /// <param name="query">A query that determines the set of record.</param>
        /// <param name="callback">Optional function to be called for each record page.</param>
        /// <returns>Entity set as 'yield' iterator.</returns>
        public static IEnumerable<Entity> RetrieveMultiple(this IOrganizationService service, FetchExpression query, Action<EntityCollection> callback = null)
        {
            CheckParam.CheckForNull(query, nameof(query));

            EntityCollection collection = new EntityCollection
            {
                MoreRecords = true
            };

            /// For performance reasons it's better to load XML once
            XDocument document = XDocument.Parse(query.Query);

            while (collection.MoreRecords)
            {
                /// Paging start working if Page > 1
                query.NextPage(document, collection.PagingCookie);

                collection = service.RetrieveMultiple(query);
                callback?.Invoke(collection);

                foreach (Entity entity in collection.Entities)
                {
                    yield return entity;
                }
            }
        }

        #endregion


        #region Execute

        /// <summary>
        /// Выполнение запроса и получение ответа указанного типа.
        /// </summary>
        /// <param name="service">Экземпляр сервиса CRM.</param>
        /// <param name="request">Запрос.</param>
        /// <typeparam name="TResponse">Тип ответа запроса.</typeparam>
        /// <returns>
        /// Метод выполняет запрос и возвращает ответ указанного типа.
        /// </returns>
        public static TResponse Execute<TResponse>(this IOrganizationService service, OrganizationRequest request) where TResponse : OrganizationResponse
        {
            CheckParam.CheckForNull(request, nameof(request));

            return (TResponse)service.Execute(request);
        }


        /// <summary>
        /// A shortcut for Upsert message. There is much more messages to create shortcut for, but this one is only useful for daily CRUD operations.
        /// </summary>
        public static EntityReference Upsert(this IOrganizationService service, Entity entity)
        {
            CheckParam.CheckForNull(entity, nameof(entity));

            UpsertResponse response = service.Execute<UpsertResponse>(new UpsertRequest
            {
                Target = entity
            });

            return response.Target;
        }

        #endregion


        #region Version

        /// <summary>
        /// Получение номера версии CRM.
        /// </summary>
        /// <returns>
        /// Метод возвращает номер версии CRM.
        /// </returns>
        public static string GetCrmVersion(this IOrganizationService service)
        {
            return service.Execute<RetrieveVersionResponse>(new RetrieveVersionRequest()).Version;
        }

        #endregion


        #region Metadata

        /// <summary>
        /// Получение метки элемента списка значений.
        /// </summary>
        /// <param name="service">Экземпляр сервиса CRM.</param>
        /// <param name="entityName">Системное имя сущности.</param>
        /// <param name="attributeName">Системное имя атрибута сущности типа "Список значений" (Picklist).</param>
        /// <param name="value">Числовое значение элемента списка.</param>
        /// <returns>
        /// Метод возвращает локальную метку элемента списка значений.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">В метаданных списка нет указанного в <paramref name="value"/> значения.</exception>
        /// <exception cref="InvalidPluginExecutionException">Неизвестная ошибка.</exception>
        public static string RetrievePicklistItemLabel(this IOrganizationService service, string entityName, string attributeName, int value)
        {
            CheckParam.CheckForNullOrWhiteSpace(entityName, nameof(entityName));
            CheckParam.CheckForNullOrWhiteSpace(attributeName, nameof(attributeName));

            try
            {
                var request = new RetrieveAttributeRequest { EntityLogicalName = entityName, LogicalName = attributeName };
                var response = (RetrieveAttributeResponse)service.Execute(request);
                var picklistMetadata = (PicklistAttributeMetadata)response.AttributeMetadata;
                var option = picklistMetadata.OptionSet.Options.FirstOrDefault(item => item.Value.HasValue && item.Value.Value == value);
                if (option == null)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Getting picklist label error. Unknown value {value}.");
                return option.Label.UserLocalizedLabel.Label;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Getting picklist label error. {ex.Message}", ex);
            }
        }

        #endregion


        #region User Info

        /// <summary>
        /// Получение данных о пользователе, вызывающем плагин.
        /// </summary>
        /// <returns>
        /// Метод возвращает данные о пользователе, вызывающем плагин.
        /// </returns>
        internal static string GetUserInfo(this IOrganizationService service)
        {
            var callerIdProperty = service.GetType().GetProperty("CallerId");
            var callerId = (Guid)callerIdProperty.GetValue(service);

            var request = new WhoAmIRequest();
            var response = (WhoAmIResponse)service.Execute(request);
            var userId = response.UserId;
            var user = service.Retrieve("systemuser", userId, new ColumnSet("fullname"));
            var data = $"{userId} | {user.GetAttributeValue<string>("fullname")} | callerId: {callerId}";
            return data;
        }

        #endregion


        #region Web Resources

        /// <summary>
        /// Создание веб-ресурса.
        /// </summary>
        /// <param name="type">Тип веб-ресурса.</param>
        /// <param name="name">Уникальное имя веб-ресурса.</param>
        /// <param name="displayName">Отображаемое имя веб-ресурса.</param>
        /// <param name="description">Описание веб-ресурса.</param>
        /// <param name="content">Содержимое файла веб-ресурса.</param>
        /// <returns>
        /// <remarks>
        /// Если параметр <paramref name="displayName"/> равен <c>null</c> или
        /// пустой строке, то в качестве отображаемого имени используется
        /// значение параметра <paramref name="name"/>.
        /// </remarks>
        /// Метод возвращает идентификатор записи созданного веб-ресурса.
        /// </returns>
        public static Guid CreateWebResource(this IOrganizationService service, WebResourceType type, string name, string displayName, string description, string content)
        {
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));
            CheckParam.CheckForNullOrWhiteSpace(content, nameof(content));

            var resource = new Entity("webresource")
            {
                Attributes = new AttributeCollection
                {
                  { "webresourcetype", new OptionSetValue((int)type) },
                  { "name", name },
                  { "displayname",
                    string.IsNullOrEmpty(displayName) ? name : displayName },
                  { "description", description },
                  { "content", Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(content)) }
                }
            };
            return service.Create(resource);
        }


        /// <summary>
        /// Обновление веб-ресурса.
        /// </summary>
        /// <param name="resourceId">Идентификатор записи веб-ресурса.</param>
        /// <param name="name">Отображаемое имя, которым нужно обновить веб-ресурс.</param>
        /// <param name="content">Содержимое файла, которым нужно обновить веб-ресурс.</param>
        public static void UpdateWebResource(this IOrganizationService service, Guid resourceId, string name, string content)
        {
            CheckParam.CheckForNotEmpty(resourceId, nameof(resourceId));
            CheckParam.CheckForNullOrWhiteSpace(name, nameof(name));
            CheckParam.CheckForNullOrWhiteSpace(content, nameof(content));

            var resource = new Entity("webresource")
            {
                Id = resourceId,
                Attributes = new AttributeCollection
                {
                  { "name", name },
                  { "content", Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(content)) }
                }
            };
            service.Update(resource);
        }


        /// <summary>
        /// Публикация одного веб-ресурса.
        /// </summary>
        /// <param name="service">Ссылка на экземпляр CRM-сервиса.</param>
        /// <param name="resourceId">Идентификатор записи веб-ресурса.</param>
        public static void PublishWebResource(this IOrganizationService service, Guid resourceId)
        {
            CheckParam.CheckForNotEmpty(resourceId, nameof(resourceId));

            service.Execute(new OrganizationRequest
            {
                RequestName = "PublishXml",
                Parameters = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ParameterXml",
                        string.Format(
                            "<importexportxml><webresources>" +
                            "<webresource>{0}</webresource>" +
                            "</webresources></importexportxml>",
                            resourceId))
                }
            });
        }

        #endregion


        #region Entity Logical Name
        /// <summary>
        /// Получение logicalName сущности по её коду в CRM.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="objectTypeCode"></param>
        /// <returns>
        /// Метод возвращает logicalName сущности по её коду в CRM.
        /// </returns>
        public static string GetEntityLogicalNameByTypeCode(this IOrganizationService service, int objectTypeCode)
        {
            var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, objectTypeCode));
            var mpe = new MetadataPropertiesExpression
            {
                AllProperties = false
            };
            mpe.PropertyNames.Add("ObjectTypeCode");

            var initialRequest = service.Execute<RetrieveMetadataChangesResponse>(new RetrieveMetadataChangesRequest
            {
                Query = new EntityQueryExpression
                {
                    Criteria = entityFilter,
                    Properties = mpe
                },
                ClientVersionStamp = null,
                DeletedMetadataFilters = DeletedMetadataFilters.OptionSet
            });

            return initialRequest.EntityMetadata.Count == 1 ? initialRequest.EntityMetadata.FirstOrDefault()?.LogicalName ?? string.Empty : string.Empty;
        }
        #endregion


        #region Plugin

        /// <summary>
        /// Обновление плагина в CRM.
        /// </summary>
        /// <param name="pluginAssemblyFileName">Полный путь к файлу со сборкой плагина.</param>
        private static void UpdatePlugin(this IOrganizationService service, string pluginAssemblyFileName)
        {
            CheckParam.CheckForNullOrWhiteSpace(pluginAssemblyFileName, nameof(pluginAssemblyFileName));

            var assemblyNameInfo = AssemblyName.GetAssemblyName(pluginAssemblyFileName);
            var assemblyName = assemblyNameInfo.Name;
            var assemblyVersion = $"{assemblyNameInfo.Version.Major}.{assemblyNameInfo.Version.Minor}.{assemblyNameInfo.Version.Build}";
            var assemblyPublicKeyToken = assemblyNameInfo.GetPublicKeyToken();

            // запрос на поиск сборки по её данным в CRM
            var query = new QueryExpression("pluginassembly")
            {
                ColumnSet = new ColumnSet("pluginassemblyid"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, assemblyName),
                        new ConditionExpression("publickeytoken", ConditionOperator.Equal, assemblyPublicKeyToken),
                        new ConditionExpression("version", ConditionOperator.BeginsWith, assemblyVersion),
                    }
                }
            };

            var assemblyEntity = service.RetrieveMultiple(query).Entities.First();
            var bytes = File.ReadAllBytes(pluginAssemblyFileName);
            var entity = new Entity(assemblyEntity.LogicalName, assemblyEntity.Id)
            {
                ["content"] = Convert.ToBase64String(bytes)
            };
            service.Update(entity);
        }


        /// <summary>
        /// Получение публичного ключа сборки плагина.
        /// </summary>
        /// <param name="assemblyNameInfo">Ссылка на класс с данными уникального идентификатора сборки.</param>
        /// <returns>
        /// Метод возвращает публичный ключ сборки плагина.
        /// </returns>
        private static string GetPublicKeyToken(this AssemblyName assemblyNameInfo)
        {
            var publicKeyTokenString = string.Empty;
            var token = assemblyNameInfo.GetPublicKeyToken();
            for (var i = 0; i < token.GetLength(0); i++)
                publicKeyTokenString += token[i].ToString("x2");
            return publicKeyTokenString;
        }


        #endregion


        #region Associate

        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter.
        /// </summary>        
        public static void Associate(this IOrganizationService service, EntityReference primaryEntity, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            CheckParam.CheckForNull(primaryEntity, nameof(primaryEntity));

            /// Associate by id if possible as is supposed to be faster 
            if (primaryEntity.Id != Guid.Empty)
            {
                service.Associate(primaryEntity.LogicalName, primaryEntity.Id, relationship, relatedEntities);
            }
            /// Use alternative key
            else
            {
                service.Execute(new AssociateRequest()
                {
                    Target = primaryEntity,
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                });
            }
        }


        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter and list of EntityReferences as related entities parameter.
        /// </summary>
        public static void Associate(this IOrganizationService service, EntityReference primaryEntity, Relationship relationship, IList<EntityReference> relatedEntities)
        {
            service.Associate(primaryEntity, relationship, new EntityReferenceCollection(relatedEntities));
        }


        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter.
        /// </summary>        
        public static void Associate(this IOrganizationService service, EntityReference primaryEntity, string relationshipName, EntityReferenceCollection relatedEntities)
        {
            service.Associate(primaryEntity.LogicalName, primaryEntity.Id, new Relationship(relationshipName), relatedEntities);
        }


        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter and list of EntityReferences as related entities parameter.
        /// </summary>
        public static void Associate(this IOrganizationService service, EntityReference primaryEntity, string relationshipName, IList<EntityReference> relatedEntities)
        {
            service.Associate(primaryEntity, relationshipName, new EntityReferenceCollection(relatedEntities));
        }

        #endregion


        #region Disassociate

        /// <summary>
        /// Disassociate method override. Takes EntityReference as primary entity input parameter.
        /// </summary>
        public static void Disassociate(this IOrganizationService service, EntityReference primaryEntity, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            CheckParam.CheckForNull(primaryEntity, nameof(primaryEntity));

            /// Disassociate by id if possible as is supposed to be faster 
            if (primaryEntity.Id != Guid.Empty)
            {
                service.Disassociate(primaryEntity.LogicalName, primaryEntity.Id, relationship, relatedEntities);
            }
            /// Use alternative key
            else
            {
                service.Execute(new DisassociateRequest()
                {
                    Target = primaryEntity,
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                });
            }
        }


        /// <summary>
        /// Disassociate method override. Takes EntityReference as primary entity input parameter and list of EntityReferences as related entities parameter.
        /// </summary>
        public static void Disassociate(this IOrganizationService service, EntityReference primaryEntity, Relationship relationship, IList<EntityReference> relatedEntities)
        {

            service.Disassociate(primaryEntity, relationship, new EntityReferenceCollection(relatedEntities));
        }


        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter.
        /// </summary>        
        public static void Disassociate(this IOrganizationService service, EntityReference primaryEntity, String relationshipName, EntityReferenceCollection relatedEntities)
        {
            service.Disassociate(primaryEntity.LogicalName, primaryEntity.Id, new Relationship(relationshipName), relatedEntities);
        }


        /// <summary>
        /// Associate method override. Takes EntityReference as primary entity input parameter and list of EntityReferences as related entities parameter.
        /// </summary>
        public static void Disassociate(this IOrganizationService service, EntityReference primaryEntity, String relationshipName, IList<EntityReference> relatedEntities)
        {
            service.Disassociate(primaryEntity, relationshipName, new EntityReferenceCollection(relatedEntities));
        }

        #endregion


        #region Delete

        /// <summary>
        /// Delete method override. Takes EntityReference as input parameter.
        /// </summary>
        /// <param name="reference">Entity to delete.</param>
        public static void Delete(this IOrganizationService service, EntityReference reference)
        {
            CheckParam.CheckForNull(reference, nameof(reference));

            /// Delete by id if possible as is supposed to be faster 
            if (reference.Id != Guid.Empty)
            {
                service.Delete(reference.LogicalName, reference.Id);
            }
            /// Use alternative key
            else
            {
                service.Delete(reference.LogicalName, reference.KeyAttributes);
            }
        }


        /// <summary>
        /// Delete method override. Takes Entity as input parameter.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        public static void Delete(this IOrganizationService service, Entity entity)
        {
            CheckParam.CheckForNull(entity, nameof(entity));

            service.Delete(entity.ToEntityReference(true));
        }


        /// <summary>
        /// Delete method override. Deletes by Alternative key.
        /// </summary>
        /// <param name="reference">Entity to delete.</param>
        public static void Delete(this IOrganizationService service, string logicalName, KeyAttributeCollection keyAttributeCollection)
        {
            CheckParam.CheckForNullOrWhiteSpace(logicalName, nameof(logicalName));
            CheckParam.CheckForNull(keyAttributeCollection, nameof(keyAttributeCollection));

            service.Execute(new DeleteRequest()
            {
                Target = new EntityReference(logicalName, keyAttributeCollection)
            });
        }


        /// <summary>
        /// Delete method override. Deletes by Alternative key.
        /// </summary>
        /// <param name="reference">Entity to delete.</param>
        public static void Delete(this IOrganizationService service, string logicalName, string keyName, object keyValue)
        {
            CheckParam.CheckForNullOrWhiteSpace(keyName, nameof(keyName));
            CheckParam.CheckForNull(keyValue, nameof(keyValue));

            KeyAttributeCollection keys = new KeyAttributeCollection();
            keys.Add(keyName, keyValue);

            service.Delete(logicalName, keys);
        }

        #endregion


        #region Assembly and Plug-in

        #region Get

        /// <summary>
        /// Get plugin assembly id by name.
        /// </summary>
        /// <param name="assembly">Plugin assembly.</param>
        /// <returns>
        /// Returns plugin assebly id.
        /// </returns>
        public static Guid GetAssemblyId(this IOrganizationService service, Assembly assembly)
        {
            var query = new QueryExpression("pluginassembly");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, assembly.GetName().Name);
            return service.RetrieveMultiple(query).Entities.Select(e => e.Id).SingleOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyId">Plugin assembly id.</param>
        /// <returns>
        /// 
        /// </returns>
        public static IEnumerable<Guid> GetPluginIds(this IOrganizationService service, Guid assemblyId)
        {
            var query = new QueryExpression("plugintype");
            query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, assemblyId.ToString());
            var entities = service.RetrieveMultiple(query);
            return entities.Entities.Select(e => e.Id).ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <returns>
        /// 
        /// </returns>
        public static IEnumerable<Guid> GetPluginStepIds(this IOrganizationService service, Guid pluginId)
        {
            var query = new QueryExpression("sdkmessageprocessingstep");
            query.Criteria.AddCondition("plugintypeid", ConditionOperator.Equal, pluginId.ToString());
            var entities = service.RetrieveMultiple(query);
            return entities.Entities.Select(e => e.Id).ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepIds"></param>
        /// <returns></returns>
        public static IEnumerable<Guid> GetPluginSecureConfigurationIds(this IOrganizationService service, IEnumerable<Guid> stepIds)
        {
            QueryExpression query = new QueryExpression("sdkmessageprocessingstep");
            query.ColumnSet.AddColumn("sdkmessageprocessingstepsecureconfigid");
            query.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.In, stepIds.Select(id => id.ToString()));
            EntityCollection entities = service.RetrieveMultiple(query);
            return entities.Entities
                .Select(e => e.GetAttributeValue<Guid?>("sdkmessageprocessingstepsecureconfigid"))
                .Where(id => id != null)
                .Select(id => id.Value)
                .Distinct();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        /// <returns>
        /// 
        /// </returns>
        public static IEnumerable<Type> GetPluginTypes(Assembly assembly)
        {
            var query = from type in assembly.GetTypes()
                        where !type.IsInterface
                        where !type.IsAbstract
                        where typeof(PluginBase).IsAssignableFrom(type)
                        select type;
            return query.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="pluginType"></param>
        /// <returns>
        /// 
        /// </returns>
        public static IEnumerable<IPluginEvent> GetPluginSteps(Assembly assembly, Type pluginType)
        {
            var pluginStepType = assembly.GetType(typeof(IPluginEvent).FullName, throwOnError: true, ignoreCase: false);
            var steps = pluginType.GetCustomAttributes(true)
                .Where(a => pluginStepType.IsAssignableFrom(a.GetType()))
                .Select(a => (IPluginEvent)a)
                .OrderBy(a => a.PrimaryEntity)
                .ThenBy(a => a.Message)
                .ToArray();
            return steps;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="step"></param>
        /// <returns>
        /// 
        /// </returns>
        public static string GetPluginStepName(Type pluginType, IPluginEvent step)
        {
            string pluginName = pluginType.FullName;
            string entityName = step.PrimaryEntity ?? "any entity";
            const string format = "{0}:{1} of {2}";
            string stepName = string.Format(format, pluginName, (int)step.Message, step.PrimaryEntity);
            return stepName;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static Guid? GetPluginUserId(this IOrganizationService service, string domain, string userName)
        {
            var domainUserName = $@"{domain}\{userName}";
            var query = new QueryExpression("systemuser");
            query.Criteria.AddCondition("domainname", ConditionOperator.Equal, domainUserName);
            var entities = service.RetrieveMultiple(query);
            return entities.Entities.Select(e => e.Id).SingleOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="step"></param>
        /// <returns>
        /// 
        /// </returns>
        public static Guid? GetPluginMessageId(this IOrganizationService service, IPluginEvent step)
        {
            var query = new QueryExpression("sdkmessage");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, (int)step.Message);
            var entities = service.RetrieveMultiple(query);
            return entities.Entities.Select(e => e.Id).SingleOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="step"></param>
        /// <returns>
        /// 
        /// </returns>
        public static Guid? GetPluginMessageFilterId(this IOrganizationService service, Guid? messageId, IPluginEvent step)
        {
            var metadata = GetEntityMetadata(service, step.PrimaryEntity);
            if (metadata == null)
            {
                return null;
            }
            var query = new QueryExpression("sdkmessagefilter");
            query.Criteria.AddCondition("sdkmessageidname", ConditionOperator.Equal, step.Message);
            query.Criteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, metadata.ObjectTypeCode);
            var entities = service.RetrieveMultiple(query);
            return entities.Entities.Select(e => e.Id).SingleOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns>
        /// 
        /// </returns>
        public static EntityMetadata GetEntityMetadata(this IOrganizationService service, string logicalName)
        {
            if (logicalName == null)
            {
                return null;
            }
            var metaRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = logicalName
            };
            var response = (RetrieveEntityResponse)service.Execute(metaRequest);
            var metadata = response.EntityMetadata;
            return metadata;
        }

        #endregion


        #region Delete

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configId"></param>
        public static void DeletePluginSecureConfigurationById(this IOrganizationService service, Guid configId)
        {
            service.Delete("sdkmessageprocessingstepsecureconfig", configId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepId"></param>
        public static void DeletePluginStepById(this IOrganizationService service, Guid stepId)
        {
            service.Delete("sdkmessageprocessingstep", stepId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginId"></param>
        public static void DeletePluginById(this IOrganizationService service, Guid pluginId)
        {
            service.Delete("plugintype", pluginId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyId"></param>
        public static void DeletePluginAssemblyById(this IOrganizationService service, Guid assemblyId)
        {
            service.Delete("pluginassembly", assemblyId);
        }

        #endregion


        #region Other

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        /// <param name="sourceType">Plugin source type.</param>
        /// <param name="isolationMode">Plugin isolation mode.</param>
        /// <param name="description">Plugin description.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Guid RegisterAssembly(this IOrganizationService service, 
            Assembly assembly, 
            SourceType sourceType = SourceType.Database,
            IsolationMode isolationMode = IsolationMode.None,
            string description = null
            )
        {
            var assemblyName = assembly.GetName();
            var entity = new Entity("pluginassembly");
            entity.Attributes.Add("name", assemblyName.Name);
            entity.Attributes.Add("sourcetype", new OptionSetValue((int)sourceType));
            entity.Attributes.Add("isolationmode", new OptionSetValue((int)isolationMode));
            entity.Attributes.Add("culture", assemblyName.CultureInfo.Name);
            entity.Attributes.Add("publickeytoken", string.Join(string.Empty, assemblyName.GetPublicKeyToken().Select(b => b.ToString("X2", CultureInfo.InvariantCulture))));
            entity.Attributes.Add("version", assemblyName.Version.ToString());
            entity.Attributes.Add("description", description);
            entity.Attributes.Add("content", Convert.ToBase64String(File.ReadAllBytes(assembly.Location)));
            return service.Create(entity);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyId">Assembly id.</param>
        /// <param name="pluginType">Plugin type.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Guid RegisterPlugin(this IOrganizationService service, Guid assemblyId, Type pluginType)
        {
            var entity = new Entity("plugintype");
            entity.Attributes.Add("pluginassemblyid", new EntityReference
            {
                LogicalName = "pluginassembly",
                Id = assemblyId
            });
            entity.Attributes.Add("typename", pluginType.FullName);
            entity.Attributes.Add("name", pluginType.FullName);
            entity.Attributes.Add("friendlyname", pluginType.Name);
            return service.Create(entity);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <param name="pluginType">Plugin type.</param>
        /// <param name="step"></param>
        /// <param name="executionMode">Execution mode.</param>
        /// <param name="executionOrder">Execution order.</param>
        /// <param name="invocationSource"></param>
        /// <param name="stage">Event pipeline stage of execution.</param>
        /// <param name="deployment">Deployment.</param>
        /// <param name="asyncAutoDelete">Delete AsyncOperation if StatusCode = Successful.</param>
        /// <returns>
        /// 
        /// </returns>
        public static Guid RegisterPluginStep(this IOrganizationService service, 
            Guid pluginId, 
            Type pluginType,
            IPluginEvent step,
            string domain,
            string userName,
            ExecutionMode executionMode = ExecutionMode.Synchronous, 
            uint executionOrder = 1,
            InvocationSource invocationSource = InvocationSource.Parent,
            Stage stage = Stage.PostOperation,
            Deployment deployment = Deployment.ServerOnly,
            bool asyncAutoDelete = false)
        {
            var entity = new Entity("sdkmessageprocessingstep");
            entity.Attributes.Add("configuration", "my unsecure config");
            entity.Attributes.Add("eventhandler", new EntityReference("plugintype", pluginId));
            entity.Attributes.Add("name", GetPluginStepName(pluginType, step));
            entity.Attributes.Add("mode", new OptionSetValue((int)executionMode));
            entity.Attributes.Add("rank", executionOrder);
            entity.Attributes.Add("invocationsource", new OptionSetValue((int)invocationSource));
            var messageId = GetPluginMessageId(service, step);
            if (messageId != null)
            {
                entity.Attributes.Add("sdkmessageid", new EntityReference("sdkmessage", messageId.Value));
                Guid? filterId = GetPluginMessageFilterId(service, messageId, step);
                if (filterId != null)
                {
                    entity.Attributes.Add("sdkmessagefilterid", new EntityReference("sdkmessagefilter", filterId.Value));
                }
            }
            var impersonatingUserId = GetPluginUserId(service, domain, userName);
            var impersonator = impersonatingUserId == null ? null : new EntityReference("systemuser", impersonatingUserId.Value);
            entity.Attributes.Add("impersonatinguserid", impersonator);
            entity.Attributes.Add("stage", new OptionSetValue((int)stage));
            entity.Attributes.Add("supporteddeployment", new OptionSetValue((int)deployment));
            entity.Attributes.Add("filteringattributes", string.Empty);
            entity.Attributes.Add("asyncautodelete", asyncAutoDelete);
            return service.Create(entity);
        }

        #endregion

        #endregion
    }
}
