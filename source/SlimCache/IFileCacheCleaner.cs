using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimCache
{
    interface IFileCacheCleaner
    {
        void CleanUp(long spaceToFreeUp);
    }
}
