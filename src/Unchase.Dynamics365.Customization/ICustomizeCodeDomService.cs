using System;
using System.CodeDom;
using System.Threading.Tasks;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface that can be used to customize the CodeDom before it generates code.
	/// </summary>
    public interface ICustomizeCodeDomService
	{
		/// <summary>
		/// Customize the generated types before code is generated
		/// </summary>
        Task CustomizeCodeDomAsync(CodeCompileUnit codeUnit, IServiceProvider services);
	}
}
