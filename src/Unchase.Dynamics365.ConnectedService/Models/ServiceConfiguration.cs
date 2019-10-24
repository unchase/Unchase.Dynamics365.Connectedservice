namespace Unchase.Dynamics365.ConnectedService.Models
{
    internal class ServiceConfiguration
    {
        #region Properties
        public string ServiceName { get; set; }

        public string Endpoint { get; set; }

        public string GeneratedFileNamePrefix { get; set; }

        public bool EnableDebugMode { get; set; }

        public bool AddClientNuGet { get; set; }

        public string LanguageOption { get; set; }

        public bool UseInteractiveLogin { get; set; }

        public string Namespace { get; set; }

        public string ConnectionString { get; set; }

        public bool UseOAuth { get; set; }

        public bool UseConnectionString { get; set; }

        public string ServiceContextName { get; set; }

        public bool GenerateMessages { get; set; }

        public string MessageNamespace { get; set; }

        public bool GenerateCustomActions { get; set; }

        public bool OpenGeneratedFilesOnComplete { get; set; }
        #endregion

        #region Network Credentials
        public bool UseNetworkCredentials { get; set; }
        public string NetworkCredentialsUserName { get; set; }
        public string NetworkCredentialsPassword { get; set; }
        public string NetworkCredentialsDomain { get; set; }
        #endregion
    }
}
