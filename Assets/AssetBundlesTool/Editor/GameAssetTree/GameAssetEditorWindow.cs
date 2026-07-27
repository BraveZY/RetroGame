using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace UnityEditor.GameAssetView
{
	public class GameAssetEditorWindow  :  EditorWindow{

		public bool checkMD5 = false;
		[NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		GameAssetTreeView m_TreeView;
		IList<GameAssetTreeElement> m_currentList;

		static string AssetFolderName
		{
			get
			{ 
				return "gameassets";
			}
		}

		static string AssetBundlePath
		{
			get {
				return "/AssetBundles/" + ResourceManager.GetPlatformName () + "/"+AssetFolderName;
			}
		}

		static string BundleAppContentPath 
		{
			get {
				string dataPath = Application.dataPath;
				dataPath = dataPath.Replace ("\\", "/");
				return dataPath.Substring (0, dataPath.LastIndexOf ("/")) + AssetBundlePath;
			}
		}

		static string BundleParentPath
		{
			get {
				string dataPath = Application.dataPath;
				dataPath = dataPath.Replace ("\\", "/");
				return dataPath.Substring (0, dataPath.LastIndexOf ("/")) + "/AssetBundles/";
			}
		}



		Rect multiColumnTreeViewRect
		{
			get { return new Rect(20, 30, position.width-40, position.height-60); }
		}

		Rect toolbarRect
		{
			get { return new Rect (20f, 10f, position.width-40f, 20f); }
		}

		Rect bottomToolbarRect
		{
			get { return new Rect(20f, position.height - 18f, position.width - 40f, 16f); }
		}

		internal GameAssetTreeView treeView
		{
			get { return m_TreeView; }
		}

		void InitIfNeeded ()
		{
			if (!m_Initialized)
			{
				// Check if it already exists (deserialized from window layout file or scriptable object)
				if (m_TreeViewState == null)
					m_TreeViewState = new TreeViewState();

				bool firstInit = m_MultiColumnHeaderState == null;
				var headerState = GameAssetTreeView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
				if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
					MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
				m_MultiColumnHeaderState = headerState;

				var multiColumnHeader = new MyMultiColumnHeader(headerState);
				multiColumnHeader.mode = MyMultiColumnHeader.Mode.MinimumHeaderWithoutSorting;
				if (firstInit)
					multiColumnHeader.ResizeToFit ();
				m_currentList = GetData ();
				var treeModel = new TreeModel<GameAssetTreeElement>(m_currentList);

				m_TreeView = new GameAssetTreeView(m_TreeViewState, multiColumnHeader, treeModel);

				foreach (var item in m_currentList) {
					if (item.enabled == true) {
						SetParentEnabled (item, 0);
					}
				}

				checkMD5 = false;
				m_Initialized = true;
			}
		}

		void SetParentEnabled(GameAssetTreeElement element,int lastDepth)
		{
			if (element.parent != null) {
				var parent = (element.parent as GameAssetTreeElement);
				parent.enabled = true;
				treeView.SetExpanded (parent.id, true);
				if (element.parent.depth != lastDepth) {
					SetParentEnabled (parent, lastDepth);
				} 
			}
		}

		IList<GameAssetTreeElement> GetData ()
		{
			List<GameAssetTreeElement> list = new List<GameAssetTreeElement> ();
			int IDCounter = 0;
			var root = new GameAssetTreeElement ("Root", -1, IDCounter++);

			list.Add (root);
			if (Directory.Exists (BundleAppContentPath)) {
				string[] directories = Directory.GetDirectories (BundleAppContentPath);

				for (int i = 0; i < directories.Length; i++) {
					var direct = new GameAssetTreeElement (directories [i].Substring(directories[i].LastIndexOf('\\')+1), root.depth + 1, IDCounter++);
					direct.versionNum = GetGameAssetsVersion (direct.name);
					direct.verNumLastModifyTime = GetGameAssetsVersionTime (direct.name);
					if (string.IsNullOrEmpty (direct.versionNum) == false) {
						direct.files = AssetBundleFilesUtils.GetAllFiles (GetGameAssetsFileName (direct.name));
					}
					direct.path = directories [i];
					list.Add (direct);
					AddChildrenRecursive (direct, directories [i], list,ref IDCounter);
				}

				for (int i = 0; i < list.Count; i++) {
					var item = list [i];
					if (item.depth > 0) {
						GameAssetTreeElement element = GetParentElement (item, 0) as GameAssetTreeElement;
						if (element != null) {
							if ((item.lastModifyTime - element.lastModifyTime).TotalSeconds > 0) {
								element.lastModifyTime = item.lastModifyTime;
							}
						}
					}
				}
			}


			return list;
		}

		TreeElement GetParentElement(TreeElement element,int parentDepth)
		{
			if (element.parent != null) {
				if (element.parent.depth == parentDepth) {
					return element.parent;
				} else {
					if (element.parent.depth < 0) {
						return null;
					} else {
						GetParentElement (element.parent, parentDepth);
					}
				}
			}
			return null;
		}

		void AddChildrenRecursive(GameAssetTreeElement root,string path,List<GameAssetTreeElement> list,ref int IDCounter)
		{
			var files = Directory.GetFiles (path);

			for (int i = 0; i < files.Length; i++) {
				if (Path.GetExtension (files [i]) != ".manifest") {
					var file = new GameAssetTreeElement (files [i].Substring (files [i].LastIndexOf ('\\') + 1), root.depth + 1, IDCounter++);
					var parent = GetParentElement (root, 0) as GameAssetTreeElement;
					if (parent != null) {
						if (parent.files != null) {
							string assetPath = files [i].Replace ("\\", "/").Replace(BundleParentPath,"");
							var config = (AssetBundleFilesUtils.GetConfig (parent.files, assetPath));
								
							if (config != null) {
								if (checkMD5 && config.md5text != MD5Utils.GetMD5HashFromFile (files [i])) {
									file.versionNum = "已修改";
									file.enabled = true;
								} else {
									file.versionNum = parent.versionNum;
								}
							} else {
								file.versionNum = "新增文件";
								file.enabled = true;
							}
						}

					}
//					file.versionNum = ;
					file.path = files [i];
					file.lastModifyTime = File.GetLastWriteTime (files [i]);
					file.parent = root;
					list.Add (file);
				}
			}

			var directories = Directory.GetDirectories (path);
			for (int i = 0; i < directories.Length; i++) {
				var direct = new GameAssetTreeElement (directories [i].Substring(directories[i].LastIndexOf('\\')+1), root.depth + 1, IDCounter++);
//				direct.versionNum = root.versionNum;
				direct.lastModifyTime = Directory.GetLastWriteTime (directories [i]);
				direct.path = directories [i];
				direct.parent = root;
				list.Add (direct);
				AddChildrenRecursive (direct, directories [i], list,ref IDCounter);
			}
		}

		void OnGUI ()
		{
			InitIfNeeded();

			DoTreeView (multiColumnTreeViewRect);
			BottomToolBar (bottomToolbarRect);
		}

		void DoTreeView (Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}

		void BottomToolBar (Rect rect)
		{
			GUILayout.BeginArea (rect);

			using (new EditorGUILayout.HorizontalScope ())
			{

				var style = "miniButton";
				if (GUILayout.Button("All", style))
				{
					for (int i = 0; i < m_currentList.Count; i++) {
						if (m_currentList [i].depth == 0) {
							m_currentList [i].enabled = true;
						}
					}
				}

				if (GUILayout.Button("None", style))
				{
					for (int i = 0; i < m_currentList.Count; i++) {
						if (m_currentList [i].depth == 0) {
							m_currentList [i].enabled = false;
						}
					}
				}

				if (GUILayout.Button("Expand All", style))
				{
					treeView.ExpandAll ();
				}

				if (GUILayout.Button("Collapse All", style))
				{
					treeView.CollapseAll ();
				}

				GUILayout.Label ("Path:", "minilabel");
				GUILayout.Label ( AssetBundlePath, "minilabel");
				GUILayout.FlexibleSpace();

				GUILayout.Label ("Tools: ", "minilabel");
				if (GUILayout.Button("Refresh MD5", style))
				{
					treeView.CollapseAll ();
					checkMD5 = true;
					m_Initialized = false;
				}
				if (GUILayout.Button("Build MD5", style))
				{
					BuildMD5 ();
				}
				if (GUILayout.Button("Only Build Script", style))
				{
					OnlyBuildScript ();
				}

			}

			GUILayout.EndArea();
		}

		void BuildMD5()
		{
			for (int i = 0; i < m_currentList.Count; i++) {
				var item = m_currentList [i];
				if (item.depth == 0 && item.enabled) {
					BuildGameVersion (item);

				}
			}
			m_Initialized = false;
		}

		void BuildGameVersion(GameAssetTreeElement element)
		{
			List<string> paths = new List<string>();
			List<string> files = new List<string>();
			FileUtils.RecursiveDirectory(element.path, null, ref paths, ref files);

			List<string> scenes = new List<string> ();
			foreach (var item in files) {
				if (item.EndsWith (".manifest") == false) {
					string directoryName = Path.GetDirectoryName (item);
					if (directoryName.EndsWith ("scenes")) {
						scenes.Add (AssetFolderName + item.Replace (BundleAppContentPath, ""));
					}
				}
			}
			string dataPath = Application.dataPath;
			dataPath = dataPath.Replace ("\\", "/");
			string assetBundleFolderPath = string.Format ("{0}/AssetBundles/{1}/",
				                               dataPath.Substring (0, dataPath.LastIndexOf ("/")),
				                               GetOS ());
			foreach (var item in scenes) {
				string[] dependencies = AssetDatabase.GetAssetBundleDependencies (item, true);
				foreach (var assetPath in dependencies) {
					string fullPath = assetBundleFolderPath + assetPath;
					if (files.Contains (fullPath) == false) {
						files.Add (fullPath);
					} else {
					}
				}
			}

			long totalFileSize = 0;
			StringBuilder stringBuilder = new StringBuilder("filename,md5text,fileSize") ;
			stringBuilder.AppendLine();
			string parentPath = GetOS () + "/" + AssetFolderName;
			foreach (var item in files) {
				if (File.Exists (item)) {
					if(item.EndsWith(".manifest") == false)
					{
						if (item.ToLower ().StartsWith (BundleAppContentPath.ToLower ())) {
							stringBuilder.Append (parentPath + item.Replace (BundleAppContentPath, ""));
						} else {
							stringBuilder.Append (GetOS () + "/"+item.Replace (assetBundleFolderPath, ""));
						}
						stringBuilder.Append (",");
						stringBuilder.Append (MD5Utils.GetMD5HashFromFile (item));
						stringBuilder.Append (",");

						FileInfo fileInfo = new FileInfo (item);
						totalFileSize += fileInfo.Length;
						stringBuilder.Append (fileInfo.Length.ToString());
						stringBuilder.AppendLine ();
					}
				} else {
					Debug.LogError ("File is not exists , " + item);
				}
			}

			string oldStr = null;
			string gameAssetPathParent = GetGameAssetsVersionFileName (element.name);
			string filePath = gameAssetPathParent+"_"+ AppConst.BundleFileName;
			if(File.Exists(filePath))
			{
				string t = string.Format ("{0}/AssetBundles/",
					dataPath.Substring (0, dataPath.LastIndexOf ("/")));

				CheckDirectory (t + "ChangeList/");
				oldStr = t + "ChangeList/" + Path.GetFileName (filePath) +System.DateTime.Now.ToString("yyMMddhhmmss");
				File.Move(filePath,
					oldStr);
				GetChangedList(stringBuilder.ToString(),oldStr,element.name);
			}

			File.WriteAllText (filePath, stringBuilder.ToString ());

			AddVersion(gameAssetPathParent);

		}

		static void AddVersion(string gameAssetPathParent)
		{
			string path = gameAssetPathParent+"_version.txt";
			int version = 1;
			string versionStr = "";
			if(File.Exists(path))
			{
				versionStr = File.ReadAllText(path);
				if(int.TryParse(versionStr,out version))
				{
					version ++;
				}
			}
			File.WriteAllText (path, version.ToString ());
			Debug.Log(path + "  | 版本号变更 : " + versionStr + "  ->  "+version);
		}

		static void GetChangedList(string text,string oldTextPath,string assetName)
		{
			string oldText = File.ReadAllText(oldTextPath);
			var newFiles = GetGameFileVersionConfig(text);
			var oldFiles = GetGameFileVersionConfig(oldText);
			StringBuilder str = new StringBuilder();
			double totalFileSize = 0;
			int changeCount = 0;
			foreach (GameFileVersionConfig item in newFiles) {
				var t = oldFiles.FirstOrDefault( a => a.filename == item.filename);

				if(t != null)
				{
					if(t.md5text != item.md5text)
					{
						str.Append("Change ");
						str.Append(t.filename);
						str.AppendLine();
						str.Append(item.md5text);
						str.AppendLine();
						str.Append(t.md5text);
						str.AppendLine();
						changeCount++;
						totalFileSize += item.fileSize;
					}
				}else
				{
					str.Append("New ");
					str.Append(item.filename);
					str.AppendLine();
					str.Append(item.md5text);
					str.AppendLine();
					changeCount++;
					totalFileSize += item.fileSize;
				}
			}
			string dataPath = Application.dataPath;
			dataPath = dataPath.Replace ("\\", "/");
			string assetBundleFolderPath = string.Format ("{0}/AssetBundles/",
				dataPath.Substring (0, dataPath.LastIndexOf ("/")));

			CheckDirectory (assetBundleFolderPath + "ChangeList/");
			File.WriteAllText (assetBundleFolderPath+ "ChangeList/" + GetOS()+"_"+assetName+"_Change.txt", str.ToString ());
//			File.WriteAllText (assetBundleFolderPath + GetOS()+"_"+assetName+"_Change.txt", str.ToString ());
			Debug.Log ("Change files count : " +changeCount+" ,Size : " + (totalFileSize/1024/1024f).ToString("f2") + " MB");
		}
		static void CheckDirectory(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists)
			{
				Debug.Log("Create Directory : " + di.FullName);
				di.Create();
			}
		}

		static public GameFileVersionConfig[] GetGameFileVersionConfig(string text)
		{
			string[] Array = text.Split(new string[] { ",", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
			int ClassLength = GameFileVersionConfig.Length;
			int Length = Array.Length / ClassLength - 1;
			Length = Length < 0 ? 0 : Length;
			GameFileVersionConfig[] tempGameFileVersionConfig = new GameFileVersionConfig[Length];
			for (int i = 0; i < Length; i++)
			{
				tempGameFileVersionConfig[i] = new GameFileVersionConfig();
				tempGameFileVersionConfig[i].filename = Array[(i + 1) * ClassLength];
				tempGameFileVersionConfig[i].md5text = Array[(i + 1) * ClassLength + 1];
				int.TryParse(Array[(i + 1) * ClassLength + 2], out tempGameFileVersionConfig[i].fileSize);
				//			int.TryParse(Array[(i+1)*ClassLength+3],out tempGameFileVersionConfig[i].useLocal);
			}
			return tempGameFileVersionConfig;
		}
		static string GetOS()
		{
			return ResourceManager.GetPlatformName();
		}

		string GetGameAssetsVersionFileName(string gameassetsName)
		{
			string dataPath = Application.dataPath;
			dataPath = dataPath.Replace ("\\", "/");
			return string.Format ("{0}/AssetBundles/{1}_{2}",
				dataPath.Substring (0, dataPath.LastIndexOf ("/")),
				GetOS(),
				gameassetsName);
		}

		string GetGameAssetsVersion(string gameassetsName)
		{
			string gameAssetPathParent = GetGameAssetsVersionFileName (gameassetsName);
			string path = gameAssetPathParent+"_version.txt";
			string versionStr = "";
			if(File.Exists(path))
			{
				versionStr = File.ReadAllText(path);
			}
			return versionStr;
		}

		string GetGameAssetsFileName(string gameassetsName)
		{
			string gameAssetPathParent = GetGameAssetsVersionFileName (gameassetsName);
			string path = gameAssetPathParent+"_files.txt";
			return path;
		}

		DateTime GetGameAssetsVersionTime(string gameassetsName)
		{
			string gameAssetPathParent = GetGameAssetsVersionFileName (gameassetsName);
			string path = gameAssetPathParent+"_version.txt";
			if(File.Exists(path))
			{
				return File.GetLastWriteTime (path);
			}
			return DateTime.MinValue;
		}


		void OnlyBuildScript()
		{
			if (EditorUtility.DisplayDialog ("Delete files", "打包Lua脚本将会清理文件夹Assets/Lua", "确定打包", "取消")) {
				string tempLuaDir = Application.dataPath + "/" + AppConst.LuaTempDir;
				if (Directory.Exists (tempLuaDir)) {
					Directory.Delete (tempLuaDir, true);
				}
				for (int i = 0; i < m_currentList.Count; i++) {
					var item = m_currentList [i];
					if (item.depth == 0 && item.enabled) {
						BuildGameScript (item);

					}
				}
				m_Initialized = false;
			}
		}

		void BuildGameScript(GameAssetTreeElement item)
		{
			
			BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets |
				BuildAssetBundleOptions.DeterministicAssetBundle ;
			string tempLuaDir = Application.dataPath + "/" + AppConst.LuaTempDir;
			if (Directory.Exists (tempLuaDir) == false) {
				Directory.CreateDirectory (tempLuaDir);
			}
			CopyLuaBytesFiles(AppConst.FrameworkRoot + "/Lua/Games/"+item.name, tempLuaDir);
			AssetDatabase.Refresh();
			string bundleName = item.name;
			string path = "Assets/" + AppConst.LuaTempDir;
			string[] files = Directory.GetFiles(path, "*.lua.bytes", SearchOption.TopDirectoryOnly);

			List<UnityEngine.Object> list = new List<UnityEngine.Object>();

			for (int i = 0; i < files.Length; i++)
			{
				LuaBundlePath bundlePath = new LuaBundlePath();
				bundlePath.bundleName = bundleName;
				bundlePath.fileName = Path.GetFileNameWithoutExtension(files[i]);

				bundlePath = new LuaBundlePath();
				bundlePath.bundleName = bundleName;
				bundlePath.fileName = bundleName + "/" + Path.GetFileNameWithoutExtension(files[i]);

				UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(files[i]);
				list.Add(obj);
			}
			if (files.Length > 0)
			{
				string output = BundleAppContentPath +"/"+bundleName+"/script/" + bundleName + ".unity3d"; //Application.streamingAssetsPath + "/bundle/lua/" 
				string p = BundleAppContentPath +"/"+bundleName+"/script/";
				if (File.Exists(output))
				{
					File.Delete(output);
				}
				if (Directory.Exists (p) == false) {
					Directory.CreateDirectory(p);
				}
				BuildPipeline.BuildAssetBundle(null, list.ToArray(), output, options, EditorUserBuildSettings.activeBuildTarget);

				Directory.Delete(tempLuaDir, true);
				AssetDatabase.Refresh();
			}

		}

		public class LuaBundlePath
		{
			public string fileName;
			public string bundleName;
		}
		static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true)
		{
			if (!Directory.Exists(sourceDir))
			{
				return;
			}

			string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
			int len = sourceDir.Length;

			if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
			{
				--len;
			}

			for (int i = 0; i < files.Length; i++)
			{
				string str = files[i].Remove(0, len);
				string dest = destDir + str;
				if (appendext) dest += ".bytes";
				string dir = Path.GetDirectoryName(dest);
				Directory.CreateDirectory(dir);

				if (AppConst.LuaByteMode)
				{
					EncodeLuaFile(files[i], dest);
				}
				else
				{
					File.Copy(files[i], dest, true);
				}
			}
		}

		/// <summary>
		/// 编码Lua File
		/// </summary>
		/// <param name="srcFile"></param>
		/// <param name="outFile"></param>
		public static void EncodeLuaFile(string srcFile, string outFile)
		{
			if (!srcFile.ToLower().EndsWith(".lua"))
			{
				//M3MFileUtil.CopyFile(srcFile, outFile);33333333333333333333
				return;
			}
			bool isWin = true;
			string luaexe = string.Empty;
			string args = string.Empty;
			string exedir = string.Empty;
			string currDir = Directory.GetCurrentDirectory();
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				isWin = true;
				luaexe = "luajit.exe";
				args = "-b " + srcFile + " " + outFile;
				exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				isWin = false;
				luaexe = "./luac";
				args = "-o " + outFile + " " + srcFile;
				exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
			}
			Directory.SetCurrentDirectory(exedir);
			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = luaexe;
			info.Arguments = args;
			info.WindowStyle = ProcessWindowStyle.Hidden;
			info.UseShellExecute = isWin;
			info.ErrorDialog = true;
			Debug.Log(info.FileName + " " + info.Arguments);

			Process pro = Process.Start(info);
			pro.WaitForExit(6000);
			Directory.SetCurrentDirectory(currDir);
		}


		/// <summary>
		/// 数据目录
		/// </summary>
		static string AppDataPath
		{
			get { return Application.dataPath.ToLower(); }
		}



	}



	internal class MyMultiColumnHeader : MultiColumnHeader
	{
		Mode m_Mode;

		public enum Mode
		{
			LargeHeader,
			DefaultHeader,
			MinimumHeaderWithoutSorting
		}

		public MyMultiColumnHeader(MultiColumnHeaderState state)
			: base(state)
		{
			mode = Mode.DefaultHeader;
		}

		public Mode mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				m_Mode = value;
				switch (m_Mode)
				{
				case Mode.LargeHeader:
					canSort = true;
					height = 37f;
					break;
				case Mode.DefaultHeader:
					canSort = true;
					height = DefaultGUI.defaultHeight;
					break;
				case Mode.MinimumHeaderWithoutSorting:
					canSort = false;
					height = DefaultGUI.minimumHeight;
					break;
				}
			}
		}

		protected override void ColumnHeaderGUI (MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
		{
			// Default column header gui
			base.ColumnHeaderGUI(column, headerRect, columnIndex);

			// Add additional info for large header
			if (mode == Mode.LargeHeader)
			{
				// Show example overlay stuff on some of the columns
				if (columnIndex > 2)
				{
					headerRect.xMax -= 3f;
					var oldAlignment = EditorStyles.largeLabel.alignment;
					EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
					GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
					EditorStyles.largeLabel.alignment = oldAlignment;
				}
			}
		}
	}
}