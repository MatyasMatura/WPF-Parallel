using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public BitmapImage ImgPhoto { get { return _imgPhoto; } set { _imgPhoto = value; NotifyPropertyChanged(); } }
        private BitmapImage _imgPhoto;
        private int[,] pixels;

        public RelayCommand UploadImage { get; set; }
        public RelayCommand UnloadImage { get; set; }
        public RelayCommand ShadesOfGray { get; set; }
        public RelayCommand ColorReduction { get; set; }

        public MainViewModel()
        {
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
            ShadesOfGray = new RelayCommand(
                () =>
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
                },
                () => true
                );
            ColorReduction = new RelayCommand(
                () =>
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
