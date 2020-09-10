//
// Copyright (c) 2010-2020 Antmicro
// Copyright (c) 2011-2015 Realtime Embedded
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;
using Antmicro.Migrant;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    public class SevenSegments : IGPIOReceiver
    {
        public SevenSegments(uint digitsCount = 1, bool invertSegments = false, bool invertDigits = false)
        {
            this.digitsCount = digitsCount;
            this.invertSegments = invertSegments;
            this.invertDigits = invertDigits;

            this.rows = new bool[8];
            this.cols = new bool[digitsCount];
            this.state = new byte[digitsCount];

            sync = new object();
        }

        public void OnGPIO(int number, bool value)
        {
            if(number < 8)
            {
                rows[number] = invertSegments ? !value : value;
            }
            else if(number - 8 < digitsCount)
            {
                cols[number - 8] = invertDigits ? !value : value;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            State = BuildImage();
        }

        public void Reset()
        {
            for(int index = 0; index < 8; index++)
            {
                rows[index] = invertSegments;
            }
            for(int index = 0; index < digitsCount; index++)
            {
                cols[index] = invertDigits;
            }
        }

        [field: Transient]
        public event Action<IPeripheral, byte[]> StateChanged;

        public string Image
        {
            get
            {
                StringBuilder result = new StringBuilder();

                foreach(byte b in state)
                { 
                    if(Digits.ContainsKey((byte)(b & ~SEGMENT_DOT)))
                    {
                        result.Append(Digits[(byte)(b & ~SEGMENT_DOT)]);
                    }
                    else if (b == 0)
                    {
                        result.Append("_");
                    }
                    else if (b != SEGMENT_DOT)
                    {
                        result.Clear();
                        break;
                    }
                    if ((b & SEGMENT_DOT) != 0)
                    {
                        result.Append(".");
                    }
                }

                if (result.Length == 0)
                {
                    foreach(byte b in state)
                        result.AppendFormat("0x{0:x2} ", b);

                }

                return result.ToString();
            }
        }

        public byte[] State 
        { 
            get => state;

            private set
            {
                lock(sync)
                {
                    if(value == state)
                    {
                        return;
                    }

                    state = value;
                    StateChanged?.Invoke(this, state);
                    this.Log(LogLevel.Noisy, "Seven Segments state changed to {0}", Image);
                }
            }
        }

        private byte[] BuildImage()
        {
            byte segments = 0;
            for(int index = 0; index < 8; index++)
            {
                if(rows[index])
                {
                    segments |= (byte)(1 << index);
                }
            }

            byte[] image = new byte[digitsCount];
            for(int index = 0; index < digitsCount; index++)
            {
                if(cols[index])
                {
                    image[index] = segments;
                }
                else
                {
                    image[index] = 0;
                }
            }
            return image;
        }

        private bool[] rows;
        private bool[] cols;
        private byte[] state;
        private readonly uint digitsCount;
        private readonly bool invertSegments;
        private readonly bool invertDigits;
        private readonly object sync;

        private const byte SEGMENT_A = (1 << 0);
        private const byte SEGMENT_B = (1 << 1);
        private const byte SEGMENT_C = (1 << 2);
        private const byte SEGMENT_D = (1 << 3);
        private const byte SEGMENT_E = (1 << 4);
        private const byte SEGMENT_F = (1 << 5);
        private const byte SEGMENT_G = (1 << 6);
        private const byte SEGMENT_DOT = (1 << 6);

        private static readonly Dictionary<byte, string> Digits = new Dictionary<byte, string>()
        {
            { SEGMENT_A | SEGMENT_B | SEGMENT_C | SEGMENT_D | SEGMENT_E | SEGMENT_F             , "0" },
            { SEGMENT_B | SEGMENT_C                                                             , "1" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_D | SEGMENT_E | SEGMENT_G                         , "2" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_C | SEGMENT_D | SEGMENT_G                         , "3" },
            { SEGMENT_B | SEGMENT_C | SEGMENT_F | SEGMENT_G                                     , "4" },
            { SEGMENT_A | SEGMENT_C | SEGMENT_D | SEGMENT_F | SEGMENT_G                         , "5" },
            { SEGMENT_A | SEGMENT_C | SEGMENT_D | SEGMENT_E | SEGMENT_F | SEGMENT_G             , "6" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_C                                                 , "7" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_C | SEGMENT_D | SEGMENT_E | SEGMENT_F | SEGMENT_G , "8" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_C | SEGMENT_F | SEGMENT_G                         , "9" },
            { SEGMENT_A | SEGMENT_B | SEGMENT_C | SEGMENT_E | SEGMENT_F | SEGMENT_G             , "A" },
            { SEGMENT_C | SEGMENT_D | SEGMENT_E | SEGMENT_F | SEGMENT_G                         , "B" },
            { SEGMENT_A | SEGMENT_D | SEGMENT_E | SEGMENT_F                                     , "C" },
            { SEGMENT_B | SEGMENT_C | SEGMENT_D | SEGMENT_E | SEGMENT_G                         , "D" },
            { SEGMENT_A | SEGMENT_D | SEGMENT_E | SEGMENT_F | SEGMENT_G                         , "E" },
            { SEGMENT_A | SEGMENT_E | SEGMENT_F | SEGMENT_G                                     , "F" },
        };
    }
}

