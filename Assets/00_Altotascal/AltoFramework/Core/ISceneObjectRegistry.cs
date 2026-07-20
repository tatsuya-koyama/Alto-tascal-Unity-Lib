using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    /// Scene 上のオブジェクトを型指定で取得するための接続口。
    /// シーンのエントリポイントで必要な GameObject を取得するといった用途を想定。
    /// </summary>
    public interface ISceneObjectRegistry
    {
        /// <summary>
        /// Scene 上のオブジェクトを型ごとに登録する。
        /// 同じ型に異なるオブジェクトを複数登録することはできない
        /// </summary>
        void Register<T>(T obj) where T : Component;

        /// <summary>
        /// 登録済みのオブジェクトを取得する。
        /// 未登録の場合は Scene から検索（見つからなければ例外）
        /// </summary>
        T Require<T>() where T : Component;
    }
}
