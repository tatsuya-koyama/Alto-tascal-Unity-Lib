using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AltoFramework
{
    class AltoTweenList
    {
        public List<AltoTween> tweens { get; } = new List<AltoTween>();
        public List<AltoTween> pendingTweens { get; } = new List<AltoTween>();  // Update ループ中に Add された Tween
        public bool isCompleted { get; private set; } = false;

        bool _isUpdating = false;

        public void Add(AltoTween tween)
        {
            if (_isUpdating)
            {
                this.pendingTweens.Add(tween);
            }
            else
            {
                this.tweens.Add(tween);
            }
            this.isCompleted = false;
        }

        public void Update(float deltaTime)
        {
            _isUpdating = true;
            bool containsUnfinished = false;
            foreach (var tween in tweens)
            {
                tween.Update(deltaTime);
                if (!tween.IsCompleted()) { containsUnfinished = true; }
            }
            this.isCompleted = !containsUnfinished;
            _isUpdating = false;

            if (this.pendingTweens.Count > 0)
            {
                this.tweens.AddRange(this.pendingTweens);
                this.pendingTweens.Clear();
                this.isCompleted = false;
            }
        }

        public void Finish()
        {
            foreach (var tween in tweens)
            {
                if (!tween.IsCompleted())
                {
                    tween.Complete();
                }
            }
        }

        public void Clear()
        {
            tweens.Clear();
            pendingTweens.Clear();
        }
    }

    /// <summary>
    /// 登録オブジェクトごとの Tween のリストを制御。
    /// （オブジェクトを指定しなかった場合は null オブジェクトに登録されたとみなされ
    ///   まとめて管理される）
    /// Tween はオブジェクト単位で動作中のものが 1 つも無くなったタイミングで
    /// まとめて削除される。そのため現状の実装では、常に新しい Tween が生み出され
    /// 常時 1 個以上の Tween が存在している場合に Tween リストが肥大化する可能性があるので注意。
    /// [ToDo] このあたりの救済策を考える
    /// </summary>
    public class AltoTweener : IAltoTweener
    {
        /// <summary>
        ///   いくつのオブジェクトについてトゥイーンを実行中か、数を返す
        ///   （null を指定したものはまとめて 1 カウント）
        /// </summary>
        public int count
        {
            get { return _tweens.Count; }
        }

        Dictionary<object, AltoTweenList> _tweens = new Dictionary<object, AltoTweenList>();

        static object _nullObject = new System.Object();

        /// <summary>
        ///   トゥイーンを登録する。obj は途中で止めたくなった時の対象指定用なので
        ///   その用途が無ければ null を渡してもよい
        /// </summary>
        public void Go(
            object obj, float from, float to, float duration,
            AltoEasingFunc easingFunc,
            AltoTweenCallback onUpdate
        )
        {
            var tween = new AltoTween(from, to, duration, easingFunc, onUpdate);
            tween.Init();
            AddTween(obj, tween);
        }

        /// <summary>
        ///   トゥイーンを作成し、それを返す。メソッドチェーン記述用
        /// </summary>
        /// <example><code>
        ///   var tweener = new AltoTweener();
        ///   tweener.NewTween(someObj).FromTo(10f, 20f, 1.0f).OnUpdate(x => ...);
        /// </code></example>
        public IAltoTween NewTween(object obj = null)
        {
            var tween = new AltoTween();
            AddTween(obj, tween);
            return tween;
        }

        public void Update(float deltaTime)
        {
            if (this.count == 0) { return; }

            bool containsCompleted = false;
            foreach (var tweenList in _tweens.Values)
            {
                tweenList.Update(deltaTime);
                if (tweenList.isCompleted) { containsCompleted = true; }
            }
            if (!containsCompleted) { return; }

            // 全て完了したトゥイーンはキーを削除
            var keys = _tweens.Keys.Where(key => _tweens[key].isCompleted).ToArray();
            foreach (var key in keys)
            {
                _tweens.Remove(key);
            }
        }

        public void Finish(object obj)
        {
            object dictKey = (obj != null) ? obj : _nullObject;
            AltoTweenList tweenList;
            if (!_tweens.TryGetValue(dictKey, out tweenList))
            {
                AltoLog.Warn($"[AltoTweener :: Finish] Target object has no tween : {dictKey}");
                return;
            }

            tweenList.Finish();
            tweenList.Clear();
            _tweens.Remove(dictKey);
        }

        public void ClearAll()
        {
            foreach (var tweenList in _tweens.Values)
            {
                tweenList.Clear();
            }
            _tweens.Clear();
        }

        void AddTween(object obj, AltoTween tween)
        {
            AltoTweenList tweenList;
            object dictKey = (obj != null) ? obj : _nullObject;
            if (_tweens.TryGetValue(dictKey, out tweenList))
            {
                tweenList.Add(tween);
                return;
            }

            tweenList = new AltoTweenList();
            tweenList.Add(tween);
            _tweens.Add(dictKey, tweenList);
        }
    }
}
