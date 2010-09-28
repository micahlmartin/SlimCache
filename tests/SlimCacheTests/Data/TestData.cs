using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SlimCacheTests.Data
{
    [DataContract]
    internal class TestData
    {
        [DataMember]
        public string String1 { get; set; }

        [DataMember]
        public int Int1 { get; set; }

        [DataMember]
        public double Double1 { get; set; }
    }
}
