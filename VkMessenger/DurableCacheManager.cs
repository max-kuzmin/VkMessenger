using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Applications;

namespace ru.MaxKuzmin.VkMessenger
{
    public static class DurableCacheManager
    {
        private const string DialogsCacheKey = "_dialogsCache";

        public static Task SaveDialogs(IReadOnlyCollection<Dialog> dialogs)
        {
            dialogs = dialogs.Take(Consts.BatchSize).ToArray();
            Preference.Set(DialogsCacheKey, JsonConvert.SerializeObject(dialogs));
            return Task.CompletedTask;
        }

        public static async Task SaveMessages(int dialogId, IReadOnlyCollection<Message> messages)
        {
            var cached = await GetDialogs();
            if (cached == null)
                return;

            var dialog = cached.FirstOrDefault(e => e.Id == dialogId);
            if (dialog == null)
                return;

            messages = messages.Take(Consts.BatchSize).ToArray();
            dialog.Messages = new ObservableCollection<Message>(messages);

            await SaveDialogs(cached);
        }

        public static async Task SaveDialog(Dialog dialog)
        {
            var cached = await GetDialogs();
            if (cached == null)
                return;

            var dialogIndex = cached.FindIndex(e => e.Id == dialog.Id);
            if (dialogIndex == -1)
                return;

            cached[dialogIndex] = dialog;

            await SaveDialogs(cached);
        }

        public static Task<List<Dialog>?> GetDialogs()
        {
            if (Preference.Contains(DialogsCacheKey))
            {
                try
                {
                    var json = Preference.Get<string>(DialogsCacheKey);
                    var result = JsonConvert.DeserializeObject<List<Dialog>>(json);
                    return Task.FromResult<List<Dialog>?>(result);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            return Task.FromResult<List<Dialog>?>(null);
        }
    }
}
