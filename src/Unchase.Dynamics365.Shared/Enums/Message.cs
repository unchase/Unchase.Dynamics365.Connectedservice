namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>
    /// Событие подключаемого модуля.
    /// </summary>
    public enum Message
    {
        /// <summary>
        /// All other messages.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Все остальные события кроме определнных явно в этом перечислении.</para>
        /// </remarks>
        Custom,


        /// <summary>
        /// Creates a record of a specific entity type, including custom entities.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Создание записи.</para>
        /// <para>
        /// To perform this action, the caller must have Create message privileges. 
        /// Access rights do not apply to the Create action, but they do apply to 
        /// the record after it has been created.
        /// </para>
        /// <para>
        /// To own a record or to retrieve the newly created record, a user or team 
        /// must also have read privileges and access rights on the new record.For 
        /// example, if you have create privileges for accounts, you can create an 
        /// account record and assign it to another user or team.However, unless you 
        /// also have read privilege for accounts, and access rights on the new 
        /// record, you cannot create an account and be the owner of that new account.
        /// </para>
        /// </remarks>
        Create,


        /// <summary>
        /// Modifies the contents of a record.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Обновление записи.</para>
        /// <para>
        /// To perform this action, the caller must have Update message privileges 
        /// and access rights on the entity records that are being updated.
        /// </para>
        /// <para>
        /// When you update a record, only the attributes for which you specify data 
        /// or for which you specify null are updated. All other values remain the 
        /// same. Also, if you specify data for attributes that are not valid for 
        /// update, they are ignored.
        /// </para>
        /// </remarks>
        Update,


        /// <summary>
        /// Deletes a record.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Удаление записи.</para>
        /// <para>
        /// To perform this action, the caller must have Delete message privileges 
        /// and access rights on the entity records being deleted.
        /// </para>
        /// <para>
        /// The cascading rules determine whether related records are deleted at 
        /// the same time.
        /// </para>
        /// <para>
        /// Typically, you should only delete records that you entered by mistake. 
        /// For some record types, you can deactivate or close the record instead 
        /// of deleting it. Not all record types can be deleted.
        /// </para>
        /// </remarks>
        Delete,


        /// <summary>
        /// Retrieves a record.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Получение одной записи.</para>
        /// <para>
        /// To perform this action, the caller must have Retrieve message privileges 
        /// and access rights on the entity records retrieved.
        /// </para>
        /// </remarks>
        Retrieve,


        /// <summary>
        /// Retrieves a collection of records.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Получение набора записей.</para>
        /// <para>
        /// To perform this action, the caller must have Retrieve message privileges 
        /// and access rights on the entity records retrieved.
        /// </para>
        /// <para>
        /// To retrieve a collection of records based on the query parameters, you 
        /// can either use a query expression or FetchXML query language. Query 
        /// expressions let you build a query tree by using a class hierarchy. 
        /// The methods that use a query expression return a collection of strongly 
        /// typed records. FetchXML lets you build a query by using an XML language. 
        /// FetchXML returns an XML string. Therefore, you must do more data 
        /// manipulation to use the query results.
        /// </para>
        /// </remarks>
        RetrieveMultiple,


        /// <summary>
        /// Set the state of a record.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Установка статуса записи.</para>
        /// <para>
        /// To perform this action, the caller must have SetState message privileges
        /// and access rights on the records that are being updated. The 
        /// SetStateRequest message updates the StateCode and StatusCode attributes 
        /// of the entity record.
        /// </para>
        /// </remarks>
        SetState,


        /// <summary>
        /// Creates links between a record and a collection of records where 
        /// there is a relationship between the entities.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Событие ассоциирования записей - установки связи М:М.</para>
        /// <para>
        /// To perform this action, the caller must have Associate message 
        /// privileges and access rights on the records that are being updated.
        /// </para>
        /// <para>
        /// Associate creates multiple associations in one transaction between the 
        /// specified record and each record in the specified collection for the 
        /// relationship specified.
        /// </para>
        /// <para>
        /// For a one-to-many relationship, this method sets the ReferencingAttribute 
        /// in the related record to the ID of the specified record.
        /// </para>
        /// <para>
        /// For a many-to-many relationship, this method creates a record in the 
        /// intersect table for the relationship, which contains the ID of both the 
        /// referenced and referencing records. The intersect table name is defined 
        /// in the IntersectEntityName property for the relationship.
        /// </para>
        /// </remarks>
        Associate,


        /// <summary>
        /// Removes links between a record and a collection of records where 
        /// there is a relationship between the entities.
        /// </summary>
        /// <remarks>
        /// <para>RUS: Событие деассоциирования записей - удаление связи М:М.</para>
        /// <para>
        /// To perform this action, the caller must have Disassociate message 
        /// privileges and access rights on the records that are being updated.
        /// </para>
        /// <para>
        /// Disassociate reverses the associate operation, by updating the 
        /// referenced and referencing records and deleting the intersect record 
        /// where appropriate.
        /// </para>
        /// </remarks>
        Disassociate,


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>RUS: Выигрыш возможной сделки.</para>
        /// </remarks>
        Win,


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>RUS: Потеря возможной сделки.</para>
        /// </remarks>
        Lose,


        /// <summary>
        /// Grants, modifies or revokes access to a record to another user or 
        /// team. Valid for user-owned or team-owned entities.
        /// </summary>
        /// <remarks>
        /// <para>RUS: </para>
        /// <para>
        /// For user-owned or team-owned entities, you can share a record with 
        /// other users or teams. To perform this action, the caller must have 
        /// GrantAccess message privileges, ModifyAccess message privileges, 
        /// and RevokeAccess message privileges and access rights on the entity 
        /// records.
        /// </para>
        /// <para>
        /// The cascading rules determine whether related records are shared at 
        /// the same time.
        /// </para>
        /// <para>
        /// Sharing is the way Microsoft Dynamics CRM users can give other users 
        /// access to customer information as needed. For example, a salesperson 
        /// might decide to share an opportunity with another salesperson so that 
        /// they can both track the progress of an important sale.
        /// </para>
        /// <para>
        /// Use the GrantAccessRequest to share a record. Use the ModifyAccessRequest 
        /// to change how a record is shared. Use the RevokeAccessRequest to remove 
        /// all sharing for the record.
        /// </para>
        /// <para>
        /// A user must have share rights to share customer-related records, such 
        /// as contacts, accounts, opportunities, cases, and orders, with any other 
        /// user in Microsoft Dynamics CRM. When a record is shared, you can specify 
        /// the rights to grant for the shared record.
        /// </para>
        /// </remarks>
        Share,

        AddItem,

        AddListMembers,

        AddMember,

        AddMembers,

        AddPrincipalToQueue,

        AddPrivileges,

        AddProductToKit,

        AddRecurrence,

        AddToQueue,

        AddUserToRecordTeam,

        ApplyRecordCreationAndUpdateRule,


        /// <summary>
        /// Changes ownership of a record. Valid for user-owned or team-owned 
        /// entities.
        /// </summary>
        /// <remarks>
        /// <para>RUS: </para>
        /// <para>
        /// For user-owned or team-owned entities, you assign a record to a new owner.
        /// </para>
        /// <para>
        /// To perform this action, the caller must have Assign message privileges 
        /// and access rights on the entity records.
        /// </para>
        /// <para>
        /// The cascading rules determine whether related records are assigned to 
        /// another user at the same time.
        /// </para>
        /// <para>
        /// When a record is assigned to another user or team, the previous owner 
        /// still has access to this record if the ShareToPreviousOwnerOnAssign 
        /// attribute is set to true. However, the previous owner will no longer 
        /// have ownership of the record.
        /// </para>
        /// </remarks>
        Assign,

        BackgroundSend,

        Book,

        CalculatePrice,

        Cancel,

        CheckIncoming,

        CheckPromote,

        Clone,

        CloneMobileOfflineProfile,

        CloneProduct,

        Close,

        CopyDynamicListToStatic,

        CopySystemForm,

        CreateException,

        CreateInstance,

        CreateKnowledgeArticleTranslation,

        CreateKnowledgeArticleVersion,

        DeleteOpenInstances,

        DeliverIncoming,

        DeliverPromote,

        Execute,

        ExecuteById,

        Export,

        GenerateSocialProfile,

        GetDefaultPriceLevel,

        GrantAccess,

        Import,

        LockInvoicePricing,

        LockSalesOrderPricing,

        Merge,

        ModifyAccess,

        PickFromQueue,

        Publish,

        PublishAll,

        PublishTheme,

        QualifyLead,

        Recalculate,

        ReleaseToQueue,

        RemoveFromQueue,

        RemoveItem,

        RemoveMember,

        RemoveMembers,

        RemovePrivilege,

        RemoveProductFromKit,

        RemoveRelated,

        RemoveUserFromRecordTeam,

        ReplacePrivileges,

        Reschedule,

        RetrieveExchangeRate,

        RetrieveFilteredForms,

        RetrievePersonalWall,

        RetrievePrincipalAccess,

        RetrieveRecordWall,

        RetrieveSharedPrincipalsAndAccess,

        RetrieveUnpublished,

        RetrieveUnpublishedMultiple,

        RetrieveUserQueues,

        RevokeAccess,

        RouteTo,

        Send,

        SendFromTemplate,

        SetLocLabels,

        SetRelated,

        TriggerServiceEndpointCheck,

        UnlockInvoicePricing,

        UnlockSalesOrderPricing,

        ValidateRecurrenceRule
    }
}
