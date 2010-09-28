using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SlimCache
{
    class FileCacheItemInfo
    {
        public FileCacheItemInfo(string fileName)
        {
            var tokens = fileName.Split('!');
            Key = tokens[0];
            Expiration = DateTime.ParseExact(tokens[1], "ddMMyyyyhhmmss", CultureInfo.CurrentCulture);

        }

        public string Key { get; private set; }
        public DateTime Expiration { get; private set; }
        public bool IsExpired
        {
            get
            {
                return !(Expiration == Cache.NoAbsoluteExpiration || Expiration > DateTime.Now);
            }
        }
        public string FullyQualifiedFileName { get; set; }
        public DateTimeOffset LastUsed { get; set; }
        public long Size { get; set; }


        public override bool Equals(object obj)
        {
            var compObj = obj as FileCacheItemInfo;
            if (compObj == null) return false;

            return Key.Equals(compObj.Key);
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
