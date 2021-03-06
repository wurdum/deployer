﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Deployer.Service.Data
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="DeployService")]
	public partial class DbDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertDeploy(Deploy instance);
    partial void UpdateDeploy(Deploy instance);
    partial void DeleteDeploy(Deploy instance);
    partial void InsertUser(User instance);
    partial void UpdateUser(User instance);
    partial void DeleteUser(User instance);
    partial void InsertDeployMode(DeployMode instance);
    partial void UpdateDeployMode(DeployMode instance);
    partial void DeleteDeployMode(DeployMode instance);
    partial void InsertDeployStatus(DeployStatus instance);
    partial void UpdateDeployStatus(DeployStatus instance);
    partial void DeleteDeployStatus(DeployStatus instance);
    partial void InsertException(Exception instance);
    partial void UpdateException(Exception instance);
    partial void DeleteException(Exception instance);
    #endregion
		
		public DbDataContext() : 
				base(global::Deployer.Service.Data.Properties.Settings.Default.DeployServiceConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DbDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DbDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DbDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DbDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Deploy> Deploys
		{
			get
			{
				return this.GetTable<Deploy>();
			}
		}
		
		public System.Data.Linq.Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}
		
		public System.Data.Linq.Table<DeployMode> DeployModes
		{
			get
			{
				return this.GetTable<DeployMode>();
			}
		}
		
		public System.Data.Linq.Table<DeployStatus> DeployStatus
		{
			get
			{
				return this.GetTable<DeployStatus>();
			}
		}
		
		public System.Data.Linq.Table<Exception> Exceptions
		{
			get
			{
				return this.GetTable<Exception>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Deploy")]
	public partial class Deploy : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id;
		
		private string _User;
		
		private int _Mode;
		
		private int _Status;
		
		private string _SessionKey;
		
		private string _SurveyName;
		
		private System.Nullable<System.DateTime> _StartUtc;
		
		private System.Nullable<System.DateTime> _EndUtc;
		
		private EntitySet<Exception> _Exceptions;
		
		private EntityRef<User> _User1;
		
		private EntityRef<DeployMode> _DeployMode;
		
		private EntityRef<DeployStatus> _DeployStatus;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(long value);
    partial void OnIdChanged();
    partial void OnUserChanging(string value);
    partial void OnUserChanged();
    partial void OnModeChanging(int value);
    partial void OnModeChanged();
    partial void OnStatusChanging(int value);
    partial void OnStatusChanged();
    partial void OnSessionKeyChanging(string value);
    partial void OnSessionKeyChanged();
    partial void OnSurveyNameChanging(string value);
    partial void OnSurveyNameChanged();
    partial void OnStartUtcChanging(System.Nullable<System.DateTime> value);
    partial void OnStartUtcChanged();
    partial void OnEndUtcChanging(System.Nullable<System.DateTime> value);
    partial void OnEndUtcChanged();
    #endregion
		
		public Deploy()
		{
			this._Exceptions = new EntitySet<Exception>(new Action<Exception>(this.attach_Exceptions), new Action<Exception>(this.detach_Exceptions));
			this._User1 = default(EntityRef<User>);
			this._DeployMode = default(EntityRef<DeployMode>);
			this._DeployStatus = default(EntityRef<DeployStatus>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="[User]", Storage="_User", DbType="NVarChar(20) NOT NULL", CanBeNull=false)]
		public string User
		{
			get
			{
				return this._User;
			}
			set
			{
				if ((this._User != value))
				{
					this.OnUserChanging(value);
					this.SendPropertyChanging();
					this._User = value;
					this.SendPropertyChanged("User");
					this.OnUserChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Mode", DbType="Int NOT NULL")]
		public int Mode
		{
			get
			{
				return this._Mode;
			}
			set
			{
				if ((this._Mode != value))
				{
					if (this._DeployMode.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnModeChanging(value);
					this.SendPropertyChanging();
					this._Mode = value;
					this.SendPropertyChanged("Mode");
					this.OnModeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Status", DbType="Int NOT NULL")]
		public int Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				if ((this._Status != value))
				{
					if (this._DeployStatus.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnStatusChanging(value);
					this.SendPropertyChanging();
					this._Status = value;
					this.SendPropertyChanged("Status");
					this.OnStatusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SessionKey", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string SessionKey
		{
			get
			{
				return this._SessionKey;
			}
			set
			{
				if ((this._SessionKey != value))
				{
					this.OnSessionKeyChanging(value);
					this.SendPropertyChanging();
					this._SessionKey = value;
					this.SendPropertyChanged("SessionKey");
					this.OnSessionKeyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SurveyName", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string SurveyName
		{
			get
			{
				return this._SurveyName;
			}
			set
			{
				if ((this._SurveyName != value))
				{
					this.OnSurveyNameChanging(value);
					this.SendPropertyChanging();
					this._SurveyName = value;
					this.SendPropertyChanged("SurveyName");
					this.OnSurveyNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StartUtc", DbType="DateTime")]
		public System.Nullable<System.DateTime> StartUtc
		{
			get
			{
				return this._StartUtc;
			}
			set
			{
				if ((this._StartUtc != value))
				{
					this.OnStartUtcChanging(value);
					this.SendPropertyChanging();
					this._StartUtc = value;
					this.SendPropertyChanged("StartUtc");
					this.OnStartUtcChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EndUtc", DbType="DateTime")]
		public System.Nullable<System.DateTime> EndUtc
		{
			get
			{
				return this._EndUtc;
			}
			set
			{
				if ((this._EndUtc != value))
				{
					this.OnEndUtcChanging(value);
					this.SendPropertyChanging();
					this._EndUtc = value;
					this.SendPropertyChanged("EndUtc");
					this.OnEndUtcChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Deploy_Exception", Storage="_Exceptions", ThisKey="Id", OtherKey="DeployId")]
		public EntitySet<Exception> Exceptions
		{
			get
			{
				return this._Exceptions;
			}
			set
			{
				this._Exceptions.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="User_Deploy", Storage="_User1", ThisKey="User", OtherKey="Login", IsForeignKey=true)]
		public User User1
		{
			get
			{
				return this._User1.Entity;
			}
			set
			{
				User previousValue = this._User1.Entity;
				if (((previousValue != value) 
							|| (this._User1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._User1.Entity = null;
						previousValue.Deploys.Remove(this);
					}
					this._User1.Entity = value;
					if ((value != null))
					{
						value.Deploys.Add(this);
						this._User = value.Login;
					}
					else
					{
						this._User = default(string);
					}
					this.SendPropertyChanged("User1");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DeployMode_Deploy", Storage="_DeployMode", ThisKey="Mode", OtherKey="Id", IsForeignKey=true)]
		public DeployMode DeployMode
		{
			get
			{
				return this._DeployMode.Entity;
			}
			set
			{
				DeployMode previousValue = this._DeployMode.Entity;
				if (((previousValue != value) 
							|| (this._DeployMode.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._DeployMode.Entity = null;
						previousValue.Deploys.Remove(this);
					}
					this._DeployMode.Entity = value;
					if ((value != null))
					{
						value.Deploys.Add(this);
						this._Mode = value.Id;
					}
					else
					{
						this._Mode = default(int);
					}
					this.SendPropertyChanged("DeployMode");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DeployStatus_Deploy", Storage="_DeployStatus", ThisKey="Status", OtherKey="Id", IsForeignKey=true)]
		public DeployStatus DeployStatus
		{
			get
			{
				return this._DeployStatus.Entity;
			}
			set
			{
				DeployStatus previousValue = this._DeployStatus.Entity;
				if (((previousValue != value) 
							|| (this._DeployStatus.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._DeployStatus.Entity = null;
						previousValue.Deploys.Remove(this);
					}
					this._DeployStatus.Entity = value;
					if ((value != null))
					{
						value.Deploys.Add(this);
						this._Status = value.Id;
					}
					else
					{
						this._Status = default(int);
					}
					this.SendPropertyChanged("DeployStatus");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Exceptions(Exception entity)
		{
			this.SendPropertyChanging();
			entity.Deploy = this;
		}
		
		private void detach_Exceptions(Exception entity)
		{
			this.SendPropertyChanging();
			entity.Deploy = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.[User]")]
	public partial class User : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Login;
		
		private string _Password;
		
		private EntitySet<Deploy> _Deploys;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnLoginChanging(string value);
    partial void OnLoginChanged();
    partial void OnPasswordChanging(string value);
    partial void OnPasswordChanged();
    #endregion
		
		public User()
		{
			this._Deploys = new EntitySet<Deploy>(new Action<Deploy>(this.attach_Deploys), new Action<Deploy>(this.detach_Deploys));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Login", DbType="NVarChar(20) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Login
		{
			get
			{
				return this._Login;
			}
			set
			{
				if ((this._Login != value))
				{
					this.OnLoginChanging(value);
					this.SendPropertyChanging();
					this._Login = value;
					this.SendPropertyChanged("Login");
					this.OnLoginChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Password", DbType="NVarChar(20) NOT NULL", CanBeNull=false)]
		public string Password
		{
			get
			{
				return this._Password;
			}
			set
			{
				if ((this._Password != value))
				{
					this.OnPasswordChanging(value);
					this.SendPropertyChanging();
					this._Password = value;
					this.SendPropertyChanged("Password");
					this.OnPasswordChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="User_Deploy", Storage="_Deploys", ThisKey="Login", OtherKey="User")]
		public EntitySet<Deploy> Deploys
		{
			get
			{
				return this._Deploys;
			}
			set
			{
				this._Deploys.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.User1 = this;
		}
		
		private void detach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.User1 = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.DeployMode")]
	public partial class DeployMode : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _Id;
		
		private string _Name;
		
		private EntitySet<Deploy> _Deploys;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    #endregion
		
		public DeployMode()
		{
			this._Deploys = new EntitySet<Deploy>(new Action<Deploy>(this.attach_Deploys), new Action<Deploy>(this.detach_Deploys));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(20) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DeployMode_Deploy", Storage="_Deploys", ThisKey="Id", OtherKey="Mode")]
		public EntitySet<Deploy> Deploys
		{
			get
			{
				return this._Deploys;
			}
			set
			{
				this._Deploys.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.DeployMode = this;
		}
		
		private void detach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.DeployMode = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.DeployStatus")]
	public partial class DeployStatus : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _Id;
		
		private string _Name;
		
		private EntitySet<Deploy> _Deploys;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    #endregion
		
		public DeployStatus()
		{
			this._Deploys = new EntitySet<Deploy>(new Action<Deploy>(this.attach_Deploys), new Action<Deploy>(this.detach_Deploys));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(20) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DeployStatus_Deploy", Storage="_Deploys", ThisKey="Id", OtherKey="Status")]
		public EntitySet<Deploy> Deploys
		{
			get
			{
				return this._Deploys;
			}
			set
			{
				this._Deploys.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.DeployStatus = this;
		}
		
		private void detach_Deploys(Deploy entity)
		{
			this.SendPropertyChanging();
			entity.DeployStatus = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Exception")]
	public partial class Exception : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id;
		
		private long _DeployId;
		
		private string _Source;
		
		private string _Message;
		
		private System.Xml.Linq.XElement _ExceptionData;
		
		private System.Nullable<System.DateTime> _TimeStamp;
		
		private EntityRef<Deploy> _Deploy;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(long value);
    partial void OnIdChanged();
    partial void OnDeployIdChanging(long value);
    partial void OnDeployIdChanged();
    partial void OnSourceChanging(string value);
    partial void OnSourceChanged();
    partial void OnMessageChanging(string value);
    partial void OnMessageChanged();
    partial void OnExceptionDataChanging(System.Xml.Linq.XElement value);
    partial void OnExceptionDataChanged();
    partial void OnTimeStampChanging(System.Nullable<System.DateTime> value);
    partial void OnTimeStampChanged();
    #endregion
		
		public Exception()
		{
			this._Deploy = default(EntityRef<Deploy>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DeployId", DbType="BigInt NOT NULL")]
		public long DeployId
		{
			get
			{
				return this._DeployId;
			}
			set
			{
				if ((this._DeployId != value))
				{
					if (this._Deploy.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnDeployIdChanging(value);
					this.SendPropertyChanging();
					this._DeployId = value;
					this.SendPropertyChanged("DeployId");
					this.OnDeployIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Source", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Source
		{
			get
			{
				return this._Source;
			}
			set
			{
				if ((this._Source != value))
				{
					this.OnSourceChanging(value);
					this.SendPropertyChanging();
					this._Source = value;
					this.SendPropertyChanged("Source");
					this.OnSourceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Message", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string Message
		{
			get
			{
				return this._Message;
			}
			set
			{
				if ((this._Message != value))
				{
					this.OnMessageChanging(value);
					this.SendPropertyChanging();
					this._Message = value;
					this.SendPropertyChanged("Message");
					this.OnMessageChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ExceptionData", DbType="Xml NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public System.Xml.Linq.XElement ExceptionData
		{
			get
			{
				return this._ExceptionData;
			}
			set
			{
				if ((this._ExceptionData != value))
				{
					this.OnExceptionDataChanging(value);
					this.SendPropertyChanging();
					this._ExceptionData = value;
					this.SendPropertyChanged("ExceptionData");
					this.OnExceptionDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TimeStamp", DbType="DateTime")]
		public System.Nullable<System.DateTime> TimeStamp
		{
			get
			{
				return this._TimeStamp;
			}
			set
			{
				if ((this._TimeStamp != value))
				{
					this.OnTimeStampChanging(value);
					this.SendPropertyChanging();
					this._TimeStamp = value;
					this.SendPropertyChanged("TimeStamp");
					this.OnTimeStampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Deploy_Exception", Storage="_Deploy", ThisKey="DeployId", OtherKey="Id", IsForeignKey=true)]
		public Deploy Deploy
		{
			get
			{
				return this._Deploy.Entity;
			}
			set
			{
				Deploy previousValue = this._Deploy.Entity;
				if (((previousValue != value) 
							|| (this._Deploy.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Deploy.Entity = null;
						previousValue.Exceptions.Remove(this);
					}
					this._Deploy.Entity = value;
					if ((value != null))
					{
						value.Exceptions.Add(this);
						this._DeployId = value.Id;
					}
					else
					{
						this._DeployId = default(long);
					}
					this.SendPropertyChanged("Deploy");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
