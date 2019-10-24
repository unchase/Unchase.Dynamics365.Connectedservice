using System.Threading.Tasks;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility.Interfaces
{
    internal interface ICommandLineArgumentSource
    {
        Task OnUnknownArgumentAsync(string argumentName, string argumentValue);

		Task OnInvalidArgumentAsync(string argument);
	}
}
