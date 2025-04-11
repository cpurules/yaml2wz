using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace YamlToWz
{
    internal class YamlEquip : YamlImportObject
    {
        public static List<string> WZ_FILES => new List<string> { "Character.wz", "String.wz" };
        public string Path { get; set; }
        public int Id { get; set; }
        public String Category { get; set; }
        public int? CloneFrom { get; set; }
        public YamlEquipOverride? Overrides { get; set; } = new();

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            int searchId = CloneFrom ?? Id;

            var wzChar = wzFiles["Character.wz"].WzDirectory;
            var wzCharCat = wzChar.GetDirectoryByName(Category);
            if (wzCharCat == null) throw new ArgumentOutOfRangeException($"No directory in Character.wz found for category {Category}");
            var wzCharImg = wzCharCat.GetImageByName(searchId.ToString().PadLeft(8, '0') + ".img");
            if (wzCharImg == null) throw new ArgumentOutOfRangeException($"No entry in Character.wz found for id {searchId}");

            var wzString = wzFiles["String.wz"].WzDirectory;
            var wzStringCat = wzString.GetImageByName("Eqp.img").GetFromPath($"Eqp/{Category}");
            if (wzStringCat == null) throw new ArgumentOutOfRangeException($"No directory in String.wz found for category {Category}");
            var wzStringNode = wzStringCat.GetFromPath(searchId.ToString());
            if (wzStringNode == null) throw new ArgumentOutOfRangeException($"No entry in String.wz found for id {searchId}");

            if (CloneFrom.HasValue)
            {
                // Dupe entry from String.wz
                var newStringNode = wzStringNode.DeepClone();
                newStringNode.Name = Id.ToString();
                wzStringCat.AddAndUpdate(newStringNode);
                wzStringNode = newStringNode;

                // Dupe entry from Character.wz
                var newCharImg = wzCharImg.DeepClone();
                newCharImg.Name = Id.ToString().PadLeft(8, '0') + ".img";
                wzCharCat.AddAndUpdate(newCharImg);
                wzCharImg = newCharImg;
            }

            if (Overrides != null)
            {
                // String.wz
                if (Overrides.Name != null) wzStringNode.AddAndUpdate(new WzStringProperty("name", Overrides.Name));
                if (Overrides.Description != null) wzStringNode.AddAndUpdate(new WzStringProperty("desc", Overrides.Description));

                // Character.wz
                var infoNode = wzCharImg.GetFromPath("info") ?? new WzSubProperty("info");
                if (Overrides.UpgradeSlots.HasValue) infoNode.AddAndUpdate(new WzIntProperty("upgradeSlots", Overrides.UpgradeSlots.Value));
                if (Overrides.TradeBlocked) infoNode.AddAndUpdate(new WzIntProperty("tradeBlock", 1));
                if (Overrides.Price.HasValue) infoNode.AddAndUpdate(new WzIntProperty("price", Overrides.Price.Value));
                if (Overrides.ReqJob.HasValue) infoNode.AddAndUpdate(new WzIntProperty("reqJob", Overrides.ReqJob.Value));
                if (Overrides.ReqLevel.HasValue) infoNode.AddAndUpdate(new WzIntProperty("reqLevel", Overrides.ReqLevel.Value));
                if (Overrides.Cash.HasValue) infoNode.AddAndUpdate(new WzIntProperty("cash", Overrides.Cash.Value));
                foreach (var stat in Overrides.Stats ?? new Dictionary<string, int>())
                {
                    switch (stat.Key)
                    {
                        case "str":
                        case "dex":
                        case "int":
                        case "luk":
                            infoNode.AddAndUpdate(new WzIntProperty($"inc{stat.Key.ToUpper()}", stat.Value));
                            break;

                        case "watk":
                        case "matk":
                        case "wdef":
                        case "mdef":
                            var name = stat.Key.Replace("w", "p").Replace("atk", "ad").Replace("def", "dd").ToUpper();
                            infoNode.AddAndUpdate(new WzIntProperty($"inc{name}", stat.Value));
                            break;

                        case "speed":
                        case "jump":
                            infoNode.AddAndUpdate(new WzIntProperty($"inc{stat.Key.UppercaseFirst()}", stat.Value));
                            break;

                        case "base":
                            infoNode.AddAndUpdate(new WzIntProperty("incSTR", stat.Value));
                            infoNode.AddAndUpdate(new WzIntProperty("incDEX", stat.Value));
                            infoNode.AddAndUpdate(new WzIntProperty("incINT", stat.Value));
                            infoNode.AddAndUpdate(new WzIntProperty("incLUK", stat.Value));
                            break;
                    }
                }
                wzCharImg.AddAndUpdate(infoNode);
            }
        }
    }

    internal class YamlEquipOverride
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? UpgradeSlots { get; set; }
        public bool TradeBlocked { get; set; } = false;
        public int? Price { get; set; }
        public int? ReqJob { get; set; } // TODO Enum this
        public int? ReqLevel { get; set; }
        public int? Cash { get; set; }
        public Dictionary<string, int>? Stats { get; set; }
    }
}