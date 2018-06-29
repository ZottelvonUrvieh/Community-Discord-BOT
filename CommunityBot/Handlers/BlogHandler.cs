using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Configuration;
using CommunityBot.Features.Blogs;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace CommunityBot.Handlers
{
    class BlogHandler
    {
        private static readonly string blogFile = "blogs.json";
        public static Embed SubscribeToBlog(ulong userId, string blogname)
        {
            var blogs = DataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (!blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Add(userId);

                    DataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Blog", "You now follow this blog", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Blog :x:", "You already follow this blog", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed("Blog :x:", $"There is no Blog with the name {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }

        public static Embed UnSubscribeToBlog(ulong userId, string blogname)
        {
            var blogs = DataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Remove(userId);

                    DataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Blog", "You stopped following this blog", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Blog :x:", "You don't follow this blog", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed("Blog :x:", $"There is no Blog with the name {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }

        public static async Task ReactionAdded(SocketReaction reaction)
        {
            var msgList = Global.MessagesIdToTrack ?? new Dictionary<ulong, string>();
            if (msgList.ContainsKey(reaction.MessageId))
            {
                if (reaction.Emote.Name == "➕")
                {
                    var item = msgList.FirstOrDefault(k => k.Key == reaction.MessageId);
                    var embed = BlogHandler.SubscribeToBlog(reaction.User.Value.Id, item.Value);
                }
            }
        }
    }
}
