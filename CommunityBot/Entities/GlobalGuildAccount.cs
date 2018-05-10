using System.Collections.Generic;
using CommunityBot.Features.PublicLists;
using CommunityBot.Features.RoleAssignment;

namespace CommunityBot.Entities
{
    public class GlobalGuildAccount : IGlobalAccount
    {
        public ulong Id { get; set; }

        public ulong AnnouncementChannelId { get; set; }

        public List<string> Prefixes { get; set; } = new List<string>();

        public List<string> WelcomeMessages { get; set; } = new List<string> { };

        public List<string> LeaveMessages { get; set; } = new List<string>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Modules.ServerBots.GuildData BotData { get; set; }

        public RoleByPhraseSettings RoleByPhraseSettings { get; set; } = new RoleByPhraseSettings();

        public Dictionary<string, Dictionary<string, string>> PublicLists { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        /* Add more values to store */
    }
}