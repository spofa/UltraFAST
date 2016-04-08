using beRemote.Core.ProtocolSystem.ProtocolBase;
using beRemote.Core.ProtocolSystem.ProtocolBase.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beeVNC.beRemoteProtocol
{
    [ProtocolMetadata(
        PluginName="beeVNC",
        PluginDescription = "beeVNC",
        PluginFullQualifiedName = "beeVNC.beRemoteProtocol.VNCProtocol",
        PluginConfigFolder = "beeVNC.beRemoteProtocol",
        PluginIniFile = "plugin.ini",
        ProtocolAuthIsHandled=false,
        PluginVersionCode = 1)]
    [Export(typeof(Protocol))]
    public class VNCProtocol : Protocol
    {
        public override beRemote.Core.ProtocolSystem.ProtocolBase.Types.ServerType[] GetPrtocolCompatibleServers()
        {
            return new ServerType[] { ServerType.LINUX, ServerType.WINDOWS };
        }

        public override Session NewSession(beRemote.Core.ProtocolSystem.ProtocolBase.Interfaces.IServer server, long connSettingsID)
        {
            return new VNCSession(server, this, connSettingsID);
        }
    }
}
