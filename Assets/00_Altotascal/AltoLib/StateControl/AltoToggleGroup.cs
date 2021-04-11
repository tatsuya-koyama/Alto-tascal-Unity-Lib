using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public interface IAltoToggleItem
    {
        void OnToggleSelect();
        void OnToggleDeselect();
    }

    public class AltoToggleItem : IAltoToggleItem
    {
        public virtual void OnToggleSelect() {}
        public virtual void OnToggleDeselect() {}
    }

    /// <summary>
    /// 複数のオプションから 1 個を選択するステート制御
    /// </summary>
    public class AltoToggleGroup<T> where T : IAltoToggleItem
    {
        const int NoSelect = -1;

        public int selectedIndex { get; private set; } = NoSelect;
        public T selectedItem { get; private set; } = default;

        List<T> _items = new List<T>();

        public void AddItem(T item)
        {
            _items.Add(item);
        }

        public void AddItems(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        public void Select(int index, bool enableReselect = false)
        {
            if (index < 0 || _items.Count <= index)
            {
                Debug.LogError($"Invalid index : {index}");
                return;
            }
            if (!enableReselect && selectedIndex == index) { return; }

            if (selectedIndex != NoSelect)
            {
                selectedItem.OnToggleDeselect();
            }
            selectedIndex = index;
            selectedItem  = _items[index];
            selectedItem.OnToggleSelect();
        }
    }
}
