using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace YamlToWz
{
    public static class WzImageExtensions
    {
        public static void DuplicateAs(this WzObject obj, string name)
        {
            WzObject parent = obj.Parent;
            if (parent is WzDirectory wzDir && wzDir.GetImageByName(name) != null) parent.RemoveAndUpdate(name);
            else if (parent is WzImage wzImg && wzImg.GetFromPath(name) != null) parent.RemoveAndUpdate(name);
            else if (parent is IPropertyContainer && parent is WzImageProperty wzImgProp && wzImgProp.GetFromPath(name) != null) parent.RemoveAndUpdate(name);
            
            WzObject newObject;
            if (obj is WzImage wzImage)
                newObject = wzImage.DeepClone();
            else if (obj is WzImageProperty wzImageProp)
                newObject = wzImageProp.DeepClone();
            else throw new Exception();

            newObject.Name = name;
            obj.Parent.AddAndUpdate(newObject);
        }

        public static void RemoveAndUpdate(this WzObject parent, string propName)
        {
            if (parent is WzFile file)
                parent = file.WzDirectory;
            if (parent is WzDirectory directory)
            {
                if (propName.EndsWith(".img"))
                {
                    var child = directory.WzImages.FirstOrDefault(img => img.Name.Equals(propName));
                    if (child != null) directory.RemoveImage(child);
                    return;
                }
                else
                {
                    var child = directory.WzDirectories.FirstOrDefault(dir => dir.Name.Equals(propName));
                    if (child != null) directory.RemoveDirectory(child);
                    return;
                }
            }
            else if (parent is WzImage image)
            {
                var child = image.WzProperties.FirstOrDefault(prop => prop.Name.Equals(propName));
                if (child != null)
                {
                    image.RemoveProperty(child);
                    image.Changed = true;
                }
            }
            else if (parent is IPropertyContainer props)
            {
                var child = props.WzProperties.FirstOrDefault(prop => prop.Name.Equals(propName));
                if (child != null)
                {
                    props.RemoveProperty(child);
                    if (parent is WzImageProperty imgProp) imgProp.ParentImage.Changed = true;
                }
            }
        }

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
