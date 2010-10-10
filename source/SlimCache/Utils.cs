using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimCache
{
    public static class Utils
    {
        public enum StorageUnit
        {
            Bits = 0,
            Bytes = 1,
            Kilobytes = 2,
            Megabyte = 3,
            GigaBytes = 4,
            TeraBytes = 5,
            PetaBytes = 6,
            Exabytes = 7,
            Zettabytes = 8
        }
        public static double ConvertStorageUnit(double unit, StorageUnit convertFromUnit, StorageUnit convertToUnit)
        {
            switch (convertFromUnit)
            {
                case StorageUnit.Bits:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit;
                        case StorageUnit.Bytes:
                            return unit * 0.125;
                        case StorageUnit.Kilobytes:
                            return unit * 0.0001220703;
                        case StorageUnit.Megabyte:
                            return unit * 0.0000001192;
                        case StorageUnit.GigaBytes:
                            return unit * 0.0000000001;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.Bytes:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 8;
                        case StorageUnit.Bytes:
                            return unit;
                        case StorageUnit.Kilobytes:
                            return unit * 0.0009765625;
                        case StorageUnit.Megabyte:
                            return unit * 0.0000009536;
                        case StorageUnit.GigaBytes:
                            return unit * 0.0000000009;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.Kilobytes:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 8192;
                        case StorageUnit.Bytes:
                            return unit * 1024;
                        case StorageUnit.Kilobytes:
                            return unit;
                        case StorageUnit.Megabyte:
                            return unit * 0.0009765625;
                        case StorageUnit.GigaBytes:
                            return unit * 0.0000009536;
                        case StorageUnit.TeraBytes:
                            return unit * 0.0000000009;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.Megabyte:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 8388608;
                        case StorageUnit.Bytes:
                            return unit * 1048576;
                        case StorageUnit.Kilobytes:
                            return unit * 1024;
                        case StorageUnit.Megabyte:
                            return unit;
                        case StorageUnit.GigaBytes:
                            return unit * 0.0009765625;
                        case StorageUnit.TeraBytes:
                            return unit * 0.0000009536;
                        case StorageUnit.PetaBytes:
                            return unit * 0.0000000009;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.GigaBytes:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 8589934592;
                        case StorageUnit.Bytes:
                            return unit * 1073741824;
                        case StorageUnit.Kilobytes:
                            return unit * 1048576;
                        case StorageUnit.Megabyte:
                            return unit * 1024;
                        case StorageUnit.GigaBytes:
                            return unit;
                        case StorageUnit.TeraBytes:
                            return unit * 0.0009765625;
                        case StorageUnit.PetaBytes:
                            return unit * 0.0000009536;
                        case StorageUnit.Exabytes:
                            return unit * 0.0000000009;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.TeraBytes:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 8796093022208;
                        case StorageUnit.Bytes:
                            return unit * 1099511627776;
                        case StorageUnit.Kilobytes:
                            return unit * 1073741824;
                        case StorageUnit.Megabyte:
                            return unit * 1048576;
                        case StorageUnit.GigaBytes:
                            return unit * 1024;
                        case StorageUnit.TeraBytes:
                            return unit;
                        case StorageUnit.PetaBytes:
                            return unit * 0.0009765625;
                        case StorageUnit.Exabytes:
                            return unit * 0.0000009536;
                        case StorageUnit.Zettabytes:
                            return unit * 0.0000000009;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                case StorageUnit.PetaBytes:
                    switch (convertToUnit)
                    {
                        case StorageUnit.Bits:
                            return unit * 9007199254740992;
                        case StorageUnit.Bytes:
                            return unit * 1125899906842624;
                        case StorageUnit.Kilobytes:
                            return unit * 1099511627776;
                        case StorageUnit.Megabyte:
                            return unit * 1073741824;
                        case StorageUnit.GigaBytes:
                            return unit * 1048576;
                        case StorageUnit.TeraBytes:
                            return unit * 1024;
                        case StorageUnit.PetaBytes:
                            return unit;
                        case StorageUnit.Exabytes:
                            return unit * 0.0009765625;
                        case StorageUnit.Zettabytes:
                            return unit * 0.0000009536;
                        default:
                            throw new NotSupportedException("Cannot convert Bits to " + Enum.GetName(typeof(StorageUnit), convertToUnit));
                    }
                default:
                    throw new NotSupportedException("Cannot convert from " + Enum.GetName(typeof(StorageUnit), convertFromUnit));
            }
        }
    }
}
