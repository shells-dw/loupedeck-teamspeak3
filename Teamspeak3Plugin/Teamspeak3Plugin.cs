namespace Loupedeck.Teamspeak3Plugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using Loupedeck.Teamspeak3Plugin.Helper;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class Teamspeak3Plugin : Plugin
    {
        // variables, events, global cancellationtoken
        private l10n.l10n _l10n;
        public String apiKey;
        public Int32 clientId;
        public Int32 micMutedStatus;
        public Int32 micDeactivatedStatus;
        public Int32 speakerMutedStatus;
        public Int32 awayStatus;
        public String awayMessage;
        public List<String> channelList;
        public Boolean tsStarted = false;
        public Boolean tsConnected = false;
        public event EventHandler<EventArgs> TeamspeakConnStatChange;
        public event EventHandler<EventArgs> IsMicMuted;
        public event EventHandler<EventArgs> IsSpeakerMuted;
        public event EventHandler<EventArgs> IsAway;
        public event EventHandler<EventArgs> AwayMessageChanged;
        public event EventHandler<EventArgs> ChannelListUpdated;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean HasNoApplication => true;

        // main thread that queries TS3 ClientQuery
        private async void QueryThread()
        {
            this.Log.Info($"{DateTime.Now} - Starting TS3 query thread");
            // local vars
            Int32 _micMuteStatus = 0;
            Int32 _speakerMuteStatus = 0;
            Int32 _awayStatus = 0;
            String _awayMessage = "";
            List<String> _channelList = new List<String>();
            // infinite while loop, cancellationToken is cancelled on UnLoad event shutting the loop down cleanly, hopefully
            while (true && !this._cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    // verify we're still connected
                    await this.CheckConnected();
                    // fill local vars with device responses
                    this.channelList = new List<String>(Helper.TS3Handler.GetChannelList());
                    this.micMutedStatus = Helper.TS3Handler.GetInputMuteStatus(this.clientId);
                    this.speakerMutedStatus = Helper.TS3Handler.GetOutputMuteStatus(this.clientId);
                    this.awayStatus = Helper.TS3Handler.GetAwayStatus(this.clientId);
                    this.awayMessage = Helper.TS3Handler.GetAwayMessage(this.clientId);
                    // comparing two Lists, Enumerable.SequenceEqual should be the best performing way on larger servers with a lots of channels
                    if (!Enumerable.SequenceEqual(_channelList, this.channelList))
                    {
                        ChannelListUpdated?.Invoke(this, new EventArgs());
                        _channelList = new List<String>(this.channelList);
                    }
                    // comparing local and global variables and raise events to update the button images on changes accordingly
                    if (_micMuteStatus != this.micMutedStatus)
                    {
                        IsMicMuted?.Invoke(this, new EventArgs());
                        _micMuteStatus = this.micMutedStatus;
                    }
                    if (_speakerMuteStatus != this.speakerMutedStatus)
                    {
                        IsSpeakerMuted?.Invoke(this, new EventArgs());
                        _speakerMuteStatus = this.speakerMutedStatus;
                    }
                    if (_awayStatus != this.awayStatus)
                    {
                        IsAway?.Invoke(this, new EventArgs());
                        _awayStatus = this.awayStatus;
                    }
                    if (_awayMessage != this.awayMessage)
                    {
                        AwayMessageChanged?.Invoke(this, new EventArgs());
                        _awayMessage = this.awayMessage;
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error($"{DateTime.Now} - TS3 QueryThread: Exception caught {ex.Message}");
                }
                await Task.Delay(500);
            }
        }


        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override async void Load() => Task.Run(() => this.Init());

        // checking if TS is connected and even running
        private async Task CheckConnected()
        {
            // get client ID, with TS3 running, this will comfortably reflect the connection status of TS as well as the actual clientID while connected
            this.clientId = Helper.TS3Handler.GetClientId();
            // not connected to a server case
            if (this.clientId == -2)
            {
                this.tsConnected = false;
                // invoke global update of button images to reflect the change
                TeamspeakConnStatChange?.Invoke(this, new EventArgs());
                // printing messages for users in the Loupedeck UI
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, this._l10n.GetL7dMessage("runningButNotConnected"), "", "");
                this.Log.Info($"{DateTime.Now} - TS3: Plugin in non-nominal status - Helper.TS3Handler.GetClientId() == -2");
                // loop until TS is connect, closed, or the plugin is unloaded
                while (Helper.TS3Handler.GetClientId() == -2 && !this._cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(2500);
                }
                // update clientID
                this.clientId = Helper.TS3Handler.GetClientId();
                // assuming it will be connected, as this is probably the most probably outcome after being disconnected, but no matter what, if it's closed, the next if cause will catch it anyway.
                this.tsConnected = true;
                TeamspeakConnStatChange?.Invoke(this, new EventArgs());
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "", "", "");
                this.Log.Info($"{DateTime.Now} - TS3: Plugin in nominal status");
            }
            // if the client is not running, waiting for it to return
            if (this.clientId == -1)
            {
                this.tsConnected = false;
                TeamspeakConnStatChange?.Invoke(this, new EventArgs());
                await this.Init();
                TeamspeakConnStatChange?.Invoke(this, new EventArgs());
            }
            this.tsConnected = true;
        }

        // Init stuff
        private async Task Init()
        {
            // initializing localization
            this._l10n = new l10n.l10n(this);
            // if TS isn't running, wait for it to be started
            if (!this.CheckIfTsAvailable())
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, this._l10n.GetL7dMessage("notRunning"), "", "");
                this.Log.Info($"{DateTime.Now} - TS3: Plugin in non-nominal status - ts3 not running");
                while (true && !this.CheckIfTsAvailable() && !this._cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(5000);
                }
            }
            // once TS3 is started, connect to ClientQuery, wait until we're connected, then start the QueryThread
            if (this.CheckApiKey() && this.apiKey != null)
            {
                if (Helper.TS3Handler.ClientQueryConnect(this.apiKey))
                {
                    await this.CheckConnected();
                    Task.Run(async () => this.QueryThread());
                }
                else
                {
                    this.DeletePluginSetting("apiKey");
                    await Task.Delay(1000);
                    this.Init();
                }
            }
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
            this._cancellationTokenSource.Cancel();
            Helper.TS3Handler._client?.Dispose();
        }

        // checking, if TS is available by checking if the corresponding process names are existing in the process list
        private Boolean CheckIfTsAvailable()
        {
            if (Process.GetProcessesByName("ts3client_win64").Any() || Process.GetProcessesByName("ts3client_mac").Any())
            {
                this.tsStarted = true;
                return true;
            }
            this.tsStarted = false;
            return false;
        }

        // make sure we have an API key to connect and interact to ClientQuery.
        private Boolean CheckApiKey()
        {
            this.TryGetSetting("apiKey", out var savedapiKey);
            if (savedapiKey != null)
            {
                this.apiKey = savedapiKey;
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "", "", "");
                this.Log.Info($"{DateTime.Now} - TS3: Plugin in nominal status");
                return true;
            }
            else
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, this._l10n.GetL7dMessage("noApiKey"), "https://github.com/shells-dw/loupedeck-teamspeak3", "GitHub Readme");
                this.Log.Info($"{DateTime.Now} - TS3: Plugin in non-nominal status - apiKey == null");
                return true;
            }
        }

        // helper to get and set settings to Loupedeck's plugin settings
        public Boolean TryGetSetting(String settingName, out String settingValue) =>
            this.TryGetPluginSetting(settingName, out settingValue);

        public void SetSetting(String settingName, String settingValue, Boolean backupOnline = false) =>
            this.SetPluginSetting(settingName, settingValue, backupOnline);
    }
}
