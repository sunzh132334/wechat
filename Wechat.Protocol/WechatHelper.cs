using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MMPro.MM;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using Google.ProtocolBuffers;
using Wechat.Util.Cache;
using CRYPT;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using System.Threading.Tasks;
using MMPro;

namespace Wechat.Protocol
{



    public class WechatHelper
    {
        [DllImport("Common.dll")]
        private static extern int GenerateECKey(int nid, byte[] pub, byte[] pri);

        [DllImport("Common.dll")]
        public static extern int ComputerECCKeyMD5(byte[] pub, int pubLen, byte[] pri, int priLen, byte[] eccKey);

        [DllImport("Common.dll")]
        private static extern uint Adler32(uint adler, byte[] buf, int len);

        //微信版本号
        private int version = 369558056;

        //RSA秘钥版本
        private uint LOGIN_RSA_VER = 174;
        /// <summary>
        /// 系统类型
        /// </summary>
        private string osType = "iMac MacBookPro9,1 OSX OSX 10.11.2 build(15C50)";

        private string phoneOsType = "iPad iPhone OS9.3.3";

        /// <summary>
        /// 获取AesKey
        /// </summary>
        /// <returns></returns>
        private byte[] GetAeskey()
        {
            string aesKeyStr = (new Random()).NextBytes(16).ToString(16, 2);
            return aesKeyStr.ToByteArray(16, 2);
        }
        /// <summary>
        /// 获取设备
        /// </summary>
        /// <returns></returns>
        private byte[] GetDeviceId()
        {
            string deviceIdStr = (new Random()).NextBytes(16).ToString(16, 2);
            return deviceIdStr.ToByteArray(16, 2);
        }

        /// <summary>
        /// 获取登录二维码
        /// </summary>
        /// <returns></returns>
        public GetLoginQRCodeResponse GetLoginQRcode()
        {
            var aesKey = GetAeskey();
            var deviceId = GetDeviceId();
            int mUid = 0;
            string cookie = null;
            GetLoginQRCodeResponse getLoginQRCodeResponse = null;
            GetLoginQRCodeRequest getLoginQRCodeRequest = new GetLoginQRCodeRequest()
            {
                aes = new AesKey()
                {
                    key = aesKey,
                    len = 16
                },
                baseRequest = GetBaseRequest(aesKey, deviceId, 0),
                opcode = 0
            };
            //序列化 protobuf
            var src = Util.Serialize(getLoginQRCodeRequest);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_GETLOGINQRCODE, bufferlen, aesKey, null, 0, null, 7);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_GETLOGINQRCODE);
            // 解包头
            if (RetDate == null) { return new GetLoginQRCodeResponse(); }
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                var RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, aesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, aesKey);
                }
                getLoginQRCodeResponse = Util.Deserialize<GetLoginQRCodeResponse>(RespProtobuf);

                if (getLoginQRCodeResponse != null && !string.IsNullOrEmpty(getLoginQRCodeResponse.uuid))
                {
                    string key = ConstCacheKey.GetUuidKey(getLoginQRCodeResponse.uuid);
                    CustomerInfoCache customerInfoCache = new CustomerInfoCache()
                    {
                        Uuid = getLoginQRCodeResponse.uuid,
                        MUid = mUid,
                        AesKey = getLoginQRCodeResponse.AESKey.key,
                        DeviceId = deviceId,
                        Cookie = cookie,
                    };
                    CacheHelper.CreateInstance().Add(key, customerInfoCache, 600);

                }
            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }
            return getLoginQRCodeResponse;
        }


        /// <summary>
        /// 检查扫码状态
        /// </summary>
        /// <param name="uuid">获取二维码时返回的uuid</param>
        /// <returns></returns>
        public CustomerInfoCache CheckLoginQRCode(string uuid, int count = 1)
        {
            string key = ConstCacheKey.GetUuidKey(uuid);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            var RespProtobuf = new byte[0];
            CheckLoginQRCodeRequest checkLoginQRCodeRequest = new CheckLoginQRCodeRequest()
            {
                aes = new AesKey()
                {
                    key = customerInfoCache.AesKey,
                    len = 16
                },
                baseRequest = GetBaseRequest(customerInfoCache.AesKey, customerInfoCache.DeviceId, 0),
                uuid = uuid,
                timeStamp = (uint)CurrentTime_(),
                opcode = 0
            };
            var src = Util.Serialize(checkLoginQRCodeRequest);

            //src = LongLinkPack(LongLinkCmdId.SEND_CHECKLOGINQRCODE_CMDID, CGI_TYPE.CGI_TYPE_CHECKLOGINQRCODE, src, 1);

            //return m_client.Send(src, src.Length);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_CHECKLOGINQRCODE, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf,
                customerInfoCache.MUid, customerInfoCache.Cookie, 7);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_CHECKLOGINQRCODE);
            if (RetDate == null) { return null; }
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
            }
            else
            {
                throw new Exception("数据包可能有问题,请重新生成二维码登录");
            }

            var result = Util.Deserialize<CheckLoginQRCodeResponse>(RespProtobuf);
            if (result == null)
            {
                return null;
            }
            else if (result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
            {
                var asd = result.data.notifyData.buffer.ToString(16, 2);
                var __ = Util.nouncompress_aes(result.data.notifyData.buffer, customerInfoCache.AesKey);
                if (__ == null)
                {
                    throw new Exception("数据包可能有问题,请重新生成二维码登录");
                }
                var r = Util.Deserialize<MMPro.MM.LoginQRCodeNotify>(__);
                if (r.headImgUrl != null)
                {
                    customerInfoCache.HeadUrl = r.headImgUrl;
                    customerInfoCache.MUid = mUid;
                    customerInfoCache.Cookie = cookie;
                    customerInfoCache.WxId = r.wxid;
                    customerInfoCache.WxNewPass = r.wxnewpass;
                    customerInfoCache.State = r.state;
                    customerInfoCache.Uuid = r.uuid;
                    customerInfoCache.NickName = r.nickName;
                    customerInfoCache.Device = r.device;

                    if (r.state == 2)
                    {
                        //发送登录包
                        checkManualAuth(customerInfoCache, count);
                        //cache.RemoveKeyCache(key);
                        cache.Add(ConstCacheKey.GetWxIdKey(customerInfoCache.WxId), customerInfoCache, DateTime.Now.AddYears(1), TimeSpan.Zero);

                    }
                }
            }
            else
            {
                customerInfoCache.State = -1;
            }
            return customerInfoCache;
        }

        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="wxId"></param>
        public InitResponse Init(string wxId)
        {
            InitResponse list = new InitResponse();
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int ret = 1;
            mm.command.NewInitResponse newInit = null;
            while (ret == 1)
            {
                newInit = NewInit(customerInfoCache);
                ret = newInit.ContinueFlag;

                foreach (var r in newInit.CmdListList)
                {
                    switch ((SyncCmdID)r.CmdId)
                    {
                        case SyncCmdID.CmdInvalid:
                            break;
                        case SyncCmdID.CmdIdModUserInfo:
                            var modUserInfo = Util.Deserialize<MM.ModUserInfo>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModUserInfos.Add(modUserInfo);
                            break;
                        case SyncCmdID.CmdIdModContact:
                            var modContact = Util.Deserialize<MM.ModContact>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModContacts.Add(modContact);
                            break;
                        case SyncCmdID.CmdIdDelContact:
                            var delContact = Util.Deserialize<MM.DelContact>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelContacts.Add(delContact);
                            break;
                        case SyncCmdID.CmdIdAddMsg:
                            var addMsg = Util.Deserialize<MM.AddMsg>(r.CmdBuf.Buffer.ToByteArray());
                            list.AddMsgs.Add(addMsg);
                            break;
                        case SyncCmdID.CmdIdModMsgStatus:
                            var mdMsgStatus = Util.Deserialize<MM.ModMsgStatus>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModMsgStatuss.Add(mdMsgStatus);
                            break;
                        case SyncCmdID.CmdIdDelChatContact:
                            var delChatContact = Util.Deserialize<MM.DelChatContact>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelChatContacts.Add(delChatContact);
                            break;
                        case SyncCmdID.CmdIdDelContactMsg:
                            var delContactMsg = Util.Deserialize<MM.DelContactMsg>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelContactMsgs.Add(delContactMsg);
                            break;
                        case SyncCmdID.CmdIdDelMsg:
                            var dlMsg = Util.Deserialize<MM.DelMsg>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelMsgs.Add(dlMsg);
                            break;
                        case SyncCmdID.CmdIdReport:
                            //Util.Deserialize<MM.Report>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdOpenQQMicroBlog:
                            var openQQMicroBlog = Util.Deserialize<MM.OpenQQMicroBlog>(r.CmdBuf.Buffer.ToByteArray());
                            list.OpenQQMicroBlogs.Add(openQQMicroBlog);
                            break;
                        case SyncCmdID.CmdIdCloseMicroBlog:
                            var closeMicroBlog = Util.Deserialize<MM.CloseMicroBlog>(r.CmdBuf.Buffer.ToByteArray());
                            list.CloseMicroBlogs.Add(closeMicroBlog);
                            break;
                        case SyncCmdID.CmdIdModMicroBlog:
                            //Util.Deserialize<MM.ModMicroBlog>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdModNotifyStatus:
                            var modNotifyStatus = Util.Deserialize<MM.ModNotifyStatus>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModNotifyStatuss.Add(modNotifyStatus);
                            break;
                        case SyncCmdID.CmdIdModChatRoomMember:
                            var modChatRoomMember = Util.Deserialize<MM.ModChatRoomMember>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModChatRoomMembers.Add(modChatRoomMember);
                            break;
                        case SyncCmdID.CmdIdQuitChatRoom:
                            var quitChatRoom = Util.Deserialize<MM.QuitChatRoom>(r.CmdBuf.Buffer.ToByteArray());
                            list.QuitChatRooms.Add(quitChatRoom);
                            break;
                        case SyncCmdID.CmdIdModContactDomainEmail:

                            //Util.Deserialize<MM.ModContactDomainEmail>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdModUserDomainEmail:
                            var modUserDomainEmail = Util.Deserialize<MM.ModUserDomainEmail>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModUserDomainEmails.Add(modUserDomainEmail);
                            break;
                        case SyncCmdID.CmdIdDelUserDomainEmail:
                            var delUserDomainEmail = Util.Deserialize<MM.DelUserDomainEmail>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelUserDomainEmails.Add(delUserDomainEmail);
                            break;
                        case SyncCmdID.CmdIdModChatRoomNotify:
                            var modChatRoomNotify = Util.Deserialize<MM.ModChatRoomNotify>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModChatRoomNotifys.Add(modChatRoomNotify);
                            break; ;
                        case SyncCmdID.CmdIdPossibleFriend:
                            var possibleFriend = Util.Deserialize<MM.PossibleFriend>(r.CmdBuf.Buffer.ToByteArray());
                            list.PossibleFriends.Add(possibleFriend);
                            break;
                        case SyncCmdID.CmdIdInviteFriendOpen:
                            var inviteFriendOpen = Util.Deserialize<MM.InviteFriendOpen>(r.CmdBuf.Buffer.ToByteArray());
                            list.InviteFriendOpens.Add(inviteFriendOpen);
                            break;
                        case SyncCmdID.CmdIdFunctionSwitch:
                            var functionSwitch = Util.Deserialize<MM.FunctionSwitch>(r.CmdBuf.Buffer.ToByteArray());
                            list.FunctionSwitchs.Add(functionSwitch);
                            break;
                        case SyncCmdID.CmdIdModQContact:
                            //Util.Deserialize<MM.ModQContact>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdModTContact:
                            //Util.Deserialize<MM.ModTContact>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdPsmStat:
                            var pSMStat = Util.Deserialize<MM.PSMStat>(r.CmdBuf.Buffer.ToByteArray());
                            list.PSMStats.Add(pSMStat);
                            break;
                        case SyncCmdID.CmdIdModChatRoomTopic:
                            var modChatRoomTopic = Util.Deserialize<MM.ModChatRoomTopic>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModChatRoomTopics.Add(modChatRoomTopic);
                            break;
                        case SyncCmdID.MM_SYNCCMD_UPDATESTAT:

                            //var updateStatOpLog = Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODDISTURBSETTING:

                            var modDisturbSetting = Util.Deserialize<micromsg.ModDisturbSetting>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModDisturbSettings.Add(modDisturbSetting);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELETEBOTTLE:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODBOTTLECONTACT:

                            var modBottleContact = Util.Deserialize<micromsg.ModBottleContact>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModBottleContacts.Add(modBottleContact);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELBOTTLECONTACT:

                            var delBottleContact = Util.Deserialize<micromsg.DelBottleContact>(r.CmdBuf.Buffer.ToByteArray());
                            list.DelBottleContacts.Add(delBottleContact);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODUSERIMG:

                            var modUserImg = Util.Deserialize<micromsg.ModUserImg>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModUserImgs.Add(modUserImg);
                            break;
                        case SyncCmdID.MM_SYNCCMD_KVSTAT:

                            var kVStatItem = Util.Deserialize<micromsg.KVStatItem>(r.CmdBuf.Buffer.ToByteArray());
                            list.KVStatItems.Add(kVStatItem);
                            break;
                        case SyncCmdID.NN_SYNCCMD_THEMESTAT:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SYNCCMD_USERINFOEXT:

                            var userInfoExt = Util.Deserialize<micromsg.UserInfoExt>(r.CmdBuf.Buffer.ToByteArray());
                            list.UserInfoExts.Add(userInfoExt);
                            break;
                        case SyncCmdID.MM_SNS_SYNCCMD_OBJECT:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SNS_SYNCCMD_ACTION:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SYNCCMD_BRAND_SETTING:

                            var modBrandSetting = Util.Deserialize<micromsg.ModBrandSetting>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModBrandSettings.Add(modBrandSetting);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODCHATROOMMEMBERDISPLAYNAME:

                            var modChatRoomMemberDisplayName = Util.Deserialize<micromsg.ModChatRoomMemberDisplayName>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModChatRoomMemberDisplayNames.Add(modChatRoomMemberDisplayName);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODCHATROOMMEMBERFLAG:

                            var modChatRoomMemberFlag = Util.Deserialize<micromsg.ModChatRoomMemberFlag>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModChatRoomMemberFlags.Add(modChatRoomMemberFlag);
                            break;
                        case SyncCmdID.MM_SYNCCMD_WEBWXFUNCTIONSWITCH:

                            var webWxFunctionSwitch = Util.Deserialize<micromsg.WebWxFunctionSwitch>(r.CmdBuf.Buffer.ToByteArray());
                            list.WebWxFunctionSwitchs.Add(webWxFunctionSwitch);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODSNSUSERINFO:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODSNSBLACKLIST:
                            var modSnsBlackList = Util.Deserialize<micromsg.ModSnsBlackList>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModSnsBlackLists.Add(modSnsBlackList);
                            break;
                        case SyncCmdID.MM_SYNCCMD_NEWDELMSG:

                            var newDelMsg = Util.Deserialize<micromsg.NewDelMsg>(r.CmdBuf.Buffer.ToByteArray());
                            list.NewDelMsgs.Add(newDelMsg);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODDESCRIPTION:

                            var modDescription = Util.Deserialize<micromsg.ModDescription>(r.CmdBuf.Buffer.ToByteArray());
                            list.ModDescriptions.Add(modDescription);
                            break;
                        case SyncCmdID.MM_SYNCCMD_KVCMD:

                            var kVCmd = Util.Deserialize<micromsg.KVCmd>(r.CmdBuf.Buffer.ToByteArray());
                            list.KVCmds.Add(kVCmd);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELETE_SNS_OLDGROUP:

                            var deleteSnsOldGroup = Util.Deserialize<micromsg.DeleteSnsOldGroup>(r.CmdBuf.Buffer.ToByteArray());
                            list.DeleteSnsOldGroups.Add(deleteSnsOldGroup);
                            break;
                        case SyncCmdID.MM_FAV_SYNCCMD_ADDITEM:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;
                        case SyncCmdID.CmdIdMax:

                            //Util.Deserialize<micromsg>(r.CmdBuf.Buffer.ToByteArray());
                            break;


                    }



                }
            }
            customerInfoCache.Sync = newInit.CurrentSynckey.ToByteArray();
            cache.Add(key, customerInfoCache);
            return list;

        }

        /// <summary>
        /// 同步消息
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        public InitResponse SyncInit(string wxId)
        {
            InitResponse list = new InitResponse();
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            if (customerInfoCache.Sync == null)
            {
                Init(wxId);
                customerInfoCache = cache.Get<CustomerInfoCache>(key);
                if (customerInfoCache == null)
                {
                    throw new ExpiredException("缓存失效，请重新生成二维码登录");
                }
            }
            int ret = 1;
            //var NewInit = new mm.command.NewInitResponse();
            while (ret == 1)
            {
                var NewSync = NewSyncEcode(customerInfoCache, 4);
                if (NewSync.cmdList == null) { break; }
                if (NewSync.cmdList.count <= 0) { break; }
                ret = NewSync.Continueflag;
                foreach (var r in NewSync.cmdList.list)
                {
                    switch ((SyncCmdID)r.cmdid)
                    {
                        case SyncCmdID.CmdInvalid:
                            break;
                        case SyncCmdID.CmdIdModUserInfo:
                            var modUserInfo = Util.Deserialize<MM.ModUserInfo>(r.cmdBuf.data);
                            list.ModUserInfos.Add(modUserInfo);
                            break;
                        case SyncCmdID.CmdIdModContact:
                            var modContact = Util.Deserialize<MM.ModContact>(r.cmdBuf.data);
                            list.ModContacts.Add(modContact);
                            break;
                        case SyncCmdID.CmdIdDelContact:
                            var delContact = Util.Deserialize<MM.DelContact>(r.cmdBuf.data);
                            list.DelContacts.Add(delContact);
                            break;
                        case SyncCmdID.CmdIdAddMsg:
                            var addMsg = Util.Deserialize<MM.AddMsg>(r.cmdBuf.data);
                            list.AddMsgs.Add(addMsg);
                            break;
                        case SyncCmdID.CmdIdModMsgStatus:
                            var mdMsgStatus = Util.Deserialize<MM.ModMsgStatus>(r.cmdBuf.data);
                            list.ModMsgStatuss.Add(mdMsgStatus);
                            break;
                        case SyncCmdID.CmdIdDelChatContact:
                            var delChatContact = Util.Deserialize<MM.DelChatContact>(r.cmdBuf.data);
                            list.DelChatContacts.Add(delChatContact);
                            break;
                        case SyncCmdID.CmdIdDelContactMsg:
                            var delContactMsg = Util.Deserialize<MM.DelContactMsg>(r.cmdBuf.data);
                            list.DelContactMsgs.Add(delContactMsg);
                            break;
                        case SyncCmdID.CmdIdDelMsg:
                            var dlMsg = Util.Deserialize<MM.DelMsg>(r.cmdBuf.data);
                            list.DelMsgs.Add(dlMsg);
                            break;
                        case SyncCmdID.CmdIdReport:
                            //Util.Deserialize<MM.Report>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdOpenQQMicroBlog:
                            var openQQMicroBlog = Util.Deserialize<MM.OpenQQMicroBlog>(r.cmdBuf.data);
                            list.OpenQQMicroBlogs.Add(openQQMicroBlog);
                            break;
                        case SyncCmdID.CmdIdCloseMicroBlog:
                            var closeMicroBlog = Util.Deserialize<MM.CloseMicroBlog>(r.cmdBuf.data);
                            list.CloseMicroBlogs.Add(closeMicroBlog);
                            break;
                        case SyncCmdID.CmdIdModMicroBlog:
                            //Util.Deserialize<MM.ModMicroBlog>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdModNotifyStatus:
                            var modNotifyStatus = Util.Deserialize<MM.ModNotifyStatus>(r.cmdBuf.data);
                            list.ModNotifyStatuss.Add(modNotifyStatus);
                            break;
                        case SyncCmdID.CmdIdModChatRoomMember:
                            var modChatRoomMember = Util.Deserialize<MM.ModChatRoomMember>(r.cmdBuf.data);
                            list.ModChatRoomMembers.Add(modChatRoomMember);
                            break;
                        case SyncCmdID.CmdIdQuitChatRoom:
                            var quitChatRoom = Util.Deserialize<MM.QuitChatRoom>(r.cmdBuf.data);
                            list.QuitChatRooms.Add(quitChatRoom);
                            break;
                        case SyncCmdID.CmdIdModContactDomainEmail:

                            //Util.Deserialize<MM.ModContactDomainEmail>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdModUserDomainEmail:
                            var modUserDomainEmail = Util.Deserialize<MM.ModUserDomainEmail>(r.cmdBuf.data);
                            list.ModUserDomainEmails.Add(modUserDomainEmail);
                            break;
                        case SyncCmdID.CmdIdDelUserDomainEmail:
                            var delUserDomainEmail = Util.Deserialize<MM.DelUserDomainEmail>(r.cmdBuf.data);
                            list.DelUserDomainEmails.Add(delUserDomainEmail);
                            break;
                        case SyncCmdID.CmdIdModChatRoomNotify:
                            var modChatRoomNotify = Util.Deserialize<MM.ModChatRoomNotify>(r.cmdBuf.data);
                            list.ModChatRoomNotifys.Add(modChatRoomNotify);
                            break; ;
                        case SyncCmdID.CmdIdPossibleFriend:
                            var possibleFriend = Util.Deserialize<MM.PossibleFriend>(r.cmdBuf.data);
                            list.PossibleFriends.Add(possibleFriend);
                            break;
                        case SyncCmdID.CmdIdInviteFriendOpen:
                            var inviteFriendOpen = Util.Deserialize<MM.InviteFriendOpen>(r.cmdBuf.data);
                            list.InviteFriendOpens.Add(inviteFriendOpen);
                            break;
                        case SyncCmdID.CmdIdFunctionSwitch:
                            var functionSwitch = Util.Deserialize<MM.FunctionSwitch>(r.cmdBuf.data);
                            list.FunctionSwitchs.Add(functionSwitch);
                            break;
                        case SyncCmdID.CmdIdModQContact:
                            //Util.Deserialize<MM.ModQContact>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdModTContact:
                            //Util.Deserialize<MM.ModTContact>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdPsmStat:
                            var pSMStat = Util.Deserialize<MM.PSMStat>(r.cmdBuf.data);
                            list.PSMStats.Add(pSMStat);
                            break;
                        case SyncCmdID.CmdIdModChatRoomTopic:
                            var modChatRoomTopic = Util.Deserialize<MM.ModChatRoomTopic>(r.cmdBuf.data);
                            list.ModChatRoomTopics.Add(modChatRoomTopic);
                            break;
                        case SyncCmdID.MM_SYNCCMD_UPDATESTAT:

                            //var updateStatOpLog = Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODDISTURBSETTING:

                            var modDisturbSetting = Util.Deserialize<micromsg.ModDisturbSetting>(r.cmdBuf.data);
                            list.ModDisturbSettings.Add(modDisturbSetting);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELETEBOTTLE:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODBOTTLECONTACT:

                            var modBottleContact = Util.Deserialize<micromsg.ModBottleContact>(r.cmdBuf.data);
                            list.ModBottleContacts.Add(modBottleContact);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELBOTTLECONTACT:

                            var delBottleContact = Util.Deserialize<micromsg.DelBottleContact>(r.cmdBuf.data);
                            list.DelBottleContacts.Add(delBottleContact);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODUSERIMG:

                            var modUserImg = Util.Deserialize<micromsg.ModUserImg>(r.cmdBuf.data);
                            list.ModUserImgs.Add(modUserImg);
                            break;
                        case SyncCmdID.MM_SYNCCMD_KVSTAT:

                            var kVStatItem = Util.Deserialize<micromsg.KVStatItem>(r.cmdBuf.data);
                            list.KVStatItems.Add(kVStatItem);
                            break;
                        case SyncCmdID.NN_SYNCCMD_THEMESTAT:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SYNCCMD_USERINFOEXT:

                            var userInfoExt = Util.Deserialize<micromsg.UserInfoExt>(r.cmdBuf.data);
                            list.UserInfoExts.Add(userInfoExt);
                            break;
                        case SyncCmdID.MM_SNS_SYNCCMD_OBJECT:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SNS_SYNCCMD_ACTION:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SYNCCMD_BRAND_SETTING:

                            var modBrandSetting = Util.Deserialize<micromsg.ModBrandSetting>(r.cmdBuf.data);
                            list.ModBrandSettings.Add(modBrandSetting);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODCHATROOMMEMBERDISPLAYNAME:

                            var modChatRoomMemberDisplayName = Util.Deserialize<micromsg.ModChatRoomMemberDisplayName>(r.cmdBuf.data);
                            list.ModChatRoomMemberDisplayNames.Add(modChatRoomMemberDisplayName);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODCHATROOMMEMBERFLAG:

                            var modChatRoomMemberFlag = Util.Deserialize<micromsg.ModChatRoomMemberFlag>(r.cmdBuf.data);
                            list.ModChatRoomMemberFlags.Add(modChatRoomMemberFlag);
                            break;
                        case SyncCmdID.MM_SYNCCMD_WEBWXFUNCTIONSWITCH:

                            var webWxFunctionSwitch = Util.Deserialize<micromsg.WebWxFunctionSwitch>(r.cmdBuf.data);
                            list.WebWxFunctionSwitchs.Add(webWxFunctionSwitch);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODSNSUSERINFO:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODSNSBLACKLIST:
                            var modSnsBlackList = Util.Deserialize<micromsg.ModSnsBlackList>(r.cmdBuf.data);
                            list.ModSnsBlackLists.Add(modSnsBlackList);
                            break;
                        case SyncCmdID.MM_SYNCCMD_NEWDELMSG:

                            var newDelMsg = Util.Deserialize<micromsg.NewDelMsg>(r.cmdBuf.data);
                            list.NewDelMsgs.Add(newDelMsg);
                            break;
                        case SyncCmdID.MM_SYNCCMD_MODDESCRIPTION:

                            var modDescription = Util.Deserialize<micromsg.ModDescription>(r.cmdBuf.data);
                            list.ModDescriptions.Add(modDescription);
                            break;
                        case SyncCmdID.MM_SYNCCMD_KVCMD:

                            var kVCmd = Util.Deserialize<micromsg.KVCmd>(r.cmdBuf.data);
                            list.KVCmds.Add(kVCmd);
                            break;
                        case SyncCmdID.MM_SYNCCMD_DELETE_SNS_OLDGROUP:

                            var deleteSnsOldGroup = Util.Deserialize<micromsg.DeleteSnsOldGroup>(r.cmdBuf.data);
                            list.DeleteSnsOldGroups.Add(deleteSnsOldGroup);
                            break;
                        case SyncCmdID.MM_FAV_SYNCCMD_ADDITEM:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;
                        case SyncCmdID.CmdIdMax:

                            //Util.Deserialize<micromsg>(r.cmdBuf.data);
                            break;


                    }



                }

                customerInfoCache.Sync = NewSync.sync_key;
            }
            cache.Add(key, customerInfoCache);
            return list;
        }
        /// <summary>
        /// 发送登陆包
        /// </summary>
        /// <param name="customerInfoCache"></param>
        /// <param name="count"></param>
        private void checkManualAuth(CustomerInfoCache customerInfoCache, int count = 1)
        {

            var manualAuth = ManualAuth(customerInfoCache);
            //-301重定向         
            if (manualAuth.baseResponse.ret == MMPro.MM.RetConst.MM_ERR_IDC_REDIRECT)
            {
                //Console.WriteLine(ManualAuth.dnsInfo.builtinIplist.shortConnectIplist[0].ip);
                //byte[] s = Util.Serialize<MM.BuiltinIP>(ManualAuth.dnsInfo.builtinIplist.shortConnectIplist.shortConnectIplist[1]);
                int len = (int)manualAuth.dnsInfo.builtinIplist.shortconnectIpcount;
                Util.shortUrl = "http://" + manualAuth.dnsInfo.newHostList.list[1].substitute;
                if (count > 5)
                {
                    customerInfoCache.State = -1;
                    return;
                }
                count++;
                CheckLoginQRCode(customerInfoCache.Uuid, count);

            }
            else if (manualAuth.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
            {
                customerInfoCache.WxId = manualAuth.accountInfo.wxid;

                byte[] strECServrPubKey = manualAuth.authParam.ecdh.ecdhkey.key;
                byte[] aesKey = new byte[16];
                ComputerECCKeyMD5(strECServrPubKey, 57, customerInfoCache.PriKeyBuf, 328, aesKey);
                //var aesKey = OpenSSLNativeClass.ECDH.DoEcdh(ManualAuth.authParam.ecdh.nid, strECServrPubKey, wechat.pri_key_buf);
                //wechat.CheckEcdh = aesKey.ToString(16, 2);
                customerInfoCache.AesKey = AES.AESDecrypt(manualAuth.authParam.session.key, aesKey).ToString(16, 2).ToByteArray(16, 2);

                var baseRequest = GetBaseRequest(GetDeviceId(), customerInfoCache.AesKey, (uint)customerInfoCache.MUid, phoneOsType);
                customerInfoCache.BaseRequest = new CustomerInfoCache.BaseRequestCache()
                {
                    sessionKey = baseRequest.sessionKey,
                    uin = baseRequest.uin,
                    devicelId = baseRequest.devicelId,
                    clientVersion = baseRequest.clientVersion,
                    osType = baseRequest.osType,
                    scene = baseRequest.scene
                };

                customerInfoCache.AuthKey = manualAuth.authParam.autoAuthKey.buffer;

            }
            else if ((int)manualAuth.baseResponse.ret == 2)
            {
                customerInfoCache.WxId = manualAuth.accountInfo.wxid;

            }

        }

        /// <summary>
        /// 精准获取通讯录  
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public InitContactResponse InitContact(string wxId, int currentWxcontactSeq = 0)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            InitContactRequest initContact_ = new InitContactRequest()
            {
                currentChatRoomContactSeq = 0,
                currentWxcontactSeq = currentWxcontactSeq,
                username = wxId,
            };
            int mUid = 0;
            string cookie = null;
            var src = Util.Serialize(initContact_);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_INITCONTACT, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_INITCONTACT);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var InitContactResponse_ = Util.Deserialize<InitContactResponse>(RespProtobuf);
            return InitContactResponse_;
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="ChatRoom"></param>
        /// <returns></returns>
        public micromsg.GetContactResponse GetContactDetail(string wxId, string searchWxId, string ChatRoom = "")
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];

            micromsg.SKBuiltinString_t UserName_ = new micromsg.SKBuiltinString_t();
            UserName_.String = searchWxId;

            micromsg.GetContactRequest getContact_a = new micromsg.GetContactRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
            };
            getContact_a.UserCount = 1;
            getContact_a.UserNameList.Add(UserName_);

            if (ChatRoom != "")
            {
                micromsg.SKBuiltinString_t ChatRoom_ = new micromsg.SKBuiltinString_t();
                ChatRoom_.String = ChatRoom;

                getContact_a.FromChatRoomCount = 1;
                getContact_a.FromChatRoom.Add(ChatRoom_);
            }

            var src = Util.Serialize(getContact_a);
            int mUid = 0;
            string cookie = null;

            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_GETCONTACT, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_GETCONTACT);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var GetContactResponse_ = Util.Deserialize<micromsg.GetContactResponse>(RespProtobuf);
            return GetContactResponse_;
        }


        /// <summary>
        /// 附近的人
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="latitude"></param>
        /// <param name="logitude"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public LbsResponse LbsLBSFind(string wxId, float latitude, float logitude, int type = 1)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            LbsRequest lbs = new LbsRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                gPSSource = 0,
                latitude = latitude,
                logitude = logitude,
                opCode = (uint)type,
                precision = 65,
            };

            var src = Util.Serialize(lbs);
            int mUid = 0;
            string cookie = null;

            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_LBSFIND, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);

            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_LBSFIND);
            if (RetDate.Length > 32)
            {

                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);


                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var LbsResponse_ = Util.Deserialize<LbsResponse>(RespProtobuf);
            return LbsResponse_;
        }


        /// <summary>
        /// 搜索联系人
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public SearchContactResponse SearchContact(string wxId, string userName)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            SKBuiltinString userName_ = new SKBuiltinString();
            userName_.@string = userName;
            SearchContactRequest searchContact = new SearchContactRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                searchScene = (uint)1,
                opCode = (uint)0,
                fromScene = (uint)1,
                userName = userName_,


            };
            var src = Util.Serialize(searchContact);
            int bufferlen = src.Length;
            int mUid = 0;
            string cookie = null;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_SEARCHCONTACT, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_SEARCHCONTACT);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var SearchContactResponse_ = Util.Deserialize<SearchContactResponse>(RespProtobuf);
            return SearchContactResponse_;
        }


        /// <summary>
        /// 授权连接
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="url"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public micromsg.GetA8KeyResp GetA8Key(string wxId, string userName, string url, int opcode = 2)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            SKBuiltinString requrl_ = new SKBuiltinString();
            requrl_.@string = url;
            GetA8KeyRequest getA8Key_ = new GetA8KeyRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                codeType = 0,
                codeVersion = 0,
                flag = 0,
                fontScale = (uint)100,
                netType = "WIFI",
                opCode = (uint)opcode,
                userName = userName,
                reqUrl = requrl_,
                friendQQ = 0,

            };

            var src = Util.Serialize(getA8Key_);
            int bufferlen = src.Length;
            //组包

            int mUid = 0;
            string cookie = null;
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_GETA8KEY, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/geta8key");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var GetA8KeyResponse_ = Util.Deserialize<micromsg.GetA8KeyResp>(RespProtobuf);
            return GetA8KeyResponse_;
        }
        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="LabelName">标签名</param>
        /// <returns></returns>
        public micromsg.AddContactLabelResponse AddContactLabel(string wxId, string LabelName)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            micromsg.AddContactLabelRequest addContactLabel_ = new micromsg.AddContactLabelRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
            };

            micromsg.LabelPair label = new micromsg.LabelPair()
            {
                LabelID = 0,
                LabelName = LabelName
            };

            addContactLabel_.LabelPairList.Add(label);
            addContactLabel_.LabelCount = 1;

            var src = Util.Serialize(addContactLabel_);

            byte[] RespProtobuf = new byte[0];

            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 635, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/addcontactlabel");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var AddContactLabelResponse_ = Util.Deserialize<micromsg.AddContactLabelResponse>(RespProtobuf);
            return AddContactLabelResponse_;
        }



        /// <summary>
        /// 修改标签列表
        /// </summary>
        /// <param name="UserLabelInfo"></param>
        /// <returns></returns>
        public micromsg.ModifyContactLabelListResponse ModifyContactLabelList(string wxId, micromsg.UserLabelInfo[] UserLabelInfo)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            micromsg.ModifyContactLabelListRequest ModifyContactLabelList_ = new micromsg.ModifyContactLabelListRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
            };
            foreach (var id in UserLabelInfo)
            {
                ModifyContactLabelList_.UserLabelInfoList.Add(id);

            }
            ModifyContactLabelList_.UserCount = (uint)UserLabelInfo.Length;

            var src = Util.Serialize(ModifyContactLabelList_);

            byte[] RespProtobuf = new byte[0];
            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 638, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/modifycontactlabellist");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var ModifyContactLabelListResponse_ = Util.Deserialize<micromsg.ModifyContactLabelListResponse>(RespProtobuf);
            return ModifyContactLabelListResponse_;


        }


        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="LabelIDList_">欲删除的标签id</param>
        /// <returns></returns>
        public micromsg.DelContactLabelResponse DelContactLabel(string wxId, string LabelIDList_)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            micromsg.DelContactLabelRequest delContactLabel = new micromsg.DelContactLabelRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
                LabelIDList = LabelIDList_
            };

            var src = Util.Serialize(delContactLabel);

            byte[] RespProtobuf = new byte[0];

            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 636, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/delcontactlabel");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");

            }

            var DelContactLabelResponse_ = Util.Deserialize<micromsg.DelContactLabelResponse>(RespProtobuf);
            return DelContactLabelResponse_;

        }


        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        public GetContactLabelListResponse GetContactLabelList(string wxId)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            GetContactLabelListRequest getContactLabelList_ = new GetContactLabelListRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
            };

            var src = Util.Serialize(getContactLabelList_);
            int bufferlen = src.Length;
            int mUid = 0;
            string cookie = null;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_GETCONTACTLABELLIST, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_GETCONTACTLABELLIST);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var GetContactLabelListResponse_ = Util.Deserialize<GetContactLabelListResponse>(RespProtobuf);
            return GetContactLabelListResponse_;
        }

        /// <summary>
        /// 绑定邮箱
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="Email"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public micromsg.BindEmailResponse BindEmail(string wxId, string Email, int opcode = 1)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            micromsg.BindEmailRequest bindEmail_ = new micromsg.BindEmailRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
                Email = Email,
                OpCode = (uint)opcode,
            };

            var src = Util.Serialize(bindEmail_);

            byte[] RespProtobuf = new byte[0];


            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 256, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            int mUid = 0;
            string cookie = null;
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/bindemail");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var BindEmailResponse_ = Util.Deserialize<micromsg.BindEmailResponse>(RespProtobuf);
            return BindEmailResponse_;
        }
        /// <summary>
        ///摇一摇
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        /// <returns></returns>
        public micromsg.ShakeGetResponse ShakeReport(string wxId, float Latitude, float Longitude)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];

            //mm.command.ShakereportRequest shakeReport_ = GoogleProto.shakereport(Latitude, Longitude, "49aa7db2f4a3ffe0e96218f6b92cde32", Encoding.Default.GetString(GetAESkey()), (uint)m_uid, "iPad iPhone OS9.3.3");
            micromsg.ShakeReportRequest shakeReport_ = new micromsg.ShakeReportRequest()
            {

                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
                GPSSource = 0,
                ImgId = 0,
                Latitude = Latitude,
                Longitude = Longitude,
                OpCode = 0,
                Precision = 0,
                Times = 1,
            };

            var src = Util.Serialize(shakeReport_);

            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 161, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/shakereport");



            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var ShakeReportResponse_ = Util.Deserialize<micromsg.ShakeReportResponse>(RespProtobuf);

            if (ShakeReportResponse_.BaseResponse.Ret == 0 && ShakeReportResponse_.Buffer.Buffer != null)
            {

                var ShakeGetResponse_ = this.ShakeGet(customerInfoCache, ShakeReportResponse_.Buffer);
                return ShakeGetResponse_;
            }

            return null;
        }
        /// <summary>
        /// 获取指定人的朋友圈
        /// </summary>
        /// <param name="fristPageMd5">/首页为空 第二页请附带md5</param>
        /// <param name="Username">要访问人的wxid</param>
        /// <param name="maxid">首页为0 次页朋友圈数据id 的最小值</param>
        /// <returns></returns>
        public SnsUserPageResponse SnsUserPage(string fristPageMd5, string wxId, string toWxId, int maxid = 0)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            byte[] RespProtobuf = new byte[0];
            SnsUserPageRequest snsUserPage = new SnsUserPageRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                fristPageMd5 = fristPageMd5,
                username = toWxId,
                maxid = (ulong)maxid,
                source = 0,
                minFilterId = 0,
                lastRequestTime = 0

            };


            var src = Util.Serialize(snsUserPage);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_MMSNSUSERPAGE, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_MMSNSUSERPAGE);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }

            var SnsUserPageResponse_ = Util.Deserialize<SnsUserPageResponse>(RespProtobuf);
            return SnsUserPageResponse_;
        }



        /// <summary>
        /// 取朋友圈首页
        /// </summary>
        /// <param name="fristPageMd5">/首页为空 第二页请附带md5</param>
        /// <param name="maxid">首页为0 次页朋友圈数据id 的最小值</param>
        /// <returns></returns>
        public SnsTimeLineResponse SnsTimeLine(string wxId, string fristPageMd5 = "", int maxid = 0)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            byte[] RespProtobuf = new byte[0];
            SnsTimeLineRequest snsTimeLine = new SnsTimeLineRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                clientLastestId = 0,
                firstPageMd5 = fristPageMd5,
                lastRequestTime = 0,
                maxId = (ulong)maxid,
                minFilterId = 0,
                networkType = 1,

            };


            var src = Util.Serialize(snsTimeLine);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_MMSNSTIMELINE, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_MMSNSTIMELINE);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }

            var SnsTimeLineResponse_ = Util.Deserialize<SnsTimeLineResponse>(RespProtobuf);
            return SnsTimeLineResponse_;
        }


        /// <summary>
        /// 操作朋友圈
        /// </summary>
        /// <param name="id">要操作的id</param>
        /// <param name="type">//1删除朋友圈2设为隐私3设为公开4删除评论5取消点赞</param>
        /// <returns></returns>
        public SnsObjectOpResponse GetSnsObjectOp(ulong id, string wxId, SnsObjectOpType type)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            byte[] RespProtobuf = new byte[0];
            SnsObjectOp snsObject = new SnsObjectOp()
            {
                id = id,
                opType = type
            };

            SnsObjectOpRequest snsObjectOp_ = new SnsObjectOpRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                opCount = 1,
                opList = snsObject
            };

            var src = Util.Serialize(snsObjectOp_);

            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_SNSOBJECTOP, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_MMSNSOBJECTOP);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }
            var SnsObjectOpResponse_ = Util.Deserialize<SnsObjectOpResponse>(RespProtobuf);
            return SnsObjectOpResponse_;
        }


        /// <summary>
        /// 发送盆友圈
        /// </summary>
        /// <param name="content">欲发送内容 使用朋友圈结构发送</param>
        /// <returns></returns>
        public SnsPostResponse SnsPost(string wxId, string content, IList<string> BlackList, IList<string> WithUserList)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            var RespProtobuf = new byte[0];
            SnsPostRequest SnsPostReq = Util.Deserialize<SnsPostRequest>("0A570A105D64797E40587E3653492B3770767C6D10E5DA8D81031A20353332363435314632303045304431333043453441453237323632423631363920A28498B0012A1369506164206950686F6E65204F53392E332E33300012810808FB0712FB073C54696D656C696E654F626A6563743E3C69643E31323534323132393139333538343234323934373C2F69643E3C757365726E616D653E777869645F6B727862626D68316A75646533313C2F757365726E616D653E3C63726561746554696D653E313439353133383331303C2F63726561746554696D653E3C636F6E74656E74446573633EE2809CE7BEA1E68595E982A3E4BA9BE4B880E6B2BEE79D80E69E95E5A4B4E5B0B1E883BDE5AE89E79DA1E79A84E4BABAE5928CE982A3E4BA9BE586B3E5BF83E694BEE6898BE4B98BE5908EE5B0B1E4B88DE5868DE59B9EE5A4B4E79A84E4BABAE2809D3C2F636F6E74656E74446573633E3C636F6E74656E744465736353686F77547970653E303C2F636F6E74656E744465736353686F77547970653E3C636F6E74656E74446573635363656E653E333C2F636F6E74656E74446573635363656E653E3C707269766174653E303C2F707269766174653E3C7369676874466F6C6465643E303C2F7369676874466F6C6465643E3C617070496E666F3E3C69643E3C2F69643E3C76657273696F6E3E3C2F76657273696F6E3E3C6170704E616D653E3C2F6170704E616D653E3C696E7374616C6C55726C3E3C2F696E7374616C6C55726C3E3C66726F6D55726C3E3C2F66726F6D55726C3E3C6973466F7263655570646174653E303C2F6973466F7263655570646174653E3C2F617070496E666F3E3C736F75726365557365724E616D653E3C2F736F75726365557365724E616D653E3C736F757263654E69636B4E616D653E3C2F736F757263654E69636B4E616D653E3C73746174697374696373446174613E3C2F73746174697374696373446174613E3C737461744578745374723E3C2F737461744578745374723E3C436F6E74656E744F626A6563743E3C636F6E74656E745374796C653E323C2F636F6E74656E745374796C653E3C7469746C653E3C2F7469746C653E3C6465736372697074696F6E3E3C2F6465736372697074696F6E3E3C6D656469614C6973743E3C2F6D656469614C6973743E3C636F6E74656E7455726C3E3C2F636F6E74656E7455726C3E3C2F436F6E74656E744F626A6563743E3C616374696F6E496E666F3E3C6170704D73673E3C6D657373616765416374696F6E3E3C2F6D657373616765416374696F6E3E3C2F6170704D73673E3C2F616374696F6E496E666F3E3C6C6F636174696F6E20636974793D5C225C2220706F69436C61737369667949643D5C225C2220706F694E616D653D5C225C2220706F69416464726573733D5C225C2220706F69436C617373696679547970653D5C22305C223E3C2F6C6F636174696F6E3E3C7075626C6963557365724E616D653E3C2F7075626C6963557365724E616D653E3C2F54696D656C696E654F626A6563743E0D0A1800280030003A13736E735F706F73745F313533343933333731384001580068008001009A010A0A0012001A0020002800AA010408001200C00100".ToByteArray(16, 2));

            SnsPostReq.baseRequest = new BaseRequest()
            {
                sessionKey = customerInfoCache.BaseRequest.sessionKey,
                uin = customerInfoCache.BaseRequest.uin,
                devicelId = customerInfoCache.BaseRequest.devicelId,
                clientVersion = customerInfoCache.BaseRequest.clientVersion,
                osType = customerInfoCache.BaseRequest.osType,
                scene = customerInfoCache.BaseRequest.scene
            };
            SnsPostReq.objectDesc.iLen = (uint)content.Length;
            SnsPostReq.objectDesc.buffer = content;

            SnsPostReq.clientId = "sns_post_" + CurrentTime_().ToString();
            //SnsPostReq.groupNum = 1;
            //SnsPostReq.groupIds = new SnsGroup[1];
            //SnsPostReq.groupIds[0] = new SnsGroup() { GroupId = 3 };
            //SnsPostReq.blackListNum = 1;
            if (BlackList != null && BlackList.Count > 0)
            {
                SnsPostReq.blackListNum = (uint)BlackList.Count;
                SnsPostReq.blackList = new SKBuiltinString[BlackList.Count];
                for (int i = 0; i < BlackList.Count; i++)
                {
                    SnsPostReq.blackList[i] = new SKBuiltinString() { @string = BlackList[i] };
                }

            }
            if (WithUserList != null && WithUserList.Count > 0)
            {
                SnsPostReq.withUserListNum = (uint)WithUserList.Count;
                SnsPostReq.withUserList = new SKBuiltinString[WithUserList.Count];
                for (int i = 0; i < WithUserList.Count; i++)
                {
                    SnsPostReq.withUserList[i] = new SKBuiltinString() { @string = WithUserList[i] };
                }
            }

            var src = Util.Serialize(SnsPostReq);



            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_MMSNSPORT, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_MMSNSPORT);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }
            var SnsPostResponse = Util.Deserialize<SnsPostResponse>(RespProtobuf);
            return SnsPostResponse;
        }

        /// <summary>
        /// 同步盆友圈
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="OStype"></param>
        /// <returns></returns>
        public SnsSyncResponse SnsSync(string wxId)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            //压缩前长度和压缩后长度
            int lenBeforeZip = 0;
            int lenAfterZip = 0;

            var RespProtobuf = new byte[0];
            //生成google对象
            mm.command.MMSnsSyncRequest nsrObj = GoogleProto.CreateMMSnsSyncRequest(customerInfoCache.DeviceId, Encoding.Default.GetString(customerInfoCache.AesKey), (uint)customerInfoCache.MUid, osType, customerInfoCache.InitSyncKey);

            byte[] nsrData = nsrObj.ToByteArray();
            lenBeforeZip = nsrData.Length;

            int bufferlen = nsrData.Length;
            //组包
            byte[] SendDate = pack(nsrData, (int)CGI_TYPE.CGI_TYPE_MMSNSSYNC, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_MMSNSSYNC);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }

            //mm.command.MMSnsSyncRequest nsrReceive = mm.command.MMSnsSyncRequest.ParseFrom(RespProtobuf);
            var SnsSyncResponse = Util.Deserialize<SnsSyncResponse>(RespProtobuf);
            return SnsSyncResponse;
        }


        /// <summary>
        /// 上传朋友圈图片不是发朋友圈图片
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns></returns>
        public micromsg.SnsUploadResponse SnsUpload(string wxId, Stream sm)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            byte[] RespProtobuf = new byte[0];



            int Startpos = 0;//起始位置
            int datalen = 65535;//数据分块长度
            long datatotalength = sm.Length;
            var SnsUploadResponse_ = new micromsg.SnsUploadResponse();

            while (Startpos != (int)datatotalength)
            {//
                int count = 0;
                if (datatotalength - Startpos > datalen)
                {
                    count = datalen;
                }
                else
                {

                    count = (int)datatotalength - Startpos;
                }

                byte[] data = new byte[count];
                sm.Seek(Startpos, SeekOrigin.Begin);
                sm.Read(data, 0, count);


                micromsg.SKBuiltinBuffer_t data_ = new micromsg.SKBuiltinBuffer_t();
                data_.Buffer = data;
                data_.iLen = (uint)data.Length;

                micromsg.SnsUploadRequest snsUpload_ = new micromsg.SnsUploadRequest()
                {
                    BaseRequest = new micromsg.BaseRequest()
                    {
                        SessionKey = customerInfoCache.BaseRequest.sessionKey,
                        Uin = (uint)customerInfoCache.BaseRequest.uin,
                        DeviceID = customerInfoCache.BaseRequest.devicelId,
                        ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                        DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                        Scene = (uint)customerInfoCache.BaseRequest.scene
                    },
                    ClientId = CurrentTime_().ToString(),
                    TotalLen = (uint)datatotalength,
                    StartPos = (uint)Startpos,
                    Buffer = data_,
                    Type = 2
                };

                var src = Util.Serialize(snsUpload_);
                //byte[] RespProtobuf = new byte[0];

                int bufferlen = src.Length;
                //组包
                byte[] SendDate = pack(src, 207, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
                //发包
                byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/snsupload");
                if (RetDate.Length > 32)
                {
                    var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                    //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                    RespProtobuf = packinfo.body;
                    if (packinfo.m_bCompressed)
                    {
                        RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }
                    else
                    {
                        RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }

                }
                else
                {
                    throw new Exception("数据包可能有问题,请稍后再试");
                }
                SnsUploadResponse_ = Util.Deserialize<micromsg.SnsUploadResponse>(RespProtobuf);

                if (SnsUploadResponse_.BaseResponse.Ret == 0)
                {
                    Startpos = Startpos + count;
                }
            }
            return SnsUploadResponse_;
        }

        /// <summary>
        /// 发送文字信息
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="toWxId"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public NewSendMsgRespone SendNewMsg(string wxId, string toWxId, string content, int type = 1)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            var RespProtobuf = new byte[0];
            //byte[] Msg = "0801129B050A150A13777869645F32306E763668336A63333735323212EC043C3F786D6C2076657273696F6E3D22312E30223F3E0A3C6D73672062696768656164696D6775726C3D22687474703A2F2F77782E716C6F676F2E636E2F6D6D686561642F7665725F312F666864536D6F503670744B483467537852495842394643544A455136436169635648317338787675506B787551706E4D344A647956696154626E64696354335172574444616E56624F48535955344F3437396D65796D5947672F302220736D616C6C68656164696D6775726C3D22687474703A2F2F77782E716C6F676F2E636E2F6D6D686561642F7665725F312F666864536D6F503670744B483467537852495842394643544A455136436169635648317338787675506B787551706E4D344A647956696154626E64696354335172574444616E56624F48535955344F3437396D65796D5947672F3133322220757365726E616D653D22777869645F7062673530786463696B7875323222206E69636B6E616D653D224C694152222066756C6C70793D224C694152222073686F727470793D222220616C6961733D2248333130373333343633322220696D6167657374617475733D223322207363656E653D223137222070726F76696E63653D22E5AF86E5858BE7BD97E5B0BCE8A5BFE4BA9A2220636974793D22E5AF86E5858BE7BD97E5B0BCE8A5BFE4BA9A22207369676E3D2222207365783D2232222063657274666C61673D2230222063657274696E666F3D2222206272616E6449636F6E55726C3D2222206272616E64486F6D6555726C3D2222206272616E64537562736372697074436F6E66696755726C3D2222206272616E64466C6167733D22302220726567696F6E436F64653D22464D22202F3E0A182A20D1ED8FDC0528DCD885C681C5ADD69B013200".ToByteArray(16, 2);
            //var newSendMsgRequest1 = ProtoBuf.Serializer.Deserialize<NewSendMsgRequest>(new MemoryStream(Msg));
            //newSendMsgRequest1.info.clientMsgId = (ulong)CurrentTime_();
            //newSendMsgRequest1.info.toid.@string = toWxId;
            //newSendMsgRequest1.info.content = content;
            //newSendMsgRequest1.info.utc = CurrentTime_();
            //newSendMsgRequest1.info.type = type;
            //var src = Util.Serialize(newSendMsgRequest1);
            //var wdawdda = sd111a.ToJson();
            //File.WriteAllText("D://a.txt", wdawdda);
            //byte[] msg = Encoding.UTF8.GetBytes(content);

            NewSendMsgRequest newSendMsgRequest = new NewSendMsgRequest();
            newSendMsgRequest.info = new ChatInfo();
            newSendMsgRequest.info.clientMsgId = (ulong)CurrentTime_();
            newSendMsgRequest.cnt = 1;
            newSendMsgRequest.info.clientMsgId = (ulong)CurrentTime_();
            newSendMsgRequest.info.toid = new SKBuiltinString();
            newSendMsgRequest.info.toid.@string = toWxId;
            newSendMsgRequest.info.content = content;
            newSendMsgRequest.info.utc = CurrentTime_();
            newSendMsgRequest.info.type = type;
            newSendMsgRequest.info.msgSource = string.Empty;
            var src = Util.Serialize(newSendMsgRequest);


            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_NEWSENDMSG, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);

            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_NEWSENDMSG);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }

            var NewSendMsg = Util.Deserialize<NewSendMsgRespone>(RespProtobuf);
            return NewSendMsg;
        }


        /// <summary>
        /// 发送音频文件
        /// </summary>
        /// <param name="to">接收者</param>
        /// <param name="from">发送者</param>
        /// <param name="path">音频路径</param>
        /// <param name="voiceFormat">音频格式</param>
        /// <param name="voiceLen">音频长度 10为一秒</param>
        /// <returns></returns>
        public UploadVoiceResponse SendVoiceMessage(string wxId, string toWxId, byte[] buffer, VoiceFormat voiceFormat = VoiceFormat.MM_VOICE_FORMAT_AMR, int voiceLen = 100)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            SKBuiltinString_ data_ = new SKBuiltinString_();
            data_.buffer = buffer;
            data_.iLen = (uint)buffer.Length;
            UploadVoiceRequest uploadVoice_ = new UploadVoiceRequest()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                from = wxId,
                to = toWxId,
                clientMsgId = CurrentTime_().ToString(),
                voiceFormat = voiceFormat,
                voiceLen = voiceLen,
                length = buffer.Length,
                data = data_,
                offset = 0,
                endFlag = 1,
                msgsource = ""
            };
            int mUid = 0;
            string cookie = null;

            var src = Util.Serialize(uploadVoice_);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 127, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/uploadvoice");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }
            var UploadVoiceResponse_ = Util.Deserialize<UploadVoiceResponse>(RespProtobuf);

            string ret = JsonConvert.SerializeObject(UploadVoiceResponse_);


            return UploadVoiceResponse_;
        }


        public NewSendMsgRespone SendVoiceMessage1(string wxId, string toWxId, string content, byte[] buffer, VoiceFormat voiceFormat = VoiceFormat.MM_VOICE_FORMAT_AMR, int voiceLen = 100, int type = 34)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;
            var RespProtobuf = new byte[0];
            //byte[] Msg = "0801129B050A150A13777869645F32306E763668336A63333735323212EC043C3F786D6C2076657273696F6E3D22312E30223F3E0A3C6D73672062696768656164696D6775726C3D22687474703A2F2F77782E716C6F676F2E636E2F6D6D686561642F7665725F312F666864536D6F503670744B483467537852495842394643544A455136436169635648317338787675506B787551706E4D344A647956696154626E64696354335172574444616E56624F48535955344F3437396D65796D5947672F302220736D616C6C68656164696D6775726C3D22687474703A2F2F77782E716C6F676F2E636E2F6D6D686561642F7665725F312F666864536D6F503670744B483467537852495842394643544A455136436169635648317338787675506B787551706E4D344A647956696154626E64696354335172574444616E56624F48535955344F3437396D65796D5947672F3133322220757365726E616D653D22777869645F7062673530786463696B7875323222206E69636B6E616D653D224C694152222066756C6C70793D224C694152222073686F727470793D222220616C6961733D2248333130373333343633322220696D6167657374617475733D223322207363656E653D223137222070726F76696E63653D22E5AF86E5858BE7BD97E5B0BCE8A5BFE4BA9A2220636974793D22E5AF86E5858BE7BD97E5B0BCE8A5BFE4BA9A22207369676E3D2222207365783D2232222063657274666C61673D2230222063657274696E666F3D2222206272616E6449636F6E55726C3D2222206272616E64486F6D6555726C3D2222206272616E64537562736372697074436F6E66696755726C3D2222206272616E64466C6167733D22302220726567696F6E436F64653D22464D22202F3E0A182A20D1ED8FDC0528DCD885C681C5ADD69B013200".ToByteArray(16, 2);
            //var newSendMsgRequest1 = ProtoBuf.Serializer.Deserialize<NewSendMsgRequest>(new MemoryStream(Msg));
            //newSendMsgRequest1.info.clientMsgId = (ulong)CurrentTime_();
            //newSendMsgRequest1.info.toid.@string = toWxId;
            //newSendMsgRequest1.info.content = content;
            //newSendMsgRequest1.info.utc = CurrentTime_();
            //newSendMsgRequest1.info.type = type;
            //var src = Util.Serialize(newSendMsgRequest1);
            //var wdawdda = sd111a.ToJson();
            //File.WriteAllText("D://a.txt", wdawdda);
            //byte[] msg = Encoding.UTF8.GetBytes(content);

            NewSendMsgRequest newSendMsgRequest = new NewSendMsgRequest();
            newSendMsgRequest.info = new ChatInfo();
            newSendMsgRequest.info.clientMsgId = (ulong)CurrentTime_();
            newSendMsgRequest.cnt = 1;
            newSendMsgRequest.info.clientMsgId = (ulong)CurrentTime_();
            newSendMsgRequest.info.toid = new SKBuiltinString();
            newSendMsgRequest.info.toid.@string = toWxId;
            newSendMsgRequest.info.content = content;
            newSendMsgRequest.info.utc = CurrentTime_();
            newSendMsgRequest.info.type = type;
            newSendMsgRequest.info.msgSource = string.Empty;
            var src = Util.Serialize(newSendMsgRequest);


            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_NEWSENDMSG, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);

            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_NEWSENDMSG);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
            }
            else
            {
                throw new Exception("数据包可能有问题,请稍后再试");
            }

            var NewSendMsg = Util.Deserialize<NewSendMsgRespone>(RespProtobuf);
            return NewSendMsg;
        }
        /// <summary>
        /// 发送视频消息
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="toWxId"></param>
        /// <param name="buffer"></param>
        /// <param name="VideoFrom"></param>
        /// <returns></returns>
        public micromsg.UploadVideoResponse SendVideoMessage(string wxId, string toWxId, byte[] buffer, int VideoFrom = 0)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int mUid = 0;
            string cookie = null;

            SKBuiltinString_ data_ = new SKBuiltinString_();
            data_.buffer = buffer;
            data_.iLen = (uint)buffer.Length;
            micromsg.UploadVideoRequest uploadVoice_ = new micromsg.UploadVideoRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
                FromUserName = wxId,
                ToUserName = toWxId,
                ClientMsgId = CurrentTime_().ToString(),
                VideoFrom = VideoFrom,
                VideoData = new micromsg.SKBuiltinBuffer_t()
                {
                    Buffer = buffer,
                    iLen = (uint)buffer.Length
                },

                PlayLength = 2,
                VideoTotalLen = (uint)buffer.Length,
                VideoStartPos = (uint)buffer.Length,
                EncryVer = 1,
                NetworkEnv = 1,
                FuncFlag = 2,
                ThumbData = new micromsg.SKBuiltinBuffer_t()
                {
                    Buffer = new byte[0],
                    iLen = 0

                },
                CameraType = 2,
                ThumbStartPos = (uint)buffer.Length

            };

            var src = Util.Serialize(uploadVoice_);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 149, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/uploadvideo");
            byte[] RespProtobuf = new byte[0];

            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var UploadVideoResponse_ = Util.Deserialize<micromsg.UploadVideoResponse>(RespProtobuf);

            string ret = JsonConvert.SerializeObject(UploadVideoResponse_);


            return UploadVideoResponse_;
        }

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="wxId"></param>
        /// <param name="toWxId"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public UploadMsgImgResponse SendImageMessage(string wxId, string toWxId, byte[] buffer)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            MemoryStream imgStream = new MemoryStream(buffer);

            int Startpos = 0;//起始位置
            int datalen = 65535;//数据分块长度
            long datatotalength = imgStream.Length;

            SKBuiltinString ClientImgId_ = new SKBuiltinString();
            ClientImgId_.@string = CurrentTime_().ToString();

            SKBuiltinString to_ = new SKBuiltinString();
            to_.@string = toWxId;

            SKBuiltinString from_ = new SKBuiltinString();
            from_.@string = wxId;

            var UploadMsgImgResponse_ = new UploadMsgImgResponse();
            int mUid = 0;
            string cookie = null;
            while (Startpos != (int)datatotalength)
            {//
                int count = 0;
                if (datatotalength - Startpos > datalen)
                {
                    count = datalen;
                }
                else
                {
                    count = (int)datatotalength - Startpos;
                }

                byte[] data = new byte[count];
                imgStream.Seek(Startpos, SeekOrigin.Begin);
                imgStream.Read(data, 0, count);


                SKBuiltinString_ data_ = new SKBuiltinString_();
                data_.buffer = data;
                data_.iLen = (uint)data.Length;

                UploadMsgImgRequest uploadMsgImg_ = new UploadMsgImgRequest()
                {
                    BaseRequest = new BaseRequest()
                    {
                        sessionKey = customerInfoCache.BaseRequest.sessionKey,
                        uin = customerInfoCache.BaseRequest.uin,
                        devicelId = customerInfoCache.BaseRequest.devicelId,
                        clientVersion = customerInfoCache.BaseRequest.clientVersion,
                        osType = customerInfoCache.BaseRequest.osType,
                        scene = customerInfoCache.BaseRequest.scene
                    },
                    clientImgId = ClientImgId_,
                    data = data_,
                    dataLen = (uint)data.Length,
                    totalLen = (uint)datatotalength,
                    to = to_,
                    msgType = (uint)3,
                    from = from_,
                    startPos = (uint)Startpos
                };
                Startpos = Startpos + count;
                var src = Util.Serialize(uploadMsgImg_);
                int bufferlen = src.Length;
                //组包
                byte[] SendDate = pack(src, (int)110, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
                //发包
                byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/uploadmsgimg");
                if (RetDate.Length > 32)
                {
                    var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                    //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                    RespProtobuf = packinfo.body;
                    if (packinfo.m_bCompressed)
                    {
                        RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }
                    else
                    {
                        RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }

                }
                else
                {
                    throw new ExpiredException("用户可能退出,请重新登陆");
                }

                UploadMsgImgResponse_ = Util.Deserialize<UploadMsgImgResponse>(RespProtobuf);
                string ret = JsonConvert.SerializeObject(UploadMsgImgResponse_);
            }//

            return UploadMsgImgResponse_;
        }





        public micromsg.SendAppMsgResponse SendCardMsg(string Content, string toWxid, string wxId)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }

            micromsg.SendCardRequest sendCardRequest = new micromsg.SendCardRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
            };
            sendCardRequest.UserName = toWxid;
            sendCardRequest.Content = Content;
            sendCardRequest.SendCardBitFlag = 1;
            sendCardRequest.Style = 1;
            sendCardRequest.ContentEx = "";

            var src = Util.Serialize(sendCardRequest);

            byte[] RespProtobuf = new byte[0];

            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 222, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/sendcard");

            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }
            var SendAppMsgResponse_ = Util.Deserialize<micromsg.SendAppMsgResponse>(RespProtobuf);
            return SendAppMsgResponse_;

        }


        /// <summary>
        /// 发送app信息
        /// </summary>
        /// <param name="Content">xml内容</param>
        /// <param name="ToUserName">接收人</param>
        /// <param name="FromUserName">发送人</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public micromsg.SendAppMsgResponse SendAppMsg(string Content, string toWxid, string wxId, int type = 8)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            micromsg.SendAppMsgRequest sendAppMsg_ = new micromsg.SendAppMsgRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
            };
            sendAppMsg_.Msg = new micromsg.AppMsg();
            sendAppMsg_.Msg.ClientMsgId = CurrentTime_().ToString();
            sendAppMsg_.Msg.Content = Content;
            sendAppMsg_.Msg.ToUserName = toWxid;
            sendAppMsg_.Msg.FromUserName = wxId;
            sendAppMsg_.Msg.Type = (uint)type;

            var src = Util.Serialize(sendAppMsg_);

            byte[] RespProtobuf = new byte[0];

            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 222, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/sendappmsg");

            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }
            var SendAppMsgResponse_ = Util.Deserialize<micromsg.SendAppMsgResponse>(RespProtobuf);
            return SendAppMsgResponse_;

        }


        /// <summary>
        /// v1 v2操作
        /// </summary>
        /// <param name="opCode">1关注公众号2打招呼 主动添加好友3通过好友请求</param>
        /// <param name="Content">内容</param>
        /// <param name="antispamTicket">v2</param>
        /// <param name="value">v1</param>
        /// <param name="sceneList">1来源QQ2来源邮箱3来源微信号14群聊15手机号18附近的人25漂流瓶29摇一摇30二维码13来源通讯录</param>
        /// <returns></returns>
        public VerifyUserResponese VerifyUser(string wxId, VerifyUserOpCode opCode, string Content, string antispamTicket, string value, byte sceneList = 0x0f)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            byte[] RespProtobuf = new byte[0];
            VerifyUser[] verifyUser_ = new VerifyUser[1];
            VerifyUser user = new VerifyUser();

            user.value = value;

            user.antispamTicket = antispamTicket;
            user.friendFlag = 0;
            user.scanQrcodeFromScene = 0;
            if (opCode == VerifyUserOpCode.MM_VERIFYUSER_VERIFYOK)
            {
                user.verifyUserTicket = antispamTicket;
            }
            verifyUser_[0] = user;
            VerifyUserRequest1 verifyUser_b = new VerifyUserRequest1()
            {
                baseRequest = new BaseRequest()
                {
                    sessionKey = customerInfoCache.BaseRequest.sessionKey,
                    uin = customerInfoCache.BaseRequest.uin,
                    devicelId = customerInfoCache.BaseRequest.devicelId,
                    clientVersion = customerInfoCache.BaseRequest.clientVersion,
                    osType = customerInfoCache.BaseRequest.osType,
                    scene = customerInfoCache.BaseRequest.scene
                },
                SceneListNumFieldNumber = (uint)1,
                opcode = opCode,
                sceneList = new byte[] { sceneList },
                verifyContent = Content,
                verifyUserListSize = 1,
                verifyUserList = verifyUser_,
            };

            var src = Util.Serialize(verifyUser_b);


            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_VERIFYUSER, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_VERIFYUSER);
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var VerifyUseResponse_ = Util.Deserialize<VerifyUserResponese>(RespProtobuf);
            return VerifyUseResponse_;
        }

        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="customerInfoCache"></param>
        /// <returns></returns>
        public mm.command.NewInitResponse NewInit(CustomerInfoCache customerInfoCache)
        {
            //string key = ConstCacheKey.GetWxIdKey(wxId);
            //var cache = CacheHelper.CreateInstance();
            //var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            //if (customerInfoCache == null)
            //{
            //    throw new ExpiredException("缓存失效，请重新生成二维码登录");
            //}
            var RespProtobuf = new byte[0];
            //压缩前长度和压缩后长度
            int lenBeforeZip = 0;
            int lenAfterZip = 0;

            //生成google对象
            mm.command.NewInitRequest niqObj = GoogleProto.CreateNewInitRequestEntity((uint)customerInfoCache.MUid, Encoding.Default.GetString(customerInfoCache.AesKey), customerInfoCache.WxId, customerInfoCache.DeviceId, "iPad iPhone OS9.3.3", customerInfoCache.InitSyncKey, customerInfoCache.MaxSyncKey);

            byte[] niqData = niqObj.ToByteArray();
            int mUid = 0;
            string cookie = null;

            int bufferlen = niqData.Length;
            //组包
            byte[] SendDate = pack(niqData, 139, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/newinit");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            mm.command.NewInitResponse niReceive = mm.command.NewInitResponse.ParseFrom(RespProtobuf);
            if (niReceive.Base.Ret == 0)
            {
                customerInfoCache.InitSyncKey = niReceive.CurrentSynckey.Buffer.ToByteArray();
                customerInfoCache.MaxSyncKey = niReceive.MaxSynckey.Buffer.ToByteArray();
            }
            return niReceive;
        }

        /// <summary>
        /// 获取高清图片
        /// </summary>
        /// <param name="datatotalength"></param>
        /// <param name="MsgId"></param>
        /// <param name="wxId"></param>
        /// <param name="toWxid"></param>
        /// <param name="CompressType"></param>
        /// <returns></returns>
        public byte[] GetMsgBigImg(long datatotalength, int MsgId, string wxId, string toWxid, uint CompressType = 1)
        {
            string key = ConstCacheKey.GetWxIdKey(wxId);
            var cache = CacheHelper.CreateInstance();
            var customerInfoCache = cache.Get<CustomerInfoCache>(key);
            if (customerInfoCache == null)
            {
                throw new ExpiredException("缓存失效，请重新生成二维码登录");
            }
            int Startpos = 0;//起始位置
            int datalen = 30720;//数据分块长度
            //long datatotalength = 1238782;//根据需要选择下载 高清图还是缩略图 长度自然是对应 高清长度和缩略长度
            List<byte> downImgData = new List<byte>();
            var GetMsgImgResponse_ = new micromsg.GetMsgImgResponse();
            while (Startpos != (int)datatotalength)
            {//
                int count = 0;
                if (datatotalength - Startpos > datalen)
                {
                    count = datalen;
                }
                else
                {
                    count = (int)datatotalength - Startpos;
                }

                micromsg.SKBuiltinString_t ToUserName_ = new micromsg.SKBuiltinString_t();
                ToUserName_.String = toWxid;

                micromsg.SKBuiltinString_t FromUserName_ = new micromsg.SKBuiltinString_t();
                FromUserName_.String = wxId;
                micromsg.GetMsgImgRequest getMsgImg_ = new micromsg.GetMsgImgRequest()
                {
                    BaseRequest = new micromsg.BaseRequest()
                    {
                        SessionKey = customerInfoCache.BaseRequest.sessionKey,
                        Uin = (uint)customerInfoCache.BaseRequest.uin,
                        DeviceID = customerInfoCache.BaseRequest.devicelId,
                        ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                        DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                        Scene = (uint)customerInfoCache.BaseRequest.scene
                    },

                    MsgId = (uint)MsgId,
                    StartPos = (uint)Startpos,
                    DataLen = (uint)count,
                    TotalLen = (uint)datatotalength,
                    CompressType = CompressType,//hdlength  1高清0缩略
                    ToUserName = ToUserName_,
                    FromUserName = FromUserName_
                };

                var src = Util.Serialize(getMsgImg_);

                byte[] RespProtobuf = new byte[0];

                int mUid = 0;
                string cookie = null;
                int bufferlen = src.Length;
                //组包
                byte[] SendDate = pack(src, 109, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
                //发包
                byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/getmsgimg");
                if (RetDate.Length > 32)
                {
                    var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                    //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                    RespProtobuf = packinfo.body;
                    if (packinfo.m_bCompressed)
                    {
                        RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }
                    else
                    {
                        RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }

                }
                else
                {
                    throw new ExpiredException("用户可能退出,请重新登陆");
                }

                GetMsgImgResponse_ = Util.Deserialize<micromsg.GetMsgImgResponse>(RespProtobuf);
                if (GetMsgImgResponse_.Data == null) { continue; }
                if (GetMsgImgResponse_.Data.iLen != 0)
                {
                    downImgData.AddRange(GetMsgImgResponse_.Data.Buffer);
                    Startpos = Startpos + GetMsgImgResponse_.Data.Buffer.Length;
                }
            }

            return downImgData.ToArray();
        }


        /// <summary>
        /// 确定登录包 改用长连接
        /// </summary>
        /// <param name="wxnewpass">这是扫码成功过来伪密码</param>
        /// <param name="wxid">wxid</param>
        /// <returns></returns>
        private ManualAuthResponse ManualAuth(CustomerInfoCache customerInfoCache)
        {
            var RespProtobuf = new byte[0];
            GenerateECKey(713, customerInfoCache.PubKeyHUb, customerInfoCache.PriKeyBuf);
            //OpenSSLNativeClass.ECDH.GenEcdh(713, ref pub_key_buf, ref pri_key_buf);
            ManualAuthAccountRequest manualAuthAccountRequest = new ManualAuthAccountRequest()
            {
                aes = new AesKey()
                {
                    len = 16,
                    key = customerInfoCache.AesKey
                },
                ecdh = new Ecdh()
                {
                    ecdhkey = new EcdhKey()
                    {
                        key = customerInfoCache.PubKeyHUb,
                        len = 57
                    },
                    nid = 713
                },

                password1 = customerInfoCache.WxNewPass,
                password2 = null,
                userName = customerInfoCache.WxId
            };
            ManualAuthDeviceRequest manualAuthDeviceRequest = new ManualAuthDeviceRequest();
            //manualAuthDeviceRequest = Util.Deserialize<ManualAuthDeviceRequest>("0A310A0010001A1049AA7DB2F4A3FFE0E96218F6B92CDE3220A08E98B0012A1169506164206950686F6E65204F53382E34300112023A001A203363616137646232663461336666653065393632313866366239326364653332228D023C736F6674747970653E3C6B333E382E343C2F6B333E3C6B393E695061643C2F6B393E3C6B31303E323C2F6B31303E3C6B31393E45313841454344332D453630422D344635332D423838372D4339343436344437303836393C2F6B31393E3C6B32303E3C2F6B32303E3C6B32313E313030333C2F6B32313E3C6B32323E286E756C6C293C2F6B32323E3C6B32343E62383A66383A38333A33393A61643A62393C2F6B32343E3C6B33333EE5BEAEE4BFA13C2F6B33333E3C6B34373E313C2F6B34373E3C6B35303E313C2F6B35303E3C6B35313E6461697669732E495041443C2F6B35313E3C6B35343E69506164322C353C2F6B35343E3C6B36313E323C2F6B36313E3C2F736F6674747970653E2800322B33636161376462326634613366666530653936323138663662393263646533322D313532383535343230314204695061644A046950616452057A685F434E5A04382E3030680070AFC6EFD8057A054170706C65920102434E9A010B6461697669732E49504144AA010769506164322C35B00102BA01D50608CF0612CF060A08303030303030303310011AC0068A8DCEEE5AB9F4E16054EDA0545F7288B7951621A41446C1AEC0621B3CFE6926737F8298D0B52F467FDFC5EC936D512D332A1AC664E7DFEE734A5E403A72225F852734BF32F6FD623B95D17B64DC8D18FBB2CA2015113CD17518274BED4687D26F5D9E270687745541FA84921A16B50CFE487B1A88C3A91D838A2520AF8757F0E5ACE55BA599B9FCDF1595C3DAAD8E3A34C28BA39951D7A4CF9075CCC28721BA61E48C2DA1B853F3BE0D79AC63F47F2E3C4FF10D4D1CCC1D3002B6F63C228641C1EEB24686BA300853C355C268057D733B7898D20E6B43621419D8BCFCAED82C45377653234B7421238D00B25089670DDEBB03274B1D0D8C45D5A0EA7ECA9086254CCEAA8674ADE4DF905914437BC73D4C9D50CEC9ABCB927590D068DC10A810D376DAFB17A31F947765FF6A7F3B191EC40EEC4AA86FF8771CD2D717D25EE2B7555179AF4C611B9C6AD802B8FDAEAE36CA3497C438E8D4A06B1A7A570D74AAF6C244E8D23BA635FF0F27DCFCF5F6C4754A0049A620AE99012EB4936D34BAD267EAFDB12B67D5274272D3BC795B6454B4C2B768929007D0993F742A519D567ACD0369FCC9196D3CC04578F795026C336F2A29A012608C66E2068F5994210173C5A3B2720A4D040A6D2C3E873D56CE88F85CEFE4847743DEF1102653D42FBC3A31CA5BFE2E666D3542E6E1C5BCCE54D99EC934B183EED69FEA87D975666065E5903F366EFFE04627603FD64861C142A5A19EBD344BF194DE427FB4B70AA0D3CD972AC0A11EA6913E17366CA48966090E10B246BABABA553DBF89BEA4F55004C37E546ABABB8AA20E80B2A0ED21B6700F89699FD01983EDA71ACE6A44B6397605D30E88683BA4BB92A50DC7AFFB820089F157B8C83F7B5DCD35BABCC90501E2E6BDF83327A1059908C72EAF1B5A07CA6565A0888883966D26386C69293649BEC0913FE12C1ABA7B0B16261176E2F7D109FCF68A46B7C3AF7126E77224AA36891B703655CFEA2AAA8B5E095D8B204308133E63D0F0309E8B1CB5A21E9C8B27090859139C076723DE4C74578F6584888220A11A45CDDEC43A1F542552604C96FFE3A01006946086A864C182361B3659C1BDE9ECEA5236F5F38BA98A4C7E8C81A39D5CBA39B7A0F9FFA75AC59BB956595B58DAED58A0851D48B0B7A7407FA576E4956C".ToByteArray(16, 2));
            manualAuthDeviceRequest.Timestamp = (int)CurrentTime_();
            //manualAuthDeviceRequest.Clientcheckdat = new SKBuiltinString_() { buffer = new byte[] { }, iLen = 0 };
            manualAuthDeviceRequest.imei = customerInfoCache.DeviceId;
            manualAuthDeviceRequest.clientSeqID = manualAuthDeviceRequest.imei + "-" + ((int)CurrentTime_()).ToString();
            manualAuthDeviceRequest.baseRequest = GetBaseRequest(customerInfoCache.AesKey, customerInfoCache.DeviceId, 1);


            var account = Util.Serialize(manualAuthAccountRequest);
            byte[] device = Util.Serialize(manualAuthDeviceRequest);
            byte[] subHeader = new byte[] { };
            int dwLenAccountProtobuf = account.Length;
            subHeader = subHeader.Concat(dwLenAccountProtobuf.ToByteArray(Endian.Big)).ToArray();
            int dwLenDeviceProtobuf = device.Length;
            subHeader = subHeader.Concat(dwLenDeviceProtobuf.ToByteArray(Endian.Big)).ToArray();
            if (subHeader.Length > 0 && account.Length > 0 && device.Length > 0)
            {
                var cdata = Util.compress_rsa_LOGIN(account);
                int dwLenAccountRsa = cdata.Length;
                subHeader = subHeader.Concat(dwLenAccountRsa.ToByteArray(Endian.Big)).ToArray();
                byte[] body = subHeader;
                ManualAuthDeviceRequest m_ManualAuthDeviceRequest = Util.Deserialize<ManualAuthDeviceRequest>(device);
                //var t2=m_ManualAuthDeviceRequest.tag2.ToString(16, 2);

                var memoryStream = Util.Serialize(m_ManualAuthDeviceRequest);

                body = body.Concat(cdata).ToArray();

                body = body.Concat(Util.compress_aes(device, customerInfoCache.AesKey)).ToArray();
                //var head = MakeHead( body, MM.CGI_TYPE.CGI_TYPE_MANUALAUTH, 7);
                var head = MakeHead((int)CGI_TYPE.CGI_TYPE_MANUALAUTH, body.Length, customerInfoCache.MUid, customerInfoCache.Cookie, 7, false);

                body = head.Concat(body).ToArray();

                byte[] RetDate = Util.HttpPost(body, URL.CGI_MANUALAUTH);
                //Console.WriteLine(RetDate.ToString(16, 2));
                //var ret = HttpPost(@short + MM.URL.CGI_MANUALAUTH, head, null);
                //var lhead = LongLinkPack(LongLinkCmdId.SEND_MANUALAUTH_CMDID, seq++, head.Length);
                if (RetDate.Length > 32)
                {
                    int muid = 0;
                    string cookie = null;
                    var packinfo = UnPackHeader(RetDate, out muid, out cookie);
                    customerInfoCache.MUid = muid;
                    customerInfoCache.Cookie = cookie;
                    //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                    RespProtobuf = packinfo.body;
                    if (packinfo.m_bCompressed)
                    {
                        RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }
                    else
                    {
                        RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                    }

                }
                else
                {
                    throw new ExpiredException("用户可能退出,请重新登陆");
                }
                if (RespProtobuf == null) return null;
                var ManualAuthResponse = Util.Deserialize<ManualAuthResponse>(RespProtobuf);
                return ManualAuthResponse;
            }
            else
                return null;
            //return null;

        }









        /// <summary>
        /// 请求的基本参数
        /// </summary>
        /// <param name="aesKey"></param>
        /// <param name="deviceId"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        private BaseRequest GetBaseRequest(byte[] aesKey, byte[] deviceId, int scene = 0)
        {
            MemoryStream memoryStream = new MemoryStream();
            var baseRequest = new BaseRequest()
            {
                clientVersion = version,

                devicelId = deviceId,
                scene = scene,
                sessionKey = aesKey,
                osType = osType,
                uin = 0
            };
            return baseRequest;
        }

        /// <summary>
        /// 设备信息和秘钥建议
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="sessionKey"></param>
        /// <param name="uIn"></param>
        /// <param name="osType"></param>
        /// <returns></returns>
        public BaseRequest GetBaseRequest(byte[] deviceID, byte[] sessionKey, uint uIn, string osType)
        {
            var baseRequest_ = GoogleProto.CreateBaseRequestEntity(deviceID, Encoding.Default.GetString(sessionKey), uIn, osType, 3);

            return Util.Deserialize<BaseRequest>(baseRequest_.ToByteArray());
        }

        /// <summary>
        /// 组包
        /// </summary>
        /// <param name="src"></param>
        /// <param name="cgi_"></param>
        /// <param name="nLenProtobuf"></param>
        /// <param name="encodetypr"></param>
        /// <param name="iscookie"></param>
        /// <param name="isuin"></param>
        /// <returns></returns>
        private byte[] pack(byte[] src, int cgi_, int nLenProtobuf, byte[] aesKey, byte[] pri_key_buf, int m_uid, string cookie, byte encodetypr = 7, bool iscookie = false, bool isuin = false)
        {
            //组包头
            var pbody = new byte[0];
            if (encodetypr == 7)
            {
                var head = MakeHead(cgi_, src.Length, m_uid, cookie, encodetypr, iscookie, isuin);
                pbody = head.Concat(Util.nocompress_rsa(src)).ToArray();
            }
            else if (encodetypr == 5)
            {
                //计算校验
                uint check_ = check((uint)m_uid, src, pri_key_buf);
                //压缩
                byte[] zipData = DeflateCompression.DeflateZip(src);
                int lenAfterZip = zipData.Length;

                //aes加密
                byte[] aesData = Util.AESEncryptorData(zipData, aesKey);

                pbody = CommonRequestPacket(src.Length, lenAfterZip, aesData, (uint)m_uid, 0xd, (short)cgi_, cookie?.ToByteArray(16, 2), 0);
            }
            else if (encodetypr == 1)
            {
                var head = MakeHead(cgi_, src.Length, m_uid, cookie, encodetypr, iscookie, isuin);
                pbody = head.Concat(Util.compress_rsa(src)).ToArray();
            }
            return pbody;
        }

        /// <summary>
        /// 计算校验
        /// </summary>
        /// <param name="uin"></param>
        /// <param name="niqData"></param>
        /// <param name="eccKey"></param>
        /// <returns></returns>
        private uint check(uint uin, byte[] niqData, byte[] eccKey)
        {
            int lenBeforeZip = niqData.Length;
            byte[] byteInt = new byte[4];
            //byteInt[0] = (byte)(((uin & 0xff000000) >> 24) & 0xff);
            //byteInt[1] = (byte)(((uin & 0x00ff0000) >> 16) & 0xff);
            //byteInt[2] = (byte)(((uin & 0x0000ff00) >> 8) & 0xff);
            //byteInt[3] = (byte)((uin & 0x000000ff) & 0xff);

            byte[] md5 = Util.MD5(byteInt.Concat(eccKey).ToArray());

            byteInt[0] = (byte)(((lenBeforeZip & 0xff000000) >> 24) & 0xff);
            byteInt[1] = (byte)(((lenBeforeZip & 0x00ff0000) >> 16) & 0xff);
            byteInt[2] = (byte)(((lenBeforeZip & 0x0000ff00) >> 8) & 0xff);
            byteInt[3] = (byte)((lenBeforeZip & 0x000000ff) & 0xff);
            md5 = Util.MD5(byteInt.Concat(eccKey).Concat(md5).ToArray());

            uint check = Adler32(1, md5, md5.Length);
            check = Adler32(check, niqData, lenBeforeZip);
            return check;
        }
        /// <summary>
        /// 组吧
        /// </summary>
        /// <param name="lengthBeforeZip"></param>
        /// <param name="lengthAfterZip"></param>
        /// <param name="aesDataPacket"></param>
        /// <param name="uin"></param>
        /// <param name="deviceID"></param>
        /// <param name="_byteVar"></param>
        /// <returns></returns>
        private byte[] CommonRequestPacket(int lengthBeforeZip, int lengthAfterZip, byte[] aesDataPacket, uint uin,
            short cmd, short cmd2, byte[] cookie, uint check)
        {
            byte[] frontPacket = {
                                     0xBF, 0x62, 0x50, 0x16, 0x07, 0x03, 0x21
                                 };

            byte[] endTag = { 0x02 };
            byte[] byteUin = new byte[4];

            uint a = (uin & 0xff000000);
            byteUin[0] = (byte)(((uin & 0xff000000) >> 24) & 0xff);
            byteUin[1] = (byte)(((uin & 0x00ff0000) >> 16) & 0xff);
            byteUin[2] = (byte)(((uin & 0x0000ff00) >> 8) & 0xff);
            byteUin[3] = (byte)((uin & 0x000000ff) & 0xff);

            byte[] packet = frontPacket.Concat(byteUin).ToArray();

            packet = packet.Concat(cookie).ToArray();

            packet = packet.Concat(toVariant(cmd2)).ToArray();

            packet = packet.Concat(toVariant(lengthBeforeZip)).ToArray();

            packet = packet.Concat(toVariant(lengthAfterZip)).ToArray();

            packet = packet.Concat(toVariant(10000)).ToArray();

            packet = packet.Concat(endTag).ToArray();
            packet = packet.Concat(toVariant((int)check)).ToArray();

            packet = packet.Concat(toVariant(0x01004567)).ToArray();

            int HeadLen = packet.Length;

            packet[1] = (byte)((HeadLen * 4) + 1);
            packet[2] = (byte)(0x50 + cookie.Length);

            packet = packet.Concat(aesDataPacket).ToArray();

            return packet;
        }
        //组包体头
        private byte[] MakeHead(int cgi, int nLenProtobuf, int m_uid, string cookie, byte encodetypr = 7, bool iscookie = false, bool isuin = false)
        {

            List<byte> strHeader = new List<byte>();
            int nCur = 0;
            byte SecondByte = 0x2;
            strHeader.Add(SecondByte);
            nCur++;
            //加密算法(前4bits),RSA加密(7)AES(5)
            byte ThirdByte = (byte)(encodetypr << 4);
            if (iscookie)
                ThirdByte += 0xf;
            strHeader.Add((byte)ThirdByte);
            nCur++;
            int dwUin = 0;
            if (isuin)
                dwUin = m_uid;
            strHeader = strHeader.Concat(version.ToByteArray(Endian.Big).ToList()).ToList();
            nCur += 4;

            strHeader = strHeader.Concat(dwUin.ToByteArray(Endian.Big).ToList()).ToList();
            nCur += 4;

            if (iscookie)
            {
                //登录包不需要cookie 全0占位即可
                if (cookie == null || cookie == "" || cookie.Length < 0xf)
                {
                    strHeader = strHeader.Concat(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }).ToList();
                    nCur += 15;
                }
                else
                {
                    strHeader = strHeader.Concat(cookie.ToByteArray(16, 2).ToList()).ToList();
                    nCur += 15;
                }
            }
            byte[] strcgi = Dword2String((UInt32)cgi);
            strHeader = strHeader.Concat(strcgi.ToList()).ToList();
            nCur += strcgi.Length;
            byte[] strLenProtobuf = Dword2String((UInt32)nLenProtobuf);
            strHeader = strHeader.Concat(strLenProtobuf.ToList()).ToList();
            nCur += strLenProtobuf.Length;
            byte[] strLenCompressed = Dword2String((UInt32)nLenProtobuf);
            strHeader = strHeader.Concat(strLenCompressed.ToList()).ToList();
            nCur += strLenCompressed.Length;
            var rsaVer = Dword2String((UInt32)LOGIN_RSA_VER);
            strHeader = strHeader.Concat(rsaVer).ToList();
            nCur += rsaVer.Length;
            strHeader = strHeader.Concat(new byte[] { 1, 2 }.ToList()).ToList();
            nCur += 2;

            //var unkwnow = (9).ToByteArray(Endian.Little).Copy(2);// "0100".ToByteArray(16, 2);
            //strHeader = strHeader.Concat(unkwnow.ToList()).ToList();
            //nCur += unkwnow.Length;
            nCur++;
            SecondByte += (byte)(nCur << 2);
            strHeader[0] = SecondByte;

            strHeader.Insert(0, 0xbf);
            return strHeader.ToArray();


        }
        private byte[] Dword2String(UInt32 dw)
        {
            List<byte> apcBuffer = new List<byte>();

            while (dw >= 0x80)
            {

                apcBuffer.Add((byte)(0x80 + (dw & 0x7f)));
                dw = dw >> 7;
            }
            apcBuffer.Add((byte)dw);
            return apcBuffer.ToArray();
            //Int32 dwData = dw;
            //Int32 dwData2 = 0x80 * 0x80 * 0x80 * 0x80;
            //int nLen = 4;
            //byte[] hex = new byte[5];
            //Int32 dwOutLen = 0;
            //while (nLen > 0)
            //{
            //    if (dwData > dwData2)
            //    {
            //        hex[nLen] = (byte)(dwData / dwData2);
            //        dwData = dwData % dwData2;
            //        dwOutLen++;
            //    }

            //    dwData2 /= 0x80;
            //    nLen--;
            //}

            //hex[0] = (byte)dwData;

            //dwOutLen++;

            //for (int i = 0; i < (int)(dwOutLen - 1); i++)
            //{
            //    hex[i] += 0x80;
            //}

            //return hex;
        }

        private int DecodeVByte32(ref int apuValue, byte[] apcBuffer, int off)
        {
            int num3;
            int num = 0;
            int num2 = 0;
            int num4 = 0;
            byte num5 = apcBuffer[off + num++];
            while ((num5 & 0xff) >= 0x80)
            {
                num3 = num5 & 0x7f;
                num4 += num3 << num2;
                num2 += 7;
                num5 = apcBuffer[off + num++];
            }
            num3 = num5;
            num4 += num3 << num2;
            apuValue = num4;
            return num;
        }

        private byte[] toVariant(int toValue)
        {
            uint va = (uint)toValue;
            List<byte> result = new List<byte>();

            while (va >= 0x80)
            {
                result.Add((byte)(0x80 + va % 0x80));
                va /= 0x80;
            }
            result.Add((byte)(va % 0x80));

            return result.ToArray();
        }
        /// <summary>
        /// 获取当前时间时间戳
        /// </summary>
        /// <returns></returns>
        private long CurrentTime_()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public NewSyncResponse NewSyncEcode(CustomerInfoCache customerInfoCache, int scane)
        {
            var RespProtobuf = new byte[0];
            //0a020800108780101a8a02088402128402081f1208080110aaa092ba021208080210a9a092ba0212080803109aa092ba021208080410f28292ba021208080510f28292ba021208080710f28292ba02120408081000120808091099a092ba021204080a10001208080b10839f92ba021204080d10001208080e10f28292ba021208081010f28292ba021204086510001204086610001204086710001204086810001204086910001204086b10001204086d10001204086f1000120408701000120408721000120908c90110f5d7fbd705120908cb0110c6bcf3d705120508cc011000120508cd011000120908e80710fdd0fad705120908e90710ba92fad705120908ea07109bf1c9d705120908d10f10d1b9f0d70520032a0a616e64726f69642d31393001
            MemoryStream memoryStream = new MemoryStream();
            NewSyncRequest request = new NewSyncRequest()
            {
                continueflag = new FLAG() { flag = 0 },
                device = customerInfoCache.Device,
                scene = scane,
                selector = 262151,//3
                syncmsgdigest = 3,
                tagmsgkey = new syncMsgKey()
                {
                    msgkey = new Synckey()
                    {
                        size = 32
                    }
                }
            };

            request.tagmsgkey = Util.Deserialize<syncMsgKey>(customerInfoCache.Sync);
            var src = Util.Serialize(request);
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, (int)CGI_TYPE.CGI_TYPE_NEWSYNC, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, URL.CGI_NEWSYNC);
            int mUid = 0;
            string cookie = null;
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var NewSync = Util.Deserialize<NewSyncResponse>(RespProtobuf);

            return NewSync;
        }

        /// <summary>
        /// 摇一摇提交bufkey
        /// </summary>
        /// <param name="sKBuiltinBuffer_T"></param>
        /// <returns></returns>
        private micromsg.ShakeGetResponse ShakeGet(CustomerInfoCache customerInfoCache, micromsg.SKBuiltinBuffer_t sKBuiltinBuffer_T)
        {

            byte[] RespProtobuf = new byte[0];
            micromsg.ShakeGetRequest shakeGet_ = new micromsg.ShakeGetRequest()
            {
                BaseRequest = new micromsg.BaseRequest()
                {
                    SessionKey = customerInfoCache.BaseRequest.sessionKey,
                    Uin = (uint)customerInfoCache.BaseRequest.uin,
                    DeviceID = customerInfoCache.BaseRequest.devicelId,
                    ClientVersion = customerInfoCache.BaseRequest.clientVersion,
                    DeviceType = Encoding.UTF8.GetBytes(customerInfoCache.BaseRequest.osType),
                    Scene = (uint)customerInfoCache.BaseRequest.scene
                },
                Buffer = sKBuiltinBuffer_T
            };

            var src = Util.Serialize(shakeGet_);
            int mUid = 0;
            string cookie = null;
            int bufferlen = src.Length;
            //组包
            byte[] SendDate = pack(src, 162, bufferlen, customerInfoCache.AesKey, customerInfoCache.PriKeyBuf, customerInfoCache.MUid, customerInfoCache.Cookie, 5, true, true);
            //发包
            byte[] RetDate = Util.HttpPost(SendDate, "/cgi-bin/micromsg-bin/shakeget");
            if (RetDate.Length > 32)
            {
                var packinfo = UnPackHeader(RetDate, out mUid, out cookie);
                //Console.WriteLine("CGI {0} BodyLength {1} m_bCompressed {2}", packinfo.CGI, packinfo.body.Length, packinfo.m_bCompressed);
                RespProtobuf = packinfo.body;
                if (packinfo.m_bCompressed)
                {
                    RespProtobuf = Util.uncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }
                else
                {
                    RespProtobuf = Util.nouncompress_aes(packinfo.body, customerInfoCache.AesKey);
                }

            }
            else
            {
                throw new ExpiredException("用户可能退出,请重新登陆");
            }

            var ShakeGetResponse_ = Util.Deserialize<micromsg.ShakeGetResponse>(RespProtobuf);
            return ShakeGetResponse_;
        }

        /// <summary>
        /// 解包头 返回 包数据结构
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        private PACKINFO UnPackHeader(byte[] pack, out int m_uid, out string cookie)
        {
            m_uid = 0;
            cookie = null;
            //Console.WriteLine(pack.ToString(16, 2));
            PACKINFO pACKINFO = new PACKINFO();
            byte[] body = new byte[] { };
            pACKINFO.body = body;
            if (pack.Length < 0x20) return pACKINFO;
            int nCur = 0;
            if (0xbf == pack[nCur])
            {
                nCur++;
            }
            //解析包头长度(前6bits)
            int nHeadLen = pack[nCur] >> 2;

            //是否使用压缩(后2bits)
            pACKINFO.m_bCompressed = (1 == (pack[nCur] & 0x3)) ? true : false;

            nCur++;

            //解密算法(前4 bits)(05:aes / 07:rsa)(仅握手阶段的发包使用rsa公钥加密,由于没有私钥收包一律aes解密)
            pACKINFO.m_nDecryptType = pack[nCur] >> 4;

            //cookie长度(后4 bits)
            int nCookieLen = pack[nCur] & 0xF;

            nCur++;

            //服务器版本,无视(4字节)
            nCur += 4;

            //登录包 保存uin
            //int dwUin;
            m_uid = (int)pack.Copy(nCur, 4).GetUInt32(Endian.Big);
            //memcpy(&dwUin, &(pack[nCur]), 4);
            //s_dwUin = ntohl(dwUin);
            nCur += 4;
            //刷新cookie(超过15字节说明协议头已更新)
            if (nCookieLen > 0 && nCookieLen <= 0xf)
            {
                string s_cookie = pack.Copy(nCur, nCookieLen).ToString(16, 2);
                //pAuthInfo->m_cookie = s_cookie;
                cookie = s_cookie;
                nCur += nCookieLen;
            }
            else if (nCookieLen > 0xf)
            {
                return null;
            }

            //cgi type,变长整数,无视

            int dwLen = DecodeVByte32(ref pACKINFO.CGI, pack.Copy(nCur, 5), 0);
            //pACKINFO. CGI = String2Dword(pack.Copy(nCur, 5));
            nCur += dwLen;

            //解压后protobuf长度，变长整数
            int m_nLenRespProtobuf = 0;//String2Dword(pack.Copy(nCur, 5));
            dwLen = DecodeVByte32(ref m_nLenRespProtobuf, pack.Copy(nCur, 5), 0);
            nCur += dwLen;

            //压缩后(加密前)的protobuf长度，变长整数
            int m_nLenRespCompressed = 0;//String2Dword(pack.Copy(nCur, 5));
            dwLen = DecodeVByte32(ref m_nLenRespCompressed, pack.Copy(nCur, 5), 0);
            nCur += dwLen;

            //后面数据无视

            //解包完毕,取包体
            if (nHeadLen < pack.Length)
            {
                body = pack.Copy(nHeadLen, pack.Length - nHeadLen);
            }
            pACKINFO.body = body;
            //Console.WriteLine("body len{0}", pACKINFO.body.Length);
            // Console.WriteLine(body.ToString(16, 2));
            return pACKINFO;
        }

        public class PACKINFO
        {
            //是否压缩
            public bool m_bCompressed;
            //是否加密
            public int m_nDecryptType;
            // CGi
            public int CGI;
            //包体
            public byte[] body;
        }
    }


}
