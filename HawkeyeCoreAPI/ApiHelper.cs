using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScoutDomains.EnhancedSampleWorkflow;

namespace HawkeyeCoreAPI
{
    public static class ApiHelper
    {
        public static void FreeMemory(List<IntPtr> memPointers)
        {
            foreach (var memPtr in memPointers) Marshal.FreeCoTaskMem(memPtr);
        }

        public static void AllocateSampleSetMemory(SampleSetDomain sampleSetDomain,
            out ScoutUtilities.Structs.SampleSet ssStruct, out List<IntPtr> memoryToBeFreed)
        {
            memoryToBeFreed = new List<IntPtr>();

            var sampleSize = Marshal.SizeOf(typeof(ScoutUtilities.Structs.SampleDefinition));
            var sampleSetSize = Marshal.SizeOf(typeof(ScoutUtilities.Structs.SampleSet));

            ssStruct = sampleSetDomain.GetSampleSetStruct();
            var sampleSetPtr = Marshal.AllocCoTaskMem(sampleSetSize);
            ssStruct.samples = Marshal.AllocCoTaskMem(ssStruct.numSamples * sampleSize);

            for (var j = 0; j < sampleSetDomain.Samples.Count; j++)
            {
                var sample = sampleSetDomain.Samples[j];
                var sampleStruct = sample.GetSampleDefinitionStruct();

                var samplePtr = ssStruct.samples + (j * sampleSize);
                Marshal.StructureToPtr(sampleStruct, samplePtr, false);
            }

            memoryToBeFreed.Add(sampleSetPtr);
            memoryToBeFreed.Add(ssStruct.samples);
        }

        public static void AllocateWorkListMemory(WorkListDomain workListDomain,
            out ScoutUtilities.Structs.WorkList wlStruct, out List<IntPtr> memoryToBeFreed)
        {
            memoryToBeFreed = new List<IntPtr>();

            var sampleSize = Marshal.SizeOf(typeof(ScoutUtilities.Structs.SampleDefinition));
            var sampleSetSize = Marshal.SizeOf(typeof(ScoutUtilities.Structs.SampleSet));
            var workListSize = Marshal.SizeOf(typeof(ScoutUtilities.Structs.WorkList));

            wlStruct = workListDomain.GetWorkListStruct();
            var wlPtr = Marshal.AllocCoTaskMem(workListSize);
            wlStruct.sampleSets = Marshal.AllocCoTaskMem(wlStruct.numSampleSets * sampleSetSize);

            if (workListDomain.SampleSets.Count < 1)
            {
                memoryToBeFreed.Add(wlPtr);
                memoryToBeFreed.Add(wlStruct.sampleSets);
                return;
            }

            for (var i = 0; i < workListDomain.SampleSets.Count; i++)
            {
                var set = workListDomain.SampleSets[i];
                var sampleSetStruct = set.GetSampleSetStruct();

                sampleSetStruct.samples = Marshal.AllocCoTaskMem(sampleSetStruct.numSamples * sampleSize);
                var sampleSetPtr = wlStruct.sampleSets + (i * sampleSetSize);

                Marshal.StructureToPtr(sampleSetStruct, sampleSetPtr, false);

                for (var j = 0; j < set.Samples.Count; j++)
                {
                    var sample = set.Samples[j];
                    var sampleStruct = sample.GetSampleDefinitionStruct();

                    var samplePtr = sampleSetStruct.samples + (j * sampleSize);
                    Marshal.StructureToPtr(sampleStruct, samplePtr, false);
                }

                memoryToBeFreed.Add(sampleSetStruct.samples);
            }

            memoryToBeFreed.Add(wlPtr);
            memoryToBeFreed.Add(wlStruct.sampleSets);
        }
    }
}