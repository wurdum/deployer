using System;
using System.Collections.Generic;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core;
using Deployer.Service.Core.Exceptions;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Settings;
using Microsoft.Practices.Unity;

namespace Deployer.Tests.Stubs
{
    public static class IocContainerStub
    {
        public static IUnityContainer GetContainer() {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<IDeployService, DeployService>(new HierarchicalLifetimeManager());
            container.RegisterType<IDeployServiceFactory, DeployServiceFactory>(new HierarchicalLifetimeManager());
            container.RegisterType<IServiceLogger, ServiceLogger>(new HierarchicalLifetimeManager());
            container.RegisterType<IPackageManager, PackageManagerStub>(new HierarchicalLifetimeManager());
            container
                .RegisterType<IValidator, ValidatorStub>("dir")
                .RegisterType<IValidator, ValidatorStub>("iis")
                .RegisterType<IValidator, ValidatorStub>("db");
            container
                .RegisterType<IDeployer, DeployerStub>("dir")
                .RegisterType<IDeployer, DeployerStub>("iis")
                .RegisterType<IDeployer, DeployerStub>("db");

            container.RegisterType<Func<IPackageManager>>(new InjectionFactory(c => new Func<IPackageManager>(() => c.Resolve<IPackageManager>())));
            container.RegisterType<Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings>>(new InjectionFactory(
                c => new Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings>((sf, dc, pm, um) => new DeploySettingsStub(sf, dc, pm, um))));
            container.RegisterType<Func<IDeployEvents>>(new InjectionFactory(
                c => new Func<IDeployEvents>(() => new CallbackChannelStub())));

            container.RegisterType<Func<IDeploySettings, IEnumerable<IValidator>>>(
                new InjectionFactory(c => new Func<IDeploySettings, IEnumerable<IValidator>>(
                    ds => {
                        return ds.DeployMode == DeployMode.Install ?
                            new List<IValidator> {
                                c.Resolve<IValidator>("dir", new ParameterOverrides { {"appSettings", ds}, {"name", "directory"}, {"type", "install"} }), 
                                c.Resolve<IValidator>("iis", new ParameterOverrides { {"appSettings", ds}, {"name", "iis"}, {"type", "install"} }), 
                                c.Resolve<IValidator>("db", new ParameterOverrides { {"appSettings", ds}, {"name", "database"}, {"type", "install"} })
                            } :
                            new List<IValidator> {
                                c.Resolve<IValidator>("dir", new ParameterOverrides { {"appSettings", ds}, {"name", "directory"}, {"type", "update"} }), 
                                c.Resolve<IValidator>("iis", new ParameterOverrides { {"appSettings", ds}, {"name", "iis"}, {"type", "update"} })
                            };
                    })));

            container.RegisterType<Func<IDeploySettings, IEnumerable<IDeployer>>>(
                new InjectionFactory(c => new Func<IDeploySettings, IEnumerable<IDeployer>>(
                    ds => {
                        return ds.DeployMode == DeployMode.Install ?
                            new List<IDeployer> {
                                c.Resolve<IDeployer>("dir", new ParameterOverrides { {"appSettings", ds}, {"name", "directory"}}), 
                                c.Resolve<IDeployer>("iis", new ParameterOverrides { {"appSettings", ds}, {"name", "iis"}}), 
                                c.Resolve<IDeployer>("db", new ParameterOverrides { {"appSettings", ds}, {"name", "database"}})
                            } :
                            new List<IDeployer> {
                                c.Resolve<IDeployer>("dir", new ParameterOverrides { {"appSettings", ds}, {"name", "directory"}})
                            };
                    })));

            container.RegisterType<Func<IDeploySettings, string, string, IDeploySession>>(new InjectionFactory(
                c => new Func<IDeploySettings, string, string, IDeploySession>((ds, s, u) => new DeploySession(ds, s, u))));
            container.RegisterType<Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler>>(new InjectionFactory(
                c => new Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler>(
                    (de, dss, sl, dsn) => new DeployExceptionHandler(de, dss, sl, dsn))));

            return container;
        }
    }
}