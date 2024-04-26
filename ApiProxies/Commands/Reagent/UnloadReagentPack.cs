using System;
using System.Runtime.InteropServices;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ApiProxies.Commands.Reagent
{
    public class UnloadReagentPack : ApiCommand<IntPtr, UInt16, IApiCallback<reagent_unload_status_callback>,
        IApiCallback<reagent_unload_complete_callback>>
    {
        public UnloadReagentPack(ReagentUnloadOption unloadOption)
        {
            ManagesMemory = true;

            var containerOption = new ReagentContainerUnloadOption();
            containerOption.container_id = new byte[8];
            containerOption.container_id[0] = 1;
            containerOption.container_action = unloadOption;

            // Make sure to track unmanaged memory that is allocated here.  Clean-up is automatic.
            IntPtr containerOptionPtr = AllocateAndTrack(Marshal.SizeOf(containerOption));
            Marshal.StructureToPtr(containerOption, containerOptionPtr, false);

            Arguments =
                new Tuple<IntPtr, UInt16, IApiCallback<reagent_unload_status_callback>,
                    IApiCallback<reagent_unload_complete_callback>>(
                    containerOptionPtr,
                    1,
                    // Get api callback instances to be supplied as arguments to the API call.
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Reagent_Unload_Status) as
                        IApiCallback<reagent_unload_status_callback>,
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Reagent_Unload_Complete) as
                        IApiCallback<reagent_unload_complete_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.UnloadReagentPackAPI(Arguments.Item1, Arguments.Item2, Arguments.Item3.Callback, Arguments.Item4.Callback);
        }
    }
}