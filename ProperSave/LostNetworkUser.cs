using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ProperSave
{
    internal class LostNetworkUser : MonoBehaviour
    {
        private static readonly Dictionary<CharacterMaster, LostNetworkUser> lostUsers = new Dictionary<CharacterMaster, LostNetworkUser>();

        private CharacterMaster master;

        public uint lunarCoins;
        public NetworkUserId userID;

        private void Awake()
        {
            master = GetComponent<CharacterMaster>();
            lostUsers[master] = this;
        }

        private void OnDestroy()
        {
            lostUsers.Remove(master);
        }

        public static bool TryGetUser(CharacterMaster master, out LostNetworkUser lostUser)
        {
            if (!master || !lostUsers.TryGetValue(master, out lostUser))
            {
                lostUser = null;
                return false;
            }

            return true;
        }

        public static void Subscribe()
        {
            NetworkUser.onNetworkUserLost += OnNetworkUserLost;
        }

        public static void Unsubscribe()
        {
            NetworkUser.onNetworkUserLost -= OnNetworkUserLost;
        }

        private static void OnNetworkUserLost(NetworkUser networkUser)
        {
            if (networkUser.master)
            {
                var lostUser = networkUser.master.gameObject.AddComponent<LostNetworkUser>();
                lostUser.lunarCoins = networkUser.lunarCoins;
                lostUser.userID = networkUser.id;
            }
        }
    }
}
