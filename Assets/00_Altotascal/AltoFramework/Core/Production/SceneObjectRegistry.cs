using System;
using System.Collections.Generic;
using UnityEngine;

namespace AltoFramework.Production
{
    /// <summary>
    /// Register() したものが Require() で取得できる。
    /// 登録状況はシーン遷移時にクリアされる。
    /// Register() していなかった場合は代わりに Object.FindAnyObjectByType() で取得する
    /// （Register() は不要だが検索負荷がかかる）
    /// </summary>
    public class SceneObjectRegistry : ISceneObjectRegistry
    {
        readonly Dictionary<Type, Component> _objects = new();

        public SceneObjectRegistry(ISceneDirector sceneDirector)
        {
            sceneDirector.sceneLoading += OnSceneLoading;
        }

        void OnSceneLoading()
        {
            _objects.Clear();
        }

        public void Register<T>(T obj) where T : Component
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Type objectType = typeof(T);
            Component registeredObject;
            if (_objects.TryGetValue(objectType, out registeredObject))
            {
                if (registeredObject == obj)
                {
                    return;
                }

                if (registeredObject != null)
                {
                    throw new InvalidOperationException(
                        $"Scene object is already registered : {objectType.FullName}"
                    );
                }

                // Destroy 済み（null 判定）の Unity オブジェクトが残っていた場合は置き換える
                _objects[objectType] = obj;
                return;
            }

            _objects.Add(objectType, obj);
        }

        public T Require<T>() where T : Component
        {
            Type objectType = typeof(T);
            Component registeredObject;
            if (_objects.TryGetValue(objectType, out registeredObject))
            {
                if (registeredObject != null)
                {
                    return (T)registeredObject;
                }

                // Destroy 済み（null 判定）の Unity オブジェクトはキャッシュから除外
                _objects.Remove(objectType);
            }

            //----- 未登録なら検索（この結果はキャッシュには乗らない）
            T foundObject = UnityEngine.Object.FindAnyObjectByType<T>();
            if (foundObject == null)
            {
                throw new InvalidOperationException(
                    $"Required scene object is not found : {objectType.FullName}"
                );
            }
            return foundObject;
        }
    }
}
