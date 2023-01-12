namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Loupedeck.Teamspeak3Plugin.l10n;
    using System.Text.RegularExpressions;
    using System.Text;

    internal class SwtichChannel : PluginDynamicCommand
    {
        // Link the plugin and localization instance, set global vars
        private Teamspeak3Plugin _plugin;
        private l10n _l10n;
        public SwtichChannel() : base() { }
        protected override Boolean OnLoad()
        {
            // assign to instances and subscribe to events
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this._plugin.ChannelListUpdated += (sender, e) => this.BuildChannelList();
            this._plugin.TeamspeakConnStatChange += (sender, e) => this.ActionImageChanged();
            this._l10n = new l10n(this._plugin);
            // load localization into local dict and build the parameter(s)
            Dictionary<String, String> l7dValues = this._l10n.GetL7dNames("switchchannel");
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
                this._plugin.Log.Info($"switchchannel : l7dValues was empty or null: DisplayName: {l7dValues["displayName"]}, groupName: {l7dValues["groupName"]}.");
            }
            this.MakeProfileAction($"list;{this._l10n.GetL7dMessage("switchChannelSelect")}");
            return !(this._plugin is null) && base.OnLoad();
        }
        // building the channel list to show in the selection field in the UI
        private void BuildChannelList()
        {
            // this removes everything there is so it's loaded every time the config for a button is loaded
            this.RemoveAllParameters();
            foreach (String channel in this._plugin.channelList)
            {
                // Teamspeak ClientQuery uses very nasty unicode strings for most non-word characters which takes a bit of RegEx effort to show pretty
                String cleanDisplayFromUnicodeMadness = Regex.Replace(Encoding.UTF8.GetString(Encoding.GetEncoding(28591).GetBytes(Regex.Unescape(channel))), @"(?:\[(.{0,1})spacer\]\s){0,1}", "");
                this.AddParameter(channel, cleanDisplayFromUnicodeMadness, groupName: "");
            }
        }
        protected override void RunCommand(String actionParameter)
        {
            Helper.TS3Handler.ChannelSwitch(actionParameter, this._plugin.clientId);
            this.ActionImageChanged();
        }
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            // Teamspeak ClientQuery uses very nasty unicode strings for most non-word characters which takes a bit of RegEx effort to show pretty
            String cleanDisplayFromUnicodeMadness = Regex.Replace(Encoding.UTF8.GetString(Encoding.GetEncoding(28591).GetBytes(Regex.Unescape(actionParameter))), @"(?:\[(.{0,1})spacer\]\s){0,1}", "");
            // as channel names in Teamspeak can be up to 40 chars long, they wouldn't fit on the display (at least not so that anyone could still read them), so cut them off
            if (cleanDisplayFromUnicodeMadness.Length > 20)
            {
                cleanDisplayFromUnicodeMadness = cleanDisplayFromUnicodeMadness.Substring(0, 20) + "...";
            }
            // single strings without spaces and more than 10 chars would go out of bounds of the button display, so cut them to a reasonable length
            if (cleanDisplayFromUnicodeMadness.Length > 10)
            {
                if (cleanDisplayFromUnicodeMadness.Substring(0, 10).Count(Char.IsWhiteSpace) == 0)
                {
                    cleanDisplayFromUnicodeMadness = cleanDisplayFromUnicodeMadness.Substring(0, 8) + "...";
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
                    bitmapBuilder.SetBackgroundImage(this._plugin.tsConnected ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("switchToChannel.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("notConnected.png")));
                    bitmapBuilder.DrawText(this._plugin.tsConnected ? cleanDisplayFromUnicodeMadness : this._l10n.GetL7dMessage("notConnected"), (Int32)x1, (Int32)y1, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255), imageSize == PluginImageSize.Width90 ? 15 : 12, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
