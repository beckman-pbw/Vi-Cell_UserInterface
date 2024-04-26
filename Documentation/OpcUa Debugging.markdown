# OpcUa Debugging {#opc_ua_debugging}

To debug both the HawkeyeUserInterface and the HawkeyeOpcUa projects you will first need to disable the UserInterface from
starting the OpcUa Server. One way to do this is to edit the file ScoutServices\Watchdog\WatchdogConfiguration.json and remove
the string inside the brackets on the "servers" line so that it looks like this: "servers": [ ]
Then rebuild the UserInterface solution. You can now start debugging both the UserInterface and the OpcUa server. Remember to 
put the OpcUa Server path string ("C:\\Instrument\\OPCUaServer\\ViCellOpcUaServer.exe") back in the brackets when you are done!
