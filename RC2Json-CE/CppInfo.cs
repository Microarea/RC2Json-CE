using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2Json
{
	internal class CppClass
	{
		List<CppClass> derivedClasses = new List<CppClass>();

		private string name;

		private CppClass baseClass = null;

		private List<DBTInfo> dbts = new List<DBTInfo>();

		private List<RecordField> recordFields = new List<RecordField>();

		public string DBTName = "";

		public string DocName = "";
		public string DeclarationFile="";
		public string ImplementationFile = "";
		
		internal List<CppClass> DerivedClasses
		{
			get { return derivedClasses; }
		}
		internal CppClass BaseClass
		{
			get { return baseClass; }
		}
		internal List<DBTInfo> Dbts
		{
			get { return dbts; }
		}
		public List<RecordField> RecordFields
		{
			get { return recordFields; }
		}

		public string Name
		{
			get { return name; }
		}
		public override string ToString()
		{
			return (baseClass == null)
				? name
				: string.Concat(baseClass.ToString(), ".", name);
		}

		public CppClass(string name)
		{
			this.name = name;
		}

		internal void SetBase(CppClass b)
		{
			b.derivedClasses.Add(this);
			if (baseClass != null)
			{
				baseClass.derivedClasses.Remove(this);
			}
			baseClass = b;
		}
		internal CppClass FindClass(string name)
		{
			foreach (CppClass c in derivedClasses)
			{
				if (c.name.Equals(name))
					return c;
				CppClass c1 = c.FindClass(name);
				if (c1 != null)
					return c1;
			}

			return null;
		}
		internal List<CppClass> FindClassesByDefinitionFile(string file)
		{
			List<CppClass> classes = new List<CppClass>();
			foreach (CppClass c in derivedClasses)
			{
				if (c.DeclarationFile.Equals(file, StringComparison.InvariantCultureIgnoreCase)
						||
						c.ImplementationFile.Equals(file, StringComparison.InvariantCultureIgnoreCase))
					classes.Add(c);
				classes.AddRange(c.FindClassesByDefinitionFile(file));
				
			}

			return classes;
		}
		internal bool IsKindOf(string className)
		{
			if (name.Equals(className))
				return true;
			return baseClass == null ? false : baseClass.IsKindOf(className);
		}

		internal void AddDBT(string dbtClassName, string recordClassName)
		{
			dbts.Add(new DBTInfo() { DBTClass = dbtClassName, RecordClass = recordClassName });
		}

		//aggiunge alla lista la colonna di database; potrebbe già essere presente se prima è stata 
		//parsata la dichiarazione del SqlRecord; il campo che li lega è il nome della variabile
		internal void AddRecordColumn(string colName, string varName, string sourceFile, int pos, long line)
		{
			foreach (var f in recordFields)
			{
				//se la trovo, aggiorno il valore
				if (f.Variable.Equals(varName))
				{
					//se c'è già, è perché è stata aggiunta parsando la dichiarazione di variabile del dataobj
					if (string.IsNullOrEmpty(f.Name) || f.Name.Equals(colName))
					{
						f.Name = colName;
						f.BindColumnSourceFile = sourceFile;
						f.BindColumnPos = pos;
						f.BindColumnLine = line;
					}
					else
					{
						//Debug.Fail("Variable already found with another name");
					}
					return;
				}

			}
			//se non l atrovo, la aggiungo
			recordFields.Add(new RecordField() { Name = colName, Variable = varName, DataType = "", BindColumnSourceFile = sourceFile, BindColumnPos = pos, BindColumnLine = line });
		}

		//aggiunge alla lista il campo DataObj; potrebbe già essere presente se prima è stata 
		//parsata la BindRecord; il campo che li lega è il nome della variabile
		internal void AddRecorDataObj(string dataObj, string varName, string sourceFile, int pos, long line)
		{
			string dataType =  ToDataType(dataObj);
			foreach (var f in recordFields)
			{
				//se la trovo, aggiorno il valore
				if (f.Variable.Equals(varName))
				{
					//se c'è già, è perché è stata aggiunta parsando la BindRecords
					if (string.IsNullOrEmpty(f.DataType) || f.DataType.Equals(dataType))
					{

						f.DataType = dataType;
						f.DeclarationSourceFile = sourceFile;
						f.DeclarationLine = line;
						f.DeclarationPos = pos;
					}
					else
					{
						Debug.Fail("Variable already found with another data type");
					}
					return;
				}

			}
			//se non l atrovo, la aggiungo
			recordFields.Add(new RecordField() { DataType = dataType, Variable = varName, Name = "", DeclarationSourceFile = sourceFile, DeclarationLine =line, DeclarationPos = pos });
		}

		private string ToDataType(string dataObj)
		{
			switch (dataObj)
			{
				case "DataStr": return "String";
				case "DataInt": return "Integer";
				case "DataLng": return "Long";
				case "DataDbl": return "Double";
				case "DataMon": return "Money";
				case "DataQty": return "Quantity";
				case "DataPerc": return "Percent";
				case "DataDate": return "Percent";
				case "DataBool": return "Bool";
				case "DataEnum": return "Enum";
				case "DataGuid": return "Guid";
				case "DataText": return "Text";
				case "DataBlob": return "Blob";
				default: return dataObj;
			}
		}
	}

	//---------------------------------------------------------------------------------------------------
	internal class RecordField
	{
		public string DataType { get; set; }
		public string Variable { get; set; }
		public string Name { get; set; }
		public string BindColumnSourceFile { get; set; }
		public int BindColumnPos { get; set; }
		public long BindColumnLine { get; set; }
		public string DeclarationSourceFile { get; set; }
		public int DeclarationPos { get; set; }
		public long DeclarationLine { get; set; }
		public override string ToString()
		{
			return string.Concat("Data type: ", DataType, " - Variable: ", Variable, " - Name: ", Name, " - Bind file: ", BindColumnSourceFile, " - Declaration file: ", DeclarationSourceFile);
		}
		public bool IsIncomplete { get { return string.IsNullOrEmpty(DataType) || string.IsNullOrEmpty(Variable) || string.IsNullOrEmpty(Name); } }
	}
	//---------------------------------------------------------------------------------------------------
	//struttura che associa ad ogni classe di documento le informazioni sui dbt
	internal class DBTInfo
	{
		public string DBTClass { get; set; }
		public string RecordClass { get; set; }

		public override string ToString()
		{
			return string.Concat(DBTClass, '.', RecordClass);
		}
	}
	//struttura che compatta le informazioni associando ad ogni dbt i campi dei record
	//è quella usata per serializzare in json
	//---------------------------------------------------------------------------------------------------
	internal class DBT
	{
		public string Name { get; set; }
		public List<RecordField> Fields = new List<RecordField>();
		public override string ToString()
		{
			return Name;
		}
	}
	internal class Document
	{
		public string Name { get; set; }
		public List<DBT> Dbts = new List<DBT>();

		public override string ToString()
		{
			return Name;
		}

	}
	
	//---------------------------------------------------------------------------------------------------
	internal class CppInfo
	{
		CppClass root = new CppClass("root");
		public Dictionary<string, List<string>> Includes = new Dictionary<string, List<string>>();
		//---------------------------------------------------------------------------------------------------
		internal void AddInclude(string includingFile, string include)
		{
			 List<string> ar;
			 if (!Includes.TryGetValue(includingFile, out ar))
			 {
				 ar = new List<string>();
				 Includes[includingFile] = ar;
			 }

			 ar.Add(include);
		}
		//---------------------------------------------------------------------------------------------------
		internal void AddClass(string theClass, string theBaseClass)
		{
			//prima cerco la base
			CppClass b = GetClass(theBaseClass);
			//poi cerco la classe
			CppClass c = root.FindClass(theClass);
			if (c == null)
			{
				//se non la trovo la creo; potrei averla già creata se mi era arrivata in precedenza come base class 
				c = new CppClass(theClass);
			}
			//infine ne imposto la base (se ne aveva già un'altra (root) le parentele vengono riallineate
			c.SetBase(b);
		}

		//---------------------------------------------------------------------------------------------------
		internal CppClass GetClass(string name)
		{
			CppClass b = root.FindClass(name);
			if (b == null)
			{
				//se non la trovo, la creo e imposto la root come suo parent
				b = new CppClass(name);
				b.SetBase(root);
			}
			return b;
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddDBTToDocumentClass(string docClassName, string dbtClassName, string recordClassName)
		{
			CppClass c = GetClass(docClassName);
			c.AddDBT(dbtClassName, recordClassName);
		}

		//---------------------------------------------------------------------------------------------------
		internal void SetDBTName(string dbtClass, string dbtName)
		{
			CppClass c = GetClass(dbtClass);
			c.DBTName = dbtName;
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddRecordColumn(string className, string colName, string varName, string sourceFile, int pos, long line)
		{
			CppClass c = GetClass(className);
			c.AddRecordColumn(colName, varName, sourceFile, pos, line);
		}
		//---------------------------------------------------------------------------------------------------
		internal void AddRecordDataObj(string className, string dataObj, string varName, string sourceFile, int pos, long line)
		{
			CppClass c = GetClass(className);
			c.AddRecorDataObj(dataObj, varName, sourceFile, pos, line);
		}

		//---------------------------------------------------------------------------------------------------
		List<DBT> GetDbts(CppClass doc)
		{
			List<DBT> l = new List<DBT>();
			foreach (DBTInfo info in doc.Dbts)
			{
				CppClass dbtClass = root.FindClass(info.DBTClass);
				if (dbtClass == null)
					continue;
				CppClass recordClass = root.FindClass(info.RecordClass);
				if (recordClass == null)
					continue;
				DBT dbt = new DBT() { Name = dbtClass.DBTName };
				//aggiungo i campi della classe e quelli ereditati
				while (recordClass != null)
				{
					dbt.Fields.AddRange(recordClass.RecordFields);
					recordClass = recordClass.BaseClass;
				}
				l.Add(dbt);
			}
			//ogni classe ha i duoi dbt più quelli ereditati
			if (doc.BaseClass != null)
				l.AddRange(GetDbts(doc.BaseClass));
			return l;
		}

		//---------------------------------------------------------------------------------------------------
		internal List<Document> GetDocuments()
		{
			List<Document> docs = new List<Document>();
			List<CppClass> docClasses = new List<CppClass>();
			CppClass b = root.FindClass("CAbstractFormDoc");
			if (b != null)
				AddDocuments(docs, b);
			return docs;
		}

		//---------------------------------------------------------------------------------------------------
		private void AddDocuments(List<Document> docs, CppClass c)
		{
			foreach (var item in c.DerivedClasses)
			{
				Document d = new Document() { Name = item.Name };
				d.Dbts.AddRange(GetDbts(item));
				docs.Add(d);

				AddDocuments(docs, item);
			}
		}



		//---------------------------------------------------------------------------------------------------
		internal List<CppClass> GetViewClassesInFile(string includeFile)
		{
			CppClass viewRootClass = root.FindClass("CMasterFormView");
			if (viewRootClass != null)
			{
				return viewRootClass.FindClassesByDefinitionFile(includeFile);
			}
			return new List<CppClass>();

		}
	}


}
