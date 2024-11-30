using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices.JavaScript;

namespace chachacha_server.HTTP.DAO
{ 
    class HTTPResponce
    {
        public bool success;
        public string? errorCode;

        /// <summary>
        /// Json 문자열을 UTF8 바이트 배열로
        /// </summary>
        /// <param name="aes">null이 아닌 경우 해당 Aes 인스턴스를 사용해 암호화된 base64 문자열을 반환</param>
        /// <returns></returns>
        public byte[] ToUTF8ByteArray(Aes aes = null)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.None);
            Console.WriteLine(json);
            if (aes != null)
            {
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(json);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    string base64 = Convert.ToBase64String(encryptedBytes);
                    return Encoding.ASCII.GetBytes(base64);
                }
            }
            return Encoding.UTF8.GetBytes(json);
        }      
       
    }

    class LoginResult
    {
        public bool resistered;
        public bool takeTrophy;
        public bool newWeek;
        public bool purchased;
        public required string newWeekStart;
        public bool newPresent;
        public long accountSeq;
    }

    class CheckServiceDTO : HTTPResponce
    {
        
    }

    class GetNoticeDTO : HTTPResponce
    {
        public required string notice;
        public required string noticeUrl;
    }    

    class LoginDataDTO : HTTPResponce
    {
        public long token;
        public required string cryptoKey;
        public required string initialVector;
        public required string blockExpire;
        public LoginResult result;
    }

    class UserInfoMDTO
    {
        public long tireCnt;
        public int tireRemainSecs;
        public bool canPresent;
        public long gold;
        public int trophyCnt;
        public int characterNo;
        public int carNo;
        public long carSeq;
        public required string carClass;
        public int carAccel;
        public int carSpeed;
        public int carFuleCost;
        public long maxDistance;
        public long maxPoint;
        public long maxCombo;
        public long maxGold;
        public long maxScore;
        public long maxSpeed;
        public long goldAmt;
        public long goldUse;
        public long playCount;
        public long crashCount;
        public long jumpCount;
        public long item1Use;
        public long item2Use;
        public long item3Use;
        public long item4Use;
        public long item5Use;
        public long item6Use;
        public long item7Use;
        public long totalDistance;
        public long totalCombo;
        public long maxSpeedCrashCount;
        public long totalCarDestroyCount;
        public long dormancyTireSendCnt;
        public int friendInviteCnt;
        public int missionsCount;
        public required int[] missions;
    }
    class UserInfoDTO : HTTPResponce
    {
        public required long token;
        public required UserInfoMDTO info;
    }

    class FriendInfoDTO
    {
        public required string userId;
        public long accountSeq;
        public required string gameMode;
        public int score;
        public int carNo;
        public required string carClass;
        public bool canPresent;
        public bool sentPresent;
        public bool boastReject;
        public int carX;
        public int carY;
        public bool matchRejectFlag;
        public bool isDormancy;
        public required string grade;
    }

    class RankInfoDTO : HTTPResponce
    {
        public long token;
        public required FriendInfoDTO[] friends;
        public int friendsCount;
    }

    class MatchesDTO
    {
        public long matchNo;
        public long score;
        public required string gameMode;
        public long gold;
        public long chacha;
        public long carDestroy;
        public long distance;
        public long versusAccountSeq;
        public long versusScore;
        public long versusGold;
        public long versusChacha;
        public long versusCarDestroy;
        public long versusDistance;
        public required string matchResult;
        public required string updateDate;
    }

    class VersusListDTO : HTTPResponce
    {
        public long token;
        public required MatchesDTO[] matches;
    }

    class CarDTO
    {
        public long carSeq;
        public int carNo;
        public required string carClass;
        public int carAccel;
        public int carSpeed;
        public int carFuleCost;
        public bool isSelected;
    }
    class CarListDTO : HTTPResponce
    {
        public long token;
        public required CarDTO[] cars;
    }

    class GiveEventCarDTO : HTTPResponce
    {
        public long token;
        public long carSeq;
    }

    class CharacterDTO
    {
        public int characterNo;
        public bool isSelected;
    }

    class UserCharacterListDTO : HTTPResponce
    {
        public long token;
        public required CharacterDTO[] characters;
    }

    class ShopItem
    {
        public int itemCode;
        public int itemCount;
    }

    class ItemListDTO : HTTPResponce
    {
        public long token;
        public required ShopItem[] items;
    }


    class Invite
    {
        public required string userId;
        public required string inviteDate;
    }

    class InviteListDTO : HTTPResponce
    {
        public long token;
        public required Invite[] invitations;
    }

    class Gift
    {
        public long presentSeq;
        public long accountSeq;
        public string presentType;
        public long presentQty;
        public string recvDate;
        public bool FindPresent;
    }
    class GiftListDTO : HTTPResponce
    {
        public long token;
        public required Gift[] presents;
    }

    class GrandPrixBestScore
    {
        public int carNo;
        public int score;
        public int chacha;
        public int gold;
        public int carDestory;
        public int distance;
    }
    class GrandPrixInfoDTO : HTTPResponce
    {
        public long token;
        public required string status;
        public required GrandPrixBestScore bestScore;
        public required string joinDate;
    }

    class GameStartItem : ShopItem
    {
        public bool autoUse;
        public bool used;
    }

    class GameStartDTO : HTTPResponce
    {
        public long token;
        public long raceValue;
        public required int[] missions;
        public required GameStartItem[] items;
    }
}
