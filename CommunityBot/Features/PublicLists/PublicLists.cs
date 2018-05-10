using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunityBot.Features.PublicLists
{
    public enum Results { Success, NoDuplicatesAllowed, NotFound }

    public static class PublicLists
    {
        public static Results AddItem(Dictionary<string, string> list, string title, string description)
        {
            // Don't allow duplicate titles
            if (list.ContainsKey(title))
                return Results.NoDuplicatesAllowed;
            list.Add(title, description);
            return Results.Success;
        }

        public static Results RemoveItems(Dictionary<string, string> list, string title)
        {
            if (list.ContainsKey(title) == false)
                return Results.NotFound;
            list.Remove(title);
            return Results.Success;
        }

        public static Results NewList(Dictionary<string, Dictionary<string, string>> lists, string listName)
        {
            if (lists.ContainsKey(listName))
                return Results.NoDuplicatesAllowed;
            lists.Add(listName, new Dictionary<string, string>());
            return Results.Success;
        }

        public static List<KeyValuePair<string, string>> PageList(Dictionary<string, string> list, int page, int pageSize) 
        {
            page--;
            var keyValuePairList = list.ToList();
            return keyValuePairList.Skip(page * pageSize).Take(pageSize).ToList();
        }
    }
}
