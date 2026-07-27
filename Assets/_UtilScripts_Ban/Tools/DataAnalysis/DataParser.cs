using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Debug = UnityEngine.Debug;
namespace Yu.Tools
{
	public class DataParser  
	{
		public static bool DebugMode = false;

		public const char SPLIT_CHAR = ',';
		public const char SPLIT_CHARHT = '\t';

		private static Dictionary<Type,MethodInfo> cachedEntityCreateMethod = new Dictionary<Type, MethodInfo>();
		private static Dictionary<Type,HeaderList> cachedEntityHeaderList = new Dictionary<Type, HeaderList>();
		private static Dictionary<FieldInfo,ArrayField> cachedClassArrayFieldDict = new Dictionary<FieldInfo, ArrayField>();
		public delegate object ImporterFunc                (string input);

		private static Dictionary<Type,ImporterFunc> importerDict = new Dictionary<Type, ImporterFunc>();

		static DataParser()
		{
			ImporterFunc importer;

			importer = delegate (string input) {
				return Convert.ToByte (input);
			};
			importerDict.Add(  
				typeof (byte), importer);


			importer = delegate(string input) {
				return Convert.ToInt64(input);
			};
			importerDict.Add(  
				typeof (long), importer);

			importer = delegate (string input) {
				return Convert.ToUInt64 ( input);
			};
			importerDict.Add(  
				typeof (ulong), importer);

			importer = delegate (string input) {
				return Convert.ToSByte ( input);
			};
			importerDict.Add(  
				typeof (sbyte), importer);

			importer = delegate (string input) {
				return Convert.ToInt16 ( input);
			};
			importerDict.Add(  
				typeof (short), importer);

			importer = delegate (string input) {
				return Convert.ToUInt16 ( input);
			};
			importerDict.Add(  
				typeof (ushort), importer);

			importer = delegate (string input) {
				return  Convert.ToInt32(input);
			};
			importerDict.Add(
				typeof (int), importer);

			importer = delegate (string input) {
				return Convert.ToUInt32 ( input);
			};
			importerDict.Add(  
				typeof (uint), importer);

			importer = delegate (string input) {
				return Convert.ToSingle ( input);
			};
			importerDict.Add(  
				typeof (float), importer);

			importer = delegate (string input) {
				return Convert.ToDouble ( input);
			};
			importerDict.Add(  
				typeof (double), importer);

			importer = delegate (string input) {
				return Convert.ToDecimal ( input);
			};
			importerDict.Add(  
				typeof (decimal), importer);



			importer = delegate (string input) {
				return  Convert.ToChar(input);
			};
			importerDict.Add(
				typeof (char), importer);

			importer = delegate (string input) {
				return  input;
			};
			importerDict.Add(
				typeof (string), importer);



			importer = delegate (string input) {
				return Convert.ToDateTime ( input,System.Globalization.DateTimeFormatInfo.InvariantInfo);
			};
			importerDict.Add(  
				typeof (DateTime), importer);
		}

		public static void DisposeCachedData()
		{
			cachedEntityCreateMethod.Clear();
			cachedEntityHeaderList.Clear();
			cachedClassArrayFieldDict.Clear();
		}

		public static Dictionary<K,T> Parse<K,T>(byte[] bytes) where T : class,new()
		{
			return Parse<K,T>(bytes,SPLIT_CHARHT);
		}

		public static Dictionary<K,T> Parse<K,T>(byte[] bytes,char splitChar) where T : class,new()
		{
			Dictionary<K,T> dictionary = new Dictionary<K, T>();
			Type t = typeof(T);



			HeaderList headerList = ParseEntityHeaderField(t);

			using(MemoryStream memoryStream = new MemoryStream(bytes))
			{
				using(StreamReader streamReader = new StreamReader(memoryStream,System.Text.Encoding.UTF8))
				{
					

					string headerText = streamReader.ReadLine();
					if(!string.IsNullOrEmpty( headerText ))
					{					
						headerText = headerText.Replace("\r\n","").Replace("\n","").Trim();

						string[] headerTexts = headerText.Split(splitChar);

						FieldInfo[] headerFieldInfo = new FieldInfo[headerTexts.Length];

						for (int i = 0; i < headerTexts.Length; i++) {

							if(headerList.headerDict.ContainsKey(headerTexts[i]))
							{
								headerFieldInfo[i] = headerList.headerDict[headerTexts[i]];
							}else if(headerList.arrayHeaderDict.ContainsKey(headerTexts[i]))
							{
								headerFieldInfo[i] = headerList.arrayHeaderDict[headerTexts[i]];
							}
							else
							{
								if(DebugMode)
								{
									Debug.LogWarning(headerTexts[i] + " Field Not Match");
								}
							}
						}
						string content = null;
						int rowCount = -1;
						while( (content = streamReader.ReadLine()) != null)
						{
							rowCount ++;
							if(!string.IsNullOrEmpty(content))
							{
								K k = default(K);

								string[] contents = content.Split(splitChar);
								T temp = (T)ParseEntity(new T(),headerList,headerFieldInfo,
									headerTexts,contents);
								if(headerList.keyField != null)
								{
									k = (K)headerList.keyField.GetValue(temp);
								}
								if(dictionary.ContainsKey(k))
								{
									Debug.LogError("Have key : "+k);
								}
								else
									dictionary.Add(k,temp);
							}
						}



					}
				}
			}



			return dictionary;
		}

		public static T CreateEntity<T>()   where T : class,new()
		{
			return new T();
		}


		public static object GetEntity(Type entityType)
		{
			MethodInfo _createEnetityMethod = null;


			if(cachedEntityCreateMethod.TryGetValue(entityType,out _createEnetityMethod))
			{
				
			}else
			{
				_createEnetityMethod = typeof(DataParser).GetMethod("CreateEntity").MakeGenericMethod(entityType);
				cachedEntityCreateMethod.Add(entityType,_createEnetityMethod);
			}

			return _createEnetityMethod.Invoke(null,null);
		}

		public static object ParseEntity(object instance, string[] headerTexts,string[] contents)
		{
			HeaderList headerList =ParseEntityHeaderField(instance.GetType());

			FieldInfo[] headerFieldInfo = new FieldInfo[headerTexts.Length];


			for (int i = 0; i < headerTexts.Length; i++) {

				if(headerList.headerDict.ContainsKey(headerTexts[i]))
				{
					headerFieldInfo[i] = headerList.headerDict[headerTexts[i]];
				}else if(headerList.arrayHeaderDict.ContainsKey(headerTexts[i]))
				{
					headerFieldInfo[i] = headerList.arrayHeaderDict[headerTexts[i]];
				}
			}
			object temp = ParseEntity(instance,headerList,headerFieldInfo,
				headerTexts,contents);

			return temp;
		}

		public static object ParseEntity(object instance,HeaderList headerList,FieldInfo[] headerFieldInfo,
			string[] headerTexts,string[] contents
		)
		{
			
			object temp = instance;
			for (int i = 0; i < headerFieldInfo.Length; i++) {

				FieldInfo fieldInfo = headerFieldInfo[i];
				if(fieldInfo != null)
				{	

					if(fieldInfo.FieldType.IsArray || fieldInfo.FieldType.IsGenericType)
					{
						object ins = fieldInfo.GetValue(temp);
						if(ins == null)
						{
							Type listType = null;
							if(fieldInfo.FieldType.IsArray)
							{
								listType = fieldInfo.FieldType.GetElementType();
								Array array = Array.CreateInstance(listType,1);
								try
								{
									array.SetValue(ChangeType(contents[i],listType),array.Length - 1);
								}catch(System.Exception ex)
								{
									Debug.LogError(string.Format("Index : {0}  , Header : {1} , Context : {2} .Covert error",i, headerTexts[i] ,contents[i]));
									Debug.LogException(ex);
								}
								fieldInfo.SetValue(temp,array);
							}else
							{
								listType = fieldInfo.FieldType.GetGenericArguments()[0];
								IList listTemp =CreateGeneric(listType);
								try
								{
									listTemp.Add(ChangeType(contents[i],listType));
								}catch(System.Exception ex)
								{
									Debug.LogError(string.Format("Index : {0}  , Header : {1} , Context : {2} .Covert error",i, headerTexts[i] ,contents[i]));
									Debug.LogException(ex);
								}
								fieldInfo.SetValue(temp,listTemp);
							}
						}else
						{
							Type listType = null;
							if(fieldInfo.FieldType.IsArray)
							{
								listType = fieldInfo.FieldType.GetElementType();
								Array oldArray = ins as Array;

								Array array = Array.CreateInstance(listType,oldArray.Length + 1);
								Array.Copy(oldArray,array,oldArray.Length);
								try
								{
									array.SetValue(ChangeType(contents[i],listType),array.Length - 1);
								}catch(System.Exception ex)
								{
									Debug.LogError(string.Format("Index : {0}  , Header : {1} , Context : {2} .Covert error",i, headerTexts[i] ,contents[i]));
									Debug.LogException(ex);
								}
								fieldInfo.SetValue(temp,array);

							}else
							{
								listType = fieldInfo.FieldType.GetGenericArguments()[0];
								IList listTemp = ins as IList;
								try
								{
									listTemp.Add(ChangeType(contents[i],listType));
								}catch(System.Exception ex)
								{
									Debug.LogError(string.Format("Index : {0}  , Header : {1} , Context : {2} .Covert error",i, headerTexts[i] ,contents[i]));
									Debug.LogException(ex);
								}
							}
						}
					}else
					{
						try
						{
							fieldInfo.SetValue(temp,ChangeType(contents[i],fieldInfo.FieldType));
						}catch(System.Exception ex)
						{
							Debug.LogError(string.Format("Index : {0}  , Header : {1} , Context : {2} .Covert error",i, headerTexts[i] ,contents[i]));
							Debug.LogException(ex);
						}
					}
				}
			}

			if(headerList.classFieldInfo.Count > 0)
			{
				for (int i = 0; i < headerList.classFieldInfo.Count; i++) {
					FieldInfo fieldInfo = headerList.classFieldInfo[i];

					object newClassValue = ParseEntity(GetEntity(fieldInfo.FieldType),headerTexts,contents);
					fieldInfo.SetValue(temp,newClassValue);
				}
			}

			if(headerList.classArrayFieldInfo.Count > 0)
			{
				for (int i = 0; i < headerList.classArrayFieldInfo.Count; i++) {
					FieldInfo fieldInfo = headerList.classArrayFieldInfo[i];
					fieldInfo.SetValue(temp, ParseClassArray(fieldInfo,headerTexts,contents));
				}
			}

			return temp;
		}

		public static object ParseClassArray(FieldInfo fieldInfo, string[] headerTexts,string[] contents)
		{
			Type entityType = null;
			bool arrayOrList = true;
			if(fieldInfo.FieldType.IsArray)
			{
				entityType = fieldInfo.FieldType.GetElementType();
			}else
			{
				entityType = fieldInfo.FieldType.GetGenericArguments()[0];
				arrayOrList = false;
			}

			HeaderList headerList = ParseEntityHeaderField(entityType);

			ArrayField arrayField;

			Dictionary<string,List<int>> classDict = null;

			int maxCount = 1;
			if(cachedClassArrayFieldDict.TryGetValue(fieldInfo,out arrayField))
			{
				classDict = arrayField.classDict;
				maxCount = arrayField.classMaxCount;
			}
			else
			{
				classDict = new Dictionary<string,List<int>>();
				for (int i = 0; i < headerTexts.Length; i++) {

					string headerText = headerTexts[i];

					if(classDict.ContainsKey(headerText))
					{
						List<int> t = classDict[headerText];
						t.Add(i);

						maxCount = System.Math.Max(t.Count,maxCount);
					}else
					{
						classDict.Add(headerText,new List<int>(){i});
					}

				}
				arrayField.classDict =  classDict;
				arrayField.classMaxCount = maxCount;
				cachedClassArrayFieldDict.Add(fieldInfo,arrayField);
			}


			object temp = null;

			if(arrayOrList)
			{
				temp = Array.CreateInstance(entityType,maxCount);
			}else
			{
				temp =CreateGeneric(entityType);
			}

			Array array = null;
			IList list = null;

			if(arrayOrList)
			{
				array = temp as Array;
			}else
			{
				list = temp as IList;
			}

			for (int i = 0; i < maxCount; i++) {
				object entity = GetEntity(entityType);
				Dictionary<string,List<int>>.Enumerator classEnumer = classDict.GetEnumerator();
				while(classEnumer.MoveNext())
				{
					KeyValuePair<string,List<int>> item = classEnumer.Current;
					int index = -1;
					FieldInfo entityField = null;
					if(headerList.headerDict.ContainsKey(item.Key))
					{
						List<int> filedList = item.Value;
						if(filedList.Count > i)
						{
							index = filedList[i];
							entityField = headerList.headerDict[item.Key];
						}
					}

					if(index != -1)
					{
						entityField.SetValue(entity, ChangeType(contents[index],entityField.FieldType));
					}
				}
				if(arrayOrList)
				{
					array.SetValue(entity,i);
				}else
				{
					list.Add(entity);
				}
				classEnumer.Dispose();
			}

			return temp;

		}


		public static HeaderList ParseEntityHeaderField(Type t)
		{
			HeaderList headerList;
			if(cachedEntityHeaderList.TryGetValue(t,out headerList))
			{
			}
			else
			{	
				headerList.arrayHeaderDict = new Dictionary<string, FieldInfo>();
				headerList.classFieldInfo = new List<FieldInfo>();
				headerList.classArrayFieldInfo = new List<FieldInfo>();
				headerList.headerDict = new Dictionary<string, FieldInfo>(); 
				
				FieldInfo[] fieldInfos = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
				foreach (var field in fieldInfos) {

					object[] attris = field.GetCustomAttributes(false);

					for (int i = 0; i < attris.Length; i++) {
						if(attris[i].GetType() == typeof(ColumnKeyMatchingAttribute))
						{
							headerList.keyField = field;
						}else if(attris[i].GetType() == typeof(ColumnMatchingAttribute))
						{
							ColumnMatchingAttribute cma =attris[i] as ColumnMatchingAttribute;
							try
							{
								if(cma.ColumnNames.Length == 0)
								{
									headerList.headerDict.Add(field.Name,field);
									if(DebugMode)
									{
										Debug.LogWarning("Match :<color=red> "+field.Name + "</color> -><color=white> " + field.Name+"</color>");
									}
								}
								else
								{
									foreach (var columnName in cma.ColumnNames) {

										headerList.headerDict.Add(columnName,field);
										if(DebugMode)
										{
											Debug.LogWarning("Match :<color=red> "+columnName + "</color> -><color=white> " + field.Name+"</color>");
										}
									}
								}
							}catch(System.ArgumentException ex)
							{
								Debug.LogError("Column name repeat , Field Name :  "+ field.Name +" \n"+ex.Message);
							}
						}else if(attris[i].GetType() == typeof(ColumnMatchingArrayAttribute))
						{
							ColumnMatchingArrayAttribute cma = attris[i] as ColumnMatchingArrayAttribute;
							if(cma != null)
							{
								try
								{
									if(cma.ColumnNames.Length == 0)
									{
										headerList.arrayHeaderDict.Add(field.Name,field);
										if(DebugMode)
										{
											Debug.LogWarning("Match array :<color=red> "+field.Name + "</color> -><color=white> " + field.Name+"</color>");
										}
									}
									else
									{
										foreach (var columnName in cma.ColumnNames) {
											headerList.arrayHeaderDict.Add(columnName,field);
											if(DebugMode)
											{
												Debug.LogWarning("Match array :<color=red> "+columnName + "</color> -><color=white> " + field.Name+"</color>");

											}
										}
									}
								}catch(System.ArgumentException ex)
								{
									Debug.LogError("Column name repeat , Field Name :  "+ field.Name +" \n"+ex.Message);

								}

							}
						}else if(attris[i].GetType() == typeof(ColumnMatchingClassAttribute))
						{
							headerList.classFieldInfo.Add(field);
							if(DebugMode)
							{
								Debug.LogWarning("Match  class  : <color=white> " + field.Name+"</color>");
							}
						}else if(attris[i].GetType() == typeof(ColumnMatchingClassArrayAttribute))
						{
							headerList.classArrayFieldInfo.Add(field);
							if(DebugMode)
							{
								Debug.LogWarning("Match  class array : <color=white> " + field.Name+"</color>");
							}
						}
					}
				}

				cachedEntityHeaderList.Add(t,headerList);
			}
			return headerList;
		}

		public static List<T> Parse<T>(byte[] bytes ) where T : class,new()
		{
			return Parse<T>(bytes,SPLIT_CHARHT);
		}


		public static List<T> Parse<T>(byte[] bytes,char splitChar ) where T : class,new()
		{
			List<T> returnList = new List<T>();

			Type t = typeof(T);

			HeaderList headerList = ParseEntityHeaderField(t);

			using(MemoryStream memoryStream = new MemoryStream(bytes))
			{
				using(StreamReader streamReader = new StreamReader(memoryStream,System.Text.Encoding.UTF8))
				{


					string headerText = streamReader.ReadLine();
					if(!string.IsNullOrEmpty( headerText ))
					{					
						headerText = headerText.Replace("\r\n","").Replace("\n","").Trim();

						string[] headerTexts = headerText.Split(splitChar);

						FieldInfo[] headerFieldInfo = new FieldInfo[headerTexts.Length];

						for (int i = 0; i < headerTexts.Length; i++) {

							if(headerList.headerDict.ContainsKey(headerTexts[i]))
							{
								headerFieldInfo[i] = headerList.headerDict[headerTexts[i]];
							}else if(headerList.arrayHeaderDict.ContainsKey(headerTexts[i]))
							{
								headerFieldInfo[i] = headerList.arrayHeaderDict[headerTexts[i]];
							}
							else
							{
								if(DebugMode)
								{
									Debug.LogWarning(headerTexts[i] + " Field Not Match");
								}
							}
						}
						string content = null;
						int rowCount = -1;
						while( (content = streamReader.ReadLine()) != null)
						{
							rowCount ++;
							if(!string.IsNullOrEmpty(content))
							{
								string[] contents = content.Split(splitChar);

								T temp = (T)ParseEntity(new T(),headerList,headerFieldInfo,
									headerTexts,contents);
									returnList.Add(temp);
							}
						}



					}
				}
			}



			return returnList;
		}

		public static object ChangeType(string input,Type type)
		{
			if(importerDict.ContainsKey(type))
			{
				return importerDict[type].Invoke(input);
			}else
			{
				return System.Convert.ChangeType(input,type);
			}
		}

		public static IList CreateGeneric(Type typeName)
		{
			Type generic = typeof(List<>);
			var list = Activator.CreateInstance(generic.MakeGenericType(typeName)) as IList;
			return list;
		}

		public struct ArrayField
		{
			public Dictionary<string,List<int>> classDict;
			public int classMaxCount;
		}

		public struct HeaderList
		{
			public List<FieldInfo> classFieldInfo;
			public List<FieldInfo> classArrayFieldInfo ;
			public Dictionary<string,FieldInfo> headerDict;
			public Dictionary<string,FieldInfo> arrayHeaderDict ;
			public FieldInfo keyField ;
		}

	}

}