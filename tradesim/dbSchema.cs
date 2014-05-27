// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from tradesim on 2014-05-26 22:13:14Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace dke.tradesim.DbLinq
{
	using System;
	using System.ComponentModel;
	using System.Data;
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Diagnostics;
	
	
	public partial class TradeSim : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public TradeSim(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public TradeSim(IDbConnection connection) : 
				base(connection)
		{
			this.OnCreated();
		}
		
		public TradeSim(string connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public TradeSim(IDbConnection connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Table<AnnualReports> AnnualReports
		{
			get
			{
				return this.GetTable <AnnualReports>();
			}
		}
		
		public Table<CorporateActions> CorporateActions
		{
			get
			{
				return this.GetTable <CorporateActions>();
			}
		}
		
		public Table<EOdBars> EOdBars
		{
			get
			{
				return this.GetTable <EOdBars>();
			}
		}
		
		public Table<Exchanges> Exchanges
		{
			get
			{
				return this.GetTable <Exchanges>();
			}
		}
		
		public Table<Industries> Industries
		{
			get
			{
				return this.GetTable <Industries>();
			}
		}
		
		public Table<QuarterlyReports> QuarterlyReports
		{
			get
			{
				return this.GetTable <QuarterlyReports>();
			}
		}
		
		public Table<SamplingDistributions> SamplingDistributions
		{
			get
			{
				return this.GetTable <SamplingDistributions>();
			}
		}
		
		public Table<SchemaInfo> SchemaInfo
		{
			get
			{
				return this.GetTable <SchemaInfo>();
			}
		}
		
		public Table<Sectors> Sectors
		{
			get
			{
				return this.GetTable <Sectors>();
			}
		}
		
		public Table<Securities> Securities
		{
			get
			{
				return this.GetTable <Securities>();
			}
		}
		
		public Table<SecuritiesTrialSets> SecuritiesTrialSets
		{
			get
			{
				return this.GetTable <SecuritiesTrialSets>();
			}
		}
		
		public Table<Strategies> Strategies
		{
			get
			{
				return this.GetTable <Strategies>();
			}
		}
		
		public Table<Trials> Trials
		{
			get
			{
				return this.GetTable <Trials>();
			}
		}
		
		public Table<TrialSamples> TrialSamples
		{
			get
			{
				return this.GetTable <TrialSamples>();
			}
		}
		
		public Table<TrialSets> TrialSets
		{
			get
			{
				return this.GetTable <TrialSets>();
			}
		}
	}
	
	[Table(Name="public.annual_reports")]
	public partial class AnnualReports : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private byte[] _balanceSheet;
		
		private byte[] _cashFlowStatement;
		
		private long _endTime;
		
		private int _id;
		
		private byte[] _incomeStatement;
		
		private long _publicationTime;
		
		private int _securityID;
		
		private long _startTime;
		
		private EntityRef<Securities> _securities = new EntityRef<Securities>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnBalanceSheetChanged();
		
		partial void OnBalanceSheetChanging(byte[] value);
		
		partial void OnCashFlowStatementChanged();
		
		partial void OnCashFlowStatementChanging(byte[] value);
		
		partial void OnEndTimeChanged();
		
		partial void OnEndTimeChanging(long value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnIncomeStatementChanged();
		
		partial void OnIncomeStatementChanging(byte[] value);
		
		partial void OnPublicationTimeChanged();
		
		partial void OnPublicationTimeChanging(long value);
		
		partial void OnSecurityIDChanged();
		
		partial void OnSecurityIDChanging(int value);
		
		partial void OnStartTimeChanged();
		
		partial void OnStartTimeChanging(long value);
		#endregion
		
		
		public AnnualReports()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_balanceSheet", Name="balance_sheet", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] BalanceSheet
		{
			get
			{
				return this._balanceSheet;
			}
			set
			{
				if (((_balanceSheet == value) == false))
				{
					this.OnBalanceSheetChanging(value);
					this.SendPropertyChanging();
					this._balanceSheet = value;
					this.SendPropertyChanged("BalanceSheet");
					this.OnBalanceSheetChanged();
				}
			}
		}
		
		[Column(Storage="_cashFlowStatement", Name="cash_flow_statement", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] CashFlowStatement
		{
			get
			{
				return this._cashFlowStatement;
			}
			set
			{
				if (((_cashFlowStatement == value) == false))
				{
					this.OnCashFlowStatementChanging(value);
					this.SendPropertyChanging();
					this._cashFlowStatement = value;
					this.SendPropertyChanged("CashFlowStatement");
					this.OnCashFlowStatementChanged();
				}
			}
		}
		
		[Column(Storage="_endTime", Name="end_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long EndTime
		{
			get
			{
				return this._endTime;
			}
			set
			{
				if ((_endTime != value))
				{
					this.OnEndTimeChanging(value);
					this.SendPropertyChanging();
					this._endTime = value;
					this.SendPropertyChanged("EndTime");
					this.OnEndTimeChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('annual_reports_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_incomeStatement", Name="income_statement", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] IncomeStatement
		{
			get
			{
				return this._incomeStatement;
			}
			set
			{
				if (((_incomeStatement == value) == false))
				{
					this.OnIncomeStatementChanging(value);
					this.SendPropertyChanging();
					this._incomeStatement = value;
					this.SendPropertyChanged("IncomeStatement");
					this.OnIncomeStatementChanged();
				}
			}
		}
		
		[Column(Storage="_publicationTime", Name="publication_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long PublicationTime
		{
			get
			{
				return this._publicationTime;
			}
			set
			{
				if ((_publicationTime != value))
				{
					this.OnPublicationTimeChanging(value);
					this.SendPropertyChanging();
					this._publicationTime = value;
					this.SendPropertyChanged("PublicationTime");
					this.OnPublicationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_securityID", Name="security_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SecurityID
		{
			get
			{
				return this._securityID;
			}
			set
			{
				if ((_securityID != value))
				{
					this.OnSecurityIDChanging(value);
					this.SendPropertyChanging();
					this._securityID = value;
					this.SendPropertyChanged("SecurityID");
					this.OnSecurityIDChanged();
				}
			}
		}
		
		[Column(Storage="_startTime", Name="start_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long StartTime
		{
			get
			{
				return this._startTime;
			}
			set
			{
				if ((_startTime != value))
				{
					this.OnStartTimeChanging(value);
					this.SendPropertyChanging();
					this._startTime = value;
					this.SendPropertyChanged("StartTime");
					this.OnStartTimeChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_securities", OtherKey="ID", ThisKey="SecurityID", Name="annual_reports_security_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Securities Securities
		{
			get
			{
				return this._securities.Entity;
			}
			set
			{
				if (((this._securities.Entity == value) == false))
				{
					if ((this._securities.Entity != null))
					{
						Securities previousSecurities = this._securities.Entity;
						this._securities.Entity = null;
						previousSecurities.AnnualReports.Remove(this);
					}
					this._securities.Entity = value;
					if ((value != null))
					{
						value.AnnualReports.Add(this);
						_securityID = value.ID;
					}
					else
					{
						_securityID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.corporate_actions")]
	public partial class CorporateActions : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<int> _declarationDate;
		
		private int _exDate;
		
		private int _id;
		
		private decimal _number;
		
		private System.Nullable<int> _payableDate;
		
		private System.Nullable<int> _recordDate;
		
		private int _securityID;
		
		private string _type;
		
		private EntityRef<Securities> _securities = new EntityRef<Securities>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnDeclarationDateChanged();
		
		partial void OnDeclarationDateChanging(System.Nullable<int> value);
		
		partial void OnExDateChanged();
		
		partial void OnExDateChanging(int value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNumberChanged();
		
		partial void OnNumberChanging(decimal value);
		
		partial void OnPayableDateChanged();
		
		partial void OnPayableDateChanging(System.Nullable<int> value);
		
		partial void OnRecordDateChanged();
		
		partial void OnRecordDateChanging(System.Nullable<int> value);
		
		partial void OnSecurityIDChanged();
		
		partial void OnSecurityIDChanging(int value);
		
		partial void OnTypeChanged();
		
		partial void OnTypeChanging(string value);
		#endregion
		
		
		public CorporateActions()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_declarationDate", Name="declaration_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> DeclarationDate
		{
			get
			{
				return this._declarationDate;
			}
			set
			{
				if ((_declarationDate != value))
				{
					this.OnDeclarationDateChanging(value);
					this.SendPropertyChanging();
					this._declarationDate = value;
					this.SendPropertyChanged("DeclarationDate");
					this.OnDeclarationDateChanged();
				}
			}
		}
		
		[Column(Storage="_exDate", Name="ex_date", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ExDate
		{
			get
			{
				return this._exDate;
			}
			set
			{
				if ((_exDate != value))
				{
					this.OnExDateChanging(value);
					this.SendPropertyChanging();
					this._exDate = value;
					this.SendPropertyChanged("ExDate");
					this.OnExDateChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('corporate_actions_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_number", Name="number", DbType="numeric(30,15)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal Number
		{
			get
			{
				return this._number;
			}
			set
			{
				if ((_number != value))
				{
					this.OnNumberChanging(value);
					this.SendPropertyChanging();
					this._number = value;
					this.SendPropertyChanged("Number");
					this.OnNumberChanged();
				}
			}
		}
		
		[Column(Storage="_payableDate", Name="payable_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> PayableDate
		{
			get
			{
				return this._payableDate;
			}
			set
			{
				if ((_payableDate != value))
				{
					this.OnPayableDateChanging(value);
					this.SendPropertyChanging();
					this._payableDate = value;
					this.SendPropertyChanged("PayableDate");
					this.OnPayableDateChanged();
				}
			}
		}
		
		[Column(Storage="_recordDate", Name="record_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> RecordDate
		{
			get
			{
				return this._recordDate;
			}
			set
			{
				if ((_recordDate != value))
				{
					this.OnRecordDateChanging(value);
					this.SendPropertyChanging();
					this._recordDate = value;
					this.SendPropertyChanged("RecordDate");
					this.OnRecordDateChanged();
				}
			}
		}
		
		[Column(Storage="_securityID", Name="security_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SecurityID
		{
			get
			{
				return this._securityID;
			}
			set
			{
				if ((_securityID != value))
				{
					this.OnSecurityIDChanging(value);
					this.SendPropertyChanging();
					this._securityID = value;
					this.SendPropertyChanged("SecurityID");
					this.OnSecurityIDChanged();
				}
			}
		}
		
		[Column(Storage="_type", Name="type", DbType="character varying(30)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Type
		{
			get
			{
				return this._type;
			}
			set
			{
				if (((_type == value) == false))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_securities", OtherKey="ID", ThisKey="SecurityID", Name="corporate_actions_security_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Securities Securities
		{
			get
			{
				return this._securities.Entity;
			}
			set
			{
				if (((this._securities.Entity == value) == false))
				{
					if ((this._securities.Entity != null))
					{
						Securities previousSecurities = this._securities.Entity;
						this._securities.Entity = null;
						previousSecurities.CorporateActions.Remove(this);
					}
					this._securities.Entity = value;
					if ((value != null))
					{
						value.CorporateActions.Add(this);
						_securityID = value.ID;
					}
					else
					{
						_securityID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.eod_bars")]
	public partial class EOdBars : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private decimal _close;
		
		private long _endTime;
		
		private decimal _high;
		
		private int _id;
		
		private decimal _low;
		
		private decimal _open;
		
		private int _securityID;
		
		private long _startTime;
		
		private long _volume;
		
		private EntityRef<Securities> _securities = new EntityRef<Securities>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCloseChanged();
		
		partial void OnCloseChanging(decimal value);
		
		partial void OnEndTimeChanged();
		
		partial void OnEndTimeChanging(long value);
		
		partial void OnHighChanged();
		
		partial void OnHighChanging(decimal value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnLowChanged();
		
		partial void OnLowChanging(decimal value);
		
		partial void OnOpenChanged();
		
		partial void OnOpenChanging(decimal value);
		
		partial void OnSecurityIDChanged();
		
		partial void OnSecurityIDChanging(int value);
		
		partial void OnStartTimeChanged();
		
		partial void OnStartTimeChanging(long value);
		
		partial void OnVolumeChanged();
		
		partial void OnVolumeChanging(long value);
		#endregion
		
		
		public EOdBars()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_close", Name="close", DbType="numeric(12,2)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal Close
		{
			get
			{
				return this._close;
			}
			set
			{
				if ((_close != value))
				{
					this.OnCloseChanging(value);
					this.SendPropertyChanging();
					this._close = value;
					this.SendPropertyChanged("Close");
					this.OnCloseChanged();
				}
			}
		}
		
		[Column(Storage="_endTime", Name="end_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long EndTime
		{
			get
			{
				return this._endTime;
			}
			set
			{
				if ((_endTime != value))
				{
					this.OnEndTimeChanging(value);
					this.SendPropertyChanging();
					this._endTime = value;
					this.SendPropertyChanged("EndTime");
					this.OnEndTimeChanged();
				}
			}
		}
		
		[Column(Storage="_high", Name="high", DbType="numeric(12,2)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal High
		{
			get
			{
				return this._high;
			}
			set
			{
				if ((_high != value))
				{
					this.OnHighChanging(value);
					this.SendPropertyChanging();
					this._high = value;
					this.SendPropertyChanged("High");
					this.OnHighChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('eod_bars_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_low", Name="low", DbType="numeric(12,2)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal Low
		{
			get
			{
				return this._low;
			}
			set
			{
				if ((_low != value))
				{
					this.OnLowChanging(value);
					this.SendPropertyChanging();
					this._low = value;
					this.SendPropertyChanged("Low");
					this.OnLowChanged();
				}
			}
		}
		
		[Column(Storage="_open", Name="open", DbType="numeric(12,2)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal Open
		{
			get
			{
				return this._open;
			}
			set
			{
				if ((_open != value))
				{
					this.OnOpenChanging(value);
					this.SendPropertyChanging();
					this._open = value;
					this.SendPropertyChanged("Open");
					this.OnOpenChanged();
				}
			}
		}
		
		[Column(Storage="_securityID", Name="security_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SecurityID
		{
			get
			{
				return this._securityID;
			}
			set
			{
				if ((_securityID != value))
				{
					this.OnSecurityIDChanging(value);
					this.SendPropertyChanging();
					this._securityID = value;
					this.SendPropertyChanged("SecurityID");
					this.OnSecurityIDChanged();
				}
			}
		}
		
		[Column(Storage="_startTime", Name="start_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long StartTime
		{
			get
			{
				return this._startTime;
			}
			set
			{
				if ((_startTime != value))
				{
					this.OnStartTimeChanging(value);
					this.SendPropertyChanging();
					this._startTime = value;
					this.SendPropertyChanged("StartTime");
					this.OnStartTimeChanged();
				}
			}
		}
		
		[Column(Storage="_volume", Name="volume", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long Volume
		{
			get
			{
				return this._volume;
			}
			set
			{
				if ((_volume != value))
				{
					this.OnVolumeChanging(value);
					this.SendPropertyChanging();
					this._volume = value;
					this.SendPropertyChanged("Volume");
					this.OnVolumeChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_securities", OtherKey="ID", ThisKey="SecurityID", Name="eod_bars_security_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Securities Securities
		{
			get
			{
				return this._securities.Entity;
			}
			set
			{
				if (((this._securities.Entity == value) == false))
				{
					if ((this._securities.Entity != null))
					{
						Securities previousSecurities = this._securities.Entity;
						this._securities.Entity = null;
						previousSecurities.EOdBars.Remove(this);
					}
					this._securities.Entity = value;
					if ((value != null))
					{
						value.EOdBars.Add(this);
						_securityID = value.ID;
					}
					else
					{
						_securityID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.exchanges")]
	public partial class Exchanges : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _label;
		
		private string _name;
		
		private EntitySet<Securities> _securities;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnLabelChanged();
		
		partial void OnLabelChanging(string value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		#endregion
		
		
		public Exchanges()
		{
			_securities = new EntitySet<Securities>(new Action<Securities>(this.Securities_Attach), new Action<Securities>(this.Securities_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('exchanges_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_label", Name="label", DbType="character varying(50)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Label
		{
			get
			{
				return this._label;
			}
			set
			{
				if (((_label == value) == false))
				{
					this.OnLabelChanging(value);
					this.SendPropertyChanging();
					this._label = value;
					this.SendPropertyChanged("Label");
					this.OnLabelChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="character varying(255)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) == false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_securities", OtherKey="ExchangeID", ThisKey="ID", Name="securities_exchange_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<Securities> Securities
		{
			get
			{
				return this._securities;
			}
			set
			{
				this._securities = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void Securities_Attach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Exchanges = this;
		}
		
		private void Securities_Detach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Exchanges = null;
		}
		#endregion
	}
	
	[Table(Name="public.industries")]
	public partial class Industries : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _name;
		
		private EntitySet<Securities> _securities;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		#endregion
		
		
		public Industries()
		{
			_securities = new EntitySet<Securities>(new Action<Securities>(this.Securities_Attach), new Action<Securities>(this.Securities_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('industries_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="character varying(255)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) == false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_securities", OtherKey="IndustryID", ThisKey="ID", Name="securities_industry_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<Securities> Securities
		{
			get
			{
				return this._securities;
			}
			set
			{
				this._securities = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void Securities_Attach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Industries = this;
		}
		
		private void Securities_Detach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Industries = null;
		}
		#endregion
	}
	
	[Table(Name="public.quarterly_reports")]
	public partial class QuarterlyReports : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private byte[] _balanceSheet;
		
		private byte[] _cashFlowStatement;
		
		private long _endTime;
		
		private int _id;
		
		private byte[] _incomeStatement;
		
		private long _publicationTime;
		
		private int _securityID;
		
		private long _startTime;
		
		private EntityRef<Securities> _securities = new EntityRef<Securities>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnBalanceSheetChanged();
		
		partial void OnBalanceSheetChanging(byte[] value);
		
		partial void OnCashFlowStatementChanged();
		
		partial void OnCashFlowStatementChanging(byte[] value);
		
		partial void OnEndTimeChanged();
		
		partial void OnEndTimeChanging(long value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnIncomeStatementChanged();
		
		partial void OnIncomeStatementChanging(byte[] value);
		
		partial void OnPublicationTimeChanged();
		
		partial void OnPublicationTimeChanging(long value);
		
		partial void OnSecurityIDChanged();
		
		partial void OnSecurityIDChanging(int value);
		
		partial void OnStartTimeChanged();
		
		partial void OnStartTimeChanging(long value);
		#endregion
		
		
		public QuarterlyReports()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_balanceSheet", Name="balance_sheet", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] BalanceSheet
		{
			get
			{
				return this._balanceSheet;
			}
			set
			{
				if (((_balanceSheet == value) == false))
				{
					this.OnBalanceSheetChanging(value);
					this.SendPropertyChanging();
					this._balanceSheet = value;
					this.SendPropertyChanged("BalanceSheet");
					this.OnBalanceSheetChanged();
				}
			}
		}
		
		[Column(Storage="_cashFlowStatement", Name="cash_flow_statement", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] CashFlowStatement
		{
			get
			{
				return this._cashFlowStatement;
			}
			set
			{
				if (((_cashFlowStatement == value) == false))
				{
					this.OnCashFlowStatementChanging(value);
					this.SendPropertyChanging();
					this._cashFlowStatement = value;
					this.SendPropertyChanged("CashFlowStatement");
					this.OnCashFlowStatementChanged();
				}
			}
		}
		
		[Column(Storage="_endTime", Name="end_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long EndTime
		{
			get
			{
				return this._endTime;
			}
			set
			{
				if ((_endTime != value))
				{
					this.OnEndTimeChanging(value);
					this.SendPropertyChanging();
					this._endTime = value;
					this.SendPropertyChanged("EndTime");
					this.OnEndTimeChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('quarterly_reports_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_incomeStatement", Name="income_statement", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] IncomeStatement
		{
			get
			{
				return this._incomeStatement;
			}
			set
			{
				if (((_incomeStatement == value) == false))
				{
					this.OnIncomeStatementChanging(value);
					this.SendPropertyChanging();
					this._incomeStatement = value;
					this.SendPropertyChanged("IncomeStatement");
					this.OnIncomeStatementChanged();
				}
			}
		}
		
		[Column(Storage="_publicationTime", Name="publication_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long PublicationTime
		{
			get
			{
				return this._publicationTime;
			}
			set
			{
				if ((_publicationTime != value))
				{
					this.OnPublicationTimeChanging(value);
					this.SendPropertyChanging();
					this._publicationTime = value;
					this.SendPropertyChanged("PublicationTime");
					this.OnPublicationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_securityID", Name="security_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SecurityID
		{
			get
			{
				return this._securityID;
			}
			set
			{
				if ((_securityID != value))
				{
					this.OnSecurityIDChanging(value);
					this.SendPropertyChanging();
					this._securityID = value;
					this.SendPropertyChanged("SecurityID");
					this.OnSecurityIDChanged();
				}
			}
		}
		
		[Column(Storage="_startTime", Name="start_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long StartTime
		{
			get
			{
				return this._startTime;
			}
			set
			{
				if ((_startTime != value))
				{
					this.OnStartTimeChanging(value);
					this.SendPropertyChanging();
					this._startTime = value;
					this.SendPropertyChanged("StartTime");
					this.OnStartTimeChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_securities", OtherKey="ID", ThisKey="SecurityID", Name="quarterly_reports_security_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Securities Securities
		{
			get
			{
				return this._securities.Entity;
			}
			set
			{
				if (((this._securities.Entity == value) == false))
				{
					if ((this._securities.Entity != null))
					{
						Securities previousSecurities = this._securities.Entity;
						this._securities.Entity = null;
						previousSecurities.QuarterlyReports.Remove(this);
					}
					this._securities.Entity = value;
					if ((value != null))
					{
						value.QuarterlyReports.Add(this);
						_securityID = value.ID;
					}
					else
					{
						_securityID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.sampling_distributions")]
	public partial class SamplingDistributions : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<decimal> _average;
		
		private byte[] _distribution;
		
		private int _id;
		
		private System.Nullable<decimal> _maX;
		
		private System.Nullable<decimal> _miN;
		
		private System.Nullable<int> _n;
		
		private System.Nullable<decimal> _percentile10;
		
		private System.Nullable<decimal> _percentile20;
		
		private System.Nullable<decimal> _percentile30;
		
		private System.Nullable<decimal> _percentile40;
		
		private System.Nullable<decimal> _percentile50;
		
		private System.Nullable<decimal> _percentile60;
		
		private System.Nullable<decimal> _percentile70;
		
		private System.Nullable<decimal> _percentile80;
		
		private System.Nullable<decimal> _percentile90;
		
		private string _sampleStatistic;
		
		private int _trialSampleID;
		
		private EntityRef<TrialSamples> _trialSamples = new EntityRef<TrialSamples>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAverageChanged();
		
		partial void OnAverageChanging(System.Nullable<decimal> value);
		
		partial void OnDistributionChanged();
		
		partial void OnDistributionChanging(byte[] value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnMaXChanged();
		
		partial void OnMaXChanging(System.Nullable<decimal> value);
		
		partial void OnMInChanged();
		
		partial void OnMInChanging(System.Nullable<decimal> value);
		
		partial void OnNChanged();
		
		partial void OnNChanging(System.Nullable<int> value);
		
		partial void OnPercentile10Changed();
		
		partial void OnPercentile10Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile20Changed();
		
		partial void OnPercentile20Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile30Changed();
		
		partial void OnPercentile30Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile40Changed();
		
		partial void OnPercentile40Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile50Changed();
		
		partial void OnPercentile50Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile60Changed();
		
		partial void OnPercentile60Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile70Changed();
		
		partial void OnPercentile70Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile80Changed();
		
		partial void OnPercentile80Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile90Changed();
		
		partial void OnPercentile90Changing(System.Nullable<decimal> value);
		
		partial void OnSampleStatisticChanged();
		
		partial void OnSampleStatisticChanging(string value);
		
		partial void OnTrialSampleIDChanged();
		
		partial void OnTrialSampleIDChanging(int value);
		#endregion
		
		
		public SamplingDistributions()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_average", Name="average", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Average
		{
			get
			{
				return this._average;
			}
			set
			{
				if ((_average != value))
				{
					this.OnAverageChanging(value);
					this.SendPropertyChanging();
					this._average = value;
					this.SendPropertyChanged("Average");
					this.OnAverageChanged();
				}
			}
		}
		
		[Column(Storage="_distribution", Name="distribution", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] Distribution
		{
			get
			{
				return this._distribution;
			}
			set
			{
				if (((_distribution == value) == false))
				{
					this.OnDistributionChanging(value);
					this.SendPropertyChanging();
					this._distribution = value;
					this.SendPropertyChanged("Distribution");
					this.OnDistributionChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('sampling_distributions_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_maX", Name="max", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> MaX
		{
			get
			{
				return this._maX;
			}
			set
			{
				if ((_maX != value))
				{
					this.OnMaXChanging(value);
					this.SendPropertyChanging();
					this._maX = value;
					this.SendPropertyChanged("MaX");
					this.OnMaXChanged();
				}
			}
		}
		
		[Column(Storage="_miN", Name="min", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> MIn
		{
			get
			{
				return this._miN;
			}
			set
			{
				if ((_miN != value))
				{
					this.OnMInChanging(value);
					this.SendPropertyChanging();
					this._miN = value;
					this.SendPropertyChanged("MIn");
					this.OnMInChanged();
				}
			}
		}
		
		[Column(Storage="_n", Name="n", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> N
		{
			get
			{
				return this._n;
			}
			set
			{
				if ((_n != value))
				{
					this.OnNChanging(value);
					this.SendPropertyChanging();
					this._n = value;
					this.SendPropertyChanged("N");
					this.OnNChanged();
				}
			}
		}
		
		[Column(Storage="_percentile10", Name="percentile_10", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile10
		{
			get
			{
				return this._percentile10;
			}
			set
			{
				if ((_percentile10 != value))
				{
					this.OnPercentile10Changing(value);
					this.SendPropertyChanging();
					this._percentile10 = value;
					this.SendPropertyChanged("Percentile10");
					this.OnPercentile10Changed();
				}
			}
		}
		
		[Column(Storage="_percentile20", Name="percentile_20", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile20
		{
			get
			{
				return this._percentile20;
			}
			set
			{
				if ((_percentile20 != value))
				{
					this.OnPercentile20Changing(value);
					this.SendPropertyChanging();
					this._percentile20 = value;
					this.SendPropertyChanged("Percentile20");
					this.OnPercentile20Changed();
				}
			}
		}
		
		[Column(Storage="_percentile30", Name="percentile_30", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile30
		{
			get
			{
				return this._percentile30;
			}
			set
			{
				if ((_percentile30 != value))
				{
					this.OnPercentile30Changing(value);
					this.SendPropertyChanging();
					this._percentile30 = value;
					this.SendPropertyChanged("Percentile30");
					this.OnPercentile30Changed();
				}
			}
		}
		
		[Column(Storage="_percentile40", Name="percentile_40", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile40
		{
			get
			{
				return this._percentile40;
			}
			set
			{
				if ((_percentile40 != value))
				{
					this.OnPercentile40Changing(value);
					this.SendPropertyChanging();
					this._percentile40 = value;
					this.SendPropertyChanged("Percentile40");
					this.OnPercentile40Changed();
				}
			}
		}
		
		[Column(Storage="_percentile50", Name="percentile_50", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile50
		{
			get
			{
				return this._percentile50;
			}
			set
			{
				if ((_percentile50 != value))
				{
					this.OnPercentile50Changing(value);
					this.SendPropertyChanging();
					this._percentile50 = value;
					this.SendPropertyChanged("Percentile50");
					this.OnPercentile50Changed();
				}
			}
		}
		
		[Column(Storage="_percentile60", Name="percentile_60", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile60
		{
			get
			{
				return this._percentile60;
			}
			set
			{
				if ((_percentile60 != value))
				{
					this.OnPercentile60Changing(value);
					this.SendPropertyChanging();
					this._percentile60 = value;
					this.SendPropertyChanged("Percentile60");
					this.OnPercentile60Changed();
				}
			}
		}
		
		[Column(Storage="_percentile70", Name="percentile_70", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile70
		{
			get
			{
				return this._percentile70;
			}
			set
			{
				if ((_percentile70 != value))
				{
					this.OnPercentile70Changing(value);
					this.SendPropertyChanging();
					this._percentile70 = value;
					this.SendPropertyChanged("Percentile70");
					this.OnPercentile70Changed();
				}
			}
		}
		
		[Column(Storage="_percentile80", Name="percentile_80", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile80
		{
			get
			{
				return this._percentile80;
			}
			set
			{
				if ((_percentile80 != value))
				{
					this.OnPercentile80Changing(value);
					this.SendPropertyChanging();
					this._percentile80 = value;
					this.SendPropertyChanged("Percentile80");
					this.OnPercentile80Changed();
				}
			}
		}
		
		[Column(Storage="_percentile90", Name="percentile_90", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile90
		{
			get
			{
				return this._percentile90;
			}
			set
			{
				if ((_percentile90 != value))
				{
					this.OnPercentile90Changing(value);
					this.SendPropertyChanging();
					this._percentile90 = value;
					this.SendPropertyChanged("Percentile90");
					this.OnPercentile90Changed();
				}
			}
		}
		
		[Column(Storage="_sampleStatistic", Name="sample_statistic", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string SampleStatistic
		{
			get
			{
				return this._sampleStatistic;
			}
			set
			{
				if (((_sampleStatistic == value) == false))
				{
					this.OnSampleStatisticChanging(value);
					this.SendPropertyChanging();
					this._sampleStatistic = value;
					this.SendPropertyChanged("SampleStatistic");
					this.OnSampleStatisticChanged();
				}
			}
		}
		
		[Column(Storage="_trialSampleID", Name="trial_sample_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TrialSampleID
		{
			get
			{
				return this._trialSampleID;
			}
			set
			{
				if ((_trialSampleID != value))
				{
					this.OnTrialSampleIDChanging(value);
					this.SendPropertyChanging();
					this._trialSampleID = value;
					this.SendPropertyChanged("TrialSampleID");
					this.OnTrialSampleIDChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_trialSamples", OtherKey="ID", ThisKey="TrialSampleID", Name="sampling_distributions_trial_sample_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public TrialSamples TrialSamples
		{
			get
			{
				return this._trialSamples.Entity;
			}
			set
			{
				if (((this._trialSamples.Entity == value) == false))
				{
					if ((this._trialSamples.Entity != null))
					{
						TrialSamples previousTrialSamples = this._trialSamples.Entity;
						this._trialSamples.Entity = null;
						previousTrialSamples.SamplingDistributions.Remove(this);
					}
					this._trialSamples.Entity = value;
					if ((value != null))
					{
						value.SamplingDistributions.Add(this);
						_trialSampleID = value.ID;
					}
					else
					{
						_trialSampleID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.schema_info")]
	public partial class SchemaInfo
	{
		
		private int _version;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnVersionChanged();
		
		partial void OnVersionChanging(int value);
		#endregion
		
		
		public SchemaInfo()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_version", Name="version", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Version
		{
			get
			{
				return this._version;
			}
			set
			{
				if ((_version != value))
				{
					this.OnVersionChanging(value);
					this._version = value;
					this.OnVersionChanged();
				}
			}
		}
	}
	
	[Table(Name="public.sectors")]
	public partial class Sectors : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _name;
		
		private EntitySet<Securities> _securities;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		#endregion
		
		
		public Sectors()
		{
			_securities = new EntitySet<Securities>(new Action<Securities>(this.Securities_Attach), new Action<Securities>(this.Securities_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('sectors_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="character varying(255)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) == false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_securities", OtherKey="SectorID", ThisKey="ID", Name="securities_sector_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<Securities> Securities
		{
			get
			{
				return this._securities;
			}
			set
			{
				this._securities = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void Securities_Attach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Sectors = this;
		}
		
		private void Securities_Detach(Securities entity)
		{
			this.SendPropertyChanging();
			entity.Sectors = null;
		}
		#endregion
	}
	
	[Table(Name="public.securities")]
	public partial class Securities : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<bool> _active;
		
		private string _bbGcID;
		
		private string _bbGiD;
		
		private System.Nullable<int> _cik;
		
		private System.Nullable<int> _endDate;
		
		private System.Nullable<int> _exchangeID;
		
		private System.Nullable<int> _fiscalYearEndDate;
		
		private int _id;
		
		private System.Nullable<int> _industryID;
		
		private string _name;
		
		private System.Nullable<int> _sectorID;
		
		private System.Nullable<int> _startDate;
		
		private string _symbol;
		
		private string _type;
		
		private EntitySet<EOdBars> _eoDBars;
		
		private EntitySet<CorporateActions> _corporateActions;
		
		private EntitySet<QuarterlyReports> _quarterlyReports;
		
		private EntitySet<AnnualReports> _annualReports;
		
		private EntitySet<SecuritiesTrialSets> _securitiesTrialSets;
		
		private EntityRef<Exchanges> _exchanges = new EntityRef<Exchanges>();
		
		private EntityRef<Industries> _industries = new EntityRef<Industries>();
		
		private EntityRef<Sectors> _sectors = new EntityRef<Sectors>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnActiveChanged();
		
		partial void OnActiveChanging(System.Nullable<bool> value);
		
		partial void OnBbGCidChanged();
		
		partial void OnBbGCidChanging(string value);
		
		partial void OnBbGiDChanged();
		
		partial void OnBbGiDChanging(string value);
		
		partial void OnCIKChanged();
		
		partial void OnCIKChanging(System.Nullable<int> value);
		
		partial void OnEndDateChanged();
		
		partial void OnEndDateChanging(System.Nullable<int> value);
		
		partial void OnExchangeIDChanged();
		
		partial void OnExchangeIDChanging(System.Nullable<int> value);
		
		partial void OnFiscalYearEndDateChanged();
		
		partial void OnFiscalYearEndDateChanging(System.Nullable<int> value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnIndustryIDChanged();
		
		partial void OnIndustryIDChanging(System.Nullable<int> value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		
		partial void OnSectorIDChanged();
		
		partial void OnSectorIDChanging(System.Nullable<int> value);
		
		partial void OnStartDateChanged();
		
		partial void OnStartDateChanging(System.Nullable<int> value);
		
		partial void OnSymbolChanged();
		
		partial void OnSymbolChanging(string value);
		
		partial void OnTypeChanged();
		
		partial void OnTypeChanging(string value);
		#endregion
		
		
		public Securities()
		{
			_eoDBars = new EntitySet<EOdBars>(new Action<EOdBars>(this.EOdBars_Attach), new Action<EOdBars>(this.EOdBars_Detach));
			_corporateActions = new EntitySet<CorporateActions>(new Action<CorporateActions>(this.CorporateActions_Attach), new Action<CorporateActions>(this.CorporateActions_Detach));
			_quarterlyReports = new EntitySet<QuarterlyReports>(new Action<QuarterlyReports>(this.QuarterlyReports_Attach), new Action<QuarterlyReports>(this.QuarterlyReports_Detach));
			_annualReports = new EntitySet<AnnualReports>(new Action<AnnualReports>(this.AnnualReports_Attach), new Action<AnnualReports>(this.AnnualReports_Detach));
			_securitiesTrialSets = new EntitySet<SecuritiesTrialSets>(new Action<SecuritiesTrialSets>(this.SecuritiesTrialSets_Attach), new Action<SecuritiesTrialSets>(this.SecuritiesTrialSets_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_active", Name="active", DbType="boolean", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<bool> Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if ((_active != value))
				{
					this.OnActiveChanging(value);
					this.SendPropertyChanging();
					this._active = value;
					this.SendPropertyChanged("Active");
					this.OnActiveChanged();
				}
			}
		}
		
		[Column(Storage="_bbGcID", Name="bb_gcid", DbType="character varying(12)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string BbGCid
		{
			get
			{
				return this._bbGcID;
			}
			set
			{
				if (((_bbGcID == value) == false))
				{
					this.OnBbGCidChanging(value);
					this.SendPropertyChanging();
					this._bbGcID = value;
					this.SendPropertyChanged("BbGCid");
					this.OnBbGCidChanged();
				}
			}
		}
		
		[Column(Storage="_bbGiD", Name="bb_gid", DbType="character varying(12)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string BbGiD
		{
			get
			{
				return this._bbGiD;
			}
			set
			{
				if (((_bbGiD == value) == false))
				{
					this.OnBbGiDChanging(value);
					this.SendPropertyChanging();
					this._bbGiD = value;
					this.SendPropertyChanged("BbGiD");
					this.OnBbGiDChanged();
				}
			}
		}
		
		[Column(Storage="_cik", Name="cik", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> CIK
		{
			get
			{
				return this._cik;
			}
			set
			{
				if ((_cik != value))
				{
					this.OnCIKChanging(value);
					this.SendPropertyChanging();
					this._cik = value;
					this.SendPropertyChanged("CIK");
					this.OnCIKChanged();
				}
			}
		}
		
		[Column(Storage="_endDate", Name="end_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> EndDate
		{
			get
			{
				return this._endDate;
			}
			set
			{
				if ((_endDate != value))
				{
					this.OnEndDateChanging(value);
					this.SendPropertyChanging();
					this._endDate = value;
					this.SendPropertyChanged("EndDate");
					this.OnEndDateChanged();
				}
			}
		}
		
		[Column(Storage="_exchangeID", Name="exchange_id", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> ExchangeID
		{
			get
			{
				return this._exchangeID;
			}
			set
			{
				if ((_exchangeID != value))
				{
					this.OnExchangeIDChanging(value);
					this.SendPropertyChanging();
					this._exchangeID = value;
					this.SendPropertyChanged("ExchangeID");
					this.OnExchangeIDChanged();
				}
			}
		}
		
		[Column(Storage="_fiscalYearEndDate", Name="fiscal_year_end_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> FiscalYearEndDate
		{
			get
			{
				return this._fiscalYearEndDate;
			}
			set
			{
				if ((_fiscalYearEndDate != value))
				{
					this.OnFiscalYearEndDateChanging(value);
					this.SendPropertyChanging();
					this._fiscalYearEndDate = value;
					this.SendPropertyChanged("FiscalYearEndDate");
					this.OnFiscalYearEndDateChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('securities_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_industryID", Name="industry_id", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> IndustryID
		{
			get
			{
				return this._industryID;
			}
			set
			{
				if ((_industryID != value))
				{
					this.OnIndustryIDChanging(value);
					this.SendPropertyChanging();
					this._industryID = value;
					this.SendPropertyChanged("IndustryID");
					this.OnIndustryIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="character varying(255)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) == false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[Column(Storage="_sectorID", Name="sector_id", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> SectorID
		{
			get
			{
				return this._sectorID;
			}
			set
			{
				if ((_sectorID != value))
				{
					this.OnSectorIDChanging(value);
					this.SendPropertyChanging();
					this._sectorID = value;
					this.SendPropertyChanged("SectorID");
					this.OnSectorIDChanged();
				}
			}
		}
		
		[Column(Storage="_startDate", Name="start_date", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> StartDate
		{
			get
			{
				return this._startDate;
			}
			set
			{
				if ((_startDate != value))
				{
					this.OnStartDateChanging(value);
					this.SendPropertyChanging();
					this._startDate = value;
					this.SendPropertyChanged("StartDate");
					this.OnStartDateChanged();
				}
			}
		}
		
		[Column(Storage="_symbol", Name="symbol", DbType="character varying(15)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Symbol
		{
			get
			{
				return this._symbol;
			}
			set
			{
				if (((_symbol == value) == false))
				{
					this.OnSymbolChanging(value);
					this.SendPropertyChanging();
					this._symbol = value;
					this.SendPropertyChanged("Symbol");
					this.OnSymbolChanged();
				}
			}
		}
		
		[Column(Storage="_type", Name="type", DbType="character varying(30)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Type
		{
			get
			{
				return this._type;
			}
			set
			{
				if (((_type == value) == false))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_eoDBars", OtherKey="SecurityID", ThisKey="ID", Name="eod_bars_security_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<EOdBars> EOdBars
		{
			get
			{
				return this._eoDBars;
			}
			set
			{
				this._eoDBars = value;
			}
		}
		
		[Association(Storage="_corporateActions", OtherKey="SecurityID", ThisKey="ID", Name="corporate_actions_security_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<CorporateActions> CorporateActions
		{
			get
			{
				return this._corporateActions;
			}
			set
			{
				this._corporateActions = value;
			}
		}
		
		[Association(Storage="_quarterlyReports", OtherKey="SecurityID", ThisKey="ID", Name="quarterly_reports_security_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<QuarterlyReports> QuarterlyReports
		{
			get
			{
				return this._quarterlyReports;
			}
			set
			{
				this._quarterlyReports = value;
			}
		}
		
		[Association(Storage="_annualReports", OtherKey="SecurityID", ThisKey="ID", Name="annual_reports_security_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<AnnualReports> AnnualReports
		{
			get
			{
				return this._annualReports;
			}
			set
			{
				this._annualReports = value;
			}
		}
		
		[Association(Storage="_securitiesTrialSets", OtherKey="SecurityID", ThisKey="ID", Name="securities_trial_sets_security_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<SecuritiesTrialSets> SecuritiesTrialSets
		{
			get
			{
				return this._securitiesTrialSets;
			}
			set
			{
				this._securitiesTrialSets = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_exchanges", OtherKey="ID", ThisKey="ExchangeID", Name="securities_exchange_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Exchanges Exchanges
		{
			get
			{
				return this._exchanges.Entity;
			}
			set
			{
				if (((this._exchanges.Entity == value) == false))
				{
					if ((this._exchanges.Entity != null))
					{
						Exchanges previousExchanges = this._exchanges.Entity;
						this._exchanges.Entity = null;
						previousExchanges.Securities.Remove(this);
					}
					this._exchanges.Entity = value;
					if ((value != null))
					{
						value.Securities.Add(this);
						_exchangeID = value.ID;
					}
					else
					{
						_exchangeID = null;
					}
				}
			}
		}
		
		[Association(Storage="_industries", OtherKey="ID", ThisKey="IndustryID", Name="securities_industry_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Industries Industries
		{
			get
			{
				return this._industries.Entity;
			}
			set
			{
				if (((this._industries.Entity == value) == false))
				{
					if ((this._industries.Entity != null))
					{
						Industries previousIndustries = this._industries.Entity;
						this._industries.Entity = null;
						previousIndustries.Securities.Remove(this);
					}
					this._industries.Entity = value;
					if ((value != null))
					{
						value.Securities.Add(this);
						_industryID = value.ID;
					}
					else
					{
						_industryID = null;
					}
				}
			}
		}
		
		[Association(Storage="_sectors", OtherKey="ID", ThisKey="SectorID", Name="securities_sector_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Sectors Sectors
		{
			get
			{
				return this._sectors.Entity;
			}
			set
			{
				if (((this._sectors.Entity == value) == false))
				{
					if ((this._sectors.Entity != null))
					{
						Sectors previousSectors = this._sectors.Entity;
						this._sectors.Entity = null;
						previousSectors.Securities.Remove(this);
					}
					this._sectors.Entity = value;
					if ((value != null))
					{
						value.Securities.Add(this);
						_sectorID = value.ID;
					}
					else
					{
						_sectorID = null;
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void EOdBars_Attach(EOdBars entity)
		{
			this.SendPropertyChanging();
			entity.Securities = this;
		}
		
		private void EOdBars_Detach(EOdBars entity)
		{
			this.SendPropertyChanging();
			entity.Securities = null;
		}
		
		private void CorporateActions_Attach(CorporateActions entity)
		{
			this.SendPropertyChanging();
			entity.Securities = this;
		}
		
		private void CorporateActions_Detach(CorporateActions entity)
		{
			this.SendPropertyChanging();
			entity.Securities = null;
		}
		
		private void QuarterlyReports_Attach(QuarterlyReports entity)
		{
			this.SendPropertyChanging();
			entity.Securities = this;
		}
		
		private void QuarterlyReports_Detach(QuarterlyReports entity)
		{
			this.SendPropertyChanging();
			entity.Securities = null;
		}
		
		private void AnnualReports_Attach(AnnualReports entity)
		{
			this.SendPropertyChanging();
			entity.Securities = this;
		}
		
		private void AnnualReports_Detach(AnnualReports entity)
		{
			this.SendPropertyChanging();
			entity.Securities = null;
		}
		
		private void SecuritiesTrialSets_Attach(SecuritiesTrialSets entity)
		{
			this.SendPropertyChanging();
			entity.Securities = this;
		}
		
		private void SecuritiesTrialSets_Detach(SecuritiesTrialSets entity)
		{
			this.SendPropertyChanging();
			entity.Securities = null;
		}
		#endregion
	}
	
	[Table(Name="public.securities_trial_sets")]
	public partial class SecuritiesTrialSets : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _securityID;
		
		private int _trialSetID;
		
		private EntityRef<Securities> _securities = new EntityRef<Securities>();
		
		private EntityRef<TrialSets> _trialSets = new EntityRef<TrialSets>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnSecurityIDChanged();
		
		partial void OnSecurityIDChanging(int value);
		
		partial void OnTrialSetIDChanged();
		
		partial void OnTrialSetIDChanging(int value);
		#endregion
		
		
		public SecuritiesTrialSets()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_securityID", Name="security_id", DbType="integer(32,0)", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SecurityID
		{
			get
			{
				return this._securityID;
			}
			set
			{
				if ((_securityID != value))
				{
					this.OnSecurityIDChanging(value);
					this.SendPropertyChanging();
					this._securityID = value;
					this.SendPropertyChanged("SecurityID");
					this.OnSecurityIDChanged();
				}
			}
		}
		
		[Column(Storage="_trialSetID", Name="trial_set_id", DbType="integer(32,0)", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TrialSetID
		{
			get
			{
				return this._trialSetID;
			}
			set
			{
				if ((_trialSetID != value))
				{
					this.OnTrialSetIDChanging(value);
					this.SendPropertyChanging();
					this._trialSetID = value;
					this.SendPropertyChanged("TrialSetID");
					this.OnTrialSetIDChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_securities", OtherKey="ID", ThisKey="SecurityID", Name="securities_trial_sets_security_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Securities Securities
		{
			get
			{
				return this._securities.Entity;
			}
			set
			{
				if (((this._securities.Entity == value) == false))
				{
					if ((this._securities.Entity != null))
					{
						Securities previousSecurities = this._securities.Entity;
						this._securities.Entity = null;
						previousSecurities.SecuritiesTrialSets.Remove(this);
					}
					this._securities.Entity = value;
					if ((value != null))
					{
						value.SecuritiesTrialSets.Add(this);
						_securityID = value.ID;
					}
					else
					{
						_securityID = default(int);
					}
				}
			}
		}
		
		[Association(Storage="_trialSets", OtherKey="ID", ThisKey="TrialSetID", Name="securities_trial_sets_trial_set_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public TrialSets TrialSets
		{
			get
			{
				return this._trialSets.Entity;
			}
			set
			{
				if (((this._trialSets.Entity == value) == false))
				{
					if ((this._trialSets.Entity != null))
					{
						TrialSets previousTrialSets = this._trialSets.Entity;
						this._trialSets.Entity = null;
						previousTrialSets.SecuritiesTrialSets.Remove(this);
					}
					this._trialSets.Entity = value;
					if ((value != null))
					{
						value.SecuritiesTrialSets.Add(this);
						_trialSetID = value.ID;
					}
					else
					{
						_trialSetID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.strategies")]
	public partial class Strategies : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _name;
		
		private EntitySet<TrialSets> _trialSets;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		#endregion
		
		
		public Strategies()
		{
			_trialSets = new EntitySet<TrialSets>(new Action<TrialSets>(this.TrialSets_Attach), new Action<TrialSets>(this.TrialSets_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('strategies_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="character varying(255)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) == false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_trialSets", OtherKey="StrategyID", ThisKey="ID", Name="trial_sets_strategy_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<TrialSets> TrialSets
		{
			get
			{
				return this._trialSets;
			}
			set
			{
				this._trialSets = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void TrialSets_Attach(TrialSets entity)
		{
			this.SendPropertyChanging();
			entity.Strategies = this;
		}
		
		private void TrialSets_Detach(TrialSets entity)
		{
			this.SendPropertyChanging();
			entity.Strategies = null;
		}
		#endregion
	}
	
	[Table(Name="public.trials")]
	public partial class Trials : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<decimal> _dailyStdDev;
		
		private long _endTime;
		
		private int _id;
		
		private System.Nullable<decimal> _mae;
		
		private System.Nullable<decimal> _mfE;
		
		private byte[] _portfolioValueLog;
		
		private long _startTime;
		
		private byte[] _transactionLog;
		
		private int _trialSetID;
		
		private System.Nullable<decimal> _yield;
		
		private EntityRef<TrialSets> _trialSets = new EntityRef<TrialSets>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnDailyStdDEVChanged();
		
		partial void OnDailyStdDEVChanging(System.Nullable<decimal> value);
		
		partial void OnEndTimeChanged();
		
		partial void OnEndTimeChanging(long value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnMaeChanged();
		
		partial void OnMaeChanging(System.Nullable<decimal> value);
		
		partial void OnMFeChanged();
		
		partial void OnMFeChanging(System.Nullable<decimal> value);
		
		partial void OnPortfolioValueLogChanged();
		
		partial void OnPortfolioValueLogChanging(byte[] value);
		
		partial void OnStartTimeChanged();
		
		partial void OnStartTimeChanging(long value);
		
		partial void OnTransactionLogChanged();
		
		partial void OnTransactionLogChanging(byte[] value);
		
		partial void OnTrialSetIDChanged();
		
		partial void OnTrialSetIDChanging(int value);
		
		partial void OnYieldChanged();
		
		partial void OnYieldChanging(System.Nullable<decimal> value);
		#endregion
		
		
		public Trials()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_dailyStdDev", Name="daily_std_dev", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> DailyStdDEV
		{
			get
			{
				return this._dailyStdDev;
			}
			set
			{
				if ((_dailyStdDev != value))
				{
					this.OnDailyStdDEVChanging(value);
					this.SendPropertyChanging();
					this._dailyStdDev = value;
					this.SendPropertyChanged("DailyStdDEV");
					this.OnDailyStdDEVChanged();
				}
			}
		}
		
		[Column(Storage="_endTime", Name="end_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long EndTime
		{
			get
			{
				return this._endTime;
			}
			set
			{
				if ((_endTime != value))
				{
					this.OnEndTimeChanging(value);
					this.SendPropertyChanging();
					this._endTime = value;
					this.SendPropertyChanged("EndTime");
					this.OnEndTimeChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('trials_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_mae", Name="mae", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Mae
		{
			get
			{
				return this._mae;
			}
			set
			{
				if ((_mae != value))
				{
					this.OnMaeChanging(value);
					this.SendPropertyChanging();
					this._mae = value;
					this.SendPropertyChanged("Mae");
					this.OnMaeChanged();
				}
			}
		}
		
		[Column(Storage="_mfE", Name="mfe", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> MFe
		{
			get
			{
				return this._mfE;
			}
			set
			{
				if ((_mfE != value))
				{
					this.OnMFeChanging(value);
					this.SendPropertyChanging();
					this._mfE = value;
					this.SendPropertyChanged("MFe");
					this.OnMFeChanged();
				}
			}
		}
		
		[Column(Storage="_portfolioValueLog", Name="portfolio_value_log", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] PortfolioValueLog
		{
			get
			{
				return this._portfolioValueLog;
			}
			set
			{
				if (((_portfolioValueLog == value) == false))
				{
					this.OnPortfolioValueLogChanging(value);
					this.SendPropertyChanging();
					this._portfolioValueLog = value;
					this.SendPropertyChanged("PortfolioValueLog");
					this.OnPortfolioValueLogChanged();
				}
			}
		}
		
		[Column(Storage="_startTime", Name="start_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long StartTime
		{
			get
			{
				return this._startTime;
			}
			set
			{
				if ((_startTime != value))
				{
					this.OnStartTimeChanging(value);
					this.SendPropertyChanging();
					this._startTime = value;
					this.SendPropertyChanged("StartTime");
					this.OnStartTimeChanged();
				}
			}
		}
		
		[Column(Storage="_transactionLog", Name="transaction_log", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] TransactionLog
		{
			get
			{
				return this._transactionLog;
			}
			set
			{
				if (((_transactionLog == value) == false))
				{
					this.OnTransactionLogChanging(value);
					this.SendPropertyChanging();
					this._transactionLog = value;
					this.SendPropertyChanged("TransactionLog");
					this.OnTransactionLogChanged();
				}
			}
		}
		
		[Column(Storage="_trialSetID", Name="trial_set_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TrialSetID
		{
			get
			{
				return this._trialSetID;
			}
			set
			{
				if ((_trialSetID != value))
				{
					this.OnTrialSetIDChanging(value);
					this.SendPropertyChanging();
					this._trialSetID = value;
					this.SendPropertyChanged("TrialSetID");
					this.OnTrialSetIDChanged();
				}
			}
		}
		
		[Column(Storage="_yield", Name="yield", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Yield
		{
			get
			{
				return this._yield;
			}
			set
			{
				if ((_yield != value))
				{
					this.OnYieldChanging(value);
					this.SendPropertyChanging();
					this._yield = value;
					this.SendPropertyChanged("Yield");
					this.OnYieldChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_trialSets", OtherKey="ID", ThisKey="TrialSetID", Name="trials_trial_set_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public TrialSets TrialSets
		{
			get
			{
				return this._trialSets.Entity;
			}
			set
			{
				if (((this._trialSets.Entity == value) == false))
				{
					if ((this._trialSets.Entity != null))
					{
						TrialSets previousTrialSets = this._trialSets.Entity;
						this._trialSets.Entity = null;
						previousTrialSets.Trials.Remove(this);
					}
					this._trialSets.Entity = value;
					if ((value != null))
					{
						value.Trials.Add(this);
						_trialSetID = value.ID;
					}
					else
					{
						_trialSetID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="public.trial_samples")]
	public partial class TrialSamples : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _attribute;
		
		private System.Nullable<decimal> _average;
		
		private byte[] _distribution;
		
		private long _endTime;
		
		private int _id;
		
		private System.Nullable<decimal> _maX;
		
		private System.Nullable<decimal> _miN;
		
		private System.Nullable<int> _n;
		
		private bool _overlappingTrials;
		
		private System.Nullable<decimal> _percentile10;
		
		private System.Nullable<decimal> _percentile20;
		
		private System.Nullable<decimal> _percentile30;
		
		private System.Nullable<decimal> _percentile40;
		
		private System.Nullable<decimal> _percentile50;
		
		private System.Nullable<decimal> _percentile60;
		
		private System.Nullable<decimal> _percentile70;
		
		private System.Nullable<decimal> _percentile80;
		
		private System.Nullable<decimal> _percentile90;
		
		private long _startTime;
		
		private int _trialSetID;
		
		private EntitySet<SamplingDistributions> _samplingDistributions;
		
		private EntityRef<TrialSets> _trialSets = new EntityRef<TrialSets>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAttributeChanged();
		
		partial void OnAttributeChanging(string value);
		
		partial void OnAverageChanged();
		
		partial void OnAverageChanging(System.Nullable<decimal> value);
		
		partial void OnDistributionChanged();
		
		partial void OnDistributionChanging(byte[] value);
		
		partial void OnEndTimeChanged();
		
		partial void OnEndTimeChanging(long value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnMaXChanged();
		
		partial void OnMaXChanging(System.Nullable<decimal> value);
		
		partial void OnMInChanged();
		
		partial void OnMInChanging(System.Nullable<decimal> value);
		
		partial void OnNChanged();
		
		partial void OnNChanging(System.Nullable<int> value);
		
		partial void OnOverlappingTrialsChanged();
		
		partial void OnOverlappingTrialsChanging(bool value);
		
		partial void OnPercentile10Changed();
		
		partial void OnPercentile10Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile20Changed();
		
		partial void OnPercentile20Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile30Changed();
		
		partial void OnPercentile30Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile40Changed();
		
		partial void OnPercentile40Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile50Changed();
		
		partial void OnPercentile50Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile60Changed();
		
		partial void OnPercentile60Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile70Changed();
		
		partial void OnPercentile70Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile80Changed();
		
		partial void OnPercentile80Changing(System.Nullable<decimal> value);
		
		partial void OnPercentile90Changed();
		
		partial void OnPercentile90Changing(System.Nullable<decimal> value);
		
		partial void OnStartTimeChanged();
		
		partial void OnStartTimeChanging(long value);
		
		partial void OnTrialSetIDChanged();
		
		partial void OnTrialSetIDChanging(int value);
		#endregion
		
		
		public TrialSamples()
		{
			_samplingDistributions = new EntitySet<SamplingDistributions>(new Action<SamplingDistributions>(this.SamplingDistributions_Attach), new Action<SamplingDistributions>(this.SamplingDistributions_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_attribute", Name="attribute", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Attribute
		{
			get
			{
				return this._attribute;
			}
			set
			{
				if (((_attribute == value) == false))
				{
					this.OnAttributeChanging(value);
					this.SendPropertyChanging();
					this._attribute = value;
					this.SendPropertyChanged("Attribute");
					this.OnAttributeChanged();
				}
			}
		}
		
		[Column(Storage="_average", Name="average", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Average
		{
			get
			{
				return this._average;
			}
			set
			{
				if ((_average != value))
				{
					this.OnAverageChanging(value);
					this.SendPropertyChanging();
					this._average = value;
					this.SendPropertyChanged("Average");
					this.OnAverageChanged();
				}
			}
		}
		
		[Column(Storage="_distribution", Name="distribution", DbType="bytea", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] Distribution
		{
			get
			{
				return this._distribution;
			}
			set
			{
				if (((_distribution == value) == false))
				{
					this.OnDistributionChanging(value);
					this.SendPropertyChanging();
					this._distribution = value;
					this.SendPropertyChanged("Distribution");
					this.OnDistributionChanged();
				}
			}
		}
		
		[Column(Storage="_endTime", Name="end_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long EndTime
		{
			get
			{
				return this._endTime;
			}
			set
			{
				if ((_endTime != value))
				{
					this.OnEndTimeChanging(value);
					this.SendPropertyChanging();
					this._endTime = value;
					this.SendPropertyChanged("EndTime");
					this.OnEndTimeChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('trial_samples_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_maX", Name="max", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> MaX
		{
			get
			{
				return this._maX;
			}
			set
			{
				if ((_maX != value))
				{
					this.OnMaXChanging(value);
					this.SendPropertyChanging();
					this._maX = value;
					this.SendPropertyChanged("MaX");
					this.OnMaXChanged();
				}
			}
		}
		
		[Column(Storage="_miN", Name="min", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> MIn
		{
			get
			{
				return this._miN;
			}
			set
			{
				if ((_miN != value))
				{
					this.OnMInChanging(value);
					this.SendPropertyChanging();
					this._miN = value;
					this.SendPropertyChanged("MIn");
					this.OnMInChanged();
				}
			}
		}
		
		[Column(Storage="_n", Name="n", DbType="integer(32,0)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> N
		{
			get
			{
				return this._n;
			}
			set
			{
				if ((_n != value))
				{
					this.OnNChanging(value);
					this.SendPropertyChanging();
					this._n = value;
					this.SendPropertyChanged("N");
					this.OnNChanged();
				}
			}
		}
		
		[Column(Storage="_overlappingTrials", Name="overlapping_trials", DbType="boolean", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool OverlappingTrials
		{
			get
			{
				return this._overlappingTrials;
			}
			set
			{
				if ((_overlappingTrials != value))
				{
					this.OnOverlappingTrialsChanging(value);
					this.SendPropertyChanging();
					this._overlappingTrials = value;
					this.SendPropertyChanged("OverlappingTrials");
					this.OnOverlappingTrialsChanged();
				}
			}
		}
		
		[Column(Storage="_percentile10", Name="percentile_10", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile10
		{
			get
			{
				return this._percentile10;
			}
			set
			{
				if ((_percentile10 != value))
				{
					this.OnPercentile10Changing(value);
					this.SendPropertyChanging();
					this._percentile10 = value;
					this.SendPropertyChanged("Percentile10");
					this.OnPercentile10Changed();
				}
			}
		}
		
		[Column(Storage="_percentile20", Name="percentile_20", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile20
		{
			get
			{
				return this._percentile20;
			}
			set
			{
				if ((_percentile20 != value))
				{
					this.OnPercentile20Changing(value);
					this.SendPropertyChanging();
					this._percentile20 = value;
					this.SendPropertyChanged("Percentile20");
					this.OnPercentile20Changed();
				}
			}
		}
		
		[Column(Storage="_percentile30", Name="percentile_30", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile30
		{
			get
			{
				return this._percentile30;
			}
			set
			{
				if ((_percentile30 != value))
				{
					this.OnPercentile30Changing(value);
					this.SendPropertyChanging();
					this._percentile30 = value;
					this.SendPropertyChanged("Percentile30");
					this.OnPercentile30Changed();
				}
			}
		}
		
		[Column(Storage="_percentile40", Name="percentile_40", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile40
		{
			get
			{
				return this._percentile40;
			}
			set
			{
				if ((_percentile40 != value))
				{
					this.OnPercentile40Changing(value);
					this.SendPropertyChanging();
					this._percentile40 = value;
					this.SendPropertyChanged("Percentile40");
					this.OnPercentile40Changed();
				}
			}
		}
		
		[Column(Storage="_percentile50", Name="percentile_50", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile50
		{
			get
			{
				return this._percentile50;
			}
			set
			{
				if ((_percentile50 != value))
				{
					this.OnPercentile50Changing(value);
					this.SendPropertyChanging();
					this._percentile50 = value;
					this.SendPropertyChanged("Percentile50");
					this.OnPercentile50Changed();
				}
			}
		}
		
		[Column(Storage="_percentile60", Name="percentile_60", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile60
		{
			get
			{
				return this._percentile60;
			}
			set
			{
				if ((_percentile60 != value))
				{
					this.OnPercentile60Changing(value);
					this.SendPropertyChanging();
					this._percentile60 = value;
					this.SendPropertyChanged("Percentile60");
					this.OnPercentile60Changed();
				}
			}
		}
		
		[Column(Storage="_percentile70", Name="percentile_70", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile70
		{
			get
			{
				return this._percentile70;
			}
			set
			{
				if ((_percentile70 != value))
				{
					this.OnPercentile70Changing(value);
					this.SendPropertyChanging();
					this._percentile70 = value;
					this.SendPropertyChanged("Percentile70");
					this.OnPercentile70Changed();
				}
			}
		}
		
		[Column(Storage="_percentile80", Name="percentile_80", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile80
		{
			get
			{
				return this._percentile80;
			}
			set
			{
				if ((_percentile80 != value))
				{
					this.OnPercentile80Changing(value);
					this.SendPropertyChanging();
					this._percentile80 = value;
					this.SendPropertyChanged("Percentile80");
					this.OnPercentile80Changed();
				}
			}
		}
		
		[Column(Storage="_percentile90", Name="percentile_90", DbType="numeric", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Percentile90
		{
			get
			{
				return this._percentile90;
			}
			set
			{
				if ((_percentile90 != value))
				{
					this.OnPercentile90Changing(value);
					this.SendPropertyChanging();
					this._percentile90 = value;
					this.SendPropertyChanged("Percentile90");
					this.OnPercentile90Changed();
				}
			}
		}
		
		[Column(Storage="_startTime", Name="start_time", DbType="bigint(64,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public long StartTime
		{
			get
			{
				return this._startTime;
			}
			set
			{
				if ((_startTime != value))
				{
					this.OnStartTimeChanging(value);
					this.SendPropertyChanging();
					this._startTime = value;
					this.SendPropertyChanged("StartTime");
					this.OnStartTimeChanged();
				}
			}
		}
		
		[Column(Storage="_trialSetID", Name="trial_set_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TrialSetID
		{
			get
			{
				return this._trialSetID;
			}
			set
			{
				if ((_trialSetID != value))
				{
					this.OnTrialSetIDChanging(value);
					this.SendPropertyChanging();
					this._trialSetID = value;
					this.SendPropertyChanged("TrialSetID");
					this.OnTrialSetIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_samplingDistributions", OtherKey="TrialSampleID", ThisKey="ID", Name="sampling_distributions_trial_sample_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<SamplingDistributions> SamplingDistributions
		{
			get
			{
				return this._samplingDistributions;
			}
			set
			{
				this._samplingDistributions = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_trialSets", OtherKey="ID", ThisKey="TrialSetID", Name="trial_samples_trial_set_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public TrialSets TrialSets
		{
			get
			{
				return this._trialSets.Entity;
			}
			set
			{
				if (((this._trialSets.Entity == value) == false))
				{
					if ((this._trialSets.Entity != null))
					{
						TrialSets previousTrialSets = this._trialSets.Entity;
						this._trialSets.Entity = null;
						previousTrialSets.TrialSamples.Remove(this);
					}
					this._trialSets.Entity = value;
					if ((value != null))
					{
						value.TrialSamples.Add(this);
						_trialSetID = value.ID;
					}
					else
					{
						_trialSetID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void SamplingDistributions_Attach(SamplingDistributions entity)
		{
			this.SendPropertyChanging();
			entity.TrialSamples = this;
		}
		
		private void SamplingDistributions_Detach(SamplingDistributions entity)
		{
			this.SendPropertyChanging();
			entity.TrialSamples = null;
		}
		#endregion
	}
	
	[Table(Name="public.trial_sets")]
	public partial class TrialSets : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<decimal> _commissionPerShare;
		
		private System.Nullable<decimal> _commissionPerTrade;
		
		private string _duration;
		
		private int _id;
		
		private System.Nullable<decimal> _principal;
		
		private int _strategyID;
		
		private EntitySet<SecuritiesTrialSets> _securitiesTrialSets;
		
		private EntitySet<Trials> _trials;
		
		private EntitySet<TrialSamples> _trialSamples;
		
		private EntityRef<Strategies> _strategies = new EntityRef<Strategies>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCommissionPerShareChanged();
		
		partial void OnCommissionPerShareChanging(System.Nullable<decimal> value);
		
		partial void OnCommissionPerTradeChanged();
		
		partial void OnCommissionPerTradeChanging(System.Nullable<decimal> value);
		
		partial void OnDurationChanged();
		
		partial void OnDurationChanging(string value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnPrincipalChanged();
		
		partial void OnPrincipalChanging(System.Nullable<decimal> value);
		
		partial void OnStrategyIDChanged();
		
		partial void OnStrategyIDChanging(int value);
		#endregion
		
		
		public TrialSets()
		{
			_securitiesTrialSets = new EntitySet<SecuritiesTrialSets>(new Action<SecuritiesTrialSets>(this.SecuritiesTrialSets_Attach), new Action<SecuritiesTrialSets>(this.SecuritiesTrialSets_Detach));
			_trials = new EntitySet<Trials>(new Action<Trials>(this.Trials_Attach), new Action<Trials>(this.Trials_Detach));
			_trialSamples = new EntitySet<TrialSamples>(new Action<TrialSamples>(this.TrialSamples_Attach), new Action<TrialSamples>(this.TrialSamples_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_commissionPerShare", Name="commission_per_share", DbType="numeric(30,2)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> CommissionPerShare
		{
			get
			{
				return this._commissionPerShare;
			}
			set
			{
				if ((_commissionPerShare != value))
				{
					this.OnCommissionPerShareChanging(value);
					this.SendPropertyChanging();
					this._commissionPerShare = value;
					this.SendPropertyChanged("CommissionPerShare");
					this.OnCommissionPerShareChanged();
				}
			}
		}
		
		[Column(Storage="_commissionPerTrade", Name="commission_per_trade", DbType="numeric(30,2)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> CommissionPerTrade
		{
			get
			{
				return this._commissionPerTrade;
			}
			set
			{
				if ((_commissionPerTrade != value))
				{
					this.OnCommissionPerTradeChanging(value);
					this.SendPropertyChanging();
					this._commissionPerTrade = value;
					this.SendPropertyChanged("CommissionPerTrade");
					this.OnCommissionPerTradeChanged();
				}
			}
		}
		
		[Column(Storage="_duration", Name="duration", DbType="character varying(12)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Duration
		{
			get
			{
				return this._duration;
			}
			set
			{
				if (((_duration == value) == false))
				{
					this.OnDurationChanging(value);
					this.SendPropertyChanging();
					this._duration = value;
					this.SendPropertyChanged("Duration");
					this.OnDurationChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="integer(32,0)", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false, Expression="nextval('trial_sets_id_seq')")]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_principal", Name="principal", DbType="numeric(30,2)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Principal
		{
			get
			{
				return this._principal;
			}
			set
			{
				if ((_principal != value))
				{
					this.OnPrincipalChanging(value);
					this.SendPropertyChanging();
					this._principal = value;
					this.SendPropertyChanged("Principal");
					this.OnPrincipalChanged();
				}
			}
		}
		
		[Column(Storage="_strategyID", Name="strategy_id", DbType="integer(32,0)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int StrategyID
		{
			get
			{
				return this._strategyID;
			}
			set
			{
				if ((_strategyID != value))
				{
					this.OnStrategyIDChanging(value);
					this.SendPropertyChanging();
					this._strategyID = value;
					this.SendPropertyChanged("StrategyID");
					this.OnStrategyIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_securitiesTrialSets", OtherKey="TrialSetID", ThisKey="ID", Name="securities_trial_sets_trial_set_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<SecuritiesTrialSets> SecuritiesTrialSets
		{
			get
			{
				return this._securitiesTrialSets;
			}
			set
			{
				this._securitiesTrialSets = value;
			}
		}
		
		[Association(Storage="_trials", OtherKey="TrialSetID", ThisKey="ID", Name="trials_trial_set_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<Trials> Trials
		{
			get
			{
				return this._trials;
			}
			set
			{
				this._trials = value;
			}
		}
		
		[Association(Storage="_trialSamples", OtherKey="TrialSetID", ThisKey="ID", Name="trial_samples_trial_set_id_fkey")]
		[DebuggerNonUserCode()]
		public EntitySet<TrialSamples> TrialSamples
		{
			get
			{
				return this._trialSamples;
			}
			set
			{
				this._trialSamples = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_strategies", OtherKey="ID", ThisKey="StrategyID", Name="trial_sets_strategy_id_fkey", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Strategies Strategies
		{
			get
			{
				return this._strategies.Entity;
			}
			set
			{
				if (((this._strategies.Entity == value) == false))
				{
					if ((this._strategies.Entity != null))
					{
						Strategies previousStrategies = this._strategies.Entity;
						this._strategies.Entity = null;
						previousStrategies.TrialSets.Remove(this);
					}
					this._strategies.Entity = value;
					if ((value != null))
					{
						value.TrialSets.Add(this);
						_strategyID = value.ID;
					}
					else
					{
						_strategyID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void SecuritiesTrialSets_Attach(SecuritiesTrialSets entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = this;
		}
		
		private void SecuritiesTrialSets_Detach(SecuritiesTrialSets entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = null;
		}
		
		private void Trials_Attach(Trials entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = this;
		}
		
		private void Trials_Detach(Trials entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = null;
		}
		
		private void TrialSamples_Attach(TrialSamples entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = this;
		}
		
		private void TrialSamples_Detach(TrialSamples entity)
		{
			this.SendPropertyChanging();
			entity.TrialSets = null;
		}
		#endregion
	}
}
