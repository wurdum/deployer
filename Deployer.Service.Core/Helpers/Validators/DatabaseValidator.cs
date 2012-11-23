using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public class DatabaseValidator : Validator
    {
        public DatabaseValidator(IDeploySettings appSettings):base(appSettings) {
            Name = "Database validator";
        }

        #region Overrides of Validator

        public override bool IsValidForInstall() {
            try {
                var connString = new SqlConnectionStringBuilder(_appSettings["ConnectionString"]);
                var validationScript = File.OpenText(_appSettings["DbValidationScript"]).ReadToEnd();
                using (var connection = new SqlConnection(connString.ConnectionString)) {
                    connection.Open();
                    var dbSelector = new SqlCommand(validationScript, connection);
                    dbSelector.Parameters.Add("@dbName", SqlDbType.NVarChar).Value = _appSettings.DbName;
                    return (int) dbSelector.ExecuteScalar() == 0;
                }
            } catch (Exception ex) {
                throw new DatabaseValidationException(Name, ex);
            }
        }

        public override bool IsValidForUpdate() {
            throw new NotImplementedException("There are no scenarios to validate " + Name + " for update.");
        }

        #endregion

        public static DatabaseValidator Create(IDeploySettings appSettings) {
            return new DatabaseValidator(appSettings);
        }
    }
}