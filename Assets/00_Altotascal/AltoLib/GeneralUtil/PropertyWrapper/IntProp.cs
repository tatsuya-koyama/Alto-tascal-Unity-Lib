using System;
using UnityEngine;

namespace AltoLib
{
  /// <summary>
  /// 変更通知や Clamp 機能のついた int 型プロパティのラッパー
  /// </summary>
  public class IntProp
  {
    public event Action<int> changedEvent;

    Func<int>   _getter;
    Action<int> _setter;
    int _min;
    int _max;

    public IntProp(
      Func<int> getter, Action<int> setter,
      int min = Int32.MinValue, int max = Int32.MaxValue
    )
    {
      _getter = getter;
      _setter = setter;
      _min = min;
      _max = max;
    }

    public int Get()
    {
      return _getter();
    }

    /// <summary>
    /// 値を設定。値に変更があった場合 changedEvent が呼ばれる
    /// </summary>
    public void Set(int _value)
    {
      int value = Clamp(_value, _min, _max);
      if (value == _getter()) { return; }

      _setter(value);
      changedEvent?.Invoke(value);
    }

    /// <summary>
    /// 値を加算（負の値が渡されるとエラー）
    /// </summary>
    public void Add(int value)
    {
      if (value < 0)
      {
        Debug.LogError($"Add value must be positive : {value}");
        return;
      }
      Set(_getter() + value);
    }

    /// <summary>
    /// 値を減算（1 を渡すと -1 される。負の値が渡されるとエラー）
    /// </summary>
    public void Reduce(int value)
    {
      if (value < 0)
      {
        Debug.LogError($"Reduce value must be positive : {value}");
        return;
      }
      Set(_getter() - value);
    }

    /// <summary>
    /// 値を単純に足す（正負によるエラーなし）
    /// </summary>
    public void Vary(int value)
    {
      Set(_getter() + value);
    }

    int Clamp(int value, int min, int max)
    {
      return (value < min) ? min :
             (value > max) ? max : value;
    }
  }
}
