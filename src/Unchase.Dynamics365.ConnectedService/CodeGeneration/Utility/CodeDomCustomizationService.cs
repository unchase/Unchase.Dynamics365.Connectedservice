using System;
using System.CodeDom;
using System.Reflection;
using System.Threading.Tasks;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CodeDomCustomizationService : ICustomizeCodeDomService
    {
        async Task ICustomizeCodeDomService.CustomizeCodeDomAsync(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStartAsync("Entering {0}", MethodBase.GetCurrentMethod().Name);
            await CrmSvcUtil.CrmSvcUtilLogger.TraceMethodStopAsync("Exiting {0}", MethodBase.GetCurrentMethod().Name);
        }
    }
}
