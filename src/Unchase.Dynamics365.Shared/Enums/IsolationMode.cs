using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>
    /// Уровень изоляции подключаемого модуля.
    /// </summary>
    public enum IsolationMode
    {
        /// <summary>
        /// Invalid.
        /// </summary>
        /// <value>0</value>
        [Description("Invalid")]
        Invalid = 0,

        /// <summary>
        /// None.
        /// </summary>
        /// <value>1</value>
        [Description("None")]
        None = 1,

        /// <summary>
        /// Песочница.
        /// </summary>
        /// <remarks>
        /// All code in this assembly will be run in a secure sandbox (reduced functionality).
        /// </remarks>
        /// <value>2</value>
        [Description("Sandbox")]
        Sandbox = 2
    }
}
