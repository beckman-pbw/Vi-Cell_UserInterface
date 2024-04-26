using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.IO;
using System.Linq;

namespace ScoutDomains.Reports.ScheduledExports
{
    /// <summary>
    /// The base class for the C# representation of a scheduled export item
    /// </summary>
    public abstract class BaseScheduledExportDomain : BaseNotifyPropertyChanged, ICloneable, IEquatable<BaseScheduledExportDomain>
    {
        
        protected BaseScheduledExportDomain()
        {
            LastRunStatus = ScheduledExportLastRunStatus.NotRun;
            LastRunTime = DateTime.Now.AddMonths(-96);
            LastSuccessRunTime = LastRunTime;
            RecurrenceRule = new RecurrenceRuleDomain();
            DataFilterCriteria = new DataFilterCriteriaDomain();
        }

        public abstract bool IsValid();

        /// <summary>
        /// The distinct id fro the Scheduled Export
        /// </summary>
        public uuidDLL Uuid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// The descriptive name for the scheduled operation
        /// </summary>
        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// Comments regarding this scheduled operation
        /// </summary>
        public string Comments
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// String containing the default filename template (for exports)
        /// </summary>
        public string FilenameTemplate
        {
            get { return GetProperty<string>(); }
            set
            {
                if (value == null)
                    return;

                var invalidChar = Path.GetInvalidFileNameChars();
                var textArray = value.ToCharArray();
                if (!textArray.Any(x => invalidChar.Any(y => y.Equals(x))))
                {
                    SetProperty(value);
                    return; // No invalid chars - don't do anything
                }

                string myStr = "";
                for (int j = 0; j < textArray.Count(); j++)
                {
                    var v = textArray[j];
                    if (!invalidChar.Contains(v))
                        myStr += v;
                }
                SetProperty(myStr);
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// Destination of output; should be mapped drive format, not URL
        /// </summary>
        public string DestinationFolder
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// Boolean for whether the export is enabled or not
        /// </summary>
        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// The email to send notifications to
        /// </summary>
        public string NotificationEmail
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// The status of the last run export
        /// </summary>
        public ScheduledExportLastRunStatus LastRunStatus
        {
            get { return GetProperty<ScheduledExportLastRunStatus>(); }
            set { SetProperty(value); }
        }

        public DateTime LastRunTime
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime LastSuccessRunTime
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// The rules that dictate when a scheduled export should repeat
        /// </summary>
        public RecurrenceRuleDomain RecurrenceRule
        {
            get { return GetProperty<RecurrenceRuleDomain>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// The filter criteria that gets applied to determine what gets exported.
        /// </summary>
        public DataFilterCriteriaDomain DataFilterCriteria
        {
            get { return GetProperty<DataFilterCriteriaDomain>(); }
            set { SetProperty(value); }
        }
        
        public virtual object Clone()
        {
            var cloneObj = (BaseScheduledExportDomain) MemberwiseClone();
            CloneBaseProperties(cloneObj);
            
            cloneObj.Uuid = new uuidDLL(Uuid.ToGuid());
            if (cloneObj.RecurrenceRule != null)
                cloneObj.RecurrenceRule = (RecurrenceRuleDomain) RecurrenceRule.Clone();

            return cloneObj;
        }

        public bool Equals(BaseScheduledExportDomain other)
        {
            return other != null && Uuid.Equals(other.Uuid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((BaseScheduledExportDomain) obj);
        }

        public override int GetHashCode()
        {
            return Uuid.GetHashCode();
        }
    }
}