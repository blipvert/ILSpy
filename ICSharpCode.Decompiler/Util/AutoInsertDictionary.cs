using System.Collections.Generic;

namespace ICSharpCode.Decompiler.Util
{
	/// <summary>
	/// A dictionary that allows multiple pairs with the same key.
	/// </summary>
	public abstract class AutoInsertDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public abstract TValue NewValue(TKey key);

		public void GetValue(TKey key, out TValue value)
		{
			if (key is null)
			{
				value = default;
			}
			else if (!TryGetValue(key, out value))
			{
				Add(key, value = NewValue(key));
			}
		}

		public TValue GetValue(TKey key)
		{
			GetValue(key, out TValue value);
			return value;
		}

		public void SetValue(TKey key, TValue value)
		{
			Remove(key);
			Add(key, value);
		}

		public new TValue this[TKey key] {
			get {
				return GetValue(key);
			}
			set {
				SetValue(key, value);
			}
		}
	}

	public class AutoValueDictionary<TKey, TValue> : AutoInsertDictionary<TKey, TValue>
		where TValue : new()
	{
		public override TValue NewValue(TKey key)
		{
			return new();
		}
	}

}
