using System;
using System.Data.SqlClient;
using System.IO;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Deployer.Service.Core.Helpers.Deplyers
{
    public class DatabaseDeployer : Deployer
    {
        public DatabaseDeployer(IConf conf) : base(conf) {
            Name = "Database";
        }

        #region Overrides of Deployer

        public override void Install() {
            try {
                var connString = new SqlConnectionStringBuilder(_conf.ConnectionStrings.Master);
                var creationScript = File.OpenText(_conf.DbScriptsPaths.Create).ReadToEnd().Replace("<%DbName%>", _conf.Survey.DbName);
                using (var connection = new SqlConnection(connString.ConnectionString)) {
                    var serverConnection = new ServerConnection(connection);
                    var sqlServer = new Server(serverConnection);
                    if(sqlServer.ConnectionContext.ExecuteNonQuery(creationScript) == 0)
                        throw new DeployException("Database creation script made no changes.");
                }
            } catch (Exception ex) {
                throw new DatabaseDeploymentException(Name, ex);
            }
        }

        public override void InstallRollback() {
            try {
                var connString = new SqlConnectionStringBuilder(_conf.ConnectionStrings.Master);
                var removingScript = File.OpenText(_conf.DbScriptsPaths.Delete).ReadToEnd().Replace("<%DbName%>", _conf.Survey.DbName);
                using (var connection = new SqlConnection(connString.ConnectionString)) {
                    connection.Open();
                    var sqlCommand = new SqlCommand(removingScript, connection);
                    sqlCommand.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        public override void Update() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        public override void UpdateRollback() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        #endregion

        public static DatabaseDeployer Create(IConf appSettings) {
            return new DatabaseDeployer(appSettings);
        }
    }
}