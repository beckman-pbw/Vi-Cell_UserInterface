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
    public class RetrieveAnnotatedImage : ApiCommand<uuidDLL, uuidDLL, string>, IInvocationResult<ImageDto>
    {
        private IntPtr _imageWrapperPtr = IntPtr.Zero;

        public Tuple<ImageDto> Results { get; private set; }

        public RetrieveAnnotatedImage(uuidDLL resultId, uuidDLL brightFieldId,
            string saveAsFilename = "")
        {
            ManagesMemory = true;
            Arguments = new Tuple<uuidDLL, uuidDLL, string>(resultId, brightFieldId, saveAsFilename);
        }

        protected override void InvokeInternal()
        {
            IntPtr imgPtr = IntPtr.Zero;
            Result = HawkeyeCoreAPI.Sample.RetrieveAnnotatedImageAPI(Arguments.Item1, Arguments.Item2, out imgPtr);
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