using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DudesBot
{
    public static class UtilityMethods
    {
        public static async Task<string> GetProfilePictureURL(DiscordUser queriedUser, HttpClient httpClient)
        {
            try //Try cast into DiscordMember to get guild avatar
            {
                DiscordMember queriedMember = (DiscordMember) queriedUser;
                await httpClient.GetByteArrayAsync(queriedMember.GuildAvatarUrl);
                return queriedMember.GuildAvatarUrl;
            }
            catch //If not get avatar off the DiscordUser object
            {
                return queriedUser.AvatarUrl;
            }
        }

        public static string GetName(DiscordUser queriedUser)
        {
            if(queriedUser as DiscordMember is null)
            {
                return queriedUser.Username;
            }
            else
            {
                DiscordMember queriedMember = queriedUser as DiscordMember;
                if(queriedMember.Nickname is not null)
                {
                    return queriedMember.Nickname;
                }
                else
                {
                    return queriedMember.Username;
                }
            }
        }

    }
}