using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Internal;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Ninject.Extensions.Logging;
using ScoutUtilities.JobManagement;
using ScoutUtilities.Common;

namespace ScoutServices.Watchdog
{
    public class ServerWatchdog : Disposable, IWatchdog
    {
        private const string ParentPid = "PARENT_PID";
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, string> _processCache;
        private Timer _pollTimer;
        private readonly int _pollingInterval;
        private readonly List<string> _servers;
        private Job _job;

        public ServerWatchdog(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            _processCache = new ConcurrentDictionary<string, string>();

            try
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../OpcUaServer/WatchdogConfiguration.json");
                var model = JsonConvert.DeserializeObject<WatchDogConfigModel>(File.ReadAllText(jsonPath));
                int.TryParse(model.PollingIntervalMS, out _pollingInterval);
                _servers = model.Servers;
                _job = new Job();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"WATCHDOG :: Exception in constructor of ServerWatchdog.");
            }
        }

        protected override void DisposeUnmanaged()
        {
            _job.Dispose();
            base.DisposeUnmanaged();
        }

        private void StartProcess(string fullExePath)
        {
            var pathDirectory = Path.GetDirectoryName(fullExePath);
            if (null == pathDirectory)
            {
                _logger.Error("Unable to get path of OPC/UA server executable.");
                return;
            }

            var processName = Path.GetFileNameWithoutExtension(fullExePath);

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = pathDirectory, FileName = $"{processName}.exe"
                };
                var childProcess = Process.Start(startInfo);
                if (null != childProcess)
                {
                    _job.AddProcess(childProcess.Handle);
                    _logger.Info($"WATCHDOG :: Process '{processName}' started and associated with the Scout process.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"WATCHDOG :: Exception starting {processName} at {fullExePath}.");
            }
        }
        
        public void Start()
        {
            _pollTimer = new Timer(_ => PollElapsed(), null, _pollingInterval, Timeout.Infinite);
        }

        private void PollElapsed()
        {
            if (_pollTimer == null)
                return;

            foreach (var kvp in _processCache)
            {
                var name = kvp.Key;
                var serverProcesses = Process.GetProcessesByName(name);
                var numProcesses = serverProcesses.Length;
                if (numProcesses > 1)
                {
                    // This should not happen, however if a process was slow to start
                    // and the watchdog started a second, this code block will attempt to
                    // clean up the state. Only one OPC server will be able to listen
                    // on a port. The rest would potentially run, but not service requests.
                    if (!KillListOfServers(serverProcesses))
                    {
                        _logger.Error($"Multiple {name} servers were running and the attempt to kill them failed.");
                    }

                    numProcesses = Process.GetProcessesByName(name).Length;
                }

                if (numProcesses == 0)
                {
                    _logger.Info($"WATCHDOG:: process '{name}' is not running, restarting...");
                    StartProcess(kvp.Value);
                }
            }

            _pollTimer?.Change(_pollingInterval, Timeout.Infinite);
        }

        public void Stop(bool clearWatchList = false)
        {
            if(clearWatchList)
                ClearAllWatches();

            _pollTimer = null;
        }

        public void AddWatch(string fullPathToExeWithExtension)
        {
            if (!File.Exists(fullPathToExeWithExtension))
            {
                _logger.Warn($"WATCHDOG :: Unable to add watch for process. Path does not exist. Path: {fullPathToExeWithExtension}");
                return;
            }

            var processName = Path.GetFileNameWithoutExtension(fullPathToExeWithExtension);
            _processCache[processName] = fullPathToExeWithExtension;
        }

        public void RemoveWatch(string processName)
        {
            _processCache.TryRemove(processName, out _);
        }

        public void ClearAllWatches()
        {
            _processCache.Clear();
            KillServers();
        }

        private void KillServers()
        {
            foreach (var server in _servers)
            {
                var name = Path.GetFileNameWithoutExtension(server);
                var processes = Process.GetProcessesByName(name);
                var allDead = KillListOfServers(processes);
                if (!allDead)
                {
                    _logger.Warn($"Failed to kill all server processes with {name} name.");
                }
            }
        }

        private static bool KillListOfServers(Process[] processes)
        {
            var allDead = true;
            foreach (var process in processes)
            {
                // ToDo: Determine: Is this Kill permitted in the security model of the ViCellBlu OS? Is it successfully terminating the process?
                process.Kill();
                allDead &= process.WaitForExit(30000);
            }

            return allDead;
        }

        public void AddAllWatches()
        {
            foreach (var server in _servers)
            {
                StopAlreadyRunningServers(server);
                AddWatch(server);
            }
        }

        private void StopAlreadyRunningServers(string name)
        {
            var serverProcesses = Process.GetProcessesByName(name);
            var numProcesses = serverProcesses.Length;
            if (numProcesses > 0)
            {
                if (!KillListOfServers(serverProcesses))
                {
                    _logger.Error($"One or more {name} servers were running and the attempt to kill them all failed.");
                }
            }
        }
    }
}
