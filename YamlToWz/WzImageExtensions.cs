using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;

namespace YamlToWz
{
    public static class WzImageExtensions
    {
        public static void AddAndUpdate(this WzObject parent, WzObject child)
        {
            if (parent is WzFile file)
                parent = file.WzDirectory;
            if (parent is WzDirectory directory)
            {
                if (child is WzDirectory wzDirectory)
                    directory.AddDirectory(wzDirectory);
                else if (child is WzImage wzImgProperty)
                    directory.AddImage(wzImgProperty);
                else
                    return;
            }
            else if (parent is WzImage wzImageProperty)
            {
                if (!wzImageProperty.Parsed)
                    wzImageProperty.ParseImage();
                if (child is WzImageProperty imgProperty)
                {
                    wzImageProperty.AddProperty(imgProperty);
                    wzImageProperty.Changed = true;
                }
                else
                    return;
            }
            else if (parent is IPropertyContainer container)
            {
                if (child is WzImageProperty property)
                {
                    container.AddProperty(property);
                    if (parent is WzImageProperty imgProperty)
                        imgProperty.ParentImage.Changed = true;
                }
                else
                    return;
            }
            else
                return;
        }
    }
}
