using System;
using System.CodeDom;
using Unchase.Dynamics365.Customization;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal sealed class CodeDomCustomizationService : ICustomizeCodeDomService
	{
        void ICustomizeCodeDomService.CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services) { }
	}
}
