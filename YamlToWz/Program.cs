using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices.Marshalling;
using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using MapleLib.WzLib.Serialization;
using WzSubProperty = MapleLib.WzLib.WzProperties.WzSubProperty;

namespace YamlToWz
{
    internal class YamlData
    {
        public List<YamlQuest> Quests { get; set; } = new();
        [YamlMember(Alias="perk", ApplyNamingConventions = false)]
        public List<YamlPerk> Perks { get; set; } = new();
        public List<YamlItem> Items { get; set; } = new();
        [YamlMember(Alias="npc", ApplyNamingConventions = false)]
        public List<YamlNpc> Npcs { get; set; } = new();
        public List<YamlBulkOperation> BulkOperations { get; set; } = new();

        public static YamlData FromFile(string path)
        {
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            using (var reader = new StreamReader(path))
            {
                var yaml = reader.ReadToEnd();
                YamlData ret = deserializer.Deserialize<YamlData>(yaml);
                ret.SetPaths(path);
                return ret;
            }
        }

        public HashSet<string> GetWzFileNames()
        {
            return (YamlQuest.WZ_FILES.Union(YamlPerk.WZ_FILES).Union(YamlItem.WZ_FILES).Union(YamlNpc.WZ_FILES))
                .ToHashSet();
        }

        public void SetPaths(string path)
        {
            Quests.ForEach(x => x.Path = path);
            Perks.ForEach(x => x.Path = path);
            Items.ForEach(x => x.Path = path);
            Npcs.ForEach(x => x.Path = path);
        }

        public void Merge(YamlData other)
        {
            Quests.AddRange(other.Quests);
            Perks.AddRange(other.Perks);
            Items.AddRange(other.Items);
            Npcs.AddRange(other.Npcs);
            BulkOperations.AddRange(other.BulkOperations);
        }
    }

    internal class Program
    {
        private static List<string> backupFiles = new List<string>();


        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: YamlToWz.exe <path to wz directory> <path to YAML file/directory>");
                return;
            }

            var wzPath = args[0];
            if (!Directory.Exists(wzPath))
            {
                Console.WriteLine($"error: {wzPath} is either not a directory or does not exist");
                return;
            }

            List<string> yamlFiles = new List<string>();
            var yamlPath = args[1];
            if (!Path.Exists(yamlPath))
            {
                Console.WriteLine($"error: yaml path {yamlPath} does not exist");
                return;
            }

            if (Directory.Exists(yamlPath))
            {
                SearchOption opt = (args.Length == 3 && args[2] == "-r"
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly);
                yamlFiles = Directory.GetFiles(yamlPath, "*.yaml", opt).ToList();
            }
            else
            {
                yamlFiles.Add(yamlPath);
            }

            YamlData combinedData = new YamlData();

            foreach (var yamlFile in yamlFiles)
            {
                Console.WriteLine($"processing file: {yamlFile}");
                combinedData.Merge(YamlData.FromFile(yamlFile));
            }

            ImportToWzFiles(combinedData, wzPath);
        }

        private static void ImportToWzFiles(YamlData data, string wzPath)
        {
            var wzFileNames = data.GetWzFileNames();
            wzFileNames.Add("Character.wz");
            var wzFM = new WzFileManager(wzPath, false, false);
            Dictionary<string, WzFile> wzFiles = new();

            Console.WriteLine($"backing up and loading wz files: " + string.Join(", ", wzFileNames));
            foreach (var wzFileName in wzFileNames)
            {
                if (!wzFiles.ContainsKey(wzFileName))
                {
                    BackupWz(wzPath, wzFileName);
                    wzFiles.Add(wzFileName, wzFM.LoadWzFile(Path.Join(wzPath, wzFileName), WzMapleVersion.GMS));
                }
            }

            if (data.Items.Count > 0)
            {
                Console.WriteLine($"beginning import of {data.Items.Count} items");
                data.Items.ForEach(i => i.AddToWz(wzFiles));
            }
            if (data.Perks.Count > 0)
            {
                Console.WriteLine($"beginning import of {data.Perks.Count} perks");
                data.Perks.ForEach(p => p.AddToWz(wzFiles));
            }
            if (data.Quests.Count > 0)
            {
                Console.WriteLine($"beginning import of {data.Quests.Count} quests");
                data.Quests.ForEach(q => q.AddToWz(wzFiles));
            }
            if (data.Npcs.Count > 0)
            {
                Console.WriteLine($"beginning import of {data.Npcs.Count} NPCs");
                data.Npcs.ForEach(n => n.AddToWz(wzFiles));
            }

            if (data.BulkOperations.Count > 0)
            {
                Console.WriteLine($"beginning {data.BulkOperations.Count} bulk operations");
                data.BulkOperations.ForEach(b => b.Process(wzFiles));
                SaveWzFile(wzFiles["Character.wz"], wzPath, "Character2.wz");
            }

            Console.WriteLine($"saving wz files");
            //wzFiles.Keys.ToList().ForEach(fn => SaveWzFile(wzFiles[fn], wzPath, fn));
        }

        private static void BackupWz(string path, string file)
        {
            string target = Path.Join(path, file);
            File.Copy(target, target + ".bak", true);
        }

        private static void SaveWzFile(WzFile file, string wzPath, string name)
        {
            Console.WriteLine($"saving {name}");
            var temp = Directory.CreateTempSubdirectory().FullName;
            var tempPath = Path.Join(temp, name);
            var targetPath = Path.Join(wzPath, name);

            file.SaveToDisk(tempPath, false, file.MapleVersion);
            file.Dispose();
            File.Copy(tempPath, targetPath, true);
            Directory.Delete(temp, true);
        }
    }
}
