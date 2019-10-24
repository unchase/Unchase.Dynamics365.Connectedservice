using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ConnectedServices;
using NuGet.VisualStudio;
using Unchase.Dynamics365.ConnectedService.Common;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration
{
    internal abstract class BaseCodeGenDescriptor
    {
        #region Properties
        public IVsPackageInstaller PackageInstaller { get; private set; }

        public IVsPackageInstallerServices PackageInstallerServices { get; private set; }

        public ConnectedServiceHandlerContext Context { get; }

        public Project Project { get; }

        public string ServiceUri { get; }

        public Instance Instance { get; }
        #endregion

        #region Constructors
        protected BaseCodeGenDescriptor(ConnectedServiceHandlerContext context, Instance serviceInstance)
        {
            this.InitNuGetInstaller();

            this.Instance = serviceInstance;
            this.ServiceUri = serviceInstance.ServiceConfig.Endpoint;
            this.Context = context;
            this.Project = context.ProjectHierarchy.GetProject();
        }
        private void InitNuGetInstaller()
        {
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            this.PackageInstallerServices = componentModel.GetService<IVsPackageInstallerServices>();
            this.PackageInstaller = componentModel.GetService<IVsPackageInstaller>();
        }
        #endregion

        #region Methods
        public abstract Task AddNugetPackagesAsync();

        public abstract Task AddGeneratedCodeAsync();

        protected string GetReferenceFileFolder()
        {
            var serviceReferenceFolderName = this.Context.HandlerHelper.GetServiceArtifactsRootFolder();
            return Path.Combine(this.Project.GetServiceFolderPath(serviceReferenceFolderName, this.Context.ServiceInstance.Name));
        }
        #endregion
    }
}
