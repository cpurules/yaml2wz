using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace YamlToWz
{
    internal class YamlItem : YamlImportObject
    {
        public static List<string> WZ_FILES => ["Item.wz", "String.wz"];
        public string Path { get; set; }
        public int Id { get; set; }
        public YamlItemType ItemType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public YamlIcon Icon { get; set; }
        public YamlXY IconOrigin { get; set; }
        public YamlIcon RawIcon { get; set; }
        public YamlXY RawIconOrigin { get; set; }
        public int SellPrice { get; set; }
        public int? MaxPerSlot { get; set; }
        public bool TradeBlocked { get; set; } = false;
        public bool QuestItem { get; set; } = false;
        public bool LimitOne { get; set; } = false;
        //public bool Unsellable { get; set; } = false;
        public string ImgName => LongId.Substring(0, 4);
        public string LongId => Id.ToString().PadLeft(8, '0');

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            var wzString = wzFiles["String.wz"];
            var wzItem = wzFiles["Item.wz"];

            Console.WriteLine($"importing item ID {Id} : {Name}");
            // String.wz
            var wzStringImg = wzString.WzDirectory.GetImageByName(ItemType.WzName() + ".img");
            var wzStringSubdir = wzStringImg.GetFromPath(ItemType.WzName());
            var wzStringNode = new WzSubProperty(Id.ToString());
            wzStringSubdir.AddAndUpdate(wzStringNode);
            wzStringNode.AddAndUpdate(new WzStringProperty("name", Name));
            wzStringNode.AddAndUpdate(new WzStringProperty("desc", Description));

            // Item.wz
            var wzItemDir = wzItem.WzDirectory.GetDirectoryByName(ItemType.WzName());
            var wzItemImg = wzItemDir.GetImageByName(ImgName + ".img");
            if (wzItemImg == null)
            {
                wzItemImg = new WzImage(ImgName + ".img", WzMapleVersion.GMS) { Changed = true };
                wzItemDir.AddAndUpdate(wzItemImg);
            }

            var wzItemNode = new WzSubProperty(LongId);
            wzItemImg.AddAndUpdate(wzItemNode);
            var wzItemInfoNode = new WzSubProperty("info");
            wzItemNode.AddAndUpdate(wzItemInfoNode);
            InsertYamlItemCanvases(wzItemInfoNode);
            wzItemInfoNode.AddAndUpdate(new WzIntProperty("price", SellPrice));
            if (MaxPerSlot.HasValue) wzItemInfoNode.AddAndUpdate(new WzIntProperty("slotMax", MaxPerSlot.Value));
            if (TradeBlocked) wzItemInfoNode.AddAndUpdate(new WzIntProperty("tradeBlock", 1));
            if (QuestItem) wzItem.AddAndUpdate(new WzIntProperty("quest", 1));
            if (LimitOne) wzItem.AddAndUpdate(new WzIntProperty("only", 1));
        }

        private void InsertYamlItemCanvases(WzObject node)
        {
            string itemDir = System.IO.Path.GetDirectoryName(Path);
            var icon = new WzCanvasProperty("icon");
            var iconPng = new WzPngProperty();
            iconPng.PixFormat = (int)WzPngProperty.CanvasPixFormat.Argb4444;
            iconPng.PNG = (Bitmap)(Image.FromFile(System.IO.Path.Join(itemDir, Icon.Path)));
            icon.PngProperty = iconPng;
            node.AddAndUpdate(icon);
            icon.AddAndUpdate(new WzVectorProperty(WzCanvasProperty.OriginPropertyName, new WzIntProperty("X", IconOrigin.X), new WzIntProperty("Y", IconOrigin.Y)));

            var iconRaw = new WzCanvasProperty("iconRaw");
            var iconRawPng = new WzPngProperty();
            iconRawPng.PixFormat = (int)WzPngProperty.CanvasPixFormat.Argb4444;
            iconRawPng.PNG = (Bitmap)(Image.FromFile(System.IO.Path.Join(itemDir, RawIcon.Path)));
            iconRaw.PngProperty = iconRawPng;
            node.AddAndUpdate(iconRaw);
            iconRaw.AddAndUpdate(new WzVectorProperty(WzCanvasProperty.OriginPropertyName, new WzIntProperty("X", RawIconOrigin.X), new WzIntProperty("Y", RawIconOrigin.Y)));
        }
    }

    internal class YamlXY
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public enum YamlItemType
    {
        CASH,
        CONSUME,
        ETC,
        INSTALL
    }

    public static class YamlItemTypeExtensions
    {
        public static string WzName(this YamlItemType itemType)
        {
            var name = Enum.GetName(itemType);
            return name[0].ToString().ToUpper() + name.Substring(1).ToLower();
        }
    }
}
