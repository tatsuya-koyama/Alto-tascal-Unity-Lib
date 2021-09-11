using System;

namespace AltoLib
{
  /// <summary>
  /// 変更通知機能のついた bool 型プロパティのラッパー
  /// </summary>
  public class BoolProp
  {
    public event Action<bool> changedEvent;

    Func<bool>   _getter;
    Action<bool> _setter;

    public BoolProp(Func<bool> getter, Action<bool> setter)
    {
      _getter = getter;
      _setter = setter;
    }

    public bool Get()
    {
      return _getter();
    }

    /// <summary>
    /// 値を設定。値に変更があった場合 changedEvent が呼ばれる
    /// </summary>
    public void Set(bool value)
    {
      if (value == _getter()) { return; }

      _setter(value);
      changedEvent?.Invoke(value);
    }
  }
}
