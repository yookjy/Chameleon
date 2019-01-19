using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class About
    {
        public About()
        {
            AddVersions();
        }

        private void AddVersions()
        {
            VersionList = new ObservableCollection<Version<VersionContent>>();


            //빙 다운로드 알림
            //빙 자동 다운로더
            //다음 락스크린 타일
            //알림 터치시 고화질 락스크린으로 변경 기능 및 앱 진입시 현재 락스크린을 고화질로 변경
            //메모리 판단해서 락스크린 해상도 높일 수 있는지... 테스트해서 적용
            //라이브러리로 공통 파일들 교체
            
            //1GB이상의 메모리에서 고품질 이미지 표시 기능 추가
            //Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory
            //빙/나사 자동/반자동 다운로드
            //반자동 고품질 이미지 표시 기능


            Version<VersionContent> version244 = new Version<VersionContent>("2.4.4");
            VersionList.Add(version244);
            version244.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory24401));
            version244.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory24402));
            
            //라이브타일 - 투명 타일 지원
            Version<VersionContent> version243 = new Version<VersionContent>("2.4.3");
            VersionList.Add(version243);
            version243.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory24301));

            Version<VersionContent> version242 = new Version<VersionContent>("2.4.2");
            VersionList.Add(version242);
            version242.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory24201));

            Version<VersionContent> version241 = new Version<VersionContent>("2.4.1");
            VersionList.Add(version241);
            version241.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory24101));

            Version<VersionContent> version240 = new Version<VersionContent>("2.4.0");
            VersionList.Add(version240);
            version240.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory24002));
            version240.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory24001));

            Version<VersionContent> version238 = new Version<VersionContent>("2.3.8");
            VersionList.Add(version238);
            version238.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23801));

            Version<VersionContent> version237 = new Version<VersionContent>("2.3.7");
            VersionList.Add(version237);
            version237.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23701));

            Version<VersionContent> version236 = new Version<VersionContent>("2.3.6");
            VersionList.Add(version236);
            version236.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23601));
            version236.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory23602));

            /*
             * 2.3.5
             * 라이브타일 - 완전 충전 표시 옵션 (Full, 99)
             * 노키아앱 - call+SMS Filter, glance, App Folder 추가
             */
            Version<VersionContent> version235 = new Version<VersionContent>("2.3.5");
            VersionList.Add(version235);
            version235.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23501));
            version235.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23502));

            /*
             * 2.3.2
             * 이미지 crop 버그 수정
             */
            Version<VersionContent> version232 = new Version<VersionContent>("2.3.2");
            VersionList.Add(version232);
            version232.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory23201));

            /*
             * 2.3.1
             * 라이브타일 업데이트 버그
             * 사소한 UI버그
             */
            Version<VersionContent> version231 = new Version<VersionContent>("2.3.1");
            VersionList.Add(version231);
            version231.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory23101));
            version231.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory23102));

            /*2.3.0
             * 플래시 기능 추가
             * 사소한 UI버그
             * 잠금화면 변경 오류 표시 
             */
            Version<VersionContent> version230 = new Version<VersionContent>("2.3.0");
            VersionList.Add(version230);
            version230.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory23001));
            version230.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory23002));

            /*2.2.1
             * 라이브타일, 잠금화면 업데이트 버그 수정
             */
            Version<VersionContent> version221 = new Version<VersionContent>("2.2.1");
            VersionList.Add(version221);
            version221.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory22101));

            /*
             * 2.2.0
             * 제조사 앱 메인 화면 메뉴
             *  appbar menu로 이동
             *  Microsoft app list 추가
             *  Velostep app list 추가
             *  Manufacturer app 추가
             *   Nokia app => NFC Writer 추가
             *   Samsung app => Family Story 추가
             * 
             * 네트워크 접속 체크 오류 
             *  데이터 네트워크 연결 확인 메세지 출력
             *  락스크린 / 라이브타일에서는 이전 데이터 사용
             * 
             * 날씨 조회중 화면 로딩 표시
             */ 
            Version<VersionContent> version220 = new Version<VersionContent>("2.2.0");
            VersionList.Add(version220);
            version220.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory22001));
            version220.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory22002));
            version220.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory22003));           

            /*
             * 2.1.2
             * -10도 이하에서 온도 글씨가 잘려 보임
             * 설정 메뉴 진입시 간혹 앱이 종료됨
             * 
             */
            Version<VersionContent> version212 = new Version<VersionContent>("2.1.2");
            VersionList.Add(version212);
            version212.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory21201));
            version212.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory21202));

            /*
             * 프리뷰 백그라운드 화면이 투명이라 이전 페이지가 보이는 버그 수정
             * 중복된 리소스 제거로 앱 용량 최적화 (1/2 => 5MB)
             * 심플 아이콘 모드 추가 (기본 아이콘과 선택적 적용가능)
             * 시스템 앱 숨김시 날씨메뉴에 앱바가 표시되지 않는 버그 수정
             * 시스템앱 : 노키아 뮤직 => 믹스 
             * 설정 버튼 탭 : 각 메뉴에 맞는 설정 피벗 페이지로 자동 이동 처리
             */
            Version<VersionContent> version210 = new Version<VersionContent>("2.1.0");
            VersionList.Add(version210);
            version210.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory21001));
            version210.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory21002));
            //version210.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory21003)); 
            version210.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory21004));
            version210.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory21005));
            version210.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory21006));
            
            Version<VersionContent> version201 = new Version<VersionContent>("2.0.1");
            VersionList.Add(version201);
            version201.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory20101));

            Version<VersionContent> version200 = new Version<VersionContent>("2.0.0");
            VersionList.Add(version200);
            version200.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory20001));

            version200.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory20002));
            version200.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory20003));

            version200.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory20004));
            version200.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory20005));
            version200.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory20006));

            version200.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory20007));
            version200.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory20008));
            version200.Add(new VersionContent(VersionContentType.FIX, AppResources.UpdateHistory20009));

            Version<VersionContent> version110 = new Version<VersionContent>("1.1.0");
            VersionList.Add(version110);
            version110.Add(new VersionContent(VersionContentType.MOD, AppResources.UpdateHistory11101));
            version110.Add(new VersionContent(VersionContentType.IMP, AppResources.UpdateHistory11102));

            Version<VersionContent> version100 = new Version<VersionContent>("1.0.0");
            VersionList.Add(version100);
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10001));
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10002));
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10003));
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10004));
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10005));
            version100.Add(new VersionContent(VersionContentType.NEW, AppResources.UpdateHistory10006));
        }

        public ObservableCollection<Version<VersionContent>> VersionList { get; private set; }

        public string CurrentVersion
        {
            get
            {
                Version version = new AssemblyName(Assembly.Load("Chameleon").FullName).Version;
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        public BitmapImage RateReview
        {
            get
            {
                return GetImage("appbar.star.png");
            }
        }

        public BitmapImage Facebook
        {
            get
            {
                return GetImage("appbar.social.facebook.variant.png");
            }
        }

        public BitmapImage Feedback
        {
            get
            {
                return GetImage("appbar.customerservice.png");
            }
        }

        public BitmapImage ShareApp
        {
            get
            {
                return GetImage("appbar.share.png");
            }
        }

        public BitmapImage UserVoice
        {
            get
            {
                return GetImage("appbar.reply.email.png");
            }
        }

        private BitmapImage GetImage(string name)
        {
            BitmapImage bi = new BitmapImage();
            bi.UriSource = new Uri(PathHelper.GetFullPath(name), UriKind.Relative);
            return bi;;
        }


    }
}
