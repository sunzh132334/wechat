using MMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wechat.Protocol
{
    public class InitResponse
    {
        public IList<MM.ModUserInfo> ModUserInfos { get; set; } = new List<MM.ModUserInfo>();
        public IList<MM.ModContact> ModContacts { get; set; } = new List<MM.ModContact>();
        public IList<MM.DelContact> DelContacts { get; set; } = new List<MM.DelContact>();
        public IList<MM.AddMsg> AddMsgs { get; set; } = new List<MM.AddMsg>();
        public IList<MM.ModMsgStatus> ModMsgStatuss { get; set; } = new List<MM.ModMsgStatus>();
        public IList<MM.DelChatContact> DelChatContacts { get; set; } = new List<MM.DelChatContact>();
        public IList<MM.DelContactMsg> DelContactMsgs { get; set; } = new List<MM.DelContactMsg>();
        public IList<MM.DelMsg> DelMsgs { get; set; } = new List<MM.DelMsg>();
        public IList<MM.OpenQQMicroBlog> OpenQQMicroBlogs { get; set; } = new List<MM.OpenQQMicroBlog>();
        public IList<MM.CloseMicroBlog> CloseMicroBlogs { get; set; } = new List<MM.CloseMicroBlog>();
        public IList<MM.ModNotifyStatus> ModNotifyStatuss { get; set; } = new List<MM.ModNotifyStatus>();
        public IList<MM.ModChatRoomMember> ModChatRoomMembers { get; set; } = new List<MM.ModChatRoomMember>();
        public IList<MM.QuitChatRoom> QuitChatRooms { get; set; } = new List<MM.QuitChatRoom>();
        public IList<MM.ModUserDomainEmail> ModUserDomainEmails { get; set; } = new List<MM.ModUserDomainEmail>();
        public IList<MM.DelUserDomainEmail> DelUserDomainEmails { get; set; } = new List<MM.DelUserDomainEmail>();
        public IList<MM.ModChatRoomNotify> ModChatRoomNotifys { get; set; } = new List<MM.ModChatRoomNotify>();
        public IList<MM.PossibleFriend> PossibleFriends { get; set; } = new List<MM.PossibleFriend>();
        public IList<MM.InviteFriendOpen> InviteFriendOpens { get; set; } = new List<MM.InviteFriendOpen>();
        public IList<MM.FunctionSwitch> FunctionSwitchs { get; set; } = new List<MM.FunctionSwitch>();
        public IList<MM.PSMStat> PSMStats { get; set; } = new List<MM.PSMStat>();
        public IList<MM.ModChatRoomTopic> ModChatRoomTopics { get; set; } = new List<MM.ModChatRoomTopic>();
        public IList<micromsg.ModDisturbSetting> ModDisturbSettings { get; set; } = new List<micromsg.ModDisturbSetting>();
        public IList<micromsg.ModBottleContact> ModBottleContacts { get; set; } = new List<micromsg.ModBottleContact>();

        public IList<micromsg.DelBottleContact> DelBottleContacts { get; set; } = new List<micromsg.DelBottleContact>();

        public IList<micromsg.ModUserImg> ModUserImgs { get; set; } = new List<micromsg.ModUserImg>();
        public IList<micromsg.ModDisturbSetting> ModDisturbSetting { get; set; } = new List<micromsg.ModDisturbSetting>();
        public IList<micromsg.KVStatItem> KVStatItems { get; set; } = new List<micromsg.KVStatItem>();

        public IList<micromsg.UserInfoExt> UserInfoExts { get; set; } = new List<micromsg.UserInfoExt>();

        public IList<micromsg.ModBrandSetting> ModBrandSettings { get; set; } = new List<micromsg.ModBrandSetting>();
        public IList<micromsg.ModChatRoomMemberDisplayName> ModChatRoomMemberDisplayNames { get; set; } = new List<micromsg.ModChatRoomMemberDisplayName>();
        public IList<micromsg.ModChatRoomMemberFlag> ModChatRoomMemberFlags { get; set; } = new List<micromsg.ModChatRoomMemberFlag>();

        public IList<micromsg.WebWxFunctionSwitch> WebWxFunctionSwitchs { get; set; } = new List<micromsg.WebWxFunctionSwitch>();

        public IList<micromsg.ModSnsBlackList> ModSnsBlackLists { get; set; } = new List<micromsg.ModSnsBlackList>();
        public IList<micromsg.NewDelMsg> NewDelMsgs { get; set; } = new List<micromsg.NewDelMsg>();
        public IList<micromsg.ModDescription> ModDescriptions { get; set; } = new List<micromsg.ModDescription>();

        public IList<micromsg.KVCmd> KVCmds { get; set; } = new List<micromsg.KVCmd>();

        public IList<micromsg.DeleteSnsOldGroup> DeleteSnsOldGroups { get; set; } = new List<micromsg.DeleteSnsOldGroup>();
         
    }
}
