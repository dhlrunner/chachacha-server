using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chachacha_server.HTTP
{
    public class HTTPPath
    {
        public static string CheckService = "/service/inspection/check";
        public static string GetNotice = "/service/notice/get";
        public static string Login = "/user/auth/login";
        public static string GetUserInfo = "/user/info/get";
        public static string GetRanking = "/ranking/current/list";
        public static string GetVersusList = "/play/versus/list";
        public static string GetUserCarList = "/user/car/list";
        public static string CarEventGive = "/event/car/give";
        public static string GetUserCharacterList = "/user/character/list";
        public static string GetItemList = "/shop/item/list";
        public static string GetInviteList = "/invitation/list";
        public static string GetGiftList = "/tire/present/list";
        public static string GetGrandPrixInfo = "/event/grandprix/info";
        public static string StartGame = "/play/game/start";
    }
}
