using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace YamlToWz
{
    internal class YamlPerk : YamlImportObject
    {
        public static List<string> WZ_FILES => ["Skill.wz", "String.wz"];

        public string Path { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public YamlIcon Icon { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }
        public bool Active { get; set; } = false;
        public YamlPerkStats? Stats { get; set; }

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            var wzString = wzFiles["String.wz"];
            var wzStringSkillImg = wzString.WzDirectory.GetImageByName("Skill.img");
            var wzSkill = wzFiles["Skill.wz"];
            var wzSkills = wzSkill.WzDirectory.GetImageByName("000.img").GetFromPath("skill");

            Console.WriteLine($"importing perk ID {Id} : {Name}");
            var perkNodeName = Id.ToString().PadLeft(7, '0');

            // String.wz
            var perkStringNode = new WzSubProperty(perkNodeName);
            wzStringSkillImg.AddAndUpdate(perkStringNode);
            perkStringNode.AddAndUpdate(new WzStringProperty("name", $"Perk: {Name}"));
            perkStringNode.AddAndUpdate(new WzStringProperty("desc", $"[Rebirth Perk]\\n{Description}"));
            if (Stats == null || !Stats.IsComputed)
                perkStringNode.AddAndUpdate(new WzStringProperty("h1", LongDescription.Trim()));
            else
                perkStringNode.AddAndUpdate(new WzStringProperty("h", LongDescription.Trim()));

            // Skill.wz
            var perkSkillNode = new WzSubProperty(perkNodeName);
            wzSkills.AddAndUpdate(perkSkillNode);
            // -> Icon loading
            string[] iconNames = ["icon", "iconDisabled", "iconMouseOver"];

            Bitmap bmp =
                (Bitmap)(Image.FromFile(System.IO.Path.Join(System.IO.Path.GetDirectoryName(Path), Icon.Path)));
            foreach (var iconName in iconNames)
            {
                var iconNode = new WzCanvasProperty(iconName);
                iconNode.PngProperty = new WzPngProperty() { PixFormat = (int)WzPngProperty.CanvasPixFormat.Argb4444, PNG = bmp };
                perkSkillNode.AddAndUpdate(iconNode);
                iconNode.AddAndUpdate(new WzVectorProperty(WzCanvasProperty.OriginPropertyName,
                    new WzIntProperty("X", 0), new WzIntProperty("Y", 32)));
                iconNode.AddAndUpdate(new WzIntProperty("z", 0));
            }
            perkSkillNode.AddAndUpdate(new WzIntProperty("invisible", 1));
            perkSkillNode.AddAndUpdate(new WzIntProperty("disable", 1));
            if (!Active)
                perkSkillNode.AddAndUpdate(new WzIntProperty("psd", 1));

            if (Stats != null && Stats.IsComputed)
            {
                var common = new WzSubProperty("common");
                perkSkillNode.AddAndUpdate(common);
                perkSkillNode.AddAndUpdate(new WzIntProperty("psd", 1));
                //TODO Stats should be a list of stats so I don't have to hard code all this shit in
                if (Stats.MaxLevel.HasValue) common.AddAndUpdate(new WzIntProperty("maxLevel", Stats.MaxLevel.Value));
                if (Stats.Speed != null) common.AddAndUpdate(new WzStringProperty("speed", Stats.Speed));
                if (Stats.PsdSpeed != null) common.AddAndUpdate(new WzStringProperty("psdSpeed", Stats.PsdSpeed));
            }
            else
            {
                var lvl = new WzSubProperty("level");
                perkSkillNode.AddAndUpdate(lvl);
                var lvl_1 = new WzSubProperty("1");
                lvl.AddAndUpdate(lvl_1);
                lvl_1.AddAndUpdate(new WzStringProperty("hs", "h1"));
            }
        }
    }

    internal class YamlPerkStats
    {
        public int? MaxLevel { get; set; }
        public bool IsComputed { get; set; } = false;
        public string? Speed { get; set; }
        public string? PsdSpeed { get; set; }
    }

    internal class YamlIcon
    {
        public string Path { get; set; }
    }
}
