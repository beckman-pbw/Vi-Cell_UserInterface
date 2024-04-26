using ApiProxies.Extensions;
using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ApiProxies.Misc;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ApiProxies.Commands.QueueManagement
{
    public class RetrieveBlackwhiteImage : ApiCommand<uuidDLL, string>, IInvocationResult<ImageDto>
    {
        private IntPtr _imageWrapperPtr = IntPtr.Zero;

        public Tuple<ImageDto> Results { get; private set; }

        public RetrieveBlackwhiteImage(uuidDLL resultId, string saveAsFilename = "")
        {
            ManagesMemory = true;
            Arguments = new Tuple<uuidDLL, string>(resultId, saveAsFilename);
        }

        protected override void InvokeInternal()
        {
            IntPtr imgPtr = IntPtr.Zero;
            Result = HawkeyeCoreAPI.Sample.RetrieveBWImageAPI(Arguments.Item1, out imgPtr);
            if (Result.Equals(HawkeyeError.eSuccess))
            {
                Results = new Tuple<ImageDto>(imgPtr.MarshalToImageDto());
                _imageWrapperPtr = imgPtr;
            }           
        }

        protected override void FreeAdditionalUnmanagedMemory()
        {
            HawkeyeCoreAPI.Sample.FreeImageWrapperAPI(_imageWrapperPtr);
        }
    }
}