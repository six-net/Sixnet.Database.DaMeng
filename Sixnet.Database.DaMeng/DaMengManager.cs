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
        internal const DatabaseType CurrentDatabaseServerType = DatabaseType.DaMeng;

        /// <summary>
        /// Key word prefix
        /// </summary>
        internal const string KeywordPrefix = "\"";

        /// <summary>
        /// Key word suffix
        /// </summary>
        internal const string KeywordSuffix = "\"";

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
        internal static IDbConnection GetConnection(DatabaseServer server)
        {
            return SixnetDataManager.GetDatabaseConnection(server) ?? new DmConnection(server.ConnectionString);
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

        #region Format keyword

        internal static string FormatKeyword(string originalValue)
        {
            return SixnetDataManager.HandleDatabaseWordAndName(CurrentDatabaseServerType, originalValue);
        }

        #endregion

        #region Wrap keyword

        /// <summary>
        /// Wrap keyword by the KeywordPrefix and the KeywordSuffix
        /// </summary>
        /// <param name="originalValue">Original value</param>
        /// <returns></returns>
        internal static string WrapKeyword(string originalValue)
        {
            return $"{KeywordPrefix}{originalValue}{KeywordSuffix}";
        }

        #endregion
    }
}
