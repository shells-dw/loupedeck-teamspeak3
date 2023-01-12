namespace Loupedeck.Teamspeak3Plugin.l10n
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class l10n
    {
        private readonly String[] SupportedLanguageCodes = { "de", "en", "fr" };
        private readonly Teamspeak3Plugin _plugin;
        private Dictionary<String, Dictionary<String, JObject>> actionL10n;

        public l10n(Teamspeak3Plugin plugin)
        {
            this._plugin = plugin;
            this.ReadL10nFiles();
        }
        private void ReadL10nFiles()
        {
            this.actionL10n = new Dictionary<String, Dictionary<String, JObject>>();
            foreach (var code in this.SupportedLanguageCodes)
            {
                this.actionL10n[code] = GetL7dData(code);
            }
        }
        private static Dictionary<String, JObject> GetL7dData(String lc)
        {
            var dataStream = EmbeddedResources.GetStream(EmbeddedResources.FindFile("l10n-" + lc + ".json"));
            Dictionary<String, JObject> result = new Dictionary<String, JObject>();
            if (dataStream.Length > 0)
            {
                var serializer = new JsonSerializer();
                var reader = new StreamReader(dataStream, Encoding.UTF8);
                using (var jtr = new JsonTextReader(reader))
                {
                    result = serializer.Deserialize<Dictionary<String, JObject>>(jtr);
                }
                return result;
            }
            else
            {
                return result;
            }
        }

        public String GetCurrentLanguageCode()
        {
            var CurrentLanguageCode = this._plugin.Localization.LoupedeckLanguage;
            var twoCharacterLanguageCode = CurrentLanguageCode.Substring(0, 2);
            return twoCharacterLanguageCode;
        }

        public Dictionary<String, String> GetL7dNames(String actionId)
        {
            // Get the current language code:
            var LanguageCode = this.GetCurrentLanguageCode();
            Dictionary<String, String> result;
            try
            {
                result = new Dictionary<String, String>() {
                { "displayName", (String)this.actionL10n[LanguageCode]["actions"][actionId][0]["displayName"] },
                { "groupName", (String)this.actionL10n[LanguageCode]["actions"][actionId][0]["groupName"] }
                };
            }
            catch
            {
                result = new Dictionary<String, String>() {
                { "displayName", (String)this.actionL10n["en"]["actions"][actionId][0]["displayName"] },
                { "groupName", (String)this.actionL10n["en"]["actions"][actionId][0]["groupName"] }
                };
            }
            return result;
        }
        public String GetL7dMessage(String messageId)
        {
            // Get the current language code:
            var LanguageCode = this.GetCurrentLanguageCode();
            String result;
            try
            {
                result = (String)this.actionL10n[LanguageCode]["messages"][messageId];
            }
            catch
            {
                result = (String)this.actionL10n["en"]["actions"][messageId];
            }
            return result;


        }
    }
}
