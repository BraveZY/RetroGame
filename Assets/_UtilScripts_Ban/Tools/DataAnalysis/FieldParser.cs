using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace Yu.Tools
{
	public class FieldParser  {

		public const char SPLIT_CHAR = DataParser.SPLIT_CHAR;
		public const char SPLIT_CHARHT =DataParser.SPLIT_CHARHT;

		public static Dictionary<K,T> Parse<K,T>(byte[] bytes,char splitChar) where T : class,new()
		{
			return DataParser.Parse<K,T>(bytes,splitChar);
		}

		public static Dictionary<K,T> Parse<K,T>(byte[] bytes )where T : class,new()
		{
					
			return Parse<K,T>(bytes,SPLIT_CHARHT);
		}

		public static List<T> Parse<T>(byte[] bytes )where T : class,new()
		{
			return Parse<T>(bytes,SPLIT_CHARHT);
		}

		public static List<T> Parse<T>(byte[] bytes,char splitChar )where T : class,new()
		{
			return DataParser.Parse<T>(bytes,splitChar);
		}


	}
}