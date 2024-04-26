using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static partial class Signature
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeDataSignature(IntPtr signatures, UInt16 num_signatures);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeDataSignatureInstance(IntPtr signatures, UInt16 num_signatures);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RemoveSignatureDefinition(string signatureShortText, UInt16 shortTextLen);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AddSignatureDefinition(IntPtr signature);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveSignatureDefinitions(out IntPtr signatures, out UInt16 num_signatures);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError SignResultRecord(uuidDLL record_id, string signature_short_text, UInt16 short_text_len);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RemoveSignatureDefinitionAPI(string signatureShortText, ushort shortTextLen) //done
        {
            return RemoveSignatureDefinition(signatureShortText, shortTextLen);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AddSignatureDefinitionAPI(SignatureDomain sign)
        {
            var signStructure = new DataSignature_t()
            {
                short_text = sign.SignatureIndicator.ToIntPtr(),
                long_text = sign.SignatureMeaning.ToIntPtr()
            };
            var wqData = Marshal.AllocCoTaskMem(Marshal.SizeOf(signStructure));
            Marshal.StructureToPtr(signStructure, wqData, false);
            var hawkeyeError = AddSignatureDefinition(wqData);
            Marshal.FreeCoTaskMem(wqData);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveSignatureDefinitionsAPI(ref List<SignatureDomain> signatureDomainListAll, ref ushort num_signatures)
        {
            var signatureDefinitionListAll = new List<DataSignature_t>();
            var signatureDefinitionState = new DataSignature_t();
            var size = Marshal.SizeOf(signatureDefinitionState);
            IntPtr SignatureDefinitions;
            var hawkeyeError = RetrieveSignatureDefinitions(out SignatureDefinitions, out num_signatures);
            var signPtr = SignatureDefinitions;
            for (var i = 0; i < num_signatures; i++)
            {
                signatureDefinitionListAll.Add(
                    (DataSignature_t)Marshal.PtrToStructure(signPtr, typeof(DataSignature_t)));
                signPtr += size;
            }

            if (signatureDefinitionListAll.Count > 0)
            {
                signatureDomainListAll = CreateSignatureList(signatureDefinitionListAll);
            }

            FreeDataSignatureAPI(SignatureDefinitions, num_signatures);

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SignResultRecordAPI(uuidDLL record_id, string signature_short_text, ushort short_text_len)
        {
            return SignResultRecord(record_id, signature_short_text, short_text_len);
        }

        #endregion


        #region Private Methods

        private static List<SignatureDomain> CreateSignatureList(List<DataSignature_t> signatureDefinitionListAll)
        {
            var signList = new List<SignatureDomain>();
            if (signatureDefinitionListAll.Any())
            {
                signatureDefinitionListAll.ForEach(signDefination =>
                {
                    var signatureDomain = new SignatureDomain();
                    signatureDomain.SignatureIndicator = signDefination.short_text.ToSystemString();
                    if (signDefination.long_text != IntPtr.Zero)
                    {
                        signatureDomain.SignatureMeaning = signDefination.long_text.ToSystemString();
                    }
                    signList.Add(signatureDomain);
                });
            }

            return signList;
        }

        private static void FreeDataSignatureAPI(IntPtr signatures, UInt16 num_signatures)
        {
            if (signatures == IntPtr.Zero)
            {
                Log.Warn("FreeDataSignatureAPI: pointer IntPtr.Zero");
            }
            else
            {
                if (num_signatures > 0)
                {
                    FreeDataSignature(signatures, num_signatures);
                }
                else
                {
#if DEBUG
                    Log.Debug("FreeDataSignatureAPI: n_items is zero");
#endif
                }
            }
        }

        private static void FreeDataSignatureInstanceAPI(IntPtr signatures, UInt16 num_signatures)
        {
            if (signatures == IntPtr.Zero)
            {
                Log.Error("FreeDataSignatureInstanceAPI: pointer IntPtr.Zero");
            }
            else
            {
                if (num_signatures > 0)
                {
                    FreeDataSignatureInstance(signatures, num_signatures);
                }
                else
                {
#if DEBUG
                    Log.Debug("FreeDataSignatureInstanceAPI: n_items is zero");
#endif
                }
            }
        }

        #endregion

    }
}
