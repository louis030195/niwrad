using System.Linq;
using Api.Session;
using Nakama;

namespace Api.Storage
{
    public static class Extensions
    {
        public static async void DeleteAllAsync(this Client client, string collection)
        {
            var session = Sm.instance.session;
            var users = await client.ListUsersStorageObjectsAsync(session, collection, session.UserId);
            await client.DeleteStorageObjectsAsync(session, users.Objects.Select(o => new StorageObjectId
            {
                Collection = collection,
                Key = o.Key,
                UserId = session.UserId
            }).ToArray());
        }
    }
}
