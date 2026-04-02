using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CleanCrow.Models;

namespace CleanCrow.Services
{
    public class DiskMonitorService
    {
        public List<DiskInfo> GetDiskInfo()
        {
            var disks = new List<DiskInfo>();
            
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                    {
                        var diskInfo = DiskInfo.GetDriveInfo(drive.Name);
                        if (diskInfo != null)
                        {
                            disks.Add(diskInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter informações dos discos: {ex.Message}");
            }
            
            return disks;
        }
        
        public string GetDiskSummary(List<DiskInfo> disks)
        {
            if (disks == null || disks.Count == 0)
                return "Nenhum disco encontrado";
                
            var result = new System.Text.StringBuilder();
            foreach (var disk in disks)
            {
                result.AppendLine($"  {disk.GetFormattedInfo()}");
            }
            return result.ToString();
        }
        
        public string GetDiskComparison(List<DiskInfo> before, List<DiskInfo> after)
        {
            var result = new System.Text.StringBuilder();
            
            foreach (var beforeDisk in before)
            {
                var afterDisk = after.FirstOrDefault(d => d.DriveName == beforeDisk.DriveName);
                if (afterDisk != null)
                {
                    result.AppendLine($"  {beforeDisk.DriveName}");
                    result.AppendLine($"    Antes: {beforeDisk.FreeFormatted} livre de {beforeDisk.TotalFormatted} ({beforeDisk.FreePercent:F1}%)");
                    result.AppendLine($"    Depois: {afterDisk.FreeFormatted} livre de {afterDisk.TotalFormatted} ({afterDisk.FreePercent:F1}%)");
                    
                    long freedBytes = afterDisk.FreeBytes - beforeDisk.FreeBytes;
                    if (freedBytes > 0)
                    {
                        result.AppendLine($"    🎉 LIBERADO: {FormatBytes(freedBytes)} de espaço!");
                    }
                    else if (freedBytes < 0)
                    {
                        result.AppendLine($"    ⚠️  PERDIDO: {FormatBytes(Math.Abs(freedBytes))} de espaço");
                    }
                    else
                    {
                        result.AppendLine($"    ➡️  Nenhuma alteração");
                    }
                    result.AppendLine("");
                }
            }
            
            // Calcular total liberado
            long totalFreed = 0;
            foreach (var beforeDisk in before)
            {
                var afterDisk = after.FirstOrDefault(d => d.DriveName == beforeDisk.DriveName);
                if (afterDisk != null)
                {
                    totalFreed += (afterDisk.FreeBytes - beforeDisk.FreeBytes);
                }
            }
            
            if (totalFreed > 0)
            {
                result.Insert(0, $"📊 TOTAL DE ESPAÇO LIBERADO: {FormatBytes(totalFreed)}\n\n");
            }
            else if (totalFreed < 0)
            {
                result.Insert(0, $"⚠️ TOTAL DE ESPAÇO PERDIDO: {FormatBytes(Math.Abs(totalFreed))}\n\n");
            }
            
            return result.ToString();
        }
        
        private string FormatBytes(long bytes)
        {
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
    }
}