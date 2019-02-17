using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog
    {
        public int Id { get; set; }
        public Message LastMessage { get; set; }
        public List<Profile> Profiles { get; set; }
        public Group Group { get; set; }
        public DialogType Type { get; set; }
        public string Name { get; set; }
        public Image Photo { get; set; }

        public enum DialogType
        {
            User,
            Group,
            Chat
        }

        public static List<Dialog> FromJsonArray(JArray dialogs, List<Profile> profiles, List<Group> groups)
        {
            var result = new List<Dialog>();

            foreach (var item in dialogs)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        public static Dialog FromJson(JObject dialog, List<Profile> profiles, List<Group> groups)
        {
            var result = new Dialog
            {
                Id = dialog["conversation"]["peer"]["id"].Value<int>(),
                LastMessage = Message.FromJson(dialog["last_message"] as JObject)
            };

            var peerType = dialog["conversation"]["peer"]["type"].Value<string>();
            if (peerType == "user")
            {
                result.Type = DialogType.User;
                result.Profiles = new List<Profile> { profiles.First(p => p.Id == result.Id) };
            }
            else if (peerType == "group")
            {
                result.Type = DialogType.Group;
                result.Group = groups.First(g => g.Id == Math.Abs(result.Id));
            }
            else if (peerType == "chat")
            {
                result.Type = DialogType.Chat;
                result.Name = dialog["conversation"]["chat_settings"]["title"].Value<string>();
                result.Profiles = new List<Profile>();
                var ids = dialog["conversation"]["chat_settings"]["active_ids"] as JArray;

                foreach (var id in ids)
                {
                    result.Profiles.Add(profiles.First(p => p.Id == id.Value<int>()));
                }

                if (dialog["conversation"]["chat_settings"]["photo"] != null)
                {
                    result.Photo = new Image { Source = dialog["conversation"]["chat_settings"]["photo"]["photo_50"].Value<string>() };
                }
            }

            return result;
        }

        public string GetTitle()
        {
            switch (Type)
            {
                case DialogType.User:
                    return Profiles.First().Name + " " + Profiles.First().Surname;
                case DialogType.Group:
                    return Group.Name;
                case DialogType.Chat:
                    return Name;
            }

            return string.Empty;
        }

        public Image GetPhoto()
        {
            if (Photo != null)
                return Photo;
            else if (Group != null)
                return Group.Photo;
            else
                return Profiles.First().Photo;
        }

        private static Profile GetFriend(JObject dialog, JArray profiles)
        {
            var dialogId = dialog["conversation"]["peer"]["id"].Value<int>();
            var profile = profiles.Where(o => o["id"].Value<int>() == dialogId).FirstOrDefault();
            if (profile != null)
            {
                return Profile.FromJson(profile as JObject);
            }
            else
            {
                return null;
            }
        }
    }
}
