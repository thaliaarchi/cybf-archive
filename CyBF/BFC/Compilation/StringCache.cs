using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using System;
using System.Collections.Generic;

namespace CyBF.BFC.Compilation
{
    /*
        Another stupid hack.
        Allows me to write out the bytes for string literals to print before 
        any other statements in the program. Makes it so that the same literal 
        doesn't actually get written out twice just because it's in a procedure
        that gets called twice or is in an iterate statement or something.
    */
    public class StringCache
    {
        public struct StringCacheItem
        {
            public string String { get; set; }
            public byte[] Bytes { get; set; }
            public BFObject BFObject { get; set; }
        }

        private Dictionary<string, StringCacheItem> _cache
            = new Dictionary<string, StringCacheItem>();

        public IEnumerable<StringCacheItem> Items { get { return _cache.Values; } }

        public StringCacheItem GetCachedString(StringInstance instance)
        {
            if (_cache.ContainsKey(instance.ProcessedString))
                return _cache[instance.ProcessedString];

            byte[] bytes = new byte[instance.AsciiBytes.Length + 2];
            bytes[0] = 0;
            Array.Copy(instance.AsciiBytes, 0, bytes, 1, instance.AsciiBytes.Length);
            bytes[bytes.Length - 1] = 0;

            BFObject bfobject = new BFObject(
                new ArrayInstance(new ByteInstance(), bytes.Length),
                "str" + (_cache.Count + 1).ToString());

            StringCacheItem cacheItem = new StringCacheItem()
            {
                String = instance.ProcessedString,
                Bytes = bytes,
                BFObject = bfobject
            };

            _cache[instance.ProcessedString] = cacheItem;

            return cacheItem;
        }
    }
}
