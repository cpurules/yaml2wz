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
                    if (wzImageProperty.GetFromPath(imgProperty.Name) != null)
                        wzImageProperty.RemoveProperty(wzImageProperty.GetFromPath(imgProperty.Name));
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
                    if (container.WzProperties.Any(x => x.Name.Equals(property.Name)))
                        container.RemoveProperty(container.WzProperties.First(x => x.Name.Equals(property.Name)));
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
