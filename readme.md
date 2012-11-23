Ers deployer installation guide:
===============================

Server side:
------------

### Requirements:

- IIS 7
- .Net 4.0
- ASP .Net MVC 3
- [Microsoft SQL Server System CLR Types][mssql-libraries]
- [Microsoft SQL Server 2008 Management Objects][mssql-libraries]

### Installation:

- Publish solution from `Deployer.Service.WebHost` project
- From `bin` folder in published sources cut `Bins` and `Scripts` folders and paste them to root folder near `web.config`
- Put all dlls that should be copied to survey's bin folder after its installation to `Bins` folder
- Copy file `DepSettings.xml` deployer's to bin forlder (you could find it in rool of `Deployer.Service.Core` project)
- Create folders `Packages` and `Backup` in the root of deployer's folder
- Create IIS application and individual appPool for .Net 4.0 with correct identity for depoyer

> We will use `main/DeployService` application for deployer (`main` - IIS Web Site)

> Further we will expect that deployer located by next path `c:\inetpub\main\DeployService\`
> All listed paths/configurations below will formed considering this location

- In deployer's `web.config` fix logs folder:

```
<variable name="logDir" value="c:\inetpub\main\DeployService\Log\${date:format=yyyy-MM-dd}" />
```

- In `bin\DepSettings.xml` make necessary changes:

```
<?xml version="1.0" encoding="utf-8" ?>
<Config>
  <DeploySettings>
    <item name="Domain"></item>
    <item name="Login"></item>
    <item name="Password"></item>
    <item name="AppPool">DefaultAppPool</item> # pool for installed surveys
    <item name="IISAdress">main</item> # IIS web site, to which surveys will be installed as applications
    <item name="DbDeleteScript">c:\inetpub\main\DeployService\Scripts\DeleteDb.sql</item>
    <item name="DbCreationScript">c:\inetpub\main\DeployService\Scripts\CreateDb.sql</item>
    <item name="DbValidationScript">c:\inetpub\main\DeployService\Scripts\ValidateDb.sql</item>
    <item name="ConnStringTemplate">c:\inetpub\main\DeployService\Scripts\ConnectionStrings.config</item>
    <iten name="AdditionalBinsFolder">c:\inetpub\main\DeployService\Bins</iten>
    <item name="UploadedZipFolder">c:\inetpub\main\DeployService\Packages</item>
    <item name="SurveysFolder">c:\inetpub\main</item>
    <item name="SurveysBackupForlder">c:\inetpub\main\DeployService\Backup</item>
    <item name="ConnectionString">Data Source=192.168.56.101\SQLEXPRESS;Initial Catalog=master;User ID=wurdum;password=123</item>
    <item name="DeployerConnectionString">Data Source=192.168.56.101\SQLEXPRESS;Initial Catalog=DeployService;User ID=wurdum;password=123</item>
  </DeploySettings>
  <!-- We update all files where each rule from rules set returns True,
       if one of the rules returns false - file will not be updated.
       Word case is ignored in comparing.  -->
  <UpdateRules>
    <mode id="1" name="Bin" updateDir="bin">
      <rule func="startsWith">bin</rule>
    </mode>
    <mode id="2" name="AppData" updateDir="app_data/questionsdata">
      <rule func="startsWith">app_data\questionsdata</rule>
    </mode>
    <mode id="3" name="AllExceptConf" updateDir="/">
      <rule modifier="not" func="equals">web.config</rule>
    </mode>
    <mode id="4" name="All" updateDir="/" />
    <neverToUpdate>
      <rule modifier="not" func="equals">connectionstrings.config</rule>
      <rule modifier="not" func="endsWith">white.csv</rule>
      <rule modifier="not" func="endsWith">black.csv</rule>
      <rule modifier="not" func="equals">Export</rule>
      <rule modifier="not" func="equals">Exceptions</rule>
      <rule modifier="not" func="equals">bin\spssio32.dll</rule>
      <rule modifier="not" func="equals">bin\system.web.mvc.dll</rule>
      <rule modifier="not" func="equals">bin\win64\icudt38.dll</rule>
      <rule modifier="not" func="equals">bin\win64\icuin38.dll</rule>
      <rule modifier="not" func="equals">bin\win64\icuuc38.dll</rule>
      <rule modifier="not" func="equals">bin\win64\spssio64.dll</rule>
    </neverToUpdate>
  </UpdateRules>
</Config>
```

- Fix connection strings template `Scripts\ConnectionStrings.config`, <%DbName%> will be replced by survey's database name

```
<?xml version="1.0"?>
<connectionStrings>
  <add name="Linq2Sql" 
       connectionString="Data Source=192.168.56.101\sqlexpress;Initial Catalog=<%DbName%>;User ID=wurdum;password=123" 
       providerName="System.Data.SqlClient"/>
  <add name="Exceptions" 
       connectionString="Data Source=192.168.56.101\sqlexpress;Initial Catalog=Exceptions;User ID=wurdum;password=123" 
       providerName="System.Data.SqlClient"/>
  <add name="Survey.AspNet" 
       connectionString="Data Source=192.168.56.101\sqlexpress;Initial Catalog=Survey.AspNet;User ID=wurdum;password=123" 
       providerName="System.Data.SqlClient"/>
</connectionStrings>
```

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
	- Specify `clientBaseAddress="http://192.168.56.101:84/"` parameter, it will be address to which server will send messages about installation process
	- Specify `address="http://192.168.56.103/DeployService/deployer.svc"` parameter, it should be address of the deployer's endpoint
	- Specify user's login/password for authentication at server:

```
  <appSettings>
    <add key="login" value="wurdum"/>
    <add key="password" value="123"/>
  </appSettings>
```

- In the same way, as specified for server side, import `RootDeployerCA.cer`, `RootDeployerCA.crl` certificates

*Created by Ritikov Pavel, pavel.ritikov@gmail.com*

[mssql-libraries]: http://www.microsoft.com/en-us/download/details.aspx?id=16177