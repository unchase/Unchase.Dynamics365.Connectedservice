using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.Models;

namespace Unchase.Dynamics365.ConnectedService
{
    internal class Instance : ConnectedServiceInstance
    {
        public ServiceConfiguration ServiceConfig { get; set; }

        public Instance()
        {
            InstanceId = Constants.ExtensionCategory;
            Name = Constants.DefaultServiceName;
        }
    }
}
