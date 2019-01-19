using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ChameleonLib.Helper;

namespace ChameleonLib.Model
{
    public class AppListModel
    {
        /// <summary>
        /// ItemViewModel 개체에 대한 컬렉션입니다.
        /// </summary>
        public ObservableCollection<AppGroup> MfItems { get; private set; }

        /// <summary>
        /// ItemViewModel 개체에 대한 컬렉션입니다.
        /// </summary>
        public ObservableCollection<AppGroup> MsItems { get; private set; }

        /// <summary>
        /// ItemViewModel 개체에 대한 컬렉션입니다.
        /// </summary>
        public ObservableCollection<AppGroup> VsItems { get; private set; }

        public AppListModel()
        {
            this.MfItems = new ObservableCollection<AppGroup>();
            this.MsItems = new ObservableCollection<AppGroup>();
            this.VsItems = new ObservableCollection<AppGroup>();
        }

        public bool IsMfDataLoaded
        {
            get;
            private set;
        }

        public bool IsMsDataLoaded
        {
            get;
            private set;
        }

        public bool IsVsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// 몇 개의 ItemViewModel 개체를 만들어 Items 컬렉션에 추가합니다.
        /// </summary>
        public void LoadMfData()
        {
            string manufacturer = DeviceHelper.ManufacturerLowerCase;
            
            AppGroup sysApp = new AppGroup(manufacturer, "AppCategorySystem");
            AppGroup imageApp = new AppGroup(manufacturer, "AppCategoryImage");
            AppGroup hereApp = new AppGroup(manufacturer, "AppCategoryHere");
            AppGroup etcApp = new AppGroup(manufacturer, "AppCategoryExtra");
            
            if (manufacturer.Equals("nokia"))
            {
                //시스템 앱 추가
                //Nokia Account
                sysApp.Add("account", null, "5c2f810e-7445-4ecb-92d9-99514a5133f4");
                //Network+
                sysApp.Add("network", "settings", "62f172d1-f552-4749-871c-2afd1c95c245");
                //Display+Touch
                sysApp.Add("display.touch", "settings", "b08997ca-60ab-4dce-b088-f92e9c7994f3");
                //Accessories
                sysApp.Add("accessories", "settings", "2fa58039-a6ea-4421-b5c6-9ffac0c3ec3d");
                //Access Point
                sysApp.Add("access.point", "settings", "ce3895c7-01d0-4daf-a4c3-25c10463942d");
                //Audio
                sysApp.Add("audio", "settings", "373cb76e-7f6c-45aa-8633-b00e85c73261");
                //Feedback
                sysApp.Add("feedback", "settings", "aec3bfad-e38c-4994-9c32-50bd030730ec");
                //Extra+Info
                sysApp.Add("extra.info", "settings", "2377fe1b-c10f-47da-92f3-fc517345a3c0");
                //glance
                sysApp.Add("glance", "settings", "106e0a97-8b19-42cf-8879-a8ed2598fcbb");
                //Extra+Info
                sysApp.Add("call.sms.filter", null, "c459945b-d93f-4aae-9207-c6ab1d971357");

                //HERE 앱
                hereApp.Add("here.city.lens", null, "b0a0ac22-cf9e-45ba-8120-815450e2fd71");
                hereApp.Add("here.maps", null, "efa4b4a7-7499-46ce-aa95-3e4ab3b39313");
                hereApp.Add("here.drive.plus", null, "31bbc68c-503e-4561-8d85-a294d54df06f");
                hereApp.Add("here.transit", null, "adfdad16-b54a-4ec3-b11e-66bd691be4e6");

                //이미지 앱
                //Nokia Pro Cam
                imageApp.Add("nokia.pro.cam", null, "bfd2d954-12da-415c-ad99-69a20f101e04");
                //Nokia Smart Cam
                imageApp.Add("nokia.smart.cam", null, "73e1801b-916e-4db5-87ba-65fbc8dfd8fc");
                //Panorama (파노라마)
                imageApp.Add("panorama", null, "8124bf8c-0db0-4688-8ec7-698a3c313f2b");
                //Smart Shoot (스마트 촬영)
                imageApp.Add("smart.shoot", null, "bb534f9b-3f38-483b-a409-9285de9c62d4");
                //Nokia Video Trimmer
                imageApp.Add("nokia.video.trimmer", null, "50ece96d-bb5d-4669-a7e1-37246d904d5c");
                //Nokia Video Upload
                imageApp.Add("nokia.video.upload", null, "0aeb1b49-f255-41ec-bf47-897b56b15584");
                //Nokia Refocus
                imageApp.Add("nokia.refocus", null, "0b5cc26d-a723-4c69-8be3-66bbd9b2bed1");
                //Nokia Glam Me
                imageApp.Add("nokia.glam.me", null, "40b6a721-15d2-4843-a746-774bd7b9bda9");
                //Creative Studio (크리에이티브 스튜디오)
                imageApp.Add("creative.studio", null, "a8ddc8f6-c12c-44e6-b22e-52e2f0905f3e");
                //PhotoBeamer
                imageApp.Add("photo.beamer", null, "971c41e5-3596-4a7a-ba2c-bcd7780d7db5");
                //소피렌즈
                imageApp.Add("sophie.lens.for.nokia", null, "b470d8f0-f2a5-4520-ad17-037258d7bd21");
                
                //기타 앱 추가
                //Transfer my Data (내 데이터 전송)
                etcApp.Add("transfer.my.data", null, "dc08943b-7b3d-4ee5-aa3c-30f1a826af02");
                //Nokia Care
                etcApp.Add("nokia.care", null, "ccfdca81-e2fe-44bd-8957-d0c55c636933  ");
                //My Nokia
                etcApp.Add("my.nokia", null, "5e242463-ad9c-489b-b1db-cc94a26e513b");
                //Nokia Xpress
                etcApp.Add("nokia.xpress", null, "cbf5f827-aa0a-4670-8ba6-775676f275b0");
                //App Social
                etcApp.Add("app.social", null, "51f96aba-9924-43d7-8d6c-76a24018d3e0");
                //Nokia Trailers
                etcApp.Add("nokia.trailers", null, "b0731ce2-cdee-4cad-af01-a74a0433fcea");
                //Play to
                etcApp.Add("play.to", null, "8257b398-f4bf-4483-97c7-6fd6a1e60bbf");
                //Ringtone Maker (벨소리 편집기)
                etcApp.Add("ringtone.maker", null, "5a99cbd9-e82a-4892-8264-17a64f9142e5");
                //Nokia Music (Nokia 뮤직)
                etcApp.Add("nokia.music", null, "f5874252-1f04-4c3f-a335-4fa3b7b85329");
                //Nokia NFC Writer 
                etcApp.Add("nokia.nfc.writer", null, "709e64e0-5849-4ce4-b252-b7b252aefacf");
                //Nokia App Folder
                etcApp.Add("app.folder", null, "7d2e7de3-95e7-4143-8a9d-aedf8c3f901e");
            }
            else if (manufacturer.Equals("htc"))
            {
                //Attentive Phone (어텐티브 폰)
                sysApp.Add("attentive.phone", null, "59fba4ce-c8d6-df11-a844-00237de2db9e");
                ////Beats Audio 
                sysApp.Add("beats.audio", null, "54b4b23e-c2cd-4433-9c34-17a4105d1679");
                ////Connection Setup
                sysApp.Add("connection.setup", null, "5edbdbbc-2ab2-df11-8a2f-00237de2db9e");
                ////Make More Space
                sysApp.Add("make.more.space", null, "fc388ddb-433d-4c70-ac48-455175a2cbf5");
                ////Photo Enhancer
                sysApp.Add("photo.enhancer", null, "8e17bc66-2bb2-df11-8a2f-00237de2db9e");
                ////HTC
                sysApp.Add("htc", null, "e462a4e4-0798-4e01-9ed7-fada88e38357");
                //기타 앱 추가
                //Flashlight
                etcApp.Add("flash.light", null, "0be0455c-c8d5-df11-a844-00237de2db9e");
                //Converter
                etcApp.Add("converter", null, "de54d3b1-47b1-df11-8a2f-00237de2db9e");

            }
            else if (manufacturer.Equals("samsung"))
            {
                //APNs
                sysApp.Add("apns", "settings", "5e5c37c0-fb9f-4587-8934-d35228de7622");
                ////Call blocking
                sysApp.Add("call.blocking", "settings", "41b58943-f500-4c13-8b9c-81c96d10e342");
                ////Additional call settings (추가 통화 설정)
                sysApp.Add("addtional.call.settings", "settings", "7df63b5e-131b-459a-a99a-7cd5abcb8f40");
                ////Advanced text messages (고급 문자 메시지)
                sysApp.Add("advanced.text.messages", "settings", "ebe5dbce-da3c-4720-b04f-77699cf2728a");
                ////Contacts import (연락처 가져오기)
                sysApp.Add("contacts.import", "settings", "e8274711-ca3e-4a91-aca0-8790af33076d");

                //기타 앱 추가
                //Now
                etcApp.Add("now", null, "4bc2a02b-86b0-df11-8a2f-00237de2db9e");
                //Beauty 
                etcApp.Add("beauty", null, "106245cc-94a2-4d89-b876-27d22aa7b168");
                //Live Wallpaper
                etcApp.Add("live.wallpaper", null, "227a4e87-e3eb-49b2-be02-526cf116631c");
                //Music Hub
                etcApp.Add("music.hub", null, "09d50986-6d17-4dd2-915a-86b98d560050");
                //RSS Times
                etcApp.Add("rss.times", null, "e7fd6b61-a095-4b06-9fba-005cc9b09267");
                //Ativ Beam
                etcApp.Add("ativ.beam", null, "0f38bc3b-f723-47b9-8444-6b7da64fe38b");
                //Mini Diary
                etcApp.Add("mini.diary", null, "1af954eb-a84e-4968-9687-bd65e58cef37");
                //ChatON
                etcApp.Add("chat.on", null, "65859ff5-5ecf-4083-a1c4-8caafbb77c8b");
                //Photogram
                etcApp.Add("photogram", null, "edcfb419-78ed-df11-9264-00237de2db9e");
                //Photo Editor
                etcApp.Add("photo.editor", null, "c6cf28b1-f83b-42b9-b6fa-b84cbda80b5c");
                //Family Story
                etcApp.Add("family.story", null, "25c21b2c-2629-4b11-92f3-d90d461d0d0c");
                //Story Album
                etcApp.Add("story.album", null, "368ce7e4-d4b6-41be-a62c-b6883db7a57b");
                
            }
            else if (manufacturer.Equals("huawei"))
            {
            }
            
            if (sysApp.Count > 0)
            {
                this.MfItems.Add(sysApp);
            }

            if (imageApp.Count > 0)
            {
                this.MfItems.Add(imageApp);
            }

            if (hereApp.Count > 0)
            {
                this.MfItems.Add(hereApp);
            }

            if (etcApp.Count > 0)
            {
                this.MfItems.Add(etcApp);
            }

            this.IsMfDataLoaded = true;
        }

        public void LoadMsData()
        {
            //microsoft corporation
            //social 
            AppGroup socialApp = new AppGroup("microsoft", "AppCategorySocial");
            //facebook
            socialApp.Add("facebook", null, "82a23635-5bd9-df11-a844-00237de2db9e");
            //youtube
            socialApp.Add("youtube", null, "dcbb1ac6-a89a-df11-a490-00237de2db9e");
            //skype Skype
            socialApp.Add("skype", null, "c3f8e570-68b3-4d6a-bdbb-c0a3f4360a51");
            //groupme GroupMe
            socialApp.Add("groupme", null, "4116f88c-ad1b-464a-b12d-c5f52b2e16e2");
            //yammer
            socialApp.Add("yammer", null, "54b05abd-9724-42a7-9b22-59fc71a8c59d");
            
            //bing
            AppGroup bingApp = new AppGroup("microsoft", "AppCategoryBing");
            //bing.weather Bing 날씨
            bingApp.Add("bing.weather", null, "63c2a117-8604-44e7-8cef-df10be3a57c8");
            //bing.news Bing News
            bingApp.Add("bing.news", null, "9c3e8cad-6702-4842-8f61-b8b33cc9caf1");
            //bing.sports Bing Sports
            bingApp.Add("bing.sports", null, "0f4c8c7e-7114-4e1e-a84c-50664db13b17");
            //bing.finance Bing Finance Bing 금융
            bingApp.Add("bing.finance", null, "1e0440f1-7abf-4b9a-863d-177970eefb5e");
            //bing.food.drink Bing Food & Drink Bing 푸드
            bingApp.Add("bing.food.drink", null, "cc512389-0456-430f-876b-704b17317de2");
            //bing.health.fitness Bing Health & Fitness Bing 헬스
            bingApp.Add("bing.health.fitness", null, "cbb8c3bd-99e8-4176-ad8c-95ec6a3641c2");
            //bing.finance Bing Travel Bing 여행
            bingApp.Add("bing.travel", null, "19cd0687-980b-4838-8880-5f68aba1671e");
            //translator Translator
            bingApp.Add("translator", null, "2cb7cda1-17d8-df11-a844-00237de2db9e");

            //etc
            AppGroup extraApp = new AppGroup("microsoft", "AppCategoryExtra");
            //skydrive SkyDrive
            extraApp.Add("skydrive", null, "ad543082-80ec-45bb-aa02-ffe7f4182ba8");
            //pdf.reader PDF Reader
            extraApp.Add("pdf.reader", null, "8f6154d6-1b70-431a-a579-b6a43477e837");
            //weather
            extraApp.Add("weather", null, "ace44e54-1dd8-df11-a844-00237de2db9e");
            //fresh.paint Fresh Paint
            extraApp.Add("fresh.paint", null, "ab89f9f8-f78b-4fa0-a244-c87d53c14319");
            //photosynth Photosynth
            extraApp.Add("photosynth", null, "ef860a79-5f68-4ed6-aa21-c038d1a55517");
            //level Level
            extraApp.Add("level", null, "c14e93aa-27d7-df11-a844-00237de2db9e");
            //unit.converter Unit Converter
            extraApp.Add("unit.converter", null, "0f69cc30-1bd8-df11-a844-00237de2db9e");
            
            //Microsoft Studios™
            AppGroup studioApp = new AppGroup("microsoft", "AppCategoryMsStudio");
            //xbox.video Xbox Video
            studioApp.Add("xbox.video", null, "6affe59e-0467-4701-851f-7ac026e21665");
            //xbox.music Xbox Music
            studioApp.Add("xbox.music", null, "d2b6a184-da39-4c9a-9e0a-8b589b03dec0");
            //xbox.360.smartglass Xbox 360 SmartGlass
            studioApp.Add("xbox.360.smartglass", null, "b057fbe2-ceb1-470f-a7fe-09c862ca6dd9");
            //xbox.one.smartglass Xbox One SmartGlass
            studioApp.Add("xbox.one.smartglass", null, "a1a67817-26e6-482a-b673-e3e906d27a4e");
            //xbox.extras Xbox Extras
            studioApp.Add("xbox.extras", null, "31e9b772-1d92-e011-986b-78e7d1fa76f8");
            //sudoku
            studioApp.Add("sudoku", null, "a81bbf9c-530e-47be-bd86-c74c156a1d71");
            //wordament Wordament
            studioApp.Add("wordament", null, "c62201b4-e059-e011-854c-00237de2db9e");
            //minesweeper Minesweeper
            studioApp.Add("minesweeper", null, "0b00c4a3-eda9-e011-a53c-78e7d1fa76f8");
            //breeze Breeze
            studioApp.Add("breeze", null, "d45853b6-2c06-4888-89e9-4ade28d6e162");
            //flowerz Flowerz
            studioApp.Add("flowerz", null, "981750c8-24cc-df11-9eae-00237de2db9e");
            //halo.lite Halo: SA Lite
            studioApp.Add("halo.lite", null, "cf3f117d-d5a6-4e81-9786-56dd337b9b02");
            //alphajax AlphaJax
            studioApp.Add("alphajax", null, "204ab97d-3bd3-4aca-8a20-8f20fe536d47");
            
            //Microsoft Research
            AppGroup researchApp = new AppGroup("microsoft", "AppCategoryMsResearch");
            //network.speed.test Network Speed Test
            researchApp.Add("network.speed.test", null, "9b9ae06b-2961-41ef-987d-b09567cffe70");
            //office.remote Office Remote
            researchApp.Add("office.remote", null, "01f53e5a-7870-49cb-8afc-d6fab6d7a3cd");
            //socl socl
            researchApp.Add("socl", null, "19036adf-16ab-439d-b1a6-b4f66e0fad0c");
            //blink BLINK
            researchApp.Add("blink", null, "3e185ac7-2d21-4a74-9cad-3d4729509446");
            //face.swap Face Swap
            researchApp.Add("face.swap", null, "0f55f905-77d3-4e09-8f2a-d8a41c77a02b");
            //face.touch Face Touch
            researchApp.Add("face.touch", null, "37609c04-e8d6-4757-9f0b-f14652d5b37e");
            //face.mask Face Mask
            researchApp.Add("face.mask", null, "b0b8a3ae-a320-48b7-9ecd-7b38cf9a82f8");
            //academic.search Academic Search
            researchApp.Add("academic.search", null, "d9d2f110-37a3-e011-986b-78e7d1fa76f8");

            this.MsItems.Add(socialApp);
            this.MsItems.Add(bingApp);
            this.MsItems.Add(extraApp);
            this.MsItems.Add(studioApp);
            this.MsItems.Add(researchApp);

            this.IsMsDataLoaded = true;
        }

        public void LoadVsData()
        {
            //velostep
            AppGroup vsTryApp = new AppGroup("velostep", "AppCategoryTry");
            //chameleon
            vsTryApp.Add("chameleon", null, "d9561d4e-b2c1-42b0-8701-a7bb2f883605");
            //wowpad WowPad
            vsTryApp.Add("wowpad", null, "bfccb3de-2d02-44bc-a990-38aafee8f38e");
            
            AppGroup vsPaidApp = new AppGroup("velostep", "AppCategoryPaid");
            //ccplayer CCPlayer
            vsPaidApp.Add("ccplayer", null, "1765dc04-edc1-4c07-a850-da8b055a6362");
            //wowpad.mk WowPad MK
            vsPaidApp.Add("wowpad.mk", null, "7173f5d4-2f24-4ebc-bb2b-076c3a746286");
            //wowpad.tk WowPad TK
            vsPaidApp.Add("wowpad.tk", null, "4742a351-c45f-4d23-8dc5-48a71e5af1f5");

            AppGroup vsFreeApp = new AppGroup("velostep", "AppCategoryFree");
            //ccplayer.free CCPlayer Free
            vsFreeApp.Add("ccplayer.free", null, "877817e5-2496-4e1f-a2f8-8f92ef829a83");
            //manufacturer.apps Manufacturer Apps
            vsFreeApp.Add("manufacturer.apps", null, "9d79a05d-7866-4772-9e3b-636535b59bd9");
            //wowpad.amk WowPad AMK
            vsFreeApp.Add("wowpad.amk", null, "ff212f10-9484-457f-bd63-152565a6c10c");
            //wowpad.atk WowPad ATK
            vsFreeApp.Add("wowpad.atk", null, "9ba8da20-eaa4-4152-8605-2e17db0f3c6b");
            
            this.VsItems.Add(vsTryApp);
            this.VsItems.Add(vsPaidApp);
            this.VsItems.Add(vsFreeApp);

            this.IsVsDataLoaded = true;
        }
    }
}