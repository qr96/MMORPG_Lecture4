using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class PlayerInfoReq
{
    public long playerId;
    public string name;
    public class Skill
    {
        public int id;
        public short level;
        public float duration;

        public void Read(ArraySegment<byte> segment, ref ushort count)
        {
            this.id = BitConverter.ToInt32(segment.Array, segment.Offset + count);
            count += sizeof(int);
            this.level = BitConverter.ToInt16(segment.Array, segment.Offset + count);
            count += sizeof(short);
            this.duration = BitConverter.ToSingle(segment.Array, segment.Offset + count);
            count += sizeof(float);
        }

        public bool Write(ArraySegment<byte> segment, ref ushort count)
        {
            bool success = true;
            Array.Copy(BitConverter.GetBytes(this.id), 0, segment.Array, segment.Offset + count, sizeof(int));
            count += sizeof(int);
            Array.Copy(BitConverter.GetBytes(this.level), 0, segment.Array, segment.Offset + count, sizeof(short));
            count += sizeof(short);
            Array.Copy(BitConverter.GetBytes(this.duration), 0, segment.Array, segment.Offset + count, sizeof(float));
            count += sizeof(float);
            return success;
        }
    }
    public List<Skill> skills = new List<Skill>();

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt64(segment.Array, segment.Offset + count);
        count += sizeof(long);
        ushort nameLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, nameLen);
        count += nameLen;
        this.skills.Clear();
        ushort skillLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
        count += sizeof(ushort);
        for (int i = 0; i < skillLen; i++)
        {
            Skill skill = new Skill();
            skill.Read(segment, ref count);
            skills.Add(skill);
        }
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(long));
        count += sizeof(long);
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        Array.Copy(BitConverter.GetBytes(nameLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        count += nameLen;
        Array.Copy(BitConverter.GetBytes((ushort)this.skills.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        foreach (Skill skill in this.skills)
            skill.Write(segment, ref count);
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}


public enum PacketID
{
    PlayerInfoReq = 1,
    PlayerInfoOk = 2,
}
