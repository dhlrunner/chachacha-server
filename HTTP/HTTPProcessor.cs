using chachacha_server.HTTP.DAO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aes = System.Security.Cryptography.Aes;

namespace chachacha_server.HTTP
{
    internal class HTTPProcessor
    {
        static HttpListener listener = new HttpListener(); //HTTPサーバ
        static Thread thread = new Thread(new ParameterizedThreadStart(WorkerThread));

        //Todo: 유저별로 정해야됨
        private static Aes aes;

        public HTTPProcessor(string prefix) 
        {
            listener.Prefixes.Add(string.Format($"{prefix}"));

        }

        public void StartListening() 
        {
            listener.Start();  //서버 스레드 시작
            if (!thread.IsAlive)
                thread.Start(listener);
            Console.WriteLine(string.Format("Server Started at {0}", listener.Prefixes.ToArray()[0]));
            InitEncryption();
        }

        private static void WorkerThread(object arg)
        {
            try
            {
                while (listener.IsListening)
                {
                    HttpListenerContext ctx = listener.GetContext();
                    ProcessRequest(ctx);
                }
            }
            catch (ThreadAbortException)
            {
                //frm.textBox1.AppendText("Normal Stopping Service");
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private static void ResponceProcessBinary(HttpListenerContext ctx, byte[] data, HttpStatusCode statusCode)
        {

            //HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;
            ctx.Response.StatusCode = (int)statusCode;
            //헤더 설정        
            response.Headers.Add("Accept-Encoding", "none"); //gzip 처리하기 귀찮으므로 비압축
            response.Headers.Add("Content-Type", "application/text");
            response.Headers.Add("Server", "DHL-Chachacha-server");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            //스트림 쓰기
            response.ContentLength64 = data.Length;
            Console.WriteLine(string.Format("Responce Length: {0}", data.Length));
            Stream output = response.OutputStream;
            output.Write(data, 0, data.Length);
        }

        public static void InitEncryption()
        {
            aes = Aes.Create();
            // AES 설정
            aes.Mode = CipherMode.CBC;           // CBC 모드
            aes.Padding = PaddingMode.PKCS7;     // PKCS7 패딩
            aes.KeySize = 128;                   // 키 사이즈 128비트
            aes.BlockSize = 128;                 // 블록 사이즈 128비트

            // 키와 IV 설정
            aes.Key = GenerateRandomBytes(16);   // 128비트 키 (16바이트)
            aes.IV = GenerateRandomBytes(16);    // 128비트 IV (16바이트)

            Console.WriteLine("AES CBC 인스턴스가 성공적으로 생성되었습니다.");
            Console.WriteLine($"키: {BitConverter.ToString(aes.Key)}");
            Console.WriteLine($"IV: {BitConverter.ToString(aes.IV)}");
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        private static T GetDecryptedDTO<T>(byte[] ClientRequestData)
        {
            string plainJson;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                byte[] encryptedBytes = Convert.FromBase64String(Encoding.ASCII.GetString(ClientRequestData));
                byte[] plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                plainJson = Encoding.UTF8.GetString(plainBytes);
            }
            Console.WriteLine(plainJson);

            return JsonConvert.DeserializeObject<T>(plainJson);
        }

        private static string GetDecryptedJSON(byte[] ClientRequestData)
        {
            string plainJson;
            try
            {
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] encryptedBytes = Convert.FromBase64String(Encoding.ASCII.GetString(ClientRequestData));
                    byte[] plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    plainJson = Encoding.UTF8.GetString(plainBytes);
                }
                Console.WriteLine(plainJson);

                return plainJson;
            }
           catch
            {
                return null;
            }
        }

        public static void ProcessRequest(HttpListenerContext ctx) 
        {
            string urlPath = ctx.Request.Url.LocalPath;
            byte[] Client_Req_data = new byte[0];
            if (ctx.Request.HttpMethod == "POST")
            {
                Client_Req_data = ReadFully(ctx.Request.InputStream);
            }

            Console.WriteLine(string.Format("Request: {0} , Methood:{2}, body: {1} byte", urlPath, Client_Req_data.Length, ctx.Request.HttpMethod));
            Console.WriteLine(Encoding.UTF8.GetString(Client_Req_data));

           
            if (urlPath == HTTPPath.CheckService)
            {
                CheckServiceDTO res = new CheckServiceDTO();
                res.success = true;

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetNotice)
            {       
                GetNoticeDTO res = new GetNoticeDTO
                {
                    success = true,
                    notice = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    noticeUrl = "https://google.com"
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.Login)
            {
                LoginDataDTO res = new LoginDataDTO
                {
                    success = true,
                    cryptoKey = Convert.ToBase64String(aes.Key),
                    initialVector = Convert.ToBase64String(aes.IV),
                    token = 123456789,
                    blockExpire = "",
                    result = new LoginResult
                    {
                        accountSeq = 1,
                        newPresent = false,
                        newWeek = false,
                        newWeekStart = DateTime.Now.ToString("yyyyMMdd"),
                        purchased = false,
                        resistered = true,
                        takeTrophy = true
                    }
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(), HttpStatusCode.OK);
                return;
            }
            else if(urlPath == HTTPPath.GetUserInfo)
            {
                ReqUserInfoGetDTO req = GetDecryptedDTO<ReqUserInfoGetDTO>(Client_Req_data);
                UserInfoDTO res = new UserInfoDTO
                {
                    success = true,
                    token = 123456789,
                    info = new UserInfoMDTO
                    {
                        tireCnt = 1000000,
                        tireRemainSecs = 0,
                        canPresent = false,
                        gold = 1000000,
                        trophyCnt = 1000000,
                        characterNo = 0,
                        carNo = 1,
                        carSeq = 0,
                        carClass = "S",
                        carAccel = 0,
                        carSpeed = 0,
                        carFuleCost = 0,
                        maxDistance = 0,
                        maxPoint = 0,
                        maxCombo = 0,
                        maxGold = 0,
                        maxScore = 0,
                        maxSpeed = 0,
                        goldAmt = 0,
                        goldUse = 0,
                        playCount = 0,
                        crashCount = 0,
                        jumpCount = 0,
                        item1Use = 0,
                        item2Use = 0,
                        item3Use = 0,
                        item4Use = 0,
                        item5Use = 0,
                        item6Use = 0,
                        item7Use = 0,
                        totalDistance = 0,
                        totalCombo = 0,
                        maxSpeedCrashCount = 0,
                        totalCarDestroyCount = 0,
                        dormancyTireSendCnt = 123,
                        friendInviteCnt = 0,
                        missionsCount = 0,
                        missions = Array.Empty<int>()
                    }
                };
              
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if(urlPath == HTTPPath.GetRanking)
            {
                RankInfoDTO res = new RankInfoDTO
                {
                    success = true,
                    token = 123456789,
                    friends =
                    [
                        
                        new FriendInfoDTO
                        {
                            userId = "1",
                            accountSeq = 2,
                            boastReject = false,
                            score = 0,
                            sentPresent = false,
                            canPresent = true,
                            carX = 0,
                            carY = 0,
                            carClass = "S",
                            carNo = 3,
                            gameMode = "001",
                            grade = "001",
                            isDormancy = true,
                            matchRejectFlag = true
                        }
                    ],
                    friendsCount = 1
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if(urlPath == HTTPPath.GetVersusList)
            {
                VersusListDTO res = new VersusListDTO
                {
                    success = true,
                    token = 123456789,
                    matches =
                    [
                        new MatchesDTO
                        {
                            carDestroy = 0,
                            score = 0,
                            versusAccountSeq = 0,
                            chacha = 0,
                            distance = 0,
                            gameMode = "001",
                            gold = 0,
                            matchNo = 0,
                            versusScore = 0,
                            matchResult = "001",
                            updateDate = "1998-09-12 00:00:00",
                            versusCarDestroy = 0,
                            versusChacha = 0,
                            versusDistance = 0,
                            versusGold = 0
                        }
                    ]
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if(urlPath == HTTPPath.GetUserCarList)
            {
                CarListDTO res = new CarListDTO
                {
                    success = true,
                    token = 123456789,
                    cars =
                    [
                        new CarDTO
                        {
                            carAccel = 0,
                            carClass = "S",
                            carSeq = 0,
                            carFuleCost = 0,
                            carSpeed = 0,
                            carNo = 1,
                            isSelected = true
                        }
                    ]
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.CarEventGive)
            {
                GiveEventCarDTO res = new GiveEventCarDTO
                {
                    success = true,
                    token = 123456789,
                    carSeq = 0
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetUserCharacterList)
            {
                UserCharacterListDTO res = new UserCharacterListDTO
                {
                    success = true,
                    token = 123456789,
                    characters =
                    [
                        new CharacterDTO
                        {
                            isSelected = true,
                            characterNo = 1
                        }
                    ]
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetItemList)
            {

                ItemListDTO res = new ItemListDTO
                {
                    success = true,
                    token = 123456789,
                    items =
                    [
                        new ShopItem
                        {
                            itemCode = 1,
                            itemCount = 100
                        }
                    ]
                };
                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetInviteList)
            {
                InviteListDTO res = new InviteListDTO
                {
                    success = true,
                    token = 123456789,
                    invitations = []
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetGiftList)
            {
                GiftListDTO res = new GiftListDTO
                {
                    success = true,
                    token = 123456789,
                    presents =
                    [
                       
                    ]
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.GetGrandPrixInfo)
            {
                GrandPrixInfoDTO res = new GrandPrixInfoDTO
                {
                    success = true,
                    token = 123456789,
                    bestScore = new GrandPrixBestScore
                    {
                        carDestory = 0,
                        carNo = 1,
                        score = 141352,
                        chacha = 100,
                        distance = 0,
                        gold = 10215
                    },
                    status = "002",
                    joinDate = "2024-11-28 23:00:00"
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            else if (urlPath == HTTPPath.StartGame)
            {
                GameStartDTO res = new GameStartDTO
                {
                    success = true,
                    token = 123456789,
                    missions = new int[0],
                    items =
                    [
                        new GameStartItem
                        {
                            itemCode = 1,
                            itemCount = 100,
                            autoUse = true,
                            used = false
                        }
                    ]
                };

                ResponceProcessBinary(ctx, res.ToUTF8ByteArray(aes), HttpStatusCode.OK);
                return;
            }
            GetDecryptedJSON(Client_Req_data);
            ResponceProcessBinary(ctx, Encoding.UTF8.GetBytes("OK"), HttpStatusCode.OK);
        }

    }


}
