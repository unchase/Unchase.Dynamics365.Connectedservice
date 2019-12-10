using Unchase.Dynamics365.Shared.Enums;

namespace Unchase.Dynamics365.Shared.Models
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPluginEvent
    {
        /// <summary>
        /// Plugin step stage.
        /// </summary>
        Stage Stage { get; }


        /// <summary>
        /// Plugin step message name.
        /// </summary>
        Message Message { get; }


        /// <summary>
        /// Plugin step custom message name.
        /// </summary>
        string CustomMessage { get; }


        /// <summary>
        /// Plugin step primary entity name.
        /// </summary>
        string PrimaryEntity { get; }
    }
}
