using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using System.Windows;

namespace Chameleon.View.Helper.Helper
{
    public class PageHelper
    {
        public static void SetDownloadImageList(IList downloadList)
        {
            ObservableCollection<DownloadItem> downloadItems = new ObservableCollection<DownloadItem>();
            if (downloadList != null)
            {
                foreach (AbstractPicture item in downloadList)
                {
                    DownloadItem di = new DownloadItem()
                    {
                        Guid = item.Guid,
                        AlumName = item.AlbumName,
                        FileName = item.Name,
                        SourceOrigin = item.SourceOrigin,
                        DownloadStatus = AppResources.ImageStatusPending
                    };

                    if (di.SourceOrigin != SourceOrigin.Phone)
                    {
                        di.ThumbnailPath = (item as WebPicture).Thumbnail.Path;
                        di.DownloadPath = (item as WebPicture).Path;
                    }

                    downloadItems.Add(di);
                }
            }
            PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST] = downloadItems;
        }

        public static void ProcessDownloadImageResult(ObservableCollection<DownloadItem> downloadList, LongListMultiSelector selector)
        {
            ChameleonAlbum album = selector.ItemsSource as ChameleonAlbum;

            //저장소에서 리턴값 삭제
            PhoneApplicationService.Current.State.Remove(Constants.DOWNLOAD_IMAGE_LIST);

            foreach (DownloadItem item in downloadList)
            {
                AbstractPicture pic = album.First(x => x.Guid == item.Guid) as AbstractPicture;
                if (pic.Guid == item.Guid)
                {
                    if (item.DownloadStatusCode == DownloadStatus.Completed)
                    {
                        //1. 해당 파일이 정상적으로 완료된 파일이라면 화면에서 삭제 처리
                        album.Remove(pic);
                    }
                    else if (item.DownloadStatusCode == DownloadStatus.DownloadFailed
                        || item.DownloadStatusCode == DownloadStatus.SaveFailed)
                    {
                        //2. 해당 파일이 실패한 경우 이면 
                        if (item.BlackListMode == BlackListMode.Domain)
                        {
                            //2.1 블랙리스트에 추가된 파일이라면 삭제
                            album.Remove(pic);
                        }
                        else
                        {
                            //2.2 아니면 실패 뱃지를 달아준다. 실패 원인을 표시한다.
                            pic.ProgressStatus = item.DownloadStatus.Replace(AppResources.MsgAddDomainFilter, string.Empty);
                        }
                    }
                }
            }
            //3. 선택 모드 해제 (여기서 즉시하면 에러가 발생하므로 딜레이 0.5초를 줘서 실행시킴)
            Deployment.Current.Dispatcher.BeginInvoke(async () =>
            {
                await Task.Delay(500);
                selector.EnforceIsSelectionEnabled = false;

            });
        }
    }
}
