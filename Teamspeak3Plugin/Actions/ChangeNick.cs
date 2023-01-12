namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;

    internal class ChangeNick : PluginDynamicCommand
    {
        // Link the plugin and localization instance, set global vars
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        public ChangeNick() : base() { }
        protected override Boolean OnLoad()
        {
            // assign to instances and subscribe to events
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            // load localization into local dict and build the parameter(s)
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("changenick");
            // since list/tree/text inputs are handled somewhat differently than normal parameters, have to assing the displayname and group differently for those
            if (l7dValues != null)
            {
                this.DisplayName = l7dValues["displayName"];
                this.GroupName = l7dValues["groupName"];
            }
            else
            {
                this.DisplayName = "Error";
                this.GroupName = "";
                this._plugin.Log.Info($"changenick : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            this.MakeProfileAction($"text;{this._l10n.GetL7dMessage("changeNickname")}");
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter) => Helper.TS3Handler.ChangeNickname(actionParameter);
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
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
                var x1 = bitmapBuilder.Width * 0.1;
                var w = bitmapBuilder.Width * 0.8;
                var y1 = bitmapBuilder.Height * 0.45;
                if (!this._plugin.tsConnected)
                {
                    y1 = bitmapBuilder.Height * 0.69;
                }
                var h = bitmapBuilder.Height * 0.3;
                bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("setNickname.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("notConnected.png")));
                bitmapBuilder.DrawText(this._plugin.tsConnected ? actionParameter : this._l10n.GetL7dMessage("notConnected"), (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255), imageSize == PluginImageSize.Width90 ? 15 : 12, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                return bitmapBuilder.ToImage();
            }
        }
    }
}
