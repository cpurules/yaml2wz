using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlToWz
{
    internal class YamlQuest : YamlImportObject
    {
        public static List<string> WZ_FILES => ["Quest.wz"];
        public string Path { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Parent { get; set; }
        public int? Order { get; set; }
        public int Area { get; set; }
        public int StartNpc { get; set; }
        public string? StartScript { get; set; }
        public int EndNpc { get; set; }
        public string? EndScript { get; set; }
        public QuestBookInfo Info { get; set; }
        public QuestPrereqs Prereqs {get; set; }
        public QuestChecks Checks { get; set; }
        public QuestRewards Rewards { get; set; }
        public QuestDialogues? Dialogues { get; set; }

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            var wzQuest = wzFiles["Quest.wz"];
            var wzQuestInfoImg = wzQuest.WzDirectory.GetImageByName("QuestInfo.img");
            var wzCheckImg = wzQuest.WzDirectory.GetImageByName("Check.img");
            var wzSayImg = wzQuest.WzDirectory.GetImageByName("Say.img");
            var wzActImg = wzQuest.WzDirectory.GetImageByName("Act.img");

            Console.WriteLine($"importing quest ID {Id} : {Name}");
            // QuestInfo.img
            var qiNode = new WzSubProperty(Id.ToString());
            wzQuestInfoImg.AddAndUpdate(qiNode);
            qiNode.AddAndUpdate(new WzStringProperty("name", Name));
            if (Parent != null) qiNode.AddAndUpdate(new WzStringProperty("parent", Parent));
            if (Order.HasValue) qiNode.AddAndUpdate(new WzIntProperty("order", Order.Value));
            qiNode.AddAndUpdate(new WzIntProperty("area", Area));
            qiNode.AddAndUpdate(new WzStringProperty("0", Info.Available));
            qiNode.AddAndUpdate(new WzStringProperty("1", Info.InProgress));
            qiNode.AddAndUpdate(new WzStringProperty("2", Info.Completed));

            // Check.img
            var checkNode = new WzSubProperty(Id.ToString());
            wzCheckImg.AddAndUpdate(checkNode);
            var check_0 = new WzSubProperty("0");
            checkNode.AddAndUpdate(check_0);
            check_0.AddAndUpdate(new WzIntProperty("npc", StartNpc));
            if (Prereqs.MinLevel.HasValue)
                check_0.AddAndUpdate(new WzIntProperty("lvmin", Prereqs.MinLevel.Value));
            if (Prereqs.MaxLevel.HasValue)
                check_0.AddAndUpdate(new WzIntProperty("lvmax", Prereqs.MaxLevel.Value));
            if (StartScript != null)
                check_0.AddAndUpdate(new WzStringProperty("startscript", StartScript));
            //TODO this can be a list we check the count of just like we're doing in YamlData
            if (Prereqs.Job != null)
            {
                var job = new WzSubProperty("job");
                check_0.AddAndUpdate(job);
                for (int i = 0; i < Prereqs.Job.Count; i++)
                    job.AddAndUpdate(new WzIntProperty(i.ToString(), Prereqs.Job[i]));
            }
            if (Prereqs.Quests != null)
            {
                var prequest = new WzSubProperty("quest");
                check_0.AddAndUpdate(prequest);
                for (int i = 0; i < Prereqs.Quests.Count; i++)
                {
                    var prequest_i = new WzSubProperty(i.ToString());
                    Quest q = Prereqs.Quests[i];
                    prequest.AddAndUpdate(prequest_i);
                    prequest_i.AddAndUpdate(new WzIntProperty("id", q.Id));
                    prequest_i.AddAndUpdate(new WzIntProperty("state", q.State));
                }
            }

            var check_1 = new WzSubProperty("1");
            checkNode.AddAndUpdate(check_1);
            check_1.AddAndUpdate(new WzIntProperty("npc", EndNpc));
            if (EndScript != null) check_1.AddAndUpdate(new WzStringProperty("endscript", EndScript));
            if (Checks != null)
            {
                if (Checks.Items != null)
                {
                    var item = new WzSubProperty("item");
                    check_1.AddAndUpdate(item);
                    for (int i = 0; i < Checks.Items.Count; i++)
                    {
                        var item_i = new WzSubProperty(i.ToString());
                        Item itemCheck = Checks.Items[i];
                        item.AddAndUpdate(item_i);
                        item_i.AddAndUpdate(new WzIntProperty("id", itemCheck.Id));
                        item_i.AddAndUpdate(new WzIntProperty("count", itemCheck.Count));
                    }
                }
                if (Checks.Mobs != null)
                {
                    var mob = new WzSubProperty("mob");
                    check_1.AddAndUpdate(mob);
                    for (int i = 0; i < Checks.Mobs.Count; i++)
                    {
                        var mob_i = new WzSubProperty(i.ToString());
                        mob.AddAndUpdate(mob_i);
                    }
                }
            }

            // Say.img
            var sayNode = new WzSubProperty(Id.ToString());
            wzSayImg.AddAndUpdate(sayNode);
            // These are always present even if no dialogue or handled by server script
            var say_0 = new WzSubProperty("0");
            var say_1 = new WzSubProperty("1");
            sayNode.AddAndUpdate(say_0);
            sayNode.AddAndUpdate(say_1);
            if (Dialogues != null)
            {
                var qd = Dialogues;
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

            // Act.img
            var actNode = new WzSubProperty(Id.ToString());
            wzActImg.AddAndUpdate(actNode);
            actNode.AddAndUpdate(new WzSubProperty("0")); // I only see quests with values in 0 where it appears to be Say.img but in Korean?....
            var act_1 = new WzSubProperty("1");
            actNode.AddAndUpdate(act_1);

            if (Rewards != null)
            {
                if (Rewards.Exp.HasValue) act_1.AddAndUpdate(new WzIntProperty("exp", Rewards.Exp.Value));
                if (Rewards.Money.HasValue) act_1.AddAndUpdate(new WzIntProperty("money", Rewards.Money.Value));
                if (Rewards.Items != null)
                {
                    var item = new WzSubProperty("item");
                    act_1.AddAndUpdate(item);
                    for (int i = 0; i < Rewards.Items.Count; i++)
                    {
                        var item_i = new WzSubProperty(i.ToString());
                        item.AddAndUpdate(item_i);
                        item_i.AddAndUpdate(new WzIntProperty("id", Rewards.Items[i].Id));
                        item_i.AddAndUpdate(new WzIntProperty("count", Rewards.Items[i].Count));
                    }

                }
            }
        }
    }

    internal class QuestDialogues
    {
        public QuestPreStartDialogues? PreStart { get; set; }
        public QuestPostStartDialogues? PostStart { get; set; }
    }

    internal class QuestPreStartDialogues
    {
        public List<String> Intro { get; set; }
        public List<String>? Accepted { get; set; }
        public List<String>? Declined { get; set; }
    }

    internal class QuestPostStartDialogues
    {
        public List<String>? OnCompletion { get; set; }
        public List<QuestMissingReq>? OnMissingReq { get; set; }
    }

    internal class QuestMissingReq
    {
        public String Type { get; set; }
        public String Dialogue { get; set; }
    }

    internal class QuestRewards
    {
        public int? Exp { get; set; }
        public int? Money { get; set; }
        public List<Item>? Items { get; set; }
    }

    internal class QuestChecks
    {
        public List<Mob>? Mobs { get; set; }
        public List<Item>? Items { get; set; }
    }

    internal class QuestBookInfo
    {
        public string Available { get; set; }
        public string InProgress { get; set; }
        public string Completed { get; set; }
    }

    internal class QuestPrereqs
    {
        public List<int>? Job { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public List<Quest>? Quests { get; set; }
        
    }

    internal class Mob
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }

    internal class Item
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }

    internal class Quest
    {
        public int Id { get; set; }
        public int State { get; set; }
    }
}
