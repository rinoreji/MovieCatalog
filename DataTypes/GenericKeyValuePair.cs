using System;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class GenericKeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public GenericKeyValuePair() { }
        public GenericKeyValuePair(TKey key, TValue value)
        {
            Key = key; Value = value;
        }
    }
}
