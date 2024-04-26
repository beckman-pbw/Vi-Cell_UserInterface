using GrpcService;
using System;
using System.Collections.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutServices.DTOs;
using ScoutUtilities.Enums;

namespace ScoutServices.Interfaces
{
    public interface IConfigurationManager
    {
        IObservable<ConfigSubjectDto> SubscribeStateChanges();
        IObservable<ulong> SubscribeAvailableDiskSpace();
        byte[] ExportConfig(string username, string password, RequestExportConfig request);
        HawkeyeError ImportConfig(string username, string password, RequestImportConfig request);
        bool GetDiskSpaceVariables(out double total_size, out double total_free, out double other_size, out double data_size, out double export_size);
        void PublishConfigStatus(ConfigSubjectDto result);
    }
}
