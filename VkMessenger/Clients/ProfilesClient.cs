using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ru.MaxKuzmin.VkMessenger.Dtos;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class ProfilesClient
    {
        private static Profile FromDto(ProfileDto profile)
        {
            return new Profile
            {
                Id = profile.id,
                Name = profile.first_name,
                Surname = profile.last_name,
                Photo = ImageSource.FromUri(profile.photo_50),
                Online = profile.online != 0
            };
        }

        public static IReadOnlyCollection<Profile> FromDtoArray(ProfileDto[]? profiles)
        {
            return profiles == null
                ? Array.Empty<Profile>()
                : profiles.Select(FromDto).ToArray();
        }
    }
}
