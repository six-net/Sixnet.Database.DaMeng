using System.Data;
using Dm;
using Sixnet.Development.Data;
using Sixnet.Development.Data.Database;

namespace Sixnet.Database.DaMeng
{
    /// <summary>
    /// Defines DaMeng manager
    /// </summary>
    internal static class DaMengManager
    {
        #region Fields

        /// <summary>
        /// Gets current database server type
        /// </summary>
        internal const SixnetDatabaseType CurrentDatabaseServerType = SixnetDatabaseType.DaMeng;

        /// <summary>
        /// DaMeng options
        /// </summary>
        internal static readonly DaMengOptions DaMengOptions = new DaMengOptions();

        /// <summary>
        /// Default query translator
        /// </summary>
        static readonly DaMengDataCommandResolver DefaultResolver = new DaMengDataCommandResolver();

        #endregion

        #region Get database connection

        /// <summary>
        /// Get database connection
        /// </summary>
        /// <param name="server">Database server</param>
        /// <returns>Return database connection</returns>
        internal static IDbConnection GetConnection(SixnetDatabaseServer server)
        {
            return SixnetDataManager.GetDatabaseConnection(server) ?? new DmConnection(SixnetDataManager.ResolveConnectionString(server));
        }

        #endregion

        #region Get command resolver

        /// <summary>
        /// Get command resolver
        /// </summary>
        /// <returns>Return a command resolver</returns>
        internal static DaMengDataCommandResolver GetCommandResolver()
        {
            return DefaultResolver;
        }

        #endregion
    }
}
