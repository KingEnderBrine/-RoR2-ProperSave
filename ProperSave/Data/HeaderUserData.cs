using System;
using System.Collections.Generic;
using System.Text;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class HeaderUserData
    {
        public BodyIndex Body { get; set; }
        public UserIDData UserId { get; set; }

        public static HeaderUserData Create(PlayerCharacterMasterController master)
        {
            var data = new HeaderUserData();
            if (master.networkUser)
            {
                data.UserId = UserIDData.Create(master.networkUser.id);
            }
            else if (LostNetworkUser.TryGetUser(master.master, out var lostNetworkUser))
            {
                data.UserId = UserIDData.Create(lostNetworkUser.userID);
            }
            else
            {
                return null;
            }
            data.Body = (master.master.originalBodyPrefab ?? master.master.bodyPrefab).GetComponent<CharacterBody>().bodyIndex;

            return data;
        }

        internal static HeaderUserData Read(ReaderContext context)
        {
            var data = new HeaderUserData();
            data.Body = SharedIndexHelpers.ResolveBody(context.Reader.ReadInt32(), context);
            data.UserId = UserIDData.Read(context);

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.Write(SharedIndexHelpers.FromBody(Body, context));
            UserId.Write(context);
        }
    }
}
