namespace Loupedeck.Teamspeak3Plugin.Actions
{
    using System;
    using System.Text.RegularExpressions;

    internal class Setup : PluginDynamicCommand
    {
        // Link the plugin instance
        private Teamspeak3Plugin _plugin;
        public Setup() : base() => this.MakeProfileAction($"text;ClientQuery API Key");
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as Teamspeak3Plugin;
            this.DisplayName = "Setup";
            this.GroupName = "Setup ClientQuery API Key";
            return !(this._plugin is null) && base.OnLoad();
        }
        protected override void RunCommand(String actionParameter)
        {
            // verify the input is actually in the required form and no bogus, then save and set reload
            if (actionParameter != null && Regex.IsMatch(actionParameter, @"^(?:[A-Z0-9]{4}\-?)+$"))
            {
                this._plugin.SetPluginSetting("apiKey", actionParameter);
                this._plugin.apiKey = actionParameter;
                this._plugin.Log.Info($"{DateTime.Now} - Set apiKey to {actionParameter}");
                this._plugin.TryLoad();
            }
        }
    }
}
