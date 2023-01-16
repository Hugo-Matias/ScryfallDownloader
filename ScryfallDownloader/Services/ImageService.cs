using ImageMagick;

namespace ScryfallDownloader.Services
{
    public class ImageService
    {
        public byte[] ConvertToJpg(byte[] data, int quality)
        {
            using (var image = new MagickImage(data))
            {
                image.Format = MagickFormat.Jpeg;
                image.Quality = quality;

                return image.ToByteArray();
            }
        }

        public byte[] Rotate180(byte[] data)
        {
            using (var image = new MagickImage(data))
            {
                image.Rotate(180);
                return image.ToByteArray();
            }
        }
    }
}
