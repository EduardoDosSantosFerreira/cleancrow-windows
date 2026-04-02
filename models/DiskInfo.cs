using System;
using System.IO;

namespace CleanCrow.Models
{
    public class DiskInfo
    {
        public string DriveName { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public long UsedBytes { get; set; }
        public double FreePercent { get; set; }
        public double UsedPercent { get; set; }
        
        public string TotalFormatted => FormatBytes(TotalBytes);
        public string FreeFormatted => FormatBytes(FreeBytes);
        public string UsedFormatted => FormatBytes(UsedBytes);
        
        private string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        public static DiskInfo? GetDriveInfo(string driveName)
        {
            try
            {
                DriveInfo drive = new DriveInfo(driveName);
                if (drive.IsReady)
                {
                    return new DiskInfo
                    {
                        DriveName = driveName,
                        TotalBytes = drive.TotalSize,
                        FreeBytes = drive.AvailableFreeSpace,
                        UsedBytes = drive.TotalSize - drive.AvailableFreeSpace,
                        FreePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100,
                        UsedPercent = (double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100
                    };
                }
            }
            catch { }
            return null;
        }
        
        public string GetFormattedInfo()
        {
            return $"{DriveName} | Total: {TotalFormatted} | Usado: {UsedFormatted} ({UsedPercent:F1}%) | Livre: {FreeFormatted} ({FreePercent:F1}%)";
        }
        
        public string GetDifference(DiskInfo after)
        {
            long freedBytes = after.FreeBytes - this.FreeBytes;
            string freedFormatted = FormatBytes(Math.Abs(freedBytes));
            
            if (freedBytes > 0)
            {
                return $"  Liberado: {freedFormatted} (de {this.FreeFormatted} para {after.FreeFormatted})";
            }
            else if (freedBytes < 0)
            {
                return $"  Ocupado: {freedFormatted} (de {this.FreeFormatted} para {after.FreeFormatted})";
            }
            else
            {
                return $"  Sem alteracao: {this.FreeFormatted}";
            }
        }
    }
}