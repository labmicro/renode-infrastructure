//
// Copyright (c) 2010-2018 Antmicro
// Copyright (c) 2011-2015 Realtime Embedded
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;

namespace Antmicro.Renode.Peripherals.GPIOPort
{
    [AllowedTranslations(AllowedTranslation.WordToDoubleWord)]

    public class LPC43xx_GPIO : BaseGPIOPort, IDoubleWordPeripheral
    {
        public LPC43xx_GPIO(Machine machine, uint port = 0) : base(machine, 32 * puertos)
        {
            this.dir = new UInt32[puertos];
            this.pin = new UInt32[puertos];
            this.mask = new UInt32[puertos];
            Reset();
        }

        public uint ReadDoubleWord(long offset)
        {
            uint returnValue;
            byte port = (byte)(offset / 4 % 8);

            switch((Registers)offset)
            {
                case Registers.GPIO_DIR0:
                case Registers.GPIO_DIR1:
                case Registers.GPIO_DIR2:
                case Registers.GPIO_DIR3:
                case Registers.GPIO_DIR4:
                case Registers.GPIO_DIR5:
                case Registers.GPIO_DIR6:
                case Registers.GPIO_DIR7:
                    returnValue = dir[port];
                    break;

                case Registers.GPIO_MASK0:
                case Registers.GPIO_MASK1:
                case Registers.GPIO_MASK2:
                case Registers.GPIO_MASK3:
                case Registers.GPIO_MASK4:
                case Registers.GPIO_MASK5:
                case Registers.GPIO_MASK6:
                case Registers.GPIO_MASK7:
                    returnValue = mask[port];
                    break;

                case Registers.GPIO_MPIN0:
                case Registers.GPIO_MPIN1:
                case Registers.GPIO_MPIN2:
                case Registers.GPIO_MPIN3:
                case Registers.GPIO_MPIN4:
                case Registers.GPIO_MPIN5:
                case Registers.GPIO_MPIN6:
                case Registers.GPIO_MPIN7:
                    returnValue = ReadInputs(port);
                    BitHelper.AndWithNot(ref returnValue, mask[port], 0, 32);
                    break;

                case Registers.GPIO_PIN0:
                case Registers.GPIO_PIN1:
                case Registers.GPIO_PIN2:
                case Registers.GPIO_PIN3:
                case Registers.GPIO_PIN4:
                case Registers.GPIO_PIN5:
                case Registers.GPIO_PIN6:
                case Registers.GPIO_PIN7:
                    returnValue = ReadInputs(port);
                    break;

                case Registers.GPIO_SET0:
                case Registers.GPIO_SET1:
                case Registers.GPIO_SET2:
                case Registers.GPIO_SET3:
                case Registers.GPIO_SET4:
                case Registers.GPIO_SET5:
                case Registers.GPIO_SET6:
                case Registers.GPIO_SET7:

                case Registers.GPIO_CLR0:
                case Registers.GPIO_CLR1:
                case Registers.GPIO_CLR2:
                case Registers.GPIO_CLR3:
                case Registers.GPIO_CLR4:
                case Registers.GPIO_CLR5:
                case Registers.GPIO_CLR6:
                case Registers.GPIO_CLR7:

                case Registers.GPIO_NOT0:
                case Registers.GPIO_NOT1:
                case Registers.GPIO_NOT2:
                case Registers.GPIO_NOT3:
                case Registers.GPIO_NOT4:
                case Registers.GPIO_NOT5:
                case Registers.GPIO_NOT6:
                case Registers.GPIO_NOT7:
                    returnValue = pin[port];
                    break;

                default:
                    this.LogUnhandledRead(offset);
                    returnValue = 0;
                    break;
            }
            return returnValue;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            byte port = (byte)(offset / 4 % 8);
            UInt32 newValue = pin[port];

            switch((Registers)offset)
            {
                case Registers.GPIO_DIR0:
                case Registers.GPIO_DIR1:
                case Registers.GPIO_DIR2:
                case Registers.GPIO_DIR3:
                case Registers.GPIO_DIR4:
                case Registers.GPIO_DIR5:
                case Registers.GPIO_DIR6:
                case Registers.GPIO_DIR7:
                    dir[port] = value;
                    break;

                case Registers.GPIO_MASK0:
                case Registers.GPIO_MASK1:
                case Registers.GPIO_MASK2:
                case Registers.GPIO_MASK3:
                case Registers.GPIO_MASK4:
                case Registers.GPIO_MASK5:
                case Registers.GPIO_MASK6:
                case Registers.GPIO_MASK7:
                    mask[port] = value;
                    break;

                case Registers.GPIO_PIN0:
                case Registers.GPIO_PIN1:
                case Registers.GPIO_PIN2:
                case Registers.GPIO_PIN3:
                case Registers.GPIO_PIN4:
                case Registers.GPIO_PIN5:
                case Registers.GPIO_PIN6:
                case Registers.GPIO_PIN7:
                    UpdateOutputs(port, value);
                    break;

                case Registers.GPIO_SET0:
                case Registers.GPIO_SET1:
                case Registers.GPIO_SET2:
                case Registers.GPIO_SET3:
                case Registers.GPIO_SET4:
                case Registers.GPIO_SET5:
                case Registers.GPIO_SET6:
                case Registers.GPIO_SET7:
                    BitHelper.OrWith(ref newValue, value, 0, 32);
                    UpdateOutputs(port, newValue);
                    break;

                case Registers.GPIO_CLR0:
                case Registers.GPIO_CLR1:
                case Registers.GPIO_CLR2:
                case Registers.GPIO_CLR3:
                case Registers.GPIO_CLR4:
                case Registers.GPIO_CLR5:
                case Registers.GPIO_CLR6:
                case Registers.GPIO_CLR7:
                    BitHelper.AndWithNot(ref newValue, value, 0, 32);
                    UpdateOutputs(port, newValue);
                    break;

                case Registers.GPIO_NOT0:
                case Registers.GPIO_NOT1:
                case Registers.GPIO_NOT2:
                case Registers.GPIO_NOT3:
                case Registers.GPIO_NOT4:
                case Registers.GPIO_NOT5:
                case Registers.GPIO_NOT6:
                case Registers.GPIO_NOT7:
                    BitHelper.XorWith(ref newValue, value, 0, 32);
                    UpdateOutputs(port, newValue);
                    break;

                default:
                    this.LogUnhandledWrite(offset, value);
                    break;
            }
            return;
        }

        public override void OnGPIO(int number, bool value)
        {
            base.OnGPIO(number, value);
            Connections[number].Set(value);
        }

        public override void Reset()
        {
            base.Reset();
            for(int index = 0; index < puertos; index++)
            {
                this.dir[index] = 0;
                this.pin[index] = 0;
                this.mask[index] = 0;
            }
        }

        private UInt32 ReadInputs(byte port)
        {
            UInt32 returnValue = 0;
            for(byte bit = 0; bit < 32; bit++)
            {
                BitHelper.SetBit(ref returnValue, bit, State[32 * port + bit]);
            }
            return returnValue;
        }

        private void UpdateOutputs(byte port, UInt32 value)
        {
            UInt32 changes = pin[port] ^ value;
            pin[port] = value;

            for(byte bit = 0; bit < 32; bit++)
            {
                if(BitHelper.IsBitSet(dir[port], bit) && (BitHelper.IsBitSet(changes, bit)))
                {
                    if(BitHelper.IsBitSet(pin[port], bit))
                    {
                        Connections[32 * port + bit].Set();
                        State[32 * port + bit] = true;
                    }
                    else
                    {
                        Connections[32 * port + bit].Unset();
                        State[32 * port + bit] = false;
                    }
                }
            }
        }

        private UInt32[] pin;
        private UInt32[] dir;
        private UInt32[] mask;

        // Source: Chapter 7.4 in RM0090 Cortex M4 Reference Manual (Doc ID 018909 Rev 4)
        // for STM32F40xxx, STM32F41xxx, STM32F42xxx, STM32F43xxx advanced ARM-based 32-bit MCUs
        private enum Registers
        {
            GPIO_DIR0 = 0x2000,
            GPIO_DIR1 = 0x2004,
            GPIO_DIR2 = 0x2008,
            GPIO_DIR3 = 0x200C,
            GPIO_DIR4 = 0x2010,
            GPIO_DIR5 = 0x2014,
            GPIO_DIR6 = 0x2018,
            GPIO_DIR7 = 0x201C,

            GPIO_MASK0 = 0x2080,
            GPIO_MASK1 = 0x2084,
            GPIO_MASK2 = 0x2088,
            GPIO_MASK3 = 0x208C,
            GPIO_MASK4 = 0x2090,
            GPIO_MASK5 = 0x2094,
            GPIO_MASK6 = 0x2098,
            GPIO_MASK7 = 0x209C,

            GPIO_PIN0 = 0x2100,
            GPIO_PIN1 = 0x2104,
            GPIO_PIN2 = 0x2108,
            GPIO_PIN3 = 0x210C,
            GPIO_PIN4 = 0x2110,
            GPIO_PIN5 = 0x2114,
            GPIO_PIN6 = 0x2118,
            GPIO_PIN7 = 0x211C,

            GPIO_MPIN0 = 0x2180,
            GPIO_MPIN1 = 0x2184,
            GPIO_MPIN2 = 0x2188,
            GPIO_MPIN3 = 0x218C,
            GPIO_MPIN4 = 0x2190,
            GPIO_MPIN5 = 0x2194,
            GPIO_MPIN6 = 0x2198,
            GPIO_MPIN7 = 0x219C,

            GPIO_SET0 = 0x2200,
            GPIO_SET1 = 0x2204,
            GPIO_SET2 = 0x2208,
            GPIO_SET3 = 0x220C,
            GPIO_SET4 = 0x2210,
            GPIO_SET5 = 0x2214,
            GPIO_SET6 = 0x2218,
            GPIO_SET7 = 0x221C,

            GPIO_CLR0 = 0x2280,
            GPIO_CLR1 = 0x2284,
            GPIO_CLR2 = 0x2288,
            GPIO_CLR3 = 0x228C,
            GPIO_CLR4 = 0x2290,
            GPIO_CLR5 = 0x2294,
            GPIO_CLR6 = 0x2298,
            GPIO_CLR7 = 0x229C,

            GPIO_NOT0 = 0x2300,
            GPIO_NOT1 = 0x2304,
            GPIO_NOT2 = 0x2308,
            GPIO_NOT3 = 0x230C,
            GPIO_NOT4 = 0x2310,
            GPIO_NOT5 = 0x2314,
            GPIO_NOT6 = 0x2318,
            GPIO_NOT7 = 0x231C,

        }
        private const int puertos = 8;

    }
}

