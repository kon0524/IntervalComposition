using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace IntervalComposition.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// 入力画像リスト
        /// </summary>
        public ObservableCollection<string> Images { get; set; }

        private int width = 0;
        private int height = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() 
        {
            Images = new ObservableCollection<string>();
        }

        /// <summary>
        /// 画像を追加します
        /// </summary>
        /// <param name="pathes"></param>
        public void AddImages(string[] pathes)
        {
            foreach (string path in pathes)
            {
                // 拡張子のチェック
                string extention = System.IO.Path.GetExtension(path).ToUpper();
                if (extention != ".JPEG" && extention != ".JPG") continue;

                // サイズチェック
                BitmapImage bImg = new BitmapImage(new Uri(path));
                if (width == 0 || height == 0)
                {
                    width = bImg.PixelWidth;
                    height = bImg.PixelHeight;
                    Images.Add(path);
                }
                else
                {
                    if (width == bImg.PixelWidth && height == bImg.PixelHeight)
                    {
                        Images.Add(path);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }
}
