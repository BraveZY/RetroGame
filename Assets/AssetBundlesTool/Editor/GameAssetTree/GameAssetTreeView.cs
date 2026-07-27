using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace UnityEditor.GameAssetView
{
	internal class GameAssetTreeView  :  TreeViewWithTreeModel<GameAssetTreeElement>{

		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		public bool showControls = true;


		// All columns
		enum MyColumns
		{
			Name,
			VersionNum,
			VersionNumLastModifyTime,
			Value1,
			Value3,
		}

		public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();

			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		public GameAssetTreeView (TreeViewState state,
			MultiColumnHeader multicolumnHeader, 
			TreeModel<GameAssetTreeElement> model) : base (state, multicolumnHeader, model)
		{
			// Custom setup
			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			extraSpaceBeforeIconAndLabel = kToggleWidth;

			Reload();
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
		{
			var columns = new[] 
			{
				new MultiColumnHeaderState.Column 
				{
					headerContent = new GUIContent("名称"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Center,
					width = 300, 
					minWidth = 300,
					autoResize = false,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column 
				{
					headerContent = new GUIContent("版本号"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Center,
					width = 60, 
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column 
				{
					headerContent = new GUIContent("版本号修改时间"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Center,
					width = 150, 
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column 
				{
					headerContent = new GUIContent("最后修改时间"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Center,
					width = 150, 
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false
				},
			};



			var state =  new MultiColumnHeaderState(columns);
			return state;
		}

		// Note we We only build the visible rows, only the backend has the full tree information. 
		// The treeview only creates info for the row list.
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows (root);
			return rows;
		}

		protected override void RowGUI (RowGUIArgs args)
		{
			var item = (TreeViewItem<GameAssetTreeElement>) args.item;

			for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
			}
		}

		void CellGUI (Rect cellRect, TreeViewItem<GameAssetTreeElement> item, MyColumns column, ref RowGUIArgs args)
		{
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
			case MyColumns.Name:
				{
					// Do toggle
					Rect toggleRect = cellRect;
					toggleRect.x += GetContentIndent(item);
					toggleRect.width = kToggleWidth;
					if(item.depth == 0)
					{
						if (toggleRect.xMax < cellRect.xMax)
							item.data.enabled = EditorGUI.Toggle(toggleRect, item.data.enabled); // hide when outside cell rect
					}

					// Default icon and label
					args.rowRect = cellRect;
					base.RowGUI(args);
				}
				break;
			case MyColumns.VersionNum:
				{
					DefaultGUI.Label (cellRect, item.data.versionNum, false, false);
				}
				break;
			case MyColumns.VersionNumLastModifyTime:
				{
					DefaultGUI.Label (cellRect, item.data.VerNumLastModifyTime, false, false);
				}
				break;
			case MyColumns.Value1:
				{
					DefaultGUI.Label (cellRect, item.data.LastModifyTime, false, false);
				}
				break;

			}
		}
	}
}