﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RemoteClient.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public double NotSent4LongTime {
            get {
                return ((double)(this["NotSent4LongTime"]));
            }
            set {
                this["NotSent4LongTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("250")]
        public double QueueExpiryTime {
            get {
                return ((double)(this["QueueExpiryTime"]));
            }
            set {
                this["QueueExpiryTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnSendNotSent4LongTiles {
            get {
                return ((bool)(this["EnSendNotSent4LongTiles"]));
            }
            set {
                this["EnSendNotSent4LongTiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnAlwaysSendChangedTiles {
            get {
                return ((bool)(this["EnAlwaysSendChangedTiles"]));
            }
            set {
                this["EnAlwaysSendChangedTiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnAlwaysSendUnchangedTiles {
            get {
                return ((bool)(this["EnAlwaysSendUnchangedTiles"]));
            }
            set {
                this["EnAlwaysSendUnchangedTiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ReliableOrdered")]
        public global::Lidgren.Network.NetDeliveryMethod TileSendingMethod {
            get {
                return ((global::Lidgren.Network.NetDeliveryMethod)(this["TileSendingMethod"]));
            }
            set {
                this["TileSendingMethod"] = value;
            }
        }
    }
}
