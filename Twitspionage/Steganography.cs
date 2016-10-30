using System.Drawing;

namespace Twitspionage
{
    public static class Steganography
    {
        private const int LsbBit = 2;
        private const int BitLength = 8;
        private const int RgbLength = 3;
        private enum State
        {
            Processing,
            Finished
        };

        public static Bitmap Embed(string text, Bitmap bmp)
        {
            var state = State.Processing;
            var charIndex = 0;
            var charValue = 0;
            long pixelIndex = 0;
            var numberOfZeros = 0;

            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    var pixel = bmp.GetPixel(x, y);

                    var r = pixel.R - pixel.R % LsbBit;
                    var g = pixel.G - pixel.G % LsbBit;
                    var b = pixel.B - pixel.B % LsbBit;

                    for (var n = 0; n < RgbLength; n++)
                    {
                        if (pixelIndex % BitLength == 0)
                        {
                            if (state == State.Finished && numberOfZeros == BitLength)
                            {
                                if ((pixelIndex - 1) % RgbLength < LsbBit)
                                {
                                    bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                state = State.Finished;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (pixelIndex % RgbLength)
                        {
                            case 0:
                                if (state == State.Processing)
                                {
                                    r += charValue % LsbBit;

                                    charValue /= LsbBit;
                                }
                                break;
                            case 1:
                                if (state == State.Processing)
                                {
                                    g += charValue % LsbBit;

                                    charValue /= LsbBit;
                                }
                                break;
                            case LsbBit:
                                if (state == State.Processing)
                                {
                                    b += charValue % LsbBit;

                                    charValue /= LsbBit;
                                }

                                bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                                break;
                        }

                        pixelIndex++;

                        if (state == State.Finished)
                        {
                            numberOfZeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string Extract(Bitmap bmp)
        {
            var colorUnitIndex = 0;
            var charValue = 0;

            var extractedText = string.Empty;

            for (var i = 0; i < bmp.Height; i++)
            {
                for (var j = 0; j < bmp.Width; j++)
                {
                    var pixel = bmp.GetPixel(j, i);
                    
                    for (var n = 0; n < RgbLength; n++)
                    {
                        switch (colorUnitIndex % RgbLength)
                        {
                            case 0:
                                charValue = charValue * LsbBit + pixel.R % LsbBit;
                                break;
                            case 1:
                                charValue = charValue * LsbBit + pixel.G % LsbBit;
                                break;
                            case LsbBit:
                                charValue = charValue * LsbBit + pixel.B % LsbBit;
                                break;
                        }

                        colorUnitIndex++;
                        
                        if (colorUnitIndex % BitLength != 0) continue;

                        var copyOfCharValue = charValue;
                        var newCharValue = 0;

                        for (var b = 0; b < BitLength; b++)
                        {
                            newCharValue = newCharValue * LsbBit + copyOfCharValue % LsbBit;

                            copyOfCharValue /= LsbBit;
                        }

                        charValue = newCharValue;

                        if (charValue == 0)
                        {
                            return extractedText;
                        }

                        extractedText += ((char)charValue).ToString();
                    }
                }
            }

            return extractedText;
        }
    }
}