using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public BitmapImage ImgPhoto { get { return _imgPhoto; } set { _imgPhoto = value; NotifyPropertyChanged(); } }
        private BitmapImage _imgPhoto;
        public BitmapImage OriginalImgPhoto { get; set; }
        private int[,] pixels;
        private object balanceLock = new object();
        
        public bool Parallel { get; set; }
        public RelayCommand UploadImage { get; set; }
        public RelayCommand UnloadImage { get; set; }
        public RelayCommand ShadesOfGray { get; set; }
        public RelayCommand ColorReduction { get; set; }
        public RelayCommand Red { get; set; }
        public RelayCommand Green { get; set; }
        public RelayCommand Blue { get; set; }
        public RelayCommand Original { get; set; }
        public RelayCommand Negative { get; set; }

        public MainViewModel()
        {
            Parallel = true;
            UploadImage = new RelayCommand(
                () =>
                {
                    OpenFileDialog op = new OpenFileDialog();
                    op.Title = "Select a picture";
                    op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                      "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                      "Portable Network Graphic (*.png)|*.png";
                    if (op.ShowDialog() == true)
                    {
                        ImgPhoto = new BitmapImage(new Uri(op.FileName));
                        OriginalImgPhoto = new BitmapImage(new Uri(op.FileName));
                    }
                },
                () => true
                );
            UnloadImage = new RelayCommand(
                () =>
                {
                    ImgPhoto = null;
                },
                () => true
                );
            Original = new RelayCommand(
                () =>
                {
                    ImgPhoto = OriginalImgPhoto;
                },
                () => true
                );
            ShadesOfGray = new RelayCommand(
                () =>
                {
                    if (Parallel == true)
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);

                        List<Thread> threads = new List<Thread>();
                        for (int i = 1; i < Environment.ProcessorCount + 1; i++)
                        {
                            lock (balanceLock)
                            {
                                double helperFrom = (pixels.GetLength(0) / Environment.ProcessorCount * (i - 1));
                                double helperTo = (pixels.GetLength(0) / Environment.ProcessorCount * i);
                                Thread oThread = new Thread(new ThreadStart(() =>
                                {
                                    for (int x = (int)helperFrom; x < (int)helperTo; x++)
                                    {
                                        for (int y = 0; y < pixels.GetLength(1); y++)
                                        {
                                            //pixels[x, y] = pixels[x, y] & color;
                                            int a = (int)(pixels[x, y] & 0xFF000000);
                                            int r = (pixels[x, y] >> 16) & 0xFF;
                                            int g = (pixels[x, y] >> 8) & 0xFF;
                                            int b = pixels[x, y] & 0xFF;
                                            int prumer = (r + g + b) / 3;
                                            pixels[x, y] = a + (prumer << 16) + (prumer << 8) + prumer;
                                        }
                                    }
                                }));
                                oThread.Start();
                                threads.Add(oThread);
                            }
                        }
                        foreach (var item in threads)
                        {
                            item.Join();
                        }

                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }
                    else
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                        //int color = (a << 24) + (r << 16) + (g << 8) + b;
                        for (int x = 0; x < pixels.GetLength(0); ++x)
                        {
                            for (int y = 0; y < pixels.GetLength(1); ++y)
                            {
                                //pixels[x, y] = pixels[x, y] & color;
                                int a = (int)(pixels[x, y] & 0xFF000000);
                                int r = (pixels[x, y] >> 16) & 0xFF;
                                int g = (pixels[x, y] >> 8) & 0xFF;
                                int b = pixels[x, y] & 0xFF;
                                int prumer = (r + g + b) / 3;
                                pixels[x, y] = a + (prumer << 16) + (prumer << 8) + prumer;
                            }
                        }
                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }
                },
                () => true
                );
            Red = new RelayCommand(
                () =>
                {
                    pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                    for (int x = 0; x < pixels.GetLength(0); ++x)
                    {
                        for (int y = 0; y < pixels.GetLength(1); ++y)
                        {
                            int color = pixels[x, y];
                            pixels[x, y] = (int)(color & 0xFFFF8888);
                        }
                    }
                    WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                    ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                },
                () => true
                );
            Green = new RelayCommand(
                () =>
                {
                    pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                    for (int x = 0; x < pixels.GetLength(0); ++x)
                    {
                        for (int y = 0; y < pixels.GetLength(1); ++y)
                        {
                            int color = pixels[x, y];
                            pixels[x, y] = (int)(color & 0xFF88FF88);
                        }
                    }
                    WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                    ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                },
                () => true
                );
            Blue = new RelayCommand(
                () =>
                {
                    pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                    for (int x = 0; x < pixels.GetLength(0); ++x)
                    {
                        for (int y = 0; y < pixels.GetLength(1); ++y)
                        {
                            int color = pixels[x, y];
                            pixels[x, y] = (int)(color & 0xFF8888FF);
                        }
                    }
                    WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                    ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                },
                () => true
                );
            ColorReduction = new RelayCommand(
                () =>
                {
                    if (Parallel == true)
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);

                        List<Thread> threads = new List<Thread>();
                        for (int i = 1; i < Environment.ProcessorCount + 1; i++)
                        {
                            lock (balanceLock)
                            {
                                double helperFrom = (pixels.GetLength(0) / Environment.ProcessorCount * (i - 1));
                                double helperTo = (pixels.GetLength(0) / Environment.ProcessorCount * i);
                                Thread oThread = new Thread(new ThreadStart(() =>
                                {
                                    for (int x = (int)helperFrom; x < (int)helperTo; x++)
                                    {
                                        for (int y = 0; y < pixels.GetLength(1); y++)
                                        {
                                            int color = pixels[x, y];
                                            pixels[x, y] = (int)(color & 0xFFC0C0C0);
                                        }
                                    }
                                }));
                                oThread.Start();
                                threads.Add(oThread);
                            }
                        }
                        foreach (var item in threads)
                        {
                            item.Join();
                        }

                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }
                    else
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                        for (int x = 0; x < pixels.GetLength(0); ++x)
                        {
                            for (int y = 0; y < pixels.GetLength(1); ++y)
                            {
                                int color = pixels[x, y];
                                pixels[x, y] = (int)(color & 0xFFC0C0C0);
                            }
                        }
                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }                    
                },
                () => true
                );

            //Vlastni filter

            Negative = new RelayCommand(
                () =>
                {
                    if (Parallel == true)
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);

                        List<Thread> threads = new List<Thread>();
                        for (int i = 1; i < Environment.ProcessorCount + 1; i++)
                        {
                            lock (balanceLock)
                            {
                                double helperFrom = (pixels.GetLength(0) / Environment.ProcessorCount * (i - 1));
                                double helperTo = (pixels.GetLength(0) / Environment.ProcessorCount * i);
                                Thread oThread = new Thread(new ThreadStart(() =>
                                {
                                    for (int x = (int)helperFrom; x < (int)helperTo; x++)
                                    {
                                        for (int y = 0; y < pixels.GetLength(1); y++)
                                        {
                                            int color = pixels[x, y];
                                            pixels[x, y] = (int)(0xFFFFFFFF - color);
                                        }
                                    }
                                }));
                                oThread.Start();
                                threads.Add(oThread);
                            }
                        }
                        foreach (var item in threads)
                        {
                            item.Join();
                        }

                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }
                    else
                    {
                        pixels = Array2DBMIConverter.BitmapImageToArray2D(ImgPhoto);
                        for (int x = 0; x < pixels.GetLength(0); x++)
                        {
                            for (int y = 0; y < pixels.GetLength(1); y++)
                            {
                                int color = pixels[x, y];
                                pixels[x, y] = (int)(0xFFFFFFFF - color);
                            }
                        }
                        WriteableBitmap wbm = Array2DBMIConverter.Array2DToWriteableBitmap(pixels, ImgPhoto);
                        ImgPhoto = Array2DBMIConverter.ConvertWriteableBitmapToBitmapImage(wbm);
                    }
                },
                () => true
                );
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}