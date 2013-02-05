Ers deployer installation guide:
===============================

System requirements:
--------------------

- IIS version 6 or 7
- .Net Framework 4.0
- ASP .Net MVC 3
- Microsoft SQL Server System CLR Types
- Microsoft SQL Server Management Objects

Installation:
------------

To build solution use `Publish.targets` msbuild script, `ConfigsDestination` dir should contain files structure the same as `configs` dir in repository root:

```
msbuild Publish.targets
  /p:PublishDestination="C:\Deployer";
     ConfigsDestination="C:\Configs";
     TargetServer="ws2003"
  /v:m

  // "ws2003" - for Windows Server 2003, "ws2008" - for Windows Server 2008
```

Received files could be published as wcf service to IIS6, IIS7 or Windows service.
Service is configured through the `deployer` configuration section in `web.config` and 2 connection strings:

```
  <deployer>
    <settings>
      <credentials domain="Domain" login="UserName" password="Password"/>
      // adress for iis6 should be taken from it's metadata, for ii7 it's just name of root website
      <iis version="7" appPool="DefaultAppPool" address="Surveys">
        <dirsWithIISAccess> // dirs that need additional access rights
          <dir name="Export" user="IIS_IUSRS" />
          <dir name="Exceptions" user="IIS_IUSRS" />
        </dirsWithIISAccess>
      </iis>
      <scriptsPaths
        dbCreate="{Path to service}\Scripts\CreateDb.sql"
        dbValidate="{Path to service}\Scripts\ValidateDb.sql"
        dbDelete="{Path to service}\Scripts\DeleteDb.sql" />
      <templates connectionStrings="{Path to service}\Scripts\ConnectionStrings.config" />
      <paths
        surveys="{Path to surveys}\Surveys" // path to installed surveys
        uploads="{Path to service}\Packages" // path to uploaded packages
        backups="{Path to service}\Backup"> // path to backup packages
        // bins that will copied to survey after its installation
        <additionalResources bins="{Path to service}\Bins" />
      </paths>
    </settings>
    <updateRules>
      <modes>
        <mode id="1" name="Bin" updateDir="bin">
          <rules>
            <rule func="startsWith"  value="bin" />
          </rules>
        </mode>
        <mode id="2" name="AppData" updateDir="app_data/questionsdata">
          <rules>
            <rule func="startsWith" value="app_data\questionsdata" />
          </rules>
        </mode>
        <mode id="3" name="AllExceptConf" updateDir="/">
          <rules>
            <rule inverted="true" func="equals"  value="web.config" />
          </rules>
        </mode>
        <mode id="4" name="All" updateDir="/"/>
      </modes>
      <neverToUpdate> // rules for files that will never be updated
        <rule inverted="true" func="equals" value="connectionstrings.config" />
        <rule inverted="true" func="endsWith" value="white.csv" />
        ...
        <rule inverted="true" func="equals" value="bin\win64\spssio64.dll" />
      </neverToUpdate>
    </updateRules>
  </deployer>

  ...

  <connectionStrings>
    <add name="master" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=master;User ID=user;password=password" providerName="System.Data.SqlClient"/>
    // this connection should designate to db that will be created with database.sql (could be found in ./src/)
    <add name="deployer" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=DeployService;User ID=user;password=password" providerName="System.Data.SqlClient"/>
  </connectionStrings>
```
Installed service requires rights to remove or add applications to IIS (as example you will need to specify identity to its AppPool in IIS).
Also you need to specify path to dir where service will save its logs:

```
<variable name="logDir" value="{Path to service}\Log\${date:format=yyyy-MM-dd}" />
```

Service uses security system that based on certificates and login-password authentication, so you need to install necessary certificates and add required users:
- Add root certificate `RootDeployerCA.cer` (you could find it in root of deployer solution) to `Local Machine certificates/Trusted Root Sertification Authorities` (`win + r -> mmc -> File -> Add\Remove Snap-in... -> Certificates -> Local Machine -> Import`)
- In the same way import revocation list `RootDeployerCA.crl`
- `DeployerCA.pfx` import to Personal certificates, you need password in order to do it (ask administrator)
- Allow user which is used as deployer appPool identity access to `DeployerCA` key
- Using `database.sql` from solution root create database for deployer
- For each client add user to `dbo.User` table deployer's database

Client side:
------------

- Build `Deployer.Client` project
- In `app.config` in client's rool directory make next changes:
	- Specify `clientBaseAddress="http://192.168.1.1:84/"` parameter, it will be address to which server will send messages about installation process
	- Specify `address="http://192.168.1.1/DeployService/deployer.svc"` parameter, it should be address of the deployer's endpoint
	- Specify user's login/password for authentication at server:

```
  <appSettings>
    <add key="login" value="UserName"/>
    <add key="password" value="Password"/>
  </appSettings>
```

- In the same way, as specified for server side, import `RootDeployerCA.cer`, `RootDeployerCA.crl` certificates