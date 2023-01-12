namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;

    internal class SpeakerMute : PluginDynamicCommand
    {
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        private Int32 muteStatus = 0;
        public SpeakerMute() : base() { }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.IsSpeakerMuted += (sender, e) => this.ActionImageChanged();
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("speakermute");
            if (l7dValues != null)
            {
                this.AddParameter("speakermute", l7dValues["displayName"], l7dValues["groupName"]);
            }
            else
            {
                this._plugin.Log.Info($"SpeakerMute : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter)
        {
            this.muteStatus = 1 - this._plugin.speakerMutedStatus;
            Helper.TS3Handler.SetOutputMuteStatus(this.muteStatus);
            this._plugin.speakerMutedStatus = this.muteStatus;
            this.ActionImageChanged();
        }
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var _muteStatus = Convert.ToBoolean(this._plugin.speakerMutedStatus);
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                if (!String.IsNullOrEmpty(actionParameter))
                {
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.69;
                    var h = bitmapBuilder.Height * 0.3;
                    bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? Convert.ToBoolean(this._plugin.speakerMutedStatus) ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("speakerOff.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("speakerOn.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("notConnected.png")));
                    bitmapBuilder.DrawText(this._plugin.tsConnected ? "" : this._l10n.GetL7dMessage("notConnected"), (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255), imageSize == PluginImageSize.Width90 ? 15 : 12, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
