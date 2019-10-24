using System.Threading.Tasks;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal interface ICommandLineArgumentSource
    {
        Task OnUnknownArgumentAsync(string argumentName, string argumentValue);

		Task OnInvalidArgumentAsync(string argument);
	}
}
