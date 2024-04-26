using ApiProxies.Extensions;
using ApiProxies.Generic;
using ScoutDomains.DataTransferObjects;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ApiProxies.Commands.QueueManagement
{
    public class RetrieveImage : ApiCommand<uuidDLL, string>, IInvocationResult<ImageDto>
    {
        private IntPtr _imageWrapperPtr = IntPtr.Zero;

        public Tuple<ImageDto> Results { get; private set; }

        public RetrieveImage(uuidDLL id, string saveAsFilename = "")
        {
            ManagesMemory = true;
            Arguments = new Tuple<uuidDLL, string>(id, saveAsFilename);
        }

        protected override void InvokeInternal()
        {
            IntPtr imgPtr = IntPtr.Zero;
            Result = HawkeyeCoreAPI.Sample.RetrieveImageAPI(Arguments.Item1, out imgPtr);
            if (Result.Equals(HawkeyeError.eSuccess))
            {
                var imageDataDto = imgPtr.MarshalToImageDto();
                Results = new Tuple<ImageDto>(imageDataDto);
                _imageWrapperPtr = imgPtr;
            }
        }

        protected override void FreeAdditionalUnmanagedMemory()
        {
            HawkeyeCoreAPI.Sample.FreeImageWrapperAPI(_imageWrapperPtr);
        }
    }
}