// Credit to https://github.com/ZerGo0 for laying the groundwork for the clientQuery code (which I heavily extended further)

namespace Loupedeck.Teamspeak3Plugin.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading;

    using PrimS.Telnet;

    internal static class TS3Handler
    {
        // assigning vars
        private static readonly Teamspeak3Plugin _plugin = new Teamspeak3Plugin();
        public static Client _client;
        private static readonly Object _client_lock = new Object();

        // connect to ClientQuery
        public static Boolean ClientQueryConnect(String apiKey)
        {
            lock (_client_lock)
            {
                if (_client != null && _client.IsConnected)
                {
                    _plugin.Log.Info($"{DateTime.Now} - ClientQueryConnect: status nominal. _client != null && _client.IsConnected");
                    return true;
                }
                try
                {
                    _client = new Client("127.0.0.1", 25639, new CancellationToken());
                }
                catch (SocketException ex)
                {
                    _client = null;
                    _plugin.Log.Error($"{DateTime.Now} - ClientQueryConnect: Exception caught while trying to connect {ex.Message}");
                    return false;
                }
                if (_client == null || !_client.IsConnected)
                {
                    _client = null;
                    _plugin.Log.Error($"{DateTime.Now} - ClientQueryConnect: _client is not connected");
                    return false;
                }
                var welcomeMessage = _client.ReadAsync().Result;
                if (!welcomeMessage.Contains("TS3 Client"))
                {
                    _plugin.Log.Error($"{DateTime.Now} - ClientQueryConnect: Error in formatting of welcomeMessage, didn't contain expected string. Message was: {welcomeMessage}");
                    _client = null;
                    return false;
                }
                _client.WriteLineAsync($"auth apikey={apiKey}");
                if (!_client.ReadAsync().Result.Contains("msg=ok"))
                {
                    _plugin.Log.Error($"{DateTime.Now} - ClientQueryConnect: Authentication not possible");
                    _client = null;
                    return false;
                }
                return true;
            }
        }
        // get the clientID (which is assigned by the server during every connect)
        public static Int32 GetClientId()
        {
            lock (_client_lock)
            {
                _client.WriteLineAsync("use");
                if (!_client.ReadAsync().Result.Contains("msg=ok"))
                {
                    return -1;
                }
                if (_client == null || !_client.IsConnected)
                {
                    return -1;
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync("whoami");
                    var whoAmIResp = _client.ReadAsync().Result;

                    if (whoAmIResp.Contains("msg=ok"))
                    {
                        return Int32.Parse(Regex.Match(whoAmIResp, @"clid=?([0-9]{1,})").Groups[1].ToString());
                    }
                    if (whoAmIResp.Contains("msg=not\\sconnected"))
                    {
                        return -2;
                    }
                    retries++;
                }
                return -1;
            }
        }
        // get if the mic is muted
        public static Int32 GetInputMuteStatus(Int32 clientId)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return -1;
                }

                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientvariable clid={clientId} client_input_muted");
                    var inputMuteStatusResp = _client.ReadAsync().Result;

                    if (inputMuteStatusResp.Contains("msg=ok"))
                    {
                        return Int32.Parse(Regex.Match(inputMuteStatusResp, @"client_input_muted=?([0-9])").Groups[1].ToString());
                    }
                    retries++;
                }
                return -1;
            }
        }
        // un/mute the mic in TS
        public static Boolean SetInputMuteStatus(Int32 micMuteStatus)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }

                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientupdate client_input_muted={micMuteStatus}");
                    var setInputMuteStatusResp = _client.ReadAsync().Result;

                    if (setInputMuteStatusResp.Contains("msg=ok"))
                    {
                        return true;
                    }
                    retries++;
                }

                return false;
            }
        }
        // get if the output is muted
        public static Int32 GetOutputMuteStatus(Int32 clientId)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return -1;
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientvariable clid={clientId} client_output_muted");
                    var outputMuteStatusResp = _client.ReadAsync().Result;

                    if (outputMuteStatusResp.Contains("msg=ok"))
                    {
                        return Int32.Parse(Regex.Match(outputMuteStatusResp, @"client_output_muted=?([0-9])").Groups[1].ToString());
                    }
                    retries++;
                }
                return -1;
            }
        }
        // un/mute the output in TS
        public static Boolean SetOutputMuteStatus(Int32 outputMuteStatus)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientupdate client_output_muted={outputMuteStatus}");
                    var setOutputMuteStatusResp = _client.ReadAsync().Result;

                    if (setOutputMuteStatusResp.Contains("msg=ok"))
                    {
                        return true;
                    }
                    retries++;
                }
                return false;
            }
        }
        // get if user is away in TS
        public static Int32 GetAwayStatus(Int32 clientId)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return -1;
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientvariable clid={clientId} client_away");
                    var awayStatusResp = _client.ReadAsync().Result;
                    if (awayStatusResp.Contains("msg=ok"))
                    {
                        return Int32.Parse(Regex.Match(awayStatusResp, @"client_away=?([0-9])").Groups[1].ToString());
                    }
                    retries++;
                }
                return -1;
            }
        }
        // set away
        public static Boolean SetAwayStatus(Int32 status)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientupdate client_away={status}");
                    var changeNicknameResp = _client.ReadAsync().Result;

                    if (changeNicknameResp.Contains("msg=ok"))
                    {
                        return true;
                    }
                    retries++;
                }
                return false;
            }
        }
        // set any message that goes with the away status (shown in brackets behind the user name when away)
        public static Boolean SetAwayMessage(String statusMessage)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                if (!String.IsNullOrWhiteSpace(statusMessage))
                {
                    statusMessage = statusMessage.FixCharsForTS();
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientupdate client_away_message={statusMessage}");
                    var changeNicknameResp = _client.ReadAsync().Result;

                    if (changeNicknameResp.Contains("msg=ok"))
                    {
                        return true;
                    }
                    retries++;
                }
                return false;
            }
        }
        // get if there is a message set
        public static String GetAwayMessage(Int32 clientId)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return "-1";
                }
                var retries = 0;
                while (retries < 10)
                {
                    _client.WriteLineAsync($"clientvariable clid={clientId} client_away_message");
                    var awayMessageResp = _client.ReadAsync().Result;
                    if (awayMessageResp.Contains("msg=ok"))
                    {
                        return Regex.Match(awayMessageResp, @"client_away_message=?(.*)").Groups[1].ToString().Replace("\\s", " ").Replace("\\p", "|");
                    }
                    retries++;
                }
                return "-1";
            }
        }
        // getting a list of all channels to have a user select existing channels for the switch channel function
        public static List<String> GetChannelList()
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return null;
                }
                _client.WriteLineAsync("channellist");
                var channelListResp = _client.ReadAsync().Result;
                List<String> channelListArray = new List<String>();
                if (channelListResp.Contains("msg=ok"))
                {
                    foreach (Match match in Regex.Matches(channelListResp, @"channel_name=(\S*)"))
                    {
                        channelListArray.Add(match.Groups[1].Value.Replace("\\s", " ").Replace("\\p", "|"));
                    }
                }
                else
                {
                    channelListResp = null;
                }

                return channelListArray;
            }
        }
        // getting the channelID for the channel the user wants to switch to (which is needed to switch to a channel, the server only accepts this ID, not the name)
        public static Int32 GetChannelId(String channelName)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return -1;
                }
                _client.WriteLineAsync("channellist");
                var channelIdResp = _client.ReadAsync().Result;
                Int32 channelId;
                if (channelIdResp.Contains("msg=ok"))
                {
                    try
                    {
                        channelId = Int32.Parse(Regex.Match(channelIdResp, $"cid\\=(\\d{{1,}})\\s\\w{{3}}\\=\\d*\\s\\w*\\=\\d*\\schannel_name\\={Regex.Replace(channelName, "(['^$.|?*+()[[\\\\\\]\\]])", "\\$1")}\\s").Groups[1].Value);
                    }
                    catch (Exception ex)
                    {
                        _plugin.Log.Error($"{DateTime.Now} - GetChannelId: Exception caught while trying to parse channelid for {channelName} {ex.Message}");
                        channelId = -1;
                    }
                }
                else
                {
                    channelId = -1;
                }
                return channelId;
            }
        }
        // connect to a server on the provided address
        public static Boolean Connect(String address)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                if (String.IsNullOrWhiteSpace(address))
                {
                    return false;
                }
                _client.WriteLineAsync($"connect address={address}");
                var connectResp = _client.ReadAsync().Result;
                return connectResp.Contains("msg=ok");
            }
        }
        // disconnect from the current server
        public static Boolean Disconnect()
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                _client.WriteLineAsync("disconnect");
                var disconnectResp = _client.ReadAsync().Result;
                return disconnectResp.Contains("msg=ok");
            }
        }
        // change nickname
        public static Boolean ChangeNickname(String nickname)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                if (String.IsNullOrWhiteSpace(nickname))
                {
                    return false;
                }
                nickname = nickname.FixCharsForTS();
                _client.WriteLineAsync($"clientupdate client_nickname={nickname}");
                var changeNicknameResp = _client.ReadAsync().Result;
                return changeNicknameResp.Contains("msg=ok");
            }
        }
        // switching the channel
        public static Boolean ChannelSwitch(String channelName, Int32 clientId)
        {
            lock (_client_lock)
            {
                if (_client == null || !_client.IsConnected)
                {
                    return false;
                }
                if (String.IsNullOrWhiteSpace(channelName))
                {
                    return false;
                }
                channelName = channelName.FixCharsForTS();
                if (String.IsNullOrWhiteSpace(channelName))
                {
                    return false;
                }
                var channelId = GetChannelId(channelName);

                if (String.IsNullOrWhiteSpace(channelName))
                {
                    return false;
                }
                _client.WriteLineAsync($"clientmove cid={channelId} clid={clientId}");
                var channelSwitchResp = _client.ReadAsync().Result;

                return channelSwitchResp.Contains("msg=ok");
            }
        }
        // TS ClientQuery prints (and expects) blanks as escaped as space \s and pipes as \p _ couldn't find any other chars, but there may be some.
        private static String FixCharsForTS(this String fixForTS)
        {
            var fixedString = fixForTS.Replace(" ", "\\s").Replace("|", "\\p");

            return fixedString;
        }
    }
}
