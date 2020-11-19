﻿// This file is automatic generated by script.  
// DO NOT EDIT DIRECTLY.   

using System;
using Konata.Utils.Protobuf;

namespace Konata.Model.Package.Oidb.OidbModel
{
    public abstract class OidbCmd0x899 : OidbSSOPkg
    {
        internal OidbCmd0x899(uint svcType, uint? svcResult, ReqBody reqBody)

            : base(0x899, svcType, svcResult, (ProtoTreeRoot root) =>
            {
                root.AddTree(reqBody.BuildTree());
            })
        {

        }

        public class ReqBody : OidbStruct
        {
            // 0x08
            public long? group_code;

            // 0x10
            public long? start_uin;

            // 0x18
            public uint? identify_flag;

            // 0x20
            public uint? uin_list;

            // 0x2A
            public MemberList memberlist;

            // 0x30
            public uint? member_num;

            // 0x38
            public uint? filter_method;

            // 0x40
            public uint? online_flag;

            public override void Write(ProtoTreeRoot root)
            {
                root.AddLeafVar("08", group_code);
                root.AddLeafVar("10", start_uin);
                root.AddLeafVar("18", identify_flag);
                root.AddLeafVar("20", uin_list);
                root.AddTree("2A", memberlist?.BuildTree());
                root.AddLeafVar("30", member_num);
                root.AddLeafVar("38", filter_method);
                root.AddLeafVar("40", online_flag);
            }
        }

        public class RspBody : OidbStruct
        {
            // 0x08
            public long? group_code;

            // 0x10
            public long? start_uin;

            // 0x18
            public uint? identify_flag;

            // 0x22
            public MemberList memberlist;

            // 0x2A
            public string errorinfo;

            public override void Write(ProtoTreeRoot root)
            {
                root.AddLeafVar("08", group_code);
                root.AddLeafVar("10", start_uin);
                root.AddLeafVar("18", identify_flag);
                root.AddTree("22", memberlist?.BuildTree());
                root.AddLeafString("2A", errorinfo);
            }
        }

        public class MemberList : OidbStruct
        {
            // 0x08
            public long? member_uin;

            // 0x10
            public uint? uin_flag;

            // 0x18
            public uint? uin_flagex;

            // 0x20
            public uint? uin_mobile_flag;

            // 0x28
            public uint? uin_arch_flag;

            // 0x30
            public uint? join_time;

            // 0x38
            public uint? old_msg_seq;

            // 0x40
            public uint? new_msg_seq;

            // 0x48
            public uint? last_speak_time;

            // 0x50
            public uint? level;

            // 0x58
            public uint? point;

            // 0x60
            public uint? shutup_timestap;

            // 0x68
            public uint? flagex2;

            // 0x72
            public byte[] special_title;

            // 0x78
            public uint? special_title_expire_time;

            // 0x8001
            public uint? active_day;

            // 0x8A01
            public byte[] uin_key;

            // 0x9001
            public uint? privilege;

            // 0x9A01
            public byte[] rich_info;

            public override void Write(ProtoTreeRoot root)
            {
                root.AddLeafVar("08", member_uin);
                root.AddLeafVar("10", uin_flag);
                root.AddLeafVar("18", uin_flagex);
                root.AddLeafVar("20", uin_mobile_flag);
                root.AddLeafVar("28", uin_arch_flag);
                root.AddLeafVar("30", join_time);
                root.AddLeafVar("38", old_msg_seq);
                root.AddLeafVar("40", new_msg_seq);
                root.AddLeafVar("48", last_speak_time);
                root.AddLeafVar("50", level);
                root.AddLeafVar("58", point);
                root.AddLeafVar("60", shutup_timestap);
                root.AddLeafVar("68", flagex2);
                root.AddLeafBytes("72", special_title);
                root.AddLeafVar("78", special_title_expire_time);
                root.AddLeafVar("8001", active_day);
                root.AddLeafBytes("8A01", uin_key);
                root.AddLeafVar("9001", privilege);
                root.AddLeafBytes("9A01", rich_info);
            }
        }

        public class uin_key : OidbStruct
        {
            // 0x08
            public long? group_code;

            // 0x10
            public long? member_uin;

            // 0x18
            public long? gen_time;

            // 0x20
            public uint? valid_time;

            // 0x28
            public uint? rand_num;

            public override void Write(ProtoTreeRoot root)
            {
                root.AddLeafVar("08", group_code);
                root.AddLeafVar("10", member_uin);
                root.AddLeafVar("18", gen_time);
                root.AddLeafVar("20", valid_time);
                root.AddLeafVar("28", rand_num);
            }
        }
    }
}