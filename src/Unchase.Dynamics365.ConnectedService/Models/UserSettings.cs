using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.Common;

namespace Unchase.Dynamics365.ConnectedService.Models
{
    [DataContract]
    internal class UserSettings
    {
        #region Private
        private const string Name = "Settings";

        private const int MaxMruEntries = 10;

        private ConnectedServiceLogger _logger;
        #endregion

        #region Public properties
        [DataMember]
        public ObservableCollection<string> MruEndpoints { get; private set; }

        [DataMember]
        public string Endpoint { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public string GeneratedFileName { get; set; }

        [DataMember]
        public bool EnableDebugMode { get; set; } = false;

        [DataMember]
        public bool AddClientNuGet { get; set; } = false;

        [DataMember]
        public bool OpenGeneratedFilesOnComplete { get; set; } = false;

        [DataMember]
        public string LanguageOption { get; set; } = "cs";

        [DataMember]
        public bool UseInteractiveLogin { get; set; } = true;

        [DataMember]
        public string Namespace { get; set; }

        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public bool UseOAuth { get; set; } = false;

        [DataMember]
        public bool UseConnectionString { get; set; } = false;

        [DataMember]
        public string ServiceContextName { get; set; }

        [DataMember]
        public bool GenerateMessages { get; set; } = false;

        [DataMember]
        public string MessageNamespace { get; set; }

        [DataMember]
        public bool GenerateCustomActions { get; set; } = false;

        [DataMember]
        public bool UseNetworkCredentials { get; set; } = false;

        #region Customization
        //[DataMember]
        public string CustomizeCodeDomService { get; set; }

        //[DataMember]
        public string CodeWriterFilterService { get; set; }

        //[DataMember]
        public string CodeWriterMessageFilterService { get; set; }

        //[DataMember]
        public string MetadataProviderService { get; set; }

        //[DataMember]
        public string MetadataProviderQueryService { get; set; }

        //[DataMember]
        public string CodeGenerationService { get; set; }

        //[DataMember]
        public string NamingService { get; set; }
        #endregion

        #endregion

        #region Constructors
        private UserSettings()
        {
            this.MruEndpoints = new ObservableCollection<string>();
        }
        #endregion

        #region Public methods
        public async Task SaveAsync()
        {
            await UserSettingsPersistenceHelper.SaveAsync(this, Constants.ProviderId, UserSettings.Name, null, this._logger);
        }

        public static async Task<UserSettings> LoadAsync(ConnectedServiceLogger logger)
        {
            var userSettings = await UserSettingsPersistenceHelper.LoadAsync<UserSettings>(
                Constants.ProviderId, UserSettings.Name, null, logger) ?? new UserSettings();
            userSettings._logger = logger;
            return userSettings;
        }

        public static void AddToTopOfMruList<T>(ObservableCollection<T> mruList, T item)
        {
            var index = mruList.IndexOf(item);
            if (index >= 0)
            {
                // Ensure there aren't any duplicates in the list.
                for (var i = mruList.Count - 1; i > index; i--)
                {
                    if (EqualityComparer<T>.Default.Equals(mruList[i], item))
                        mruList.RemoveAt(i);
                }

                if (index > 0)
                {
                    // The item is in the MRU list but it is not at the top.
                    mruList.Move(index, 0);
                }
            }
            else
            {
                // The item is not in the MRU list, make room for it by clearing out the oldest item.
                while (mruList.Count >= UserSettings.MaxMruEntries)
                    mruList.RemoveAt(mruList.Count - 1);

                mruList.Insert(0, item);
            }
        }
        #endregion
    }
}
