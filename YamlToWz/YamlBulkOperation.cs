using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;

namespace YamlToWz
{
    internal class YamlBulkOperation
    {
        public BulkOperationType Type { get; set; }
        public string Path { get; set; }
        public List<string> Nodes { get; set; }

        public void Process(Dictionary<string, WzFile> wzFiles)
        {
            List<string> pieces = Path.Split("/").ToList();
            var wzFile = wzFiles[pieces.First()];
            
            List<WzObject> nodesToTraverse = [wzFile.WzDirectory];

            foreach (var piece in pieces.Skip(1))
            {
                List<WzObject> foundMatchingNodes = new();

                foreach (var node in nodesToTraverse)
                {
                    bool getAll = piece.StartsWith("*");
                    bool getImg = piece.EndsWith(".img");

                    if (node is WzDirectory dir)
                    {
                        if (getImg)
                            foundMatchingNodes.AddRange(dir.WzImages.Where(img => getAll || img.Name.Equals(piece)));
                        else foundMatchingNodes.AddRange(dir.WzDirectories.Where(d => getAll || d.Name.Equals(piece)));
                    }
                    else if (node is WzImage img)
                    {
                        foundMatchingNodes.AddRange(img.WzProperties.Where(prop => getAll || prop.Name.Equals(piece)));
                    }
                    else if (node is IPropertyContainer props)
                    {
                        foundMatchingNodes.AddRange(props.WzProperties.Where(prop =>
                            getAll || prop.Name.Equals(piece)));
                    }
                }

                nodesToTraverse = foundMatchingNodes;
            }

            foreach (var node in nodesToTraverse)
            {
                foreach (var toRem in Nodes)
                {
                    node.RemoveAndUpdate(toRem);
                    if (node is WzImage img) img.Changed = true;
                    else if (node is WzImageProperty prop) prop.ParentImage.Changed = true;
                }
            }
        }
    }

    internal enum BulkOperationType
    {
        REMOVE_NODE,
    }
}
