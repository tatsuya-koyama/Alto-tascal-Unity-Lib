using System;

namespace AltoLib
{
  /// <summary>
  /// 変更通知機能のついた string 型プロパティのラッパー
  /// </summary>
  public class StringProp
  {
    public event Action<string> changedEvent;

    Func<string>   _getter;
    Action<string> _setter;

    public StringProp(Func<string> getter, Action<string> setter)
    {
      _getter = getter;
      _setter = setter;
    }

    public string Get()
    {
      return _getter();
    }

    /// <summary>
    /// 値を設定。値に変更があった場合 changedEvent が呼ばれる
    /// </summary>
    public void Set(string value)
    {
      if (value == _getter()) { return; }

      _setter(value);
      changedEvent?.Invoke(value);
    }
  }
}
