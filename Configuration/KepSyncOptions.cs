using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.Configuration
{
    public enum SyncDirection
    {
        KepwareToDisk,
        DiskToKepware,
    }

    public enum SyncMode
    {
        OneWay,
        TwoWay,
    }

    public class KepSyncOptions
    {
        public SyncDirection SyncDirection { get; set; } = SyncDirection.KepwareToDisk;
        public SyncMode SyncMode { get; set; } = SyncMode.TwoWay;
        public int SyncThrottlingMs { get; set; } = 1000;
    }
}
