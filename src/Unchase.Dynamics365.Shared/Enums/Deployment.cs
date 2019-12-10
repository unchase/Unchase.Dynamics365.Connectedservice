using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum Deployment
    {
        /// <summary>
        /// Server Only.
        /// </summary>
        /// <value>0</value>
        [Description("Server Only")]
        ServerOnly = 0,

        /// <summary>
        /// Offline Only.
        /// </summary>
        /// <value>1</value>
        [Description("Offline Only")]
        OfflineOnly = 1,

        /// <summary>
        /// Both.
        /// </summary>
        /// <value>2</value>
        [Description("Both")]
        Both = 2
    }
}
