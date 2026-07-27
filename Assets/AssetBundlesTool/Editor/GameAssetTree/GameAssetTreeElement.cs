using System;
using UnityEngine;

namespace UnityEditor.GameAssetView
{
	[Serializable]
	internal class GameAssetTreeElement : TreeElement {

		public System.DateTime lastModifyTime;
		public string versionNum;
		public System.DateTime verNumLastModifyTime;
		public GameFileVersionConfig[] files;

		public string path;

		public bool enabled;

		public String VerNumLastModifyTime
		{
			get {
				string str = string.Empty;
				if (verNumLastModifyTime != null && verNumLastModifyTime.Year >= 2000) {
					str = verNumLastModifyTime.ToString ("yyyy-MM-dd HH:mm:ss");
				}
				return str;
			}
		}

		public string LastModifyTime
		{
			get {
				string str = string.Empty;
				if (lastModifyTime != null && lastModifyTime.Year >= 2000) {
					str = lastModifyTime.ToString ("yyyy-MM-dd HH:mm:ss");
				}
				return str;
			}
		}

		public GameAssetTreeElement (string name, int depth, int id) : base (name, depth, id)
		{

			enabled = false;
		}
	
	}
}