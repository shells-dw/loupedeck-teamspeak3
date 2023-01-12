namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;

    internal class Away : PluginDynamicCommand
    {
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        private Int32 awayStatus = 0;
        public Away() : base() { }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.IsAway += (sender, e) => this.ActionImageChanged();
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("away");
            if (l7dValues != null)
            {
                this.AddParameter("away", l7dValues["displayName"], l7dValues["groupName"]);
            }
            else
            {
                this._plugin.Log.Info($"Away : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter)
        {
            this.awayStatus = 1 - this._plugin.awayStatus;
            Helper.TS3Handler.SetAwayStatus(this.awayStatus);
            this._plugin.awayStatus = this.awayStatus;
            this.ActionImageChanged();
        }
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var _awayStatus = Convert.ToBoolean(this._plugin.awayStatus);
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                if (!String.IsNullOrEmpty(actionParameter))
                {
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.69;
                    var h = bitmapBuilder.Height * 0.3;
                    bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? Convert.ToBoolean(this._plugin.awayStatus) ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("awayOn.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("awayOff.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("notConnected.png")));
                    bitmapBuilder.DrawText(this._plugin.tsConnected ? "" : this._l10n.GetL7dMessage("notConnected"), (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255), imageSize == PluginImageSize.Width90 ? 15 : 12, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
