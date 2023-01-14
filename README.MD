 ![GitHub](https://img.shields.io/github/license/shells-dw/loupedeck-teamspeak3)
 ![GitHub last commit](https://img.shields.io/github/last-commit/shells-dw/loupedeck-teamspeak3)
 [![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2) / [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)

 # Loupedeck Teamspeak 3 Plugin

![Overview](/docs/overview.png)

 ## What Is This (and what does it do?)

It's a plugin for the [Loupedeck Consoles][Loupedeck] which allows you to control your Teamspeak 3 client from your Loupedeck.

It allows you to place buttons to control the following functions on your Loupedeck
- **mute/unmute** microphone and output
- toggle **away** status
- set **custom away messages**
- **remove these away messages** again
- can **move you into channels** (which are read from the client and presented in a list, so you don't have to type them in)
- **change your nickname** (if these kinds of shenanigans are your thing _and the server permits that, obviously_) 
- **disconnect** from the current server
- **connect** to a server.

Most functions are mirrored from Teamspeak, so when you mute your mic by a hotkey or in the Teamspeak client directly, the Loupedeck will reflect that change.

The plugin requires the ClientQuery Plugin, which is already included with every Teamspeak 3 client installation, to be active.

- [x] **Works on Windows and MacOS**
- [x] localized to German and French in addition to English

## Release / Installation

You can find the precompiled plugin lplug4 file in the [Releases][Releases]. Download and open it, your computer should already recognize this as a Loupedeck plugin file and offer to open it with Loupedeck Configuration Console - which will have the plugin available in the list then.

## Setup

Since a while, Teamspeak requires ClientQuery connections to be authenticated. Because of that, the plugin needs to know the necessary API key, which is generated randomly during the installation of Teamspeak.

To access your API key, in Teamspeak open **Tools** - **Options**, navigate to **Addons** - **Plugins** and click on **Settings** on the **ClientQuery** plugin. A new window opens in which the API key is displayed.


![ApiKey1](/docs/ApiKey1.png)

Copy that key, then temporarily pull the **Setup** action on a free slot of the Loupedeck, paste the API key in the text input field of the Loupedeck UI, save and touch the button once.


![ApiKey1](/docs/ApiKey2.png)

**You can now delete the button again, as the key is securely stored in the plugin's settings.** In case you decide to renew the key or reinstall Teamspeak and the key changes that way, just do the same again and update the stored key.

*note: the key shown in the picture is obviously just an example - yours will have the same format, but be a different key*

## Notes / Restrictions

- Teamspeak 5 is not supported, as the current Beta does not support editing settings remotely, it can only be used to display values, to which I opted against. Once they support a fully working API, I'm planning on supporting it too, but until then - do what everybody does and use Teamspeak 3 :wink:
 
# I have an issue or miss a feature

You can submit an issue or request a feature with [GitHub issues] or start a [discussion](https://github.com/shells-dw/loupedeck-teamspeak3/discussions). Please describe as good as possible what went wrong or doesn't act like you'd expect it to when you open an issue. 

# Support

If you'd like to drop me a coffee for the hours I've spent on this:
[![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2), sponsor me on [GitHub](https://github.com/sponsors/shells-dw) 
or use Ko-Fi [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)

# Changelog
## [1.1.0] - 2023-01-14
- Removed the need for Newtonsoft.Json and went with JSON functions that come with the SDK to avoid potential mismatch issues between the bundled and the NuGet Newtonsoft.Json version in the future

<details><summary>Changelog History</summary><p>

## [1.0.0] - 2023-01-12
- initial release

</p></details>


<!-- Reference Links -->

[Loupedeck]: https://loupedeck.com "Loupedeck.com"
[Releases]: https://github.com/shells-dw/loupedeck-teamspeak3/releases "Releases"
[GitHub issues]: https://github.com/shells-dw/loupedeck-teamspeak3/issues "GitHub issues link"

