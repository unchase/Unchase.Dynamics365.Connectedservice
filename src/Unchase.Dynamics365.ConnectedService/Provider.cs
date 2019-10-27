using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.Properties;

namespace Unchase.Dynamics365.ConnectedService
{
    [ConnectedServiceProviderExport(Constants.ProviderId, SupportsUpdate = true)]
    internal class Provider : ConnectedServiceProvider
    {
        public Provider()
        {
            Category = Constants.ExtensionCategory;
            Name = Constants.ExtensionName;
            Description = Constants.ExtensionDescription;
            Icon = Imaging.CreateBitmapSourceFromHBitmap(
                Resources.icon.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(64, 64)
            );
            CreatedBy = Constants.Author;
            Version = new Version(1, 1, 0, 0);
            Version = typeof(Provider).Assembly.GetName().Version;
            MoreInfoUri = new Uri(Constants.Website);
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            return Task.FromResult<ConnectedServiceConfigurator>(new Wizard(context));
        }

        public override IEnumerable<Tuple<string, Uri>> GetSupportedTechnologyLinks()
        {
            yield return Tuple.Create("Developer Guide for Dynamics 365 Customer Engagement (on-premises), version 9.x", new Uri("https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/overview"));
            yield return Tuple.Create("Use the Common Data Service Organization Service", new Uri("https://docs.microsoft.com/en-US/powerapps/developer/common-data-service/org-service/overview"));
        }
    }
}
