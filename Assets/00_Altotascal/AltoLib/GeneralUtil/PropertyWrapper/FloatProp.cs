using System;
using UnityEngine;

namespace AltoLib
{
  /// <summary>
  /// 変更通知や Clamp 機能のついた float 型プロパティのラッパー
  /// </summary>
  public class FloatProp
  {
    public event Action<float> changedEvent;

    Func<float>   _getter;
    Action<float> _setter;
    float _min;
    float _max;

    public FloatProp(
      Func<float> getter, Action<float> setter,
      float min = Single.MinValue, float max = Single.MaxValue
    )
    {
      _getter = getter;
      _setter = setter;
      _min = min;
      _max = max;
    }

    public float Get()
    {
      return _getter();
    }

    /// <summary>
    /// 値を設定。値に変更があった場合 changedEvent が呼ばれる
    /// </summary>
    public void Set(float _value)
    {
      float value = Mathf.Clamp(_value, _min, _max);
      if (value == _getter()) { return; }

      _setter(value);
      changedEvent?.Invoke(value);
    }

    /// <summary>
    /// 値を加算（負の値が渡されるとエラー）
    /// </summary>
    public void Add(float value)
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
    public void Reduce(float value)
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
    public void Vary(float value)
    {
      Set(_getter() + value);
    }
  }
}
