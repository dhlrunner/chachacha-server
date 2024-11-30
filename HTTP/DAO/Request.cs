using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aes = System.Security.Cryptography.Aes;

namespace chachacha_server.HTTP.DAO
{
    public class ReqUserInfoReqDTO
    {
        public long accountSeq;
        public long token;
    }
    public class ReqUserInfoGetDTO
    {
        public ReqUserInfoReqDTO? infoReq;
    }
}
