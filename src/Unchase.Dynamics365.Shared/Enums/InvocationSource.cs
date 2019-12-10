using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum InvocationSource
    {
        /// <summary>
        /// Parent.
        /// </summary>
        /// <value>0</value>
        [Description("Parent")]
        Parent = 0,

        /// <summary>
        /// Child.
        /// </summary>
        /// <value>0</value>
        [Description("Child")]
        Child = 1
    }
}
