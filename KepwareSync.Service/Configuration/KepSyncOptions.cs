using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public enum SyncDirection
    {
        KepwareToDisk,
        DiskToKepware,
        KepwareToKepware,
        /// <summary>
        /// Primary Kepware to Secondary Kepware and Disk
        /// </summary>
        KepwareToDiskAndSecondary,
    }

    public enum SyncMode
    {
        /// <summary>
        /// Changes made in primary are synced to secondary and disk
        /// </summary>
        OneWay,
        /// <summary>
        /// Changes made both in primary and on disk are synced
        /// </summary>
        TwoWay,
    }

    public class KepSyncOptions
    {
        public SyncDirection SyncDirection { get; set; } = SyncDirection.KepwareToDiskAndSecondary;
        public SyncMode SyncMode { get; set; } = SyncMode.TwoWay;
        public int SyncThrottlingMs { get; set; } = 1000;
    }
}
