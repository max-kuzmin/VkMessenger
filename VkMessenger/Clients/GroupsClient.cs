using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ru.MaxKuzmin.VkMessenger.Dtos;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class GroupsClient
    {
        private static Group FromDto(GroupDto group)
        {
            return new Group
            {
                Id = group.id,
                Name = group.name,
                Photo = ImageSource.FromUri(group.photo_50)
            };
        }

        public static IReadOnlyCollection<Group> FromDtoArray(GroupDto[]? groups)
        {
            return groups == null
                ? Array.Empty<Group>()
                : groups.Select(FromDto).ToArray();
        }
    }
}
