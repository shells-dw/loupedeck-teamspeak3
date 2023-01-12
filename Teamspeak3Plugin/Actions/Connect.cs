namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;

    internal class Connect : PluginDynamicCommand
    {
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        public Connect() : base() { }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("connect");
            if (l7dValues != null)
            {
                this.DisplayName = l7dValues["displayName"];
                this.GroupName = l7dValues["groupName"];
            }
            else
            {
                this.DisplayName = "Error";
                this.GroupName = "";
                this._plugin.Log.Info($"connect : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            this.MakeProfileAction($"text;{this._l10n.GetL7dMessage("connectText")}");
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter) => Helper.TS3Handler.Connect(actionParameter);
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                if (!String.IsNullOrEmpty(actionParameter))
                {
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.71;
                    var h = bitmapBuilder.Height * 0.3;
                    bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("connectOff.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("connectOn.png")));
                    bitmapBuilder.DrawText(this._plugin.tsConnected ? this._l10n.GetL7dMessage("connected") : actionParameter, (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, this._plugin.tsConnected ? new BitmapColor(100, 255, 0) : new BitmapColor(255, 255, 255), this._plugin.tsConnected ? imageSize == PluginImageSize.Width90 ? 15 : 13 : imageSize == PluginImageSize.Width90 ? 12 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                    if (this._plugin.tsConnected)
                    {
                        bitmapBuilder.DrawRectangle(0, 40, 80, 32, BitmapColor.Transparent);
                        bitmapBuilder.FillRectangle(0, 0, 80, 80, color: new BitmapColor(0, 0, 0, 140));
                    }
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
