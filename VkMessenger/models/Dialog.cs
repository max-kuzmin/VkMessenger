using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Dialog
    {
        private Image chatPhoto;
        private string profileName;

        public int Id { get; set; }
        public Message LastMessage { get; set; }
        public List<Profile> Profiles { get; set; }
        public Group Group { get; set; }
        public DialogType Type { get; set; }
        public int UnreadCount { get; set; }
        public Color TextColor => UnreadCount > 0 ? Color.Yellow : Color.White;
        public string Text => LastMessage.Text;

        public enum DialogType
        {
            User,
            Group,
            Chat
        }

        public class Comparer : IComparer<Dialog>
        {
            public int Compare(Dialog x, Dialog y)
            {
                if (x.LastMessage.Date < y.LastMessage.Date) return 1;
                else if (x.LastMessage.Date > y.LastMessage.Date) return -1;
                else return 0;
            }
        }

        public string Title
        {
            get
            {
                switch (Type)
                {
                    case DialogType.User:
                        return Profiles.First().Name + " " + Profiles.First().Surname;
                    case DialogType.Group:
                        return Group.Name;
                    case DialogType.Chat:
                        return profileName;
                }

                return string.Empty;
            }
        }

        public int PeerId
        {
            get
            {
                switch (Type)
                {
                    case DialogType.User:
                    case DialogType.Chat:
                        return Id;
                    case DialogType.Group:
                        return -Id;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public Image Photo
        {
            get
            {
                if (chatPhoto != null)
                    return chatPhoto;
                else if (Group != null)
                    return Group.Photo;
                else
                    return Profiles.First().Photo;
            }
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
                UnreadCount = dialog["conversation"]["unread_count"]?.Value<int>() ?? 0,
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
                result.profileName = dialog["conversation"]["chat_settings"]["title"].Value<string>();
                result.Profiles = new List<Profile>();
                var ids = dialog["conversation"]["chat_settings"]["active_ids"] as JArray;

                foreach (var id in ids)
                {
                    result.Profiles.Add(profiles.First(p => p.Id == id.Value<int>()));
                }

                if (dialog["conversation"]["chat_settings"]["photo"] != null)
                {
                    result.chatPhoto = new Image { Source = dialog["conversation"]["chat_settings"]["photo"]["photo_50"].Value<string>() };
                }
            }

            return result;
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

        public static List<Dialog> GetDialogs()
        {
            var json = JObject.Parse(Api.GetDialogsJson());
            var profiles = Profile.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = Group.FromJsonArray(json["response"]["groups"] as JArray);
            return Dialog.FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
        }
    }
}
