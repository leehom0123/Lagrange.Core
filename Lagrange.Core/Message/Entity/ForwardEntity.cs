using Lagrange.Core.Internal.Packets.Message.Element;
using Lagrange.Core.Internal.Packets.Message.Element.Implementation;
using ProtoBuf;

namespace Lagrange.Core.Message.Entity;

[MessageElement(typeof(SrcMsg))]
public class ForwardEntity : IMessageEntity
{
    public uint Sequence { get; internal set; }
    
    public string? Uid { get; internal set; }

    public uint TargetUin { get; internal set; }

    internal List<Elem> Elements { get; }

    private string? SelfUin { get; set; }
    
    public ForwardEntity()
    {
        Sequence = 0;
        Uid = null;
        Elements = new List<Elem>();
    }

    public ForwardEntity(MessageChain chain)
    {
        Sequence = chain.Sequence;
        Uid = chain.Uid;
        Elements = chain.Elements;
    }
    
    IEnumerable<Elem> IMessageEntity.PackElement()
    {
        var reserve = new SrcMsg.Preserve
        {
            Field3 = Random.Shared.Next(0, int.MaxValue),
            ReceiverUid = Uid,
            SenderUid = SelfUin,
            Field8 = Random.Shared.Next(0, 10000)
        };
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, reserve);
        
        return new Elem[]
        {
            new()
            {
                SrcMsg = new SrcMsg
                {
                    OrigSeqs = new List<uint> { Sequence },
                    SenderUin = 0,
                    Time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Elems = Elements,
                    PbReserve = stream.ToArray(),
                    ToUin = 0
                }
            }
        };
    }
    
    IMessageEntity? IMessageEntity.UnpackElement(Elem elems)
    {
        if (elems.SrcMsg is { } srcMsg)
        {
            return new ForwardEntity
            {
                Sequence = srcMsg.OrigSeqs?[0] ?? 0,
                TargetUin = (uint)srcMsg.SenderUin,
            };
        }

        return null;
    }

    public void SetSelfUid(string selfUid) => SelfUin = selfUid;

    public string ToPreviewString() => $"[Forward]: Sequence: {Sequence}";
}