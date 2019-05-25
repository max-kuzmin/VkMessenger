using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tizen;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private Dictionary<int, MessagesPage> messagesPages = new Dictionary<int, MessagesPage>();
        private bool setupScroll = true;

        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell))
        };
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Setup();
            Update(null);
        }

        private async void Update(IReadOnlyCollection<int> dialogIds)
        {
            try
            {
                var newDialogs = await DialogsClient.GetDialogs(dialogIds);

                foreach (var newDialog in newDialogs.AsEnumerable().Reverse())
                {
                    var foundDialog = dialogs.FirstOrDefault(d => d.Id == newDialog.Id);

                    if (foundDialog == null)
                        dialogs.Insert(0, newDialog);
                    else
                    {
                        UpdateDialog(newDialog, foundDialog);

                        if (dialogs.Last() != foundDialog)
                        {
                            dialogs.Remove(foundDialog);
                            dialogs.Insert(0, foundDialog);
                        }
                        else foundDialog.InvokePropertyChanged();
                    }
                }

                Scroll();
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
                Toast.DisplayText(e.Message);
            }
        }

        private void Scroll()
        {
            if (setupScroll)
            {
                var firstDialog = dialogs.FirstOrDefault();
                if (firstDialog != null)
                {
                    dialogsListView.ScrollTo(firstDialog, ScrollToPosition.Center, false);
                    setupScroll = false;
                }
            }
        }

        private static void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foundDialog.LastMessage = newDialog.LastMessage;
            foundDialog.UnreadCount = newDialog.UnreadCount;

            if (newDialog.Profiles != null)
            {
                foreach (var newProfile in newDialog.Profiles)
                {
                    var foundProfile = foundDialog.Profiles.FirstOrDefault(p => p.Id == newProfile.Id);
                    if (foundProfile != null)
                        foundProfile.IsOnline = newDialog.IsOnline;
                }
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemTapped += OnDialogTapped;
            dialogsListView.ItemsSource = dialogs;
            Content = dialogsListView;

            LongPollingClient.OnMessageUpdate += (s, e) => Update(new[] { e.DialogId });
            LongPollingClient.OnDialogUpdate += (s, e) => Update(new[] { e });

            LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
        }

        private void OnUserStatusUpdate(object sender, UserStatusEventArgs e)
        {
            foreach (var dialog in dialogs)
            {
                var profile = dialog.Profiles?.FirstOrDefault(p => p.Id == e.UserId);
                if (profile != null)
                {
                    profile.IsOnline = e.IsOnline;
                    dialog.InvokePropertyChanged();
                }
            }
        }

        private async void OnDialogTapped(object sender, ItemTappedEventArgs args)
        {
            try
            {
                var dialog = args.Item as Dialog;
                MessagesPage messagesPage;
                if (messagesPages.ContainsKey(dialog.Id))
                {
                    messagesPage = messagesPages[dialog.Id];
                }
                else
                {
                    messagesPage = new MessagesPage(dialog.Id);
                    messagesPages.Add(dialog.Id, messagesPage);
                }

                await Navigation.PushAsync(messagesPage);
                await DialogsClient.MarkAsRead(dialog.Id);
            }
            catch (Exception e)
            {
                Log.Error(nameof(VkMessenger), e.ToString());
                Toast.DisplayText(e.Message);
            }
        }
    }
}
