using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Dm;

using Sixnet.Development.Data.Command;
using Sixnet.Development.Data.Dapper;
using Sixnet.Development.Data.Database;
using Sixnet.Exceptions;

namespace Sixnet.Database.DaMeng
{
    /// <summary>
    /// Defines database provider implementation for DaMeng
    /// </summary>
    public class DaMengProvider : SixnetBaseDatabaseProvider
    {
        #region Constructor

        public DaMengProvider()
        {
            queryTablesScript = "SELECT TABLE_NAME AS \"TableName\" FROM USER_TABLES WHERE TABLE_NAME NOT LIKE '%$%' AND TABLE_NAME NOT LIKE '%LOGMNRC_%' AND TABLE_NAME NOT LIKE '%LOGMNR%' AND TABLE_NAME NOT LIKE '%SQLPLUS_%' AND TABLE_NAME!='HELP' AND TABLE_NAME!= 'REDO_DB' AND TABLE_NAME!='REDO_LOG' AND TABLE_NAME!='SCHEDULER_PROGRAM_ARGS_TBL' AND TABLE_NAME!='SCHEDULER_JOB_ARGS_TBL'";
        }

        #endregion

        #region Connection

        /// <summary>
        /// Get database connection
        /// </summary>
        /// <param name="server">Database server</param>
        /// <returns></returns>
        public override IDbConnection GetDbConnection(SixnetDatabaseServer server)
        {
            return DaMengManager.GetConnection(server);
        }

        /// <summary>
        /// Get db connection meta
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public override SixnetDatabaseConnectionMeta GetDbConnectionMeta(IDbConnection connection)
        {
            var sqlBuilder = new DmConnectionStringBuilder(connection.ConnectionString);
            return new SixnetDatabaseConnectionMeta()
            {
                UserName = sqlBuilder.User,
                Password = sqlBuilder.Password,
                DataSource = sqlBuilder.Server,
                DatabaseName = sqlBuilder.Database,
            };
        }

        #endregion

        #region Data command resolver

        /// <summary>
        /// Get data command resolver
        /// </summary>
        /// <returns></returns>
        protected override ISixnetDataCommandResolver GetDataCommandResolver()
        {
            return DaMengManager.GetCommandResolver();
        }

        #endregion

        #region Parameter

        /// <summary>
        /// Convert data command parametes
        /// </summary>
        /// <param name="parameters">Data command parameters</param>
        /// <returns></returns>
        protected override DynamicParameters ConvertDataCommandParameters(SixnetDataCommandParameters parameters)
        {
            return parameters?.ConvertToDynamicParameters(DaMengManager.CurrentDatabaseServerType);
        }

        #endregion

        #region Insert

        /// <summary>
        /// Insert data and return auto identities
        /// </summary>
        /// <param name="command">Database multiple command</param>
        /// <returns>Added data identities,Key: command id, Value: identity value</returns>
        public override Dictionary<string, TIdentity> InsertAndReturnIdentity<TIdentity>(SixnetMultipleDatabaseCommand command)
        {
            var dataCommandResolver = GetDataCommandResolver() as DaMengDataCommandResolver;
            var statements = dataCommandResolver.GenerateDatabaseExecutionStatements(command);
            var identityDict = new Dictionary<string, TIdentity>();
            var dbConnection = command.Connection.DbConnection;
            foreach (var statement in statements)
            {
                var commandDefinition = GetCommandDefinition(command, statement);
                dbConnection.Execute(commandDefinition);
                if (commandDefinition.Parameters is DynamicParameters commandParameters && statement.Parameters != null)
                {
                    foreach (var parItem in statement.Parameters.Items)
                    {
                        if (parItem.Value.ParameterDirection == ParameterDirection.Output)
                        {
                            identityDict[parItem.Key] = commandParameters.Get<TIdentity>(parItem.Key);
                        }
                    }
                }
            }
            return identityDict;
        }

        /// <summary>
        /// Insert data and return auto identities
        /// </summary>
        /// <param name="command">Database multiple command</param>
        /// <returns>Added data identities,Key: command id, Value: identity value</returns>
        public override async Task<Dictionary<string, TIdentity>> InsertAndReturnIdentityAsync<TIdentity>(SixnetMultipleDatabaseCommand command)
        {
            var dataCommandResolver = GetDataCommandResolver() as DaMengDataCommandResolver;
            var statements = dataCommandResolver.GenerateDatabaseExecutionStatements(command);
            var identityDict = new Dictionary<string, TIdentity>();
            var dbConnection = command.Connection.DbConnection;
            foreach (var statement in statements)
            {
                var commandDefinition = GetCommandDefinition(command, statement);
                await dbConnection.ExecuteAsync(commandDefinition).ConfigureAwait(false);
                if (commandDefinition.Parameters is DynamicParameters commandParameters && statement.Parameters != null)
                {
                    foreach (var parItem in statement.Parameters.Items)
                    {
                        if (parItem.Value.ParameterDirection == ParameterDirection.Output)
                        {
                            identityDict[parItem.Key.LSplit(dataCommandResolver.ParameterPrefix)[0]] = commandParameters.Get<TIdentity>(parItem.Key);
                        }
                    }
                }
            }
            return identityDict;
        }

        #endregion

        #region Bulk

        /// <summary>
        /// Bulk insert datas
        /// </summary>
        /// <param name="server">Database server</param>
        /// <param name="dataTable">Data table</param>
        /// <param name="bulkInsertOptions">Insert options</param>
        public override async Task BulkInsertAsync(SixnetBulkInsertDatabaseCommand command)
        {
            BulkInsert(command);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Bulk insert datas
        /// </summary>
        /// <param name="server">Database server</param>
        /// <param name="dataTable">Data table</param>
        /// <param name="bulkInsertOptions">Insert options</param>
        public override void BulkInsert(SixnetBulkInsertDatabaseCommand command)
        {
            try
            {
                var dataTable = command.DataTable;
                SixnetDirectThrower.ThrowArgNullIf(dataTable == null, nameof(SixnetBulkInsertDatabaseCommand.DataTable));

                var dmBulkInsertOptions = command.BulkInsertionOptions as DaMengBulkInsertionOptions;
                dmBulkInsertOptions ??= new DaMengBulkInsertionOptions();
                var conn = command.Connection.DbConnection as DmConnection;
                var dmResolver = DaMengManager.GetCommandResolver();
                using (var dmBulkCopy = new DmBulkCopy(conn))
                {
                    dmBulkCopy.DestinationTableName = dataTable.TableName;
                    if (!dmBulkInsertOptions.ColumnMappings.IsNullOrEmpty())
                    {
                        dmBulkInsertOptions.ColumnMappings.ForEach(c =>
                        {
                            c.DestinationColumn = dmResolver.GetObjectFullName(dmResolver.FormatObjectName(SixnetDatabaseObjectName.Create(c.DestinationColumn, Development.Data.SixnetDatabaseObjectType.Column)));
                            dmBulkCopy.ColumnMappings.Add(c);
                        });
                    }
                    else
                    {
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            var destName = column.ColumnName;
                            destName = dmResolver.GetObjectFullName(dmResolver.FormatObjectName(SixnetDatabaseObjectName.Create(destName, Development.Data.SixnetDatabaseObjectType.Column)));
                            dmBulkCopy.ColumnMappings.Add(new DmBulkCopyColumnMapping()
                            {
                                SourceColumn = column.ColumnName,
                                DestinationColumn = destName
                            });
                        }
                    }
                    if (dmBulkInsertOptions.BulkCopyTimeout > 0)
                    {
                        dmBulkCopy.BulkCopyTimeout = dmBulkInsertOptions.BulkCopyTimeout;
                    }
                    if (dmBulkInsertOptions.BatchSize > 0)
                    {
                        dmBulkCopy.BatchSize = dmBulkInsertOptions.BatchSize;
                    }
                    if (dmBulkInsertOptions.NotifyAfter > 0)
                    {
                        dmBulkCopy.NotifyAfter = dmBulkInsertOptions.NotifyAfter;
                    }
                    dmBulkCopy.DestinationTableName = dmResolver.GetObjectFullName(dmResolver.FormatObjectName(SixnetDatabaseObjectName.Create(dmBulkCopy.DestinationTableName, Development.Data.SixnetDatabaseObjectType.Column)));
                    dmBulkCopy.WriteToServer(dataTable);
                }
            }
            catch (Exception ex)
            {
                throw GetSqlException(ex);
            }
        }

        #endregion                                                                                                                                                           

        #region Get exception

        protected override Exception GetSqlException(Exception ex)
        {
            if (ex is DmException sqlException)
            {
                switch (sqlException.Number)
                {
                    case 1:
                        return new SixnetSqlAlreadExistsException(sqlException.Message, sqlException);
                }
            }
            return ex;
        }

        #endregion
    }
}
