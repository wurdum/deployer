using System;

namespace Deployer.Service.Contract.Exceptions
{
    public class DeployServiceBaseException : Exception
    {
        public DeployServiceBaseException(string message) : base(message) {}
        public DeployServiceBaseException(string message, Exception innerException)
            : base(string.Format("{0} fired an exception: {1}", message, innerException.Message), innerException) { }
    }

    #region Validation exceptions

    public class ValidationException : DeployServiceBaseException
    {
        public ValidationException(string message) : base(message) {}
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DirectoryValidationException : ValidationException
    {
        public DirectoryValidationException(string message) : base(message) {}
        public DirectoryValidationException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class IISValidationException : ValidationException
    {
        public IISValidationException(string message) : base(message) {}
        public IISValidationException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class DatabaseValidationException : ValidationException
    {
        public DatabaseValidationException(string message) : base(message) {}
        public DatabaseValidationException(string message, Exception innerException) : base(message, innerException) {}
    }

    #endregion

    #region Deployment exceptions

    public class DeployException : DeployServiceBaseException
    {
        public DeployException(string message) : base(message) {}
        public DeployException(string message, Exception innerException): base(message + " deployer", innerException) { }
    }

    public class DirectoryDeploymentException : DeployException
    {
        public DirectoryDeploymentException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class IISDeploymentException : DeployException
    {
        public IISDeploymentException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class DatabaseDeploymentException : DeployException
    {
        public DatabaseDeploymentException(string message, Exception innerException) : base(message, innerException) {}
    }

    #endregion

    public class RollbackException : DeployServiceBaseException
    {
        public RollbackException(string message) : base(message) {}
        public RollbackException(string message, Exception innerException) : base(message + " rollback", innerException) { }
    }
}