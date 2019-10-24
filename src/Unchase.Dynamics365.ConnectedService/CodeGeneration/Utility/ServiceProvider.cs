using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class ServiceProvider : IServiceProvider
	{
        internal ServiceProvider()
		{
			this._services = new Dictionary<Type, object>();
		}

		internal ITypeMappingService TypeMappingService => (ITypeMappingService)this._services[typeof(ITypeMappingService)];

        internal IMetadataProviderService MetadataProviderService => (IMetadataProviderService)this._services[typeof(IMetadataProviderService)];

        internal IMetadataProviderQueryService MetadataProviderQueryService => (IMetadataProviderQueryService)this._services[typeof(IMetadataProviderQueryService)];

        internal ICustomizeCodeDomService CodeCustomizationService => (ICustomizeCodeDomService)this._services[typeof(ICustomizeCodeDomService)];

        internal ICodeGenerationService CodeGenerationService => (ICodeGenerationService)this._services[typeof(ICodeGenerationService)];

        internal ICodeWriterFilterService CodeFilterService => (ICodeWriterFilterService)this._services[typeof(ICodeWriterFilterService)];

        internal ICodeWriterMessageFilterService CodeMessageFilterService => (ICodeWriterMessageFilterService)this._services[typeof(ICodeWriterMessageFilterService)];

        internal INamingService NamingService => (INamingService)this._services[typeof(INamingService)];

        object IServiceProvider.GetService(Type serviceType)
		{
			if (this._services.ContainsKey(serviceType))
			{
				return this._services[serviceType];
			}
			return null;
		}

		internal async Task InitializeServicesAsync(CrmSvcUtilParameters parameters)
		{
			var defaultServiceInstance = new CodeWriterFilterService(parameters);
			this._services.Add(typeof(ICodeWriterFilterService), await ServiceFactory.CreateInstanceAsync<ICodeWriterFilterService>(defaultServiceInstance, parameters.CodeWriterFilterService, parameters));
			this._services.Add(typeof(ICodeWriterMessageFilterService), await ServiceFactory.CreateInstanceAsync<ICodeWriterMessageFilterService>(defaultServiceInstance, parameters.CodeWriterMessageFilterService, parameters));
			this._services.Add(typeof(IMetadataProviderService), await ServiceFactory.CreateInstanceAsync<IMetadataProviderService>(new SdkMetadataProviderService(parameters), parameters.MetadataProviderService, parameters));
			this._services.Add(typeof(IMetadataProviderQueryService), await ServiceFactory.CreateInstanceAsync<IMetadataProviderQueryService>(new MetadataProviderQueryService(parameters), parameters.MetadataQueryProvider, parameters));
			this._services.Add(typeof(ICodeGenerationService), await ServiceFactory.CreateInstanceAsync<ICodeGenerationService>(new CodeGenerationService(), parameters.CodeGenerationService, parameters));
			this._services.Add(typeof(INamingService), await ServiceFactory.CreateInstanceAsync<INamingService>(new NamingService(parameters), parameters.NamingService, parameters));
			this._services.Add(typeof(ICustomizeCodeDomService), await ServiceFactory.CreateInstanceAsync<ICustomizeCodeDomService>(new CodeDomCustomizationService(), parameters.CodeCustomizationService, parameters));
			this._services.Add(typeof(ITypeMappingService), new TypeMappingService(parameters));
		}

		private readonly Dictionary<Type, object> _services;
	}
}
