using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Core.Helpers;

namespace Deployer.Service.Core
{
    public class ServiceExceptionsHandler : IErrorHandler
    {
        private readonly IServiceLogger _logger;

        public ServiceExceptionsHandler(string bindingName, IServiceLogger logger) {
            _logger = logger;
            BindingName = bindingName;
        }

        public string BindingName { get; private set; }

        #region Implementation of IErrorHandler

        public void ProvideFault(Exception ex, MessageVersion version, ref Message msg) {
            _logger.Error("ServiceExceptionsHandler received unhandled exception", ex);
            
            var fe = new FaultException<UnhandledException>(UnhandledException.CreateFromEx(ex));
            var mf = fe.CreateMessageFault();
            msg = Message.CreateMessage(version, mf, string.Empty);

            _logger.Error("ServiceExceptionsHandler handled exception and sent ex data to client");
        }

        public bool HandleError(Exception ex) {
            _logger.Error("ServiceExceptionsHandler received unhandled exception", ex);
            return true;
        }

        #endregion
    }

    public class InterceptAllExceptions : Attribute, IServiceBehavior
    {
        #region Implementation of IServiceBehavior

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
            var logger = new ServiceLogger();
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers) {
                logger.Info(string.Format("Dispatcher at {0} is shielded by error handler.", dispatcher.BindingName));
                dispatcher.ErrorHandlers.Add(new ServiceExceptionsHandler(dispatcher.BindingName, logger));
            }
        }

        #endregion
    }
}