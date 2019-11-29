﻿using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class NavigationMainPage : NavigationPage
    {
        public NavigationMainPage()
        {
            SetHasNavigationBar(this, false);

            LongPollingClient.OnFullReset += async (s, e) =>
            {
                Navigation.InsertPageBefore(new DialogsPage(), Navigation.NavigationStack[0]);
                await PopToRootAsync();
            };

            if (Authorization.Token != null)
                PushAsync(new DialogsPage());
            else
                PushAsync(new AuthorizationPage());
        }
    }
}