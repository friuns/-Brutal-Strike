using System;
using System.Collections.Generic;

public class Config
{
    #if game

    public Dictionary<string,Dictionary<string, object>> m_configLangs = new Dictionary<string, Dictionary<string, object>>();
    
    public bool enableRoomCustomPropertiesFetch = false;

    public string lang="English";

    public int unlockShopPlayTime = 0;
    public string rateGameText = "Write skin code {0} and rate game to unlock skin!";
    public string unlockRateGame = "5star";
    public string notificationTitle2 = "{0} players currently playing on {1}";
    public string notificationText2 = "Click to join {0}";
    
    public string notificationTitle = "Skin Code for Brutal Strike is ready! {0}";
    public string notificationText = "Come back and try your new skin code! {0}";
    public string notificationCode = "XT8OK"; 
    
    public string unlockSkinsText = "Send this code to a friend to unlock new skin! {0}, Skin give aways at <color=blue>https://www.facebook.com/brutalstrike.net/</color>";
    public string unlockSkinsLink = "https://www.facebook.com/pg/BrutalStrike.net/posts/";

    public string[] products = new[] { Tag.bc1000, Tag.bc100, Tag.bc10,"bc10"};
    public string[] subscriptions = new[] { Tag.battle_pass, Tag.trial_pass, "battlepassbase", "test" };
    public string imageAdImage = "";
    public string imageAdText = "Skin code to a friend {0}";
    public string imageAdScriptClicks = "bs._Loader.ShowUnlockSkinWindow();";
    public string modTutorial = "https://vk.com/@brutalstrike2-moding2";
    public string getFreeBcoins = ""; 

    public string credits = "Top contributors\nVan dark:\n cs go, quake, brutal striker, zombie mod, skins and maps creator, video trailers\nDuque_br:\n Site, account profile system, achievements, clan system, discord bot creator \nbedrockoff CSS MOD\n vlad dev streams\n SᕼᗩᘺᒪᒪIᘻ br voice";
    public string profileUrl = "https://site.brutalstrike.net/profile/p.php?@userID={0}&@sessionID={1}&id={2}&user={3}";
    public string twitchPreview = "https://production.assets.clips.twitchcdn.net/AT-cm%7C1055083572-360.mp4?sig=32dbcc3287f6129ca5a7f864488a3ca1e8671b92&token=%7B%22authorization%22%3A%7B%22forbidden%22%3Afalse%2C%22reason%22%3A%22%22%7D%2C%22clip_uri%22%3A%22%22%2C%22device_id%22%3A%22Meg2Qd2d9DWKDHm8tnoI82JQ2vpeIyp2%22%2C%22expires%22%3A1613659111%2C%22user_id%22%3A%22130916674%22%2C%22version%22%3A2%7D";
    public string twitchName="";
    public string twitchUrl = ""; // = "https://www.twitch.tv/videos/916159430";
    public string twitchAudioUrl = "";
    public string AdUnitIDBanner = "Banner_Android";
    public string AndroidGameId = "4761513";
    
    public string shopText = "Unlock Market Place, Its a skin trading place where you can buy or sell skins online! ";
    public List<MpVersion> mpVersions = new List<MpVersion>() { new MpVersion() { version = 2621, mpVersion = 42 } };
    public struct MpVersion
    {
        public int mpVersion;
        public int version;
    }
    public ConfigScripts scripts = new ConfigScripts(){
        
        /*loaderStart = @"import ('Assembly-CSharp')
import ('Assembly-CSharp-firstpass')

f = WWWForm()
f:AddField('MethodName','ReplaceUser')
f:AddField('user',Serializer.Serialize(bs._PlayerStats2))
WWW('https://game2.brutalstrike.net/cs/asp/',f)"
*/
        
    }; 
    public List<NameID> moderators2 = new List<NameID>()
    {
        new NameID() {id = 149759970, name = "<color=purple>friuns</color> Dev"},
        new NameID() {id = 1863767845, name = "Godlok21"},
        new NameID() {id = 146664521, name = "Semen2012"},
        new NameID() {id = 188102669, name = "MrMarmok"},
    };
    public string m_uploadSite = "game.brutalstrike.net/cs/";
    public string[] m_mainSite = { "game2.brutalstrike.net/cs/", "cs.tmrace.net/cs/", "cs.lolgames.net/cs/","brutalstrike2.net/cs/"};
    public string m_mainSiteDev = "game.brutalstrike.net/cs/"; //cloudFlare denies large file uploads 100MB
    public Device device { get { return Base.Android ? android : Base.webgl ? webgl : pc; } }


    

    public Device android = new Device() { };
    public Device pc = new Device();
    public Device webgl = new Device() {links = new[] {"Download for android", "http://brutalstrike.net/cs/brutalStrike.apk", "Download for pc", "http://brutalstrike.net/cs/brutalStrike.zip"}};
    public float pingCorrection=-20;
    public bool useCustomServer = false;
    public string[] photonServers3 = new string[]
    {
        //"127.0.0.1 test"
        "95.216.6.19 eu2"
    };
    public int[] blockVersions = { };
    public string[] ignoreMaps =
    {
        "https://gamebanana.com/maps/197392",
        "https://gamebanana.com/maps/191533",
        "https://gamebanana.com/maps/190197",
        "https://gamebanana.com/maps/171185",
        "https://gamebanana.com/maps/161453",
        "https://gamebanana.com/maps/204164",
        "https://gamebanana.com/maps/196471",
        "https://gamebanana.com/maps/205667",
        "https://gamebanana.com/maps/179509",
        "https://gamebanana.com/maps/155398",
        "https://gamebanana.com/maps/198386"
    };
    // public string[]  quickPlayMaps { get { return MapsFinder.sceneMaps; } } 
    //     = 
    // {
    //     // "http://cs.tmrace.net/cs/bundlesDir/3E71A1B917A90E16A715A8D9C056EE87.unity3dlevel"
    //     //"https://gamebanana.com/maps/189644",
    //     "map_css_assault2", "https://gamebanana.com/maps/171427", "https://gamebanana.com/maps/194489"
    // };
    public string[] appIds =
    {
         
        "d49c09b4-6aa6-47c1-b62f-12f43a2a0e16", //mad gunz pixel shooter
        "c9459579-dfb8-4e89-9b86-ac16f2db4dde"//my
        
    };
    public string devPC="friuns";
    
    public class Device
    {
        public int minVer=0;
        public int force=0;
        public string updateFile="http://brutalstrike.net/cs/brutalStrike.apk";
        public string updateMessage = "New Version available {0} \ncurrent version: {1}";
        public string[] links = new string[0];
    }
#endif
}