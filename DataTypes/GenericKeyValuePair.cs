using System;
using System.ComponentModel;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class GenericKeyValuePair<TKey, TValue> : UIDataBase
    {
        private TKey _key;
        public TKey Key { get { return _key; } set { _key = value; RaizePropertyChanged("Key"); } }

        private TValue _value;
        public TValue Value { get { return _value; } set { _value = value; RaizePropertyChanged("Value"); } }

        public GenericKeyValuePair() { }
        public GenericKeyValuePair(TKey key, TValue value)
        {
            Key = key; Value = value;
        }
    }
}
