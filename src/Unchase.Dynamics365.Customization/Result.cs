using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// SDK Message result
	/// </summary>
    [XmlType]
	[Serializable]
	public sealed class Result
	{
		/// <summary>
		/// Message name
		/// </summary>
        [XmlElement(ElementName = "name", Form = XmlSchemaForm.Unqualified)]
		public string Name
		{
			get => this._nameField;
            set => this._nameField = value;
        }

		/// <summary>
		/// Gets or sets whether the message is private
		/// </summary>
        [XmlElement(ElementName = "isprivate", Form = XmlSchemaForm.Unqualified)]
		public bool IsPrivate
		{
			get => this._isPrivateField;
            set => this._isPrivateField = value;
        }

		/// <summary>
		/// Gets or sets the customization level
		/// </summary>
        [XmlElement(ElementName = "customizationlevel", Form = XmlSchemaForm.Unqualified)]
		public byte CustomizationLevel
		{
			get => this._customizationLevel;
            set => this._customizationLevel = value;
        }

		/// <summary>
		/// Gets or sets the message id
		/// </summary>
        [XmlElement(ElementName = "sdkmessageid", Form = XmlSchemaForm.Unqualified)]
		public Guid SdkMessageId
		{
			get => this._sdkMessageIdField;
            set => this._sdkMessageIdField = value;
        }

		/// <summary>
		/// Gets or sets the message pair id
		/// </summary>
        [XmlElement("sdkmessagepair.sdkmessagepairid", Form = XmlSchemaForm.Unqualified)]
		public Guid SdkMessagePairId
		{
			get => this._sdkMessagePairIdField;
            set => this._sdkMessagePairIdField = value;
        }

		/// <summary>
		/// Gets or sets the message pair namespace
		/// </summary>
        [XmlElement("sdkmessagepair.namespace", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessagePairNamespace
		{
			get => this._sdkMessagePairNamespace;
            set => this._sdkMessagePairNamespace = value;
        }

		/// <summary>
		/// Gets or sets the message request id
		/// </summary>
        [XmlElement("sdkmessagerequest.sdkmessagerequestid", Form = XmlSchemaForm.Unqualified)]
		public Guid SdkMessageRequestId
		{
			get => this._sdkMessageRequestIdField;
            set => this._sdkMessageRequestIdField = value;
        }

		/// <summary>
		/// Gets or sets the message request name
		/// </summary>
        [XmlElement("sdkmessagerequest.name", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageRequestName
		{
			get => this._sdkMessageRequestNameField;
            set => this._sdkMessageRequestNameField = value;
        }

		/// <summary>
		/// Gets or sets the message request field name
		/// </summary>
        [XmlElement("sdkmessagerequestfield.name", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageRequestFieldName
		{
			get => this._sdkMessageRequestFieldNameField;
            set => this._sdkMessageRequestFieldNameField = value;
        }

		/// <summary>
		/// Gets or sets whether the message request field is optional
		/// </summary>
        [XmlElement("sdkmessagerequestfield.optional", Form = XmlSchemaForm.Unqualified)]
		public bool SdkMessageRequestFieldIsOptional
		{
			get => this._sdkMessageRequestFieldIsOptionalField;
            set => this._sdkMessageRequestFieldIsOptionalField = value;
        }

		/// <summary>
		/// Gets or sets the message request field parser
		/// </summary>
        [XmlElement("sdkmessagerequestfield.parser", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageRequestFieldParser
		{
			get => this._sdkMessageRequestFieldParserField;
            set => this._sdkMessageRequestFieldParserField = value;
        }

		/// <summary>
		/// Gets or sets the message request field CLR parser
		/// </summary>
        [XmlElement("sdkmessagerequestfield.clrparser", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageRequestFieldClrParser
		{
			get => this._sdkMessageRequestFieldCLRParserField;
            set => this._sdkMessageRequestFieldCLRParserField = value;
        }

		/// <summary>
		/// Gets or sets the message request response id
		/// </summary>
        [XmlElement("sdkmessageresponse.sdkmessageresponseid", Form = XmlSchemaForm.Unqualified)]
		public Guid SdkMessageResponseId
		{
			get => this._sdkMessageResponseIdField;
            set => this._sdkMessageResponseIdField = value;
        }

		/// <summary>
		/// Gets or sets the message request response field value
		/// </summary>
        [XmlElement("sdkmessageresponsefield.value", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageResponseFieldValue
		{
			get => this._sdkMessageResponseFieldValueField;
            set => this._sdkMessageResponseFieldValueField = value;
        }

		/// <summary>
		/// Gets or sets the message request response field formatter
		/// </summary>
        [XmlElement("sdkmessageresponsefield.formatter", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageResponseFieldFormatter
		{
			get => this._sdkMessageResponseFieldFormatterField;
            set => this._sdkMessageResponseFieldFormatterField = value;
        }

		/// <summary>
		/// Gets or sets the message request response field CLR formatter
		/// </summary>
        [XmlElement("sdkmessageresponsefield.clrformatter", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageResponseFieldClrFormatter
		{
			get => this._sdkMessageResponseFieldCLRFormatterField;
            set => this._sdkMessageResponseFieldCLRFormatterField = value;
        }

		/// <summary>
		/// Gets or sets the message request response field name
		/// </summary>
        [XmlElement("sdkmessageresponsefield.name", Form = XmlSchemaForm.Unqualified)]
		public string SdkMessageResponseFieldName
		{
			get => this._sdkMessageResponseFieldNameField;
            set => this._sdkMessageResponseFieldNameField = value;
        }

		/// <summary>
		/// Gets or sets the message request field position
		/// </summary>
        [XmlElement("sdkmessagerequestfield.position", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
		public int? SdkMessageRequestFieldPosition
		{
			get => this._sdkMessageRequestFieldPositionField;
            set => this._sdkMessageRequestFieldPositionField = value;
        }

		/// <summary>
		/// Gets or sets the message response field position
		/// </summary>
        [XmlElement("sdkmessageresponsefield.position", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
		public int? SdkMessageResponseFieldPosition
		{
			get => this._sdkMessageResponseFieldPositionField;
            set => this._sdkMessageResponseFieldPositionField = value;
        }

		/// <summary>
		/// Gets or sets the message filter id
		/// </summary>
        [XmlElement("sdmessagefilter.sdkmessagefilterid", Form = XmlSchemaForm.Unqualified)]
		public Guid SdkMessageFilterId
		{
			get => this._sdkMessageFilterIdField;
            set => this._sdkMessageFilterIdField = value;
        }

		/// <summary>
		/// Gets or sets the message primary OTC filter
		/// </summary>
        [XmlElement("sdmessagefilter.primaryobjecttypecode", Form = XmlSchemaForm.Unqualified)]
		public int SdkMessagePrimaryOTCFilter
		{
			get => this._sdkMessageFilterPrimaryOTCField;
            set => this._sdkMessageFilterPrimaryOTCField = value;
        }

		/// <summary>
		/// Gets or sets the message secondary OTC filter
		/// </summary>
        [XmlElement("sdmessagefilter.secondaryobjecttypecode", Form = XmlSchemaForm.Unqualified)]
		public int SdkMessageSecondaryOTCFilter
		{
			get => this._sdkMessageFilterSecondaryOTCField;
            set => this._sdkMessageFilterSecondaryOTCField = value;
        }

		private string _nameField;

		private bool _isPrivateField;

		private byte _customizationLevel;

		private Guid _sdkMessageIdField;

		private Guid _sdkMessageRequestIdField;

		private Guid _sdkMessagePairIdField;

		private string _sdkMessagePairNamespace;

		private Guid _sdkMessageFilterIdField;

		private string _sdkMessageRequestNameField;

		private string _sdkMessageRequestFieldNameField;

		private bool _sdkMessageRequestFieldIsOptionalField;

		private string _sdkMessageRequestFieldParserField;

		private string _sdkMessageRequestFieldCLRParserField;

		private Guid _sdkMessageResponseIdField;

		private string _sdkMessageResponseFieldValueField;

		private string _sdkMessageResponseFieldFormatterField;

		private string _sdkMessageResponseFieldCLRFormatterField;

		private string _sdkMessageResponseFieldNameField;

		private int? _sdkMessageRequestFieldPositionField;

		private int? _sdkMessageResponseFieldPositionField;

		private int _sdkMessageFilterPrimaryOTCField;

		private int _sdkMessageFilterSecondaryOTCField;
	}
}
