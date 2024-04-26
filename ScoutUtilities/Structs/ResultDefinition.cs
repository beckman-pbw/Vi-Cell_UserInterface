using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct uuidDLL : IEquatable<uuidDLL>, ICloneable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] u;

        public uuidDLL(byte[] array)
        {
            u = array;
        }

        public uuidDLL(Guid guid)
        {
            u = guid.ToByteArray();
        }

        public bool IsNull()
        {
            return u == null;
        }

        public bool IsEmpty()
        {
            return IsNull() || u.Length < 1;
        }

        public bool IsNullOrEmpty()
        {
            return IsEmpty() || new Guid(u).Equals(Guid.Empty) || new Guid(u) == Guid.Empty;
        }

        public override string ToString()
        {
            if (u == null) return string.Empty;

            var guid = new Guid(u);
            return guid.ToString().ToUpper();
        }

        public Guid ToGuid()
        {
            if (u == null) return Guid.Empty;

            var guid = new Guid(u);
            return guid;
        }

        public bool Equals(uuidDLL other)
        {
            if (u == null && other.u == null) return true;
            if (u == null && other.u != null) return false;
            if (u != null && other.u == null) return false;
            if (u.Length != other.u.Length) return false;

            return u.SequenceEqual(other.u);
        }

        public override bool Equals(object obj)
        {
            return obj is uuidDLL other && Equals(other);
        }

        public override int GetHashCode()
        {
            return u != null ? u.GetHashCode() : 0;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public unsafe struct uuid
    {
        public fixed byte u[16];
    }


    /* 
    A Characteristic is a particular property measured by the image analysis system.
    Characteristics are identified by a <Key, Subkey, Sub-Subkey> triplet.
       The base Key identifies the root measurement
       The Subkey identifies a particular fluorescent channel
       The Sub-Subkey identifies a sub-measurement within the channel

    All Characteristics have a non-zero value for Key
    Some characteristics may accept a sub-key (otherwise a value of '0' is used)
    A characteristic which uses a sub-key may also accept a sub-sub-key (otherwse a value of '0' is used)

    A caller may request an English text description of a particular characteristic.
    */

    [StructLayout(LayoutKind.Sequential)]
    public struct Characteristic_t
    {
        public UInt16 key;
        public UInt16 s_key;
        public UInt16 s_s_key;
    }

    public enum E_ERRORCODE
    {
        eSuccess = 0, /*<! Process Successfully */
        eInvalidInputPath = 1, /*!< Invalid input path*/
        eValueOutOfRange = 2, /*<! Parameter value out of range */
        eZeroInput = 3, /*<! Zero Input */
        eNullCellsData = 4, /*<! Images having no cells  */
        eInvalidImage = 5, /*<! Invalid Function Called*/
        eResultNotAvailable = 6, /*<!Result not ready*/
        eFileWriteError = 7, /*!< File write error*/
        eZeroOutputData = 8, /*!< Zero output data*/
        eParameterIsNegative = 9, /*!< Negative input parameter*/
        eInvalidParameter = 10, /*!< Invalid Parameter value*/
        eBubbleImage = 11, /*!<Image containing big bubbles*/
        eFLChannelsMissing = 12, /*!< Bright field image set missing FL images*/
        eInvalidCharacteristics = 13, /*!< Invalid characteristics of the blob*/
        eInvalidAlgorithmMode = 14, /*!< Invalid ALgorithm mode*/
        eMoreThanOneFLImageSupplied = 15, /*!<More than One FL Image Supplied*/
        eTransformationMatrixMissing = 16, /*!<TransMatrix not supplied*/
        eFailure = 17, /*!<Indicate Failure of a Process*/
        eInvalidBackgroundIntensity = 18, /*!<Image rejected due to invalid background intensity*/
        eDefault = 19
    }

	public enum QCStatus : UInt16
	{
        [Description("LID_CSV_QC_Fail")] eFail = 0,
        [Description("LID_CSV_QC_Pass")] ePass,
        [Description("LID_CSV_QC_NA")] eNotApplicable,
	}

    public struct BasicResultAnswers
    {
        public E_ERRORCODE eProcessedStatus;
        public UInt32 nTotalCumulative_Imgs;
        public UInt32 count_pop_general;
        public UInt32 count_pop_ofinterest;
        public float concentration_general; // x10^6
        public float concentration_ofinterest; // x10^6
        public float percent_pop_ofinterest;

        /// SECONDARY
        public float avg_diameter_pop;

        public float avg_diameter_ofinterest;
        public float avg_circularity_pop;
        public float avg_circularity_ofinterest;

        /// TERTIARY
        public float coefficient_variance;

        public UInt16 average_cells_per_image;
        public UInt16 average_brightfield_bg_intensity;
        public UInt16 bubble_count;
        public UInt16 large_cluster_count;
    }

    public struct imagesetwrapper_t
    {
        public imagewrapper_t brightfield_image;
        public byte num_fl_channels;
        public IntPtr fl_images; //convert into fl_imagewrapper_t structure
    }

    //TODO: not currently used.
    // This may be used in the future should Hunter ever happen.
    //public struct fl_imagewrapper_t
    //{
    //    UInt16 fl_channel;
    //    imagewrapper_t fl_image;
    //}

    public struct ResultSummary
    {
        public uuidDLL uuid;
        public IntPtr user_id;
        public UInt64 time_stamp;

        // Copy of analysis settings at the time of creation
        public CellType cell_type_settings;
        public AnalysisDefinition analysis_settings; // use for labeling population of interest ("Viable", "Apoptotic"...)

        public BasicResultAnswers cumulative_result;

        // Signature Set
        public UInt16 num_signatures;
        public IntPtr signature_set;

        public QCStatus qcStatus;
    }

    public struct ResultRecord
    {
        public ResultSummary summary_info;

        public UInt16 num_image_results;
        public IntPtr per_image_result;

        //MASSIVE DATA STORM --- upon reqest??
    }

    public struct ImageRecord
    {
        public uuidDLL uuid;
        public IntPtr user_id;
        public UInt64 time_stamp;
    }

    public struct SampleImageSetRecord
    {
        public uuidDLL uuid;
        public IntPtr userId;
        public UInt64 timeStamp;
        public UInt16 sequenceNumber;
        public uuidDLL brightfieldImage;
        public byte numFlChannels;
        public IntPtr flChannelNumbers;
        public IntPtr flImages;
    }

    public struct SampleRecord
    {
        public uuidDLL uuid;
        public IntPtr user_id;
        public UInt64 time_stamp;
        public IntPtr sample_identifier;
        public IntPtr bp_qc_identifier;
        public UInt16 dilution_factor;
        public SamplePostWash wash;
        public IntPtr comment;
        public UInt16 num_reagent_records;
        public IntPtr reagent_info_records;
        public UInt16 num_image_sets;

        //uint16_t* image_set_sequence_numbers; Commented based on discussion
        public IntPtr image_sets;
        public UInt16 num_result_summary;
        public IntPtr result_summaries;
        public sample_completion_status sam_comp_status;
        public SamplePosition position;
    }

    public struct ReagentInfoRecord
    {
        public IntPtr pack_number;
        public UInt32 lot_number;
        public IntPtr reagent_label;
        public UInt64 expiration_date;
        public UInt64 in_service_date;
        public UInt64 effective_expiration_date;
    }

    public struct WorkQueueRecord
    {
        public uuidDLL uuid;
        public IntPtr userId;
        public IntPtr wqLabel;
        public UInt64 timeStamp;
        public UInt16 numSampleRecords;
        public IntPtr sampleRecords;
    }

    public struct DataSignature_t
    {
        public IntPtr short_text;
        public IntPtr long_text;
    }

    public struct DataSignatureInstance_t
    {
        public DataSignature_t signature;
        public IntPtr signing_user; /// user_id copy
        public UInt64 timestamp; /// seconds from epoch UTC
    }
}