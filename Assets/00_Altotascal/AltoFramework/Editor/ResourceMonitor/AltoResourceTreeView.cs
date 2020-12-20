#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AltoFramework.Editor
{
    /// <summary>
    ///   TreeView の列に渡すデータ
    /// </summary>
    public class AltoResourceTreeViewItem : TreeViewItem
    {
        public bool   isGlobal;
        public string category;
        public string assetName;
        public long   memorySize;
        public string info;

        public AltoResourceTreeViewItem(int id) : base(id) {}
    }

    /// <summary>
    ///   ロード中のリソース一覧をリスト表示するマルチカラム TreeView
    /// </summary>
    public class AltoResourceTreeView : TreeView
    {
        List<AltoResourceTreeViewItem> _items = new List<AltoResourceTreeViewItem>();
        AltoResourceCollector _collector = new AltoResourceCollector();

        public long TotalMemory()
        {
            long memorySum = 0;
            _items.ForEach(item => memorySum += item.memorySize);
            return memorySum;
        }

        public void WatchAll()
        {
            var items = _collector.CollectAll();
            if (items == null) { return; }

            _items = items;
            ReloadAndSort();
        }

        public void WatchSpriteAtlas()
        {
            var items = _collector.CollectSprites();
            if (items == null) { return; }

            _items = items;
            ReloadAndSort();
        }

        public void WatchScriptableObjects()
        {
            var items = _collector.CollectScriptableObjects();
            if (items == null) { return; }

            _items = items;
            ReloadAndSort();
        }

        public void WatchAudioClips()
        {
            var items = _collector.CollectAudioClips();
            if (items == null) { return; }

            _items = items;
            ReloadAndSort();
        }

        //----------------------------------------------------------------------
        // GUI implementation
        //----------------------------------------------------------------------

        static readonly MultiColumnHeaderState.Column[] headerColumns = new[]
        {
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Scope"),      width = 10 },
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Category"),   width = 15 },
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Id"),         width =  5 },
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Asset Name"), width = 20 },
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Memory"),     width = 10 },
            new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Info"),       width = 15 },
        };

        public AltoResourceTreeView(TreeViewState state)
            : this(state, new MultiColumnHeader(new MultiColumnHeaderState(headerColumns)))
        {
        }

        AltoResourceTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            header.sortingChanged += OnSortingChanged;
            header.ResizeToFit();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            if (!EditorApplication.isPlaying)
            {
                var emptyItem = new List<TreeViewItem>();
                SetupParentsAndChildrenFromDepths(root, emptyItem);
                return root;
            }

            root.children = _items.Cast<TreeViewItem>().ToList();
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AltoResourceTreeViewItem;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var rect = args.GetCellRect(i);
                var colIndex = args.GetColumn(i);
                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                switch (colIndex)
                {
                    case 0:
                        string scope = item.isGlobal ? "- Global -" : "Scene";
                        EditorGUI.LabelField(rect, scope, labelStyle);
                        break;
                    case 1: EditorGUI.LabelField(rect, item.category,      labelStyle); break;
                    case 2: EditorGUI.LabelField(rect, item.id.ToString(), labelStyle); break;
                    case 3: EditorGUI.LabelField(rect, item.assetName,     labelStyle); break;
                    case 4:
                        string memorySize = item.memorySize > 0
                            ? (item.memorySize / 1024f / 1024f).ToString("0.000") + " MB"
                            : "-";
                        EditorGUI.LabelField(rect, memorySize, labelStyle);
                        break;
                    case 5: EditorGUI.LabelField(rect, item.info, labelStyle); break;
                }
            }
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            int index = multiColumnHeader.sortedColumnIndex;
            if (index == -1) { return; }
            bool isAsc = multiColumnHeader.IsSortedAscending(index);

            var items = rootItem.children.Cast<AltoResourceTreeViewItem>();
            IOrderedEnumerable<AltoResourceTreeViewItem> orderedItems = null;
            switch (index)
            {
                case 0:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.isGlobal)
                        : items.OrderByDescending(item => item.isGlobal);
                    break;
                case 1:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.category)
                        : items.OrderByDescending(item => item.category);
                    break;
                case 2:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.id)
                        : items.OrderByDescending(item => item.id);
                    break;
                case 3:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.assetName)
                        : items.OrderByDescending(item => item.assetName);
                    break;
                case 4:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.memorySize)
                        : items.OrderByDescending(item => item.memorySize);
                    break;
                case 5:
                    orderedItems = isAsc
                        ? items.OrderBy(item => item.info)
                        : items.OrderByDescending(item => item.info);
                    break;
            }

            _items = orderedItems.ToList();
            rootItem.children = _items.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        void ReloadAndSort()
        {
            var currentSelected = this.state.selectedIDs;
            Reload();
            OnSortingChanged(this.multiColumnHeader);
            this.state.selectedIDs = currentSelected;
        }

    }
}
#endif
