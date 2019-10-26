using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Unchase.Dynamics365.Customization
{
	/// <summary>
	/// Interface for code writer message filter service
	/// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICodeWriterMessageFilterService
	{
		/// <summary>
		/// Returns true to generate code for the SDK Message and false otherwise.
		/// </summary>
        Task<bool> GenerateSdkMessageAsync(SdkMessage sdkMessage, IServiceProvider services);

		/// <summary>
		/// Returns true to generate code for the SDK Message Pair and false otherwise.
		/// </summary>
        Task<bool> GenerateSdkMessagePairAsync(SdkMessagePair sdkMessagePair, IServiceProvider services);
	}
}
