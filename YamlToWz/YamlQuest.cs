using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlToWz
{
    internal class YamlQuest
    {
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
