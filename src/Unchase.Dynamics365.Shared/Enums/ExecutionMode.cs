using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// Синхронный.
        /// </summary>
        /// <value>0</value>
        [Description("Synchronous")]
        Synchronous = 0,

        /// <summary>
        /// Асинхронный.
        /// </summary>
        /// <value>1</value>
        [Description("Asynchronous")]
        Asynchronous = 1
    }
}
