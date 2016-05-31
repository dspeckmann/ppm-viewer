using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Collections;

namespace PpmViewer
{
    static class Anymap
    {
        // Async because Anymap loading takes pretty long right now
        public async static Task<Bitmap> Load(string path)
        {
            return await Task<Bitmap>.Run(new Func<Bitmap>(() =>
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
                        throw new Exception(); // TODO: Invalid filename exception
                }
            }));
        }
        
        public static void Save(Image image, string path)
        {
            string extension = Path.GetExtension(path);
            switch(extension)
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
                    throw new Exception(); // TODO: Invalid filename exception
            }
        }

        private static Bitmap LoadFromPbm(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                // Check magic number
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '4') throw new Exception(); // TODO: Exception;

                // Read width, height and maximum color value while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();
                reader.ReadWhitespace();

                // Read the actual pixels
                Bitmap bitmap = new Bitmap(width, height);
                int x = 0, y = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    BitArray bits = new BitArray(reader.ReadByte());

                    foreach (bool bit in bits)
                    {
                        Color color;
                        if (bit)
                        {
                            color = Color.Black;
                        }
                        else
                        {
                            color = Color.White;
                        }

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
                // Check magic number
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '5') throw new Exception(); // TODO: Exception;

                // Read width, height and maximum color value while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();
                reader.ReadWhitespace();
                int maximumGrayValue = reader.ReadValue();
                reader.ReadWhitespace();

                // Read the actual pixels
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
                // Check magic number
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '6') throw new Exception(); // TODO: Exception;

                // Read width, height and maximum color value while ignoring whitespace and comments
                reader.ReadWhitespace();
                int width = reader.ReadValue();
                reader.ReadWhitespace();
                int height = reader.ReadValue();
                reader.ReadWhitespace();
                int maximumColorValue = reader.ReadValue();
                reader.ReadWhitespace();

                // Read the actual pixels
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
            throw new NotImplementedException();
        }

        private static void SaveToPgm(Image image, string path)
        {
            throw new NotImplementedException();
        }

        // TODO: SaveToPpm doesn't work
        private static void SaveToPpm(Image image, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.CreateNew)))
            using (Bitmap bitmap = new Bitmap(image))
            {
                // TODO: How to read/calculate maximum color value?
                writer.Write(ASCIIEncoding.ASCII.GetBytes(string.Format("P6\n{0} {1}\n{2}", bitmap.Width, bitmap.Height, 255)));

                for (int y = 0; y < bitmap.Height; y++)
                {
                    writer.Write('\n');
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

            while (c != ' ' && c != '\t' && c != '\n')
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

            while (c == ' ' || c == '\t' || c == '\n')
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
