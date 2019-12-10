using System.ComponentModel;

namespace Unchase.Dynamics365.Shared.Enums
{
    /// <summary>    
    /// Место хранения подключаемого модуля.
    /// </summary>
    /// <remarks>
    /// Location where the assembly should be stored.
    /// </remarks>
    public enum SourceType
    {
        /// <summary>
        /// База данных.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assembly is stored and loaded from the database.
        /// </para>
        /// <para>
        /// For debbuging purposes, the Symbols (.PDB files) must be in \Server\bin\assembly 
        /// of the main installation folder for each server that needs to be debugged.
        /// </para>
        /// </remarks>
        /// <value>0</value>
        [Description("Database")]
        Database = 0,


        /// <summary>
        /// Диск.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assembly is stored and loaded from \Server\bin\assembly in the main installation directory for each server.
        /// </para>
        /// <para>
        /// For debbuging purposes, the Symbols (.PDB files) must be located in the same place as the assembly.
        /// </para>
        /// </remarks>
        /// <value>1</value>
        [Description("Disk")]
        Disk = 1,


        /// <summary>
        /// GAC.
        /// </summary>
        /// <remarks>
        /// File is placed in the GAC of each server where it will used.
        /// </remarks>
        /// <value>2</value>
        [Description("GAC")]
        GAC = 2
    }
}
