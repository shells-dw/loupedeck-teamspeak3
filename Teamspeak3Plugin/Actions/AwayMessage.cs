namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;

    internal class AwayMessage : PluginDynamicCommand
    {
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        public AwayMessage() : base() { }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.AwayMessageChanged += (sender, e) => this.ActionImageChanged();
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("awaymessage");
            if (l7dValues != null)
            {
                this.DisplayName = l7dValues["displayName"];
                this.GroupName = l7dValues["groupName"];
            }
            else
            {
                this.DisplayName = "Error";
                this.GroupName = "";
                this._plugin.Log.Info($"awaymessage : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            this.MakeProfileAction($"text;{this._l10n.GetL7dMessage("enterAwayMessage")}");
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter)
        {
            Helper.TS3Handler.SetAwayMessage(actionParameter);
            this.ActionImageChanged();
        }
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean _toggle = false;
            if (actionParameter == this._plugin.awayMessage)
            {
                _toggle= true;
            }
            if (actionParameter.Length > 20)
            {
                actionParameter = actionParameter.Substring(0, 20) + "...";
                if (actionParameter.Substring(0, 8).Count(Char.IsWhiteSpace) == 0)
                {
                    actionParameter = actionParameter.Substring(0, 7) + "...";
                }
            }
        
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                if (!String.IsNullOrEmpty(actionParameter))
                {
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.55;
                    if (!this._plugin.tsConnected)
                    {
                        y1 = bitmapBuilder.Height * 0.69;
                    }
                    var h = bitmapBuilder.Height * 0.3;
                    bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? Convert.ToBoolean(_toggle) && Convert.ToBoolean(this._plugin.awayStatus) ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("awayMessageOn.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("awayMessageOff.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("notConnected.png")));
                    bitmapBuilder.DrawText(this._plugin.tsConnected ? actionParameter : this._l10n.GetL7dMessage("notConnected"), (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255), imageSize == PluginImageSize.Width90 ? 15 : 12, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
