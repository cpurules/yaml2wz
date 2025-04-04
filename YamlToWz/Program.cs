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
    internal class YamlFile
    {
        public List<YamlQuest>? Quests { get; set; }
        
        [YamlMember(Alias = "perk", ApplyNamingConventions = false)]
        public List<YamlPerk>? Perks { get; set; }

        public void InitializeEmpty()
        {
            Quests ??= new List<YamlQuest>();
            Perks ??= new List<YamlPerk>();
        }
    }

    internal class Program
    {
        private static void ImportQuestsToWz(List<YamlQuest> quests, string wzPath)
        {
            // Backups
            BackupWz(wzPath, "Quest.wz");

            Console.WriteLine($"beginning import of {quests.Count} quests");
            var wzFM = new WzFileManager(wzPath, false, false);
            var wzQuest = wzFM.LoadWzFile(Path.Join(wzPath, "Quest.wz"), WzMapleVersion.GMS);
            var wzQuestInfoImg = wzQuest.WzDirectory.GetImageByName("QuestInfo.img");
            var wzCheckImg = wzQuest.WzDirectory.GetImageByName("Check.img");
            var wzSayImg = wzQuest.WzDirectory.GetImageByName("Say.img");
            var wzActImg = wzQuest.WzDirectory.GetImageByName("Act.img");

            foreach (var quest in quests)
            {
                Console.WriteLine($"importing quest ID {quest.Id}: {quest.Name}");

                Console.WriteLine($"-> QuestInfo.img");
                var qiNode = new WzSubProperty(quest.Id.ToString());
                wzQuestInfoImg.AddAndUpdate(qiNode);
                qiNode.AddAndUpdate(new WzStringProperty("name", quest.Name));
                if (quest.Parent != null) qiNode.AddAndUpdate(new WzStringProperty("parent", quest.Parent));
                if (quest.Order.HasValue) qiNode.AddAndUpdate(new WzIntProperty("order", quest.Order.Value));
                qiNode.AddAndUpdate(new WzIntProperty("area", quest.Area));
                qiNode.AddAndUpdate(new WzStringProperty("0", quest.Info.Available));
                qiNode.AddAndUpdate(new WzStringProperty("1", quest.Info.InProgress));
                qiNode.AddAndUpdate(new WzStringProperty("2", quest.Info.Completed));

                Console.WriteLine($"-> Check.img");
                var checkNode = new WzSubProperty(quest.Id.ToString());
                wzCheckImg.AddAndUpdate(checkNode);
                var check_0 = new WzSubProperty("0");
                checkNode.AddAndUpdate(check_0);
                check_0.AddAndUpdate(new WzIntProperty("npc", quest.StartNpc));
                if (quest.Prereqs.MinLevel.HasValue)
                    check_0.AddAndUpdate(new WzIntProperty("lvmin", quest.Prereqs.MinLevel.Value));
                if (quest.Prereqs.MaxLevel.HasValue)
                    check_0.AddAndUpdate(new WzIntProperty("lvmax", quest.Prereqs.MaxLevel.Value));
                if (quest.StartScript != null)
                    check_0.AddAndUpdate(new WzStringProperty("startscript", quest.StartScript));

                if (quest.Prereqs.Job != null)
                {
                    var job = new WzSubProperty("job");
                    check_0.AddAndUpdate(job);
                    for (int i = 0; i < quest.Prereqs.Job.Count; i++)
                        job.AddAndUpdate(new WzIntProperty(i.ToString(), quest.Prereqs.Job[i]));
                }

                if (quest.Prereqs.Quests != null)
                {
                    var prequest = new WzSubProperty("quest");
                    check_0.AddAndUpdate(prequest);
                    for (int i = 0; i < quest.Prereqs.Quests.Count; i++)
                    {
                        var prequest_i = new WzSubProperty(i.ToString());
                        Quest q = quest.Prereqs.Quests[i];
                        prequest.AddAndUpdate(prequest_i);
                        prequest_i.AddAndUpdate(new WzIntProperty("id", q.Id));
                        prequest_i.AddAndUpdate(new WzIntProperty("state", q.State));
                    }
                }

                var check_1 = new WzSubProperty("1");
                checkNode.AddAndUpdate(check_1);
                check_1.AddAndUpdate(new WzIntProperty("npc", quest.EndNpc));
                if (quest.EndScript != null) check_1.AddAndUpdate(new WzStringProperty("endscript", quest.EndScript));

                if (quest.Checks != null)
                {
                    if (quest.Checks.Items != null)
                    {
                        var item = new WzSubProperty("item");
                        check_1.AddAndUpdate(item);
                        for (int i = 0; i < quest.Checks.Items.Count; i++)
                        {
                            var item_i = new WzSubProperty(i.ToString());
                            Item itemCheck = quest.Checks.Items[i];
                            item.AddAndUpdate(item_i);
                            item_i.AddAndUpdate(new WzIntProperty("id", itemCheck.Id));
                            item_i.AddAndUpdate(new WzIntProperty("count", itemCheck.Count));
                        }
                    }

                    if (quest.Checks.Mobs != null)
                    {
                        var mob = new WzSubProperty("mob");
                        check_1.AddAndUpdate(mob);
                        for (int i = 0; i < quest.Checks.Mobs.Count; i++)
                        {
                            var mob_i = new WzSubProperty(i.ToString());
                            mob.AddAndUpdate(mob_i);
                        }
                    }
                }

                Console.WriteLine($"-> Say.img");
                var sayNode = new WzSubProperty(quest.Id.ToString());
                wzSayImg.AddAndUpdate(sayNode);
                // These are always present even if no dialogue or handled by server script
                var say_0 = new WzSubProperty("0");
                var say_1 = new WzSubProperty("1");
                sayNode.AddAndUpdate(say_0);
                sayNode.AddAndUpdate(say_1);

                if (quest.Dialogues != null)
                {
                    var qd = quest.Dialogues;
                    if (qd.PreStart != null)
                    {
                        var predialogue = qd.PreStart;
                        for (int i = 0; i < predialogue.Intro.Count(); i++)
                            say_0.AddAndUpdate(new WzStringProperty(i.ToString(), predialogue.Intro[i]));

                        if (predialogue.Accepted != null)
                        {
                            var yes = new WzSubProperty("yes");
                            say_0.AddAndUpdate(yes);
                            for (int i = 0; i < predialogue.Accepted.Count; i++)
                                yes.AddAndUpdate(new WzStringProperty(i.ToString(), predialogue.Accepted[i]));
                        }

                        if (predialogue.Declined != null)
                        {
                            var no = new WzSubProperty("no");
                            say_0.AddAndUpdate(no);
                            for (int i = 0; i < predialogue.Declined.Count; i++)
                                no.AddAndUpdate(new WzStringProperty(i.ToString(), predialogue.Declined[i]));
                        }
                    }

                    if (qd.PostStart != null)
                    {
                        var postdialogue = qd.PostStart;
                        if (postdialogue.OnCompletion != null)
                            for (int i = 0; i < postdialogue.OnCompletion.Count; i++)
                                say_1.AddAndUpdate(new WzStringProperty(i.ToString(), postdialogue.OnCompletion[i]));

                        if (postdialogue.OnMissingReq != null)
                        {
                            var stop = new WzSubProperty("stop");
                            say_1.AddAndUpdate(stop);
                            var grouped = postdialogue.OnMissingReq.GroupBy(x => x.Type);
                            foreach (var group in grouped)
                            {
                                var stop_type = new WzSubProperty(group.Key);
                                stop.AddAndUpdate(stop_type);
                                for (int i = 0; i < group.Count(); i++)
                                    stop_type.AddAndUpdate(new WzStringProperty(i.ToString(),
                                        group.Skip(i).First().Dialogue));
                            }
                        }
                    }
                }

                Console.WriteLine($"-> Act.img");
                var actNode = new WzSubProperty(quest.Id.ToString());
                wzActImg.AddAndUpdate(actNode);
                actNode.AddAndUpdate(new WzSubProperty("0")); // I only see quests with values in 0 where it appears to be Say.img but in Korean?....
                var act_1 = new WzSubProperty("1");
                actNode.AddAndUpdate(act_1);

                if (quest.Rewards != null)
                {
                    if (quest.Rewards.Exp.HasValue) act_1.AddAndUpdate(new WzIntProperty("exp", quest.Rewards.Exp.Value));
                    if (quest.Rewards.Money.HasValue) act_1.AddAndUpdate(new WzIntProperty("money", quest.Rewards.Money.Value));
                    if (quest.Rewards.Items != null)
                    {
                        var item = new WzSubProperty("item");
                        act_1.AddAndUpdate(item);
                        for (int i = 0; i < quest.Rewards.Items.Count; i++)
                        {
                            var item_i = new WzSubProperty(i.ToString());
                            item.AddAndUpdate(item_i);
                            item_i.AddAndUpdate(new WzIntProperty("id", quest.Rewards.Items[i].Id));
                            item_i.AddAndUpdate(new WzIntProperty("count", quest.Rewards.Items[i].Count));
                        }

                    }
                }
            }
            Console.WriteLine("Saving Quest.wz...");
            SaveWzFile(wzQuest, wzPath, "Quest.wz");
        }

        private static void ImportPerksToWz(List<YamlPerk> perks, string wzPath)
        {
            // Backups
            BackupWz(wzPath, "Skill.wz");
            BackupWz(wzPath, "String.wz");

            Console.WriteLine($"beginning import of {perks.Count} perks");
            var wzFM = new WzFileManager(wzPath, false, false);
            var wzString = wzFM.LoadWzFile(Path.Join(wzPath, "String.wz"), WzMapleVersion.GMS);
            var wzStringSkillImg = wzString.WzDirectory.GetImageByName("Skill.img");
            var wzSkill = wzFM.LoadWzFile(Path.Join(wzPath, "Skill.wz"), WzMapleVersion.GMS);
            var wzSkills = wzSkill.WzDirectory.GetImageByName("000.img").GetFromPath("skill");

            foreach (var perk in perks)
            {
                Console.WriteLine($"importing perk ID {perk.Id}: {perk.Name}");
                var perkNodeName= perk.Id.ToString().PadLeft(7, '0');

                Console.WriteLine($"-> String.wz");
                var perkStringNode = new WzSubProperty(perkNodeName);
                wzStringSkillImg.AddAndUpdate(perkStringNode);
                perkStringNode.AddAndUpdate(new WzStringProperty("name", $"Perk: {perk.Name}"));
                perkStringNode.AddAndUpdate(new WzStringProperty("desc", $"[Rebirth Perk]\\n{perk.Description}"));
                perkStringNode.AddAndUpdate(new WzStringProperty("h1", perk.LongDescription));

                Console.WriteLine($"-> Skill.wz");
                var perkSkillNode = new WzSubProperty(perk.Id.ToString());
                wzSkills.AddAndUpdate(perkSkillNode);
                InsertYamlPerkCanvas(perkSkillNode, perk);
                perkSkillNode.AddAndUpdate(new WzIntProperty("invisible", 1));
                var lvl = new WzSubProperty("level");
                perkSkillNode.AddAndUpdate(lvl);
                var lvl_1 = new WzSubProperty("1");
                lvl.AddAndUpdate(lvl_1);
                lvl_1.AddAndUpdate(new WzStringProperty("hs", "h1"));
            }

            Console.WriteLine("Saving String.wz...");
            SaveWzFile(wzString, wzPath, "String.wz");
            Console.WriteLine("Saving Skill.wz...");
            SaveWzFile(wzSkill, wzPath, "Skill.wz");
        }

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
                yamlFiles = Directory.GetFiles(yamlPath).ToList();
            }
            else
            {
                yamlFiles.Add(yamlPath);
            }

            YamlFile combinedFiles = new YamlFile();
            combinedFiles.InitializeEmpty();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            foreach (var yamlFile in yamlFiles)
            {
                Console.WriteLine($"processing file: {yamlFile}");
                using (var reader = new StreamReader(yamlFile))
                {
                    var yaml = reader.ReadToEnd();
                    YamlFile yamlData = deserializer.Deserialize<YamlFile>(yaml);
                    if (yamlData.Quests != null)
                    {
                        Console.WriteLine($"adding {yamlData.Quests.Count} quests");
                        yamlData.Quests.ForEach(x => x.Path = yamlFile);
                        combinedFiles.Quests.AddRange(yamlData.Quests);
                    }
                    if (yamlData.Perks != null)
                    {
                        Console.WriteLine($"adding {yamlData.Perks.Count} perks");
                        yamlData.Perks.ForEach(x => x.Path = yamlFile);
                        combinedFiles.Perks.AddRange(yamlData.Perks);
                    }
                }
            }

            Console.WriteLine($"finished processing. found {combinedFiles.Quests.Count} quests and {combinedFiles.Perks.Count} perks");

            if (combinedFiles.Quests.Count > 0)
            {
                ImportQuestsToWz(combinedFiles.Quests, wzPath);
            }

            if (combinedFiles.Perks.Count > 0)
            {
                ImportPerksToWz(combinedFiles.Perks, wzPath);
            }
        }

        private static void BackupWz(string path, string file)
        {
            string target = Path.Join(path, file);
            File.Copy(target, target + ".bak", true);
        }

        private static void SaveWzFile(WzFile file, string wzPath, string name)
        {
            var temp = Directory.CreateTempSubdirectory().FullName;
            var tempPath = Path.Join(temp, name);
            var targetPath = Path.Join(wzPath, name);

            file.SaveToDisk(tempPath, false, file.MapleVersion);
            file.Dispose();
            File.Copy(tempPath, targetPath, true);
            Directory.Delete(temp, true);
        }

        private static void InsertYamlPerkCanvas(WzObject node, YamlPerk perk)
        {
            string perkDir = Path.GetDirectoryName(perk.Path);
            string[] names = { "icon", "iconDisabled", "iconOnMouseOver" };
            var bmp = (Bitmap)Image.FromFile(Path.Join(perkDir, perk.Icon.Path));
            foreach (var name in names)
            {
                var insertNode = new WzCanvasProperty(name);
                var pngProp = new WzPngProperty();
                pngProp.PixFormat = (int)WzPngProperty.CanvasPixFormat.Argb4444;
                pngProp.PNG = bmp;
                insertNode.PngProperty = pngProp;
                node.AddAndUpdate(insertNode);
                insertNode.AddAndUpdate(new WzVectorProperty(WzCanvasProperty.OriginPropertyName,
                    new WzIntProperty("X", 0), new WzIntProperty("Y", 32)));
                insertNode.AddAndUpdate(new WzIntProperty("z", 0));
            }
        }
    }
}
