using Lagrange.Core.Internal.Event.Message;
using Lagrange.Core.Internal.Event.System;
using Lagrange.Core.Internal.Packets.Service.Highway;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context.Uploader;

[HighwayUploader(typeof(ImageEntity))]
internal class ImageUploader : IHighwayUploader
{
    public async Task UploadPrivate(ContextCollection context, MessageChain chain, IMessageEntity entity)
    {
        if (entity is ImageEntity { ImageStream: not null } image)
        {
            var uploadEvent = ImageUploadEvent.Create(image, chain.FriendInfo?.Uid ?? "");
            var uploadResult = await context.Business.SendEvent(uploadEvent);
            var metaResult = (ImageUploadEvent)uploadResult[0];

            var hwUrlEvent = HighwayUrlEvent.Create();
            var highwayUrlResult = await context.Business.SendEvent(hwUrlEvent);
            var ticketResult = (HighwayUrlEvent)highwayUrlResult[0];
            
            var index = metaResult.MsgInfo.MsgInfoBody[0].Index;
            var extend = new NTV2RichMediaHighwayExt
            {
                FileUuid = index.FileUuid,
                UKey = metaResult.UKey,
                Network = Common.Convert(metaResult.Network),
                MsgInfoBody = metaResult.MsgInfo.MsgInfoBody,
                BlockSize = 1024 * 1024,
                Hash = new NTHighwayHash { FileSha1 = index.Info.FileSha1.UnHex() }
            };
            var extStream = extend.Serialize();
            
            bool hwSuccess = await context.Highway.UploadSrcByStreamAsync(1003, image.ImageStream, ticketResult.SigSession, index.Info.FileHash.UnHex(), extStream.ToArray());
            if (!hwSuccess) throw new Exception();
            
            image.MsgInfo = metaResult.MsgInfo;  // directly constructed by Tencent's BDH Server
            image.Compat = metaResult.Compat;  // for legacy QQ
            await image.ImageStream.DisposeAsync();
        }
    }

    public async Task UploadGroup(ContextCollection context, MessageChain chain, IMessageEntity entity)
    {
        if (entity is ImageEntity { ImageStream: not null } image)
        {
            var uploadEvent = ImageGroupUploadEvent.Create(image, chain.GroupUin ?? 0);
            var uploadResult = await context.Business.SendEvent(uploadEvent);
            var metaResult = (ImageGroupUploadEvent)uploadResult[0];

            var hwUrlEvent = HighwayUrlEvent.Create();
            var highwayUrlResult = await context.Business.SendEvent(hwUrlEvent);
            var ticketResult = (HighwayUrlEvent)highwayUrlResult[0];
            
            var index = metaResult.MsgInfo.MsgInfoBody[0].Index;
            var extend = new NTV2RichMediaHighwayExt
            {
                FileUuid = index.FileUuid,
                UKey = metaResult.UKey,
                Network = Common.Convert(metaResult.Network),
                MsgInfoBody = metaResult.MsgInfo.MsgInfoBody,
                BlockSize = 1024 * 1024,
                Hash = new NTHighwayHash { FileSha1 = index.Info.FileSha1.UnHex() }
            };
            var extStream = extend.Serialize();
            
            bool hwSuccess = await context.Highway.UploadSrcByStreamAsync(1004, image.ImageStream, ticketResult.SigSession, index.Info.FileHash.UnHex(), extStream.ToArray());
            if (!hwSuccess) throw new Exception();
            
            image.MsgInfo = metaResult.MsgInfo;  // directly constructed by Tencent's BDH Server
            image.Compat = metaResult.Compat;  // for legacy QQ
            await image.ImageStream.DisposeAsync();
        }
    }
}