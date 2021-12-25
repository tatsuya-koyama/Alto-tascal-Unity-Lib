using AltoFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class LevelDesignUtil : EditorWindow
    {
        [MenuItem("Alto/Level Design Util")]
        static void ShowWindow()
        {
            var window = CreateInstance<LevelDesignUtil>();
            window.titleContent = new GUIContent("LDUtil");
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("選択 obj の上位 2 つ\nを基準に等間隔に整列", GUILayout.MinHeight(40)))
            {
                ArrangeByFirstTwo();
            }
        }

        /// <summary>
        /// 選択したオブジェクトを、選択したもののうち Hierarchy の index の若い 2 つの座標
        /// を基準にして等間隔に並べる。（選択順が取得できなかったのでこんな仕様）
        /// </summary>
        void ArrangeByFirstTwo()
        {
            var objs = Selection.gameObjects.OrderBy(go => go.transform.GetSiblingIndex()).ToList();
            if (objs.Count <= 2)
            {
                AltoLog.Error("3 つ以上のオブジェクトを選択してください");
                return;
            }

            var first  = objs[0];
            var second = objs[1];
            Vector3 diff = second.transform.position - first.transform.position;
            for (int i = 2; i < objs.Count; ++i)
            {
                Vector3 pos = first.transform.position + (i * diff);
                Undo.RecordObject(objs[i].transform, "ArrangeByFirstTwo");
                objs[i].transform.position = pos;
            }
        }
    }
}
