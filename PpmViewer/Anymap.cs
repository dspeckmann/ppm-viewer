using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections;

namespace PpmViewer
{
    static class Anymap
    {
        /// <summary>
        /// Loads an anymap file into a bitmap asynchronously.
        /// </summary>
        /// <param name="path">The path of the anymap file to load.</param>
        /// <returns>The task that delivers a bitmap containing the loaded image.</returns>
        public static async Task<Bitmap> LoadAsync(string path)
        {
            return await Task<Bitmap>.Run(new Func<Bitmap>(() =>
            {
                return Load(path);
            }));
        }

        /// <summary>
        /// Loads an anymap file into a bitmap.
        /// </summary>
        /// <param name="path">The path of the anymap file to load.</param>
        /// <returns>A bitmap containing the loaded image.</returns>
        public static Bitmap Load(string path)
        {
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".pbm":
                    return LoadFromPbm(path);
                case ".pgm":
                    return LoadFromPgm(path);
                case ".ppm":
                    return LoadFromPpm(path);
                default:
                    throw new Exception("Unknown file extension.");
            }
        }
        /// <summary>
        /// Saves an image to an anymap file.
        /// </summary>
        /// <param name="image">The image to save.</param>
        /// <param name="path">The path where the anymap file should be saved.</param>
        public static void Save(Image image, string path)
        {
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".pbm":
                    SaveToPbm(image, path);
                    break;
                case ".pgm":
                    SaveToPgm(image, path);
                    break;
                case ".ppm":
                    SaveToPpm(image, path);
                    break;
                default:
                    throw new Exception("Unknown file extension.");
            }
        }

        private static Bitmap LoadFromPbm(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                // Check magic number P4
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '4') throw new Exception("Invalid PBM file.");

                // Read width and height while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();

                // Read the actual pixel color values
                Bitmap bitmap = new Bitmap(width, height);
                int x = 0, y = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    BitArray bits = new BitArray(reader.ReadBytes(1));
                    foreach (bool bit in bits)
                    {
                        Color color = bit ? Color.Black : Color.White;
                        bitmap.SetPixel(x, y, color);

                        if (++x == width)
                        {
                            x = 0;
                            y++;
                            break;
                        }
                    }
                }

                return bitmap;
            }
        }

        private static Bitmap LoadFromPgm(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                // Check magic number P5
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '5') throw new Exception("Invalid PGM file.");

                // Read width, height and maximum gray value while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();
                reader.ReadWhitespace();
                int maximumGrayValue = reader.ReadValue();

                // Read the actual pixel color values
                Bitmap bitmap = new Bitmap(width, height);
                int i = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    byte grayValue = reader.ReadByte();
                    Color color = Color.FromArgb(grayValue, grayValue, grayValue);
                    bitmap.SetPixel(i % width, (int)Math.Floor((double)i / width), color);
                    i++;
                }

                return bitmap;
            }
        }
        
        private static Bitmap LoadFromPpm(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                // Check magic number P6
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '6') throw new Exception("Invalid PPM file.");

                // Read width, height and maximum color value while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();
                reader.ReadWhitespace();
                int maximumColorValue = reader.ReadValue();
                
                // Read the actual pixel color values
                Bitmap bitmap = new Bitmap(width, height);
                int i = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    byte[] rgb = reader.ReadBytes(3);
                    Color color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                    bitmap.SetPixel(i % width, (int)Math.Floor((double)i / width), color);
                    i++;
                }

                return bitmap;
            }
        }

        private static void SaveToPbm(Image image, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew)))
            using (Bitmap bitmap = new Bitmap(image))
            {
                // Write magic number P4, width and height
                writer.Write(ASCIIEncoding.ASCII.GetBytes(string.Format("P4\n{0} {1}\n", bitmap.Width, bitmap.Height)));

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x += 8)
                    {
                        BitArray bits = new BitArray(8);
                        for (int i = 0; i < (bitmap.Width - x - i) && i < 8; i++)
                        {
                            Color c = bitmap.GetPixel(x + i, y);
                            bits[i] = c.GetBrightness() < 0.5 ? true : false;
                        }
                        byte[] buffer = new byte[1];
                        bits.CopyTo(buffer, 0);
                        writer.Write(buffer);
                    }
                }
            }
        }

        private static void SaveToPgm(Image image, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew)))
            using (Bitmap bitmap = new Bitmap(image))
            {
                // TODO: How to read/calculate maximum gray value?
                // Write magic number P5, width, height and maximum gray value
                writer.Write(ASCIIEncoding.ASCII.GetBytes(string.Format("P5\n{0} {1}\n{2}\n", bitmap.Width, bitmap.Height, 255)));

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color c = bitmap.GetPixel(x, y);
                        writer.Write((byte)(c.GetBrightness() * 255));
                    }
                }
            }
        }
        
        private static void SaveToPpm(Image image, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew)))
            using (Bitmap bitmap = new Bitmap(image))
            {
                // TODO: How to read/calculate maximum color value?
                // Write magic number P6, width, height and maximum color value
                writer.Write(ASCIIEncoding.ASCII.GetBytes(string.Format("P6\n{0} {1}\n{2}\n", bitmap.Width, bitmap.Height, 255)));

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color c = bitmap.GetPixel(x, y);
                        writer.Write(c.R);
                        writer.Write(c.G);
                        writer.Write(c.B);
                    }
                }
            }
        }
        
        /// <summary>
        /// Reads from the BinaryReader until it encounters whitespace and tries to convert the read bytes to an integer.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <returns>The read integer.</returns>
        private static int ReadValue(this BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char c = reader.ReadChar();

            while (c != ' ' && c != '\t' && c != '\n' && c != '\0')
            {
                builder.Append(c);
                c = reader.ReadChar();
            }

            return int.Parse(builder.ToString());
        }

        /// <summary>
        /// Reads from the BinaryReader until it encounters non-whitespace characters.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        private static void ReadWhitespace(this BinaryReader reader)
        {
            char c = reader.ReadChar();

            while (c == ' ' || c == '\t' || c == '\n' || c == '\0')
            {
                c = reader.ReadChar();

                // When we encounter a comment, read until the end of it (has to be a newline)
                if (c == '#')
                {
                    reader.ReadChar();
                    while (c != '\n')
                    {
                        c = reader.ReadChar();
                    }
                }
            }

            // When we encounter a non-whitespace character again we have to go back one byte, so it can be read by other functions
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
        }
    }
}
