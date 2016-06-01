using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        private string information;
        public string Information
        {
            get { return information; }
            set
            {
                information = value;
                NotifyPropertyChanged("Information");
            }
        }


        /// <summary>
        /// 合成画像
        /// </summary>
        private WriteableBitmap compositImage;
        public WriteableBitmap CompositImage
        {
            get
            {
                return compositImage;
            }
            set
            {
                compositImage = value;
                NotifyPropertyChanged("CompositImage");
            }
        }

        /// <summary>
        /// クリアコマンド
        /// </summary>
        public ICommand Clear { get; set; }

        /// <summary>
        /// 合成コマンド
        /// </summary>
        public ICommand Composition { get; set; }

        private int width = 0;
        private int height = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() 
        {
            Images = new ObservableCollection<string>();

            Clear = new DelegateCommand(clearExecute, null);
            Composition = new DelegateCommand(compositionExucute, null);
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

        /// <summary>
        /// クリアボタン押下
        /// </summary>
        /// <param name="param"></param>
        private void clearExecute(object param)
        {
            Images.Clear();
            width = height = 0;
        }

        /// <summary>
        /// 合成ボタン押下
        /// </summary>
        /// <param name="param"></param>
        private void compositionExucute(object param)
        {
            Task.Run(() => 
            {
                Stopwatch sw = new Stopwatch();

                byte[] composit = null;
                byte[] current = null;
                int[] bright = null;
                int count = 0;

                sw.Start();
                foreach (string path in Images)
                {
                    WriteableBitmap wb = new WriteableBitmap(new BitmapImage(new Uri(path)));
                    wb.Lock();
                    if (composit == null)
                    {
                        composit = new byte[wb.BackBufferStride * wb.PixelHeight];
                        current = new byte[wb.BackBufferStride * wb.PixelHeight];
                        bright = new int[wb.PixelHeight * wb.PixelWidth];
                        Marshal.Copy(wb.BackBuffer, composit, 0, composit.Length);
                        for (int i = 0; i < bright.Length; i++)
                        {
                            int pos = i * 4;
                            bright[i] = brightness(composit[pos + 2], composit[pos + 1], composit[pos]);
                        }
                    }
                    else
                    {
                        Marshal.Copy(wb.BackBuffer, current, 0, current.Length);
                        for (int i = 0; i < current.Length / 4; i++)
                        {
                            int pos = i * 4;
                            int currentBright = brightness(current[pos + 2], current[pos + 1], current[pos]);
                            if (bright[i] < currentBright)
                            {
                                bright[i] = currentBright;
                                Array.Copy(current, pos, composit, pos, 4);
                            }
                        }
                    }
                    wb.Unlock();

                    count++;
                    Information = count + "/" + Images.Count;
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds + "ms");

                WriteableBitmap compositImage = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgr32, null);
                compositImage.Lock();
                Marshal.Copy(composit, 0, compositImage.BackBuffer, composit.Length);
                compositImage.AddDirtyRect(new Int32Rect(0, 0, compositImage.PixelWidth, compositImage.PixelHeight));
                compositImage.Unlock();
                compositImage.Freeze();

                CompositImage = compositImage;
            });
        }

        /// <summary>
        /// 輝度を計算する
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int brightness(byte r, byte g, byte b)
        {
            return (int)(0.299 * r + 0.587 * g + 0.114 * b);
        }
    }
}
