using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using CASCEdit;
using CASCEdit.IO;
using CASCEdit.Structs;
using CASCEdit.Helpers;

namespace CASCBuilder
{
    public class Cache : CASCEdit.Helpers.ICache
    {
        public string Version => "1";

        public HashSet<string> ToPurge = new HashSet<string>();

        private Dictionary<string, CacheEntry> RootFiles = new Dictionary<string, CacheEntry>();

        public IReadOnlyCollection<CacheEntry> Entries => RootFiles.Values;

        public uint MaxId => RootFiles.Values.Count == 0 ? 1 : RootFiles.Values.Max(x => x.FileDataId);

		public bool HasFiles => RootFiles.Count > 0;

        HashSet<string> ICache.ToPurge => throw new NotImplementedException();

        public bool HasId(uint fileId) => RootFiles.Any(x => x.Value.FileDataId == fileId);

        public void AddOrUpdate(CacheEntry item)
        {
            RootFiles.Add(item.Path, item);
        }

        public void Clean()
        {
            RootFiles.Clear();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Remove(string file)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            return;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = new CASSettings() {
                OutputPath = "wwwroot",
                Cache = new Cache(),
                CDNs = new HashSet<string>(new [] {"localhost"}),
                StaticMode = true
            };

            CASContainer.New(settings);

            var installFile = new CASFile(File.ReadAllBytes("Install.exe"), EncodingType.None);
            var installEntry = DataHandler.Write(WriteMode.CDN, new [] {installFile});
            installEntry.DataHash = installFile.DataHash;
            installEntry.Path = "Install.exe";
            installEntry.HighPriority = true;

            CASContainer.InstallHandler.Add(installEntry);

            CASContainer.RootHandler.AddFile("logo_800.avi", "interface/cinematics/logo_800.avi");

            CASContainer.Save();
        }
    }
}
