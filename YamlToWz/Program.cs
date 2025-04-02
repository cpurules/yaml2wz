using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices.Marshalling;
using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlToWz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: YamlToWz.exe <path to quest yaml> <path to Quest.wz>");
                return;
            }

            var path = args[0];
            if (!Path.Exists(path))
            {
                Console.WriteLine("could not find file: " + path);
                return;
            }

            var wzPath = args[1];
            if (!Path.Exists(wzPath))
            {
                Console.WriteLine("could not find file: " + wzPath);
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            List<YamlQuest> quests;
            using (var reader = new StreamReader(path))
            {
                var o = reader.ReadToEnd();
                Console.Write(o);
                quests = deserializer.Deserialize<List<YamlQuest>>(o);
            }

            Console.WriteLine($"found {quests.Count} quests to add");
            Console.WriteLine($"attempting to load wz file: {wzPath}");

            var _w = new WzFileManager(Path.GetDirectoryName(wzPath), false, false).LoadWzFile(wzPath,
                WzMapleVersion.GMS);
            var w = _w.WzDirectory;

            foreach (var quest in quests)
            {
                Console.WriteLine("adding quest ID " + quest.Id);
                Console.WriteLine("questinfo.img");
                var q_img = w.GetImageByName("QuestInfo.img");

                var this_qi = new WzSubProperty(quest.Id.ToString());
                q_img.AddAndUpdate(this_qi);

                this_qi.AddAndUpdate(new WzStringProperty("name", quest.Name));
                if (quest.Parent != null)
                    this_qi.AddAndUpdate(new WzStringProperty("parent", quest.Parent));
                if (quest.Order.HasValue)
                    this_qi.AddAndUpdate(new WzIntProperty("order", quest.Order.Value));

                this_qi.AddAndUpdate(new WzIntProperty("area", quest.Area));
                this_qi.AddAndUpdate(new WzStringProperty("0", quest.Info.Available));
                this_qi.AddAndUpdate(new WzStringProperty("1", quest.Info.InProgress));
                this_qi.AddAndUpdate(new WzStringProperty("2", quest.Info.Completed));

                Console.WriteLine("check.img");
                var c_img = w.GetImageByName("Check.img");
                var this_check = new WzSubProperty(quest.Id.ToString());
                c_img.AddAndUpdate(this_check);

                var check_0 = new WzSubProperty("0");
                this_check.AddAndUpdate(check_0);

                check_0.AddAndUpdate(new WzIntProperty("npc", quest.StartNpc));
                if (quest.Prereqs.MinLevel.HasValue)
                    check_0.AddAndUpdate(new WzIntProperty("lvmin", quest.Prereqs.MinLevel.Value));
                if (quest.Prereqs.MaxLevel.HasValue)
                    check_0.AddAndUpdate(new WzIntProperty("lvmax", quest.Prereqs.MaxLevel.Value));

                if (quest.Prereqs.Job != null)
                {
                    var j = new WzSubProperty("job");
                    check_0.AddAndUpdate(j);
                    for (int i = 0; i < quest.Prereqs.Job.Count; i++)
                        j.AddAndUpdate(new WzIntProperty(i.ToString(), quest.Prereqs.Job[i])); ;
                }

                if (quest.Prereqs.Quests != null)
                {
                    var q = new WzSubProperty("quest");
                    check_0.AddAndUpdate(q);

                    for (int i = 0; i < quest.Prereqs.Quests.Count; i++)
                    {
                        var q_i = new WzSubProperty(i.ToString());
                        q.AddAndUpdate(q_i);
                        q_i.AddAndUpdate(new WzIntProperty("id", quest.Prereqs.Quests[i].Id));
                        q_i.AddAndUpdate(new WzIntProperty("state", quest.Prereqs.Quests[i].State));
                    }
                }

                if (quest.StartScript != null)
                    check_0.AddAndUpdate(new WzStringProperty("startscript", quest.StartScript));

                var check_1 = new WzSubProperty("1");
                this_check.AddAndUpdate(check_1);


                check_1.AddAndUpdate(new WzIntProperty("npc", quest.EndNpc));
                if (quest.Checks != null)
                {
                    if (quest.Checks.Items != null)
                    {
                        var i = new WzSubProperty("item");
                        check_1.AddAndUpdate(i);
                        for (int j = 0; j < quest.Checks.Items.Count; j++)
                        {
                            var i_j = new WzSubProperty(j.ToString());
                            i.AddAndUpdate(i_j);
                            i_j.AddAndUpdate(new WzIntProperty("id", quest.Checks.Items[j].Id));
                            i_j.AddAndUpdate(new WzIntProperty("count", quest.Checks.Items[j].Count));
                        }
                    }
                    if (quest.Checks.Mobs != null)
                    {
                        var m = new WzSubProperty("mob");
                        check_1.AddAndUpdate(m);
                        for (int j = 0; j < quest.Checks.Mobs.Count; j++)
                        {
                            var m_j = new WzSubProperty(j.ToString());
                            m.AddAndUpdate(m_j);
                            m_j.AddAndUpdate(new WzIntProperty("id", quest.Checks.Mobs[j].Id));
                            m_j.AddAndUpdate(new WzIntProperty("count", quest.Checks.Mobs[j].Count));
                        }
                    }
                }
                
                if (quest.EndScript != null)
                    check_1.AddAndUpdate(new WzStringProperty("endscript", quest.EndScript));

                Console.WriteLine("Say.img");
                var s_img = w.GetImageByName("Say.img");
                var this_say = new WzSubProperty(quest.Id.ToString());
                s_img.AddAndUpdate(this_say);

                var s0 = new WzSubProperty("0");
                this_say.AddAndUpdate(s0);
                var s1 = new WzSubProperty("1");
                this_say.AddAndUpdate(s1);

                if (quest.Dialogues != null)
                {
                    if (quest.Dialogues.PreStart != null)
                    {
                        var dps = quest.Dialogues.PreStart;
                        for (int d = 0; d < dps.Intro.Count(); d++)
                            s0.AddAndUpdate(new WzStringProperty(d.ToString(), dps.Intro[d]));

                        if (dps.Accepted != null)
                        {
                            var yes = new WzSubProperty("yes");
                            s0.AddAndUpdate(yes);
                            for (int d = 0; d < dps.Accepted.Count(); d++)
                                yes.AddAndUpdate(new WzStringProperty(d.ToString(), dps.Accepted[d]));
                        }

                        if (dps.Declined != null)
                        {
                            var no = new WzSubProperty("no");
                            s0.AddAndUpdate(no);
                            for (int d = 0; d < dps.Declined.Count(); d++)
                                no.AddAndUpdate(new WzStringProperty(d.ToString(), dps.Declined[d]));
                        }
                    }

                    if (quest.Dialogues.PostStart != null)
                    {
                        var dps = quest.Dialogues.PostStart;

                        if (dps.OnCompletion != null)
                            for (int d = 0; d < dps.OnCompletion.Count(); d++)
                                s1.AddAndUpdate(new WzStringProperty(d.ToString(), dps.OnCompletion[d]));

                        if (dps.OnMissingReq != null)
                        {
                            var stop = new WzSubProperty("stop");
                            s1.AddAndUpdate(stop);

                            var grouped = dps.OnMissingReq.GroupBy(x => x.Type);
                            foreach (var group in grouped)
                            {
                                var stop_type = new WzSubProperty(group.Key);
                                stop.AddAndUpdate(stop_type);
                                for (int d = 0; d < group.Count(); d++)
                                    stop_type.AddAndUpdate(new WzStringProperty(d.ToString(), group.Skip(d).First().Dialogue));
                            }
                        }
                    }
                }
                

                Console.WriteLine("Act.img");
                var a_img = w.GetImageByName("Act.img");
                var this_act = new WzSubProperty(quest.Id.ToString());
                a_img.AddAndUpdate(this_act);

                this_act.AddAndUpdate(new WzSubProperty("0"));
                
                var act_1 = new WzSubProperty("1");
                this_act.AddAndUpdate(act_1);

                if (quest.Rewards.Exp.HasValue)
                    act_1.AddAndUpdate(new WzIntProperty("exp", quest.Rewards.Exp.Value));

                if (quest.Rewards.Items != null)
                {
                    var act_i = new WzSubProperty("item");
                    act_1.AddAndUpdate(act_i);
                    for (int j = 0; j < quest.Rewards.Items.Count; j++)
                    {
                        var i_j = new WzSubProperty(j.ToString());
                        act_i.AddAndUpdate(i_j);
                        i_j.AddAndUpdate(new WzIntProperty("id", quest.Rewards.Items[j].Id));
                        i_j.AddAndUpdate(new WzIntProperty("count", quest.Rewards.Items[j].Count));
                    }

                }
                Console.WriteLine("next");
            }
            Console.WriteLine("OK!");
            _w.SaveToDisk("D:\\MapleDev\\Game\\WIP\\___\\Quest.wz", false, _w.MapleVersion);
        }

        private static void PrepareAllImgs(WzDirectory d)
        {
            foreach (WzImage i in d.WzImages) i.Changed = true;
            foreach (WzDirectory d2 in d.WzDirectories) PrepareAllImgs(d2);
        }
    }
}
