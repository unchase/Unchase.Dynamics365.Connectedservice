using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>    
    /// Стадии выполнения подключаемого модуля.
    /// </summary>
    /// <seealso cref="StageExtensions"/>
    public enum Stage
    {
        /// <summary>
        /// Стадия Pre-Validation.
        /// </summary>
        /// <value>10</value>
        [Description("Pre-Validation")]
        PreValidation = 10,


        /// <summary>
        /// Стадия Pre-Operation.
        /// </summary>
        /// <value>20</value>
        [Description("Pre-Operation")]
        PreOperation = 20,


        /// <summary>
        /// Стадия Post-Operation.
        /// </summary>
        /// <value>40</value>
        [Description("Post-Operation")]
        PostOperation = 40
    }
}
