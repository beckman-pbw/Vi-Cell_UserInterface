using ScoutServices.Interfaces;
using System;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace ScoutServices
{
    public class OpcUaCfgManager : IOpcUaCfgManager
    {
        private readonly IConfiguration _configuration;

        #region Fields

        private XmlDocument _opcUaServerDoc = new XmlDocument();
        private string _opcUaCfgFname;
        private const string OpcUaCfgNodePrefix = "opc.tcp://localhost:";
        private const string OpcUaCfgNodeSuffix = "/ViCellBlu/Server";
        private const string OpcUaCfgCfgNamespace = "http://opcfoundation.org/UA/SDK/Configuration.xsd";
        private const string OpcUaCfgAddressesNode = "descendant::sv:ServerConfiguration/sv:BaseAddresses";

        #endregion

        public OpcUaCfgManager(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize();
        }

        private void Initialize()
        {
            _opcUaCfgFname = _configuration["opcua:server:configlocation"];
            if (!File.Exists(_opcUaCfgFname))
            {
                throw new FileNotFoundException(_opcUaCfgFname);
            }
            _opcUaServerDoc = new XmlDocument();
            _opcUaServerDoc.PreserveWhitespace = true;
            _opcUaServerDoc.Load(_opcUaCfgFname);
        }

        #region Methods

        // Get or set the port value in the OpcUa Server config file
        // If new port number specified, sets this in the config file. Else returns the current cfg port number
        public UInt32 GetOrSetOpcUaPort(UInt32 newPort = 0)
        {
            UInt32 cfgPort = newPort;

            XmlNode root = _opcUaServerDoc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(_opcUaServerDoc.NameTable);
            nsmgr.AddNamespace("sv", OpcUaCfgCfgNamespace);
            XmlNode addressesNode = root.SelectSingleNode(OpcUaCfgAddressesNode, nsmgr);
            XmlNodeList addresses = addressesNode.ChildNodes;
            // Find node that contains the OpcUa server port number
            for (int i = 0; i < addresses.Count; i++)
            {
                var serverNode = addresses[i].InnerXml;
                if (serverNode.Contains(OpcUaCfgNodePrefix))
                {
                    // If new port not specified, get the current port number. Else set the specified port number in the config
                    if (cfgPort == 0)
                    {
                        var strList = serverNode.Split(new string[] { OpcUaCfgNodePrefix }, StringSplitOptions.None);
                        if (strList.Length > 1)
                        {
                            var rightSide = strList[1];
                            strList = rightSide.Split(new string[] {OpcUaCfgNodeSuffix}, StringSplitOptions.None);
                            var portStr = strList[0];
                            UInt32.TryParse(portStr, out cfgPort);
                        }
                    }
                    else
                    {
                        // Make a backup of the OpcUa server config file
                        var opcUaCfgBackupFname = _opcUaCfgFname + ".bak";
                        File.Copy(_opcUaCfgFname, opcUaCfgBackupFname, true);
                        // Save the config file with the new port number
                        var portStr = cfgPort.ToString();
                        addresses[i].InnerXml = OpcUaCfgNodePrefix + portStr + OpcUaCfgNodeSuffix;
                        _opcUaServerDoc.Save(_opcUaCfgFname);
                    }
                    break;
                }
            }

            return cfgPort;
        }

        #endregion
    }
}
