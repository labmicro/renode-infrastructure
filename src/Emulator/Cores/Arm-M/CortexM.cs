//
// Copyright (c) 2010-2018 Antmicro
// Copyright (c) 2011-2015 Realtime Embedded
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Peripherals.IRQControllers;
using Antmicro.Renode.Utilities.Binding;
using Antmicro.Renode.Logging;
using Antmicro.Migrant.Hooks;
using Antmicro.Renode.Exceptions;
using ELFSharp.ELF;
using ELFSharp.UImage;
using Machine = Antmicro.Renode.Core.Machine;

namespace Antmicro.Renode.Peripherals.CPU
{
    public partial class CortexM : Arm, IControllableCPU
    {
        public CortexM(string cpuType, Machine machine, NVIC nvic, Endianess endianness = Endianess.LittleEndian) : base(cpuType, machine, endianness)
        {
            if(nvic == null)
            {
                throw new RecoverableException(new ArgumentNullException("nvic"));
            }

            this.nvic = nvic;
            nvic.AttachCPU(this);
        }

        public override void Start()
        {
            InitPCAndSP();
            base.Start();
        }

        public override void Reset()
        {
            pcNotInitialized = true;
            vtorInitialized = false;
            base.Reset();
        }

        public override void Resume()
        {
            InitPCAndSP();
            base.Resume();
        }

        public override string Architecture { get { return "arm-m"; } }

        public override List<IGBDFeature> GDBFeatures {
            get
            {
                List<IGBDFeature> features = new List<IGBDFeature>();

                IGBDFeature feature = new IGBDFeature("org.gnu.gdb.arm.m-profile");
                for(uint index = 0; index <= 12; index++)
                {
                    feature.Registers.Add(new IGBDRegister($"r{index}", index, 32, "uint32", "general"));
                }
                feature.Registers.Add(new IGBDRegister("sp", 13, 32, "data_ptr", "general"));
                feature.Registers.Add(new IGBDRegister("lr", 14, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("ps", 15, 32, "code_ptr", "general"));
                feature.Registers.Add(new IGBDRegister("xpsr", 25, 32, "uint32", "general"));
                features.Add(feature);

                feature = new IGBDFeature("org.gnu.gdb.arm.m-system");
                feature.Registers.Add(new IGBDRegister("msp", 26, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("psp", 27, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("primask", 28, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("basepri", 29, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("faultmask", 20, 32, "uint32", "general"));
                feature.Registers.Add(new IGBDRegister("control", 32, 32, "uint32", "general"));
                features.Add(feature);

                return features;
            }
        }

        public uint VectorTableOffset
        {
            get
            {
                return tlibGetInterruptVectorBase();
            }
            set
            {
                vtorInitialized = true;
                if(machine.SystemBus.FindMemory(value) == null)
                {
                    this.Log(LogLevel.Warning, "Tried to set VTOR address at 0x{0:X} which does not lay in memory. Aborted.", value);
                    return;
                }
                this.NoisyLog("VectorTableOffset set to 0x{0:X}.", value);
                tlibSetInterruptVectorBase(value);
            }
        }

        public bool FpuEnabled
        {
            set
            {
                tlibToggleFpu(value ? 1 : 0);
            }
        }

        void IControllableCPU.InitFromElf(IELF elf)
        {
            // do nothing
        }

        void IControllableCPU.InitFromUImage(UImage uImage)
        {
            // do nothing
        }

        protected override UInt32 BeforePCWrite(UInt32 value)
        {
            if(value % 2 == 0)
            {
                this.Log(LogLevel.Warning, "Patching PC 0x{0:X} for Thumb mode.", value);
                value += 1;
            }
            pcNotInitialized = false;
            return base.BeforePCWrite(value);
        }

        private void InitPCAndSP()
        {
            var firstNotNullSection = machine.SystemBus.Lookup.FirstNotNullSectionAddress;
            if(!vtorInitialized && firstNotNullSection.HasValue)
            {
                if((firstNotNullSection.Value & (2 << 6 - 1)) > 0)
                {
                    this.Log(LogLevel.Warning, "Alignment of VectorTableOffset register is not correct.");
                }
                else
                {
                    var value = firstNotNullSection.Value;
                    this.Log(LogLevel.Info, "Guessing VectorTableOffset value to be 0x{0:X}.", value);
                    VectorTableOffset = checked((uint)value);
                }
            }
            if(pcNotInitialized)
            {
                pcNotInitialized = false;
                // stack pointer and program counter are being sent according
                // to VTOR (vector table offset register)
                var sysbus = machine.SystemBus;
                var pc = sysbus.ReadDoubleWord(VectorTableOffset + 4);
                var sp = sysbus.ReadDoubleWord(VectorTableOffset);
                if(sysbus.FindMemory(pc) == null || (pc == 0 && sp == 0))
                {
                    this.Log(LogLevel.Error, "PC does not lay in memory or PC and SP are equal to zero. CPU was halted.");
                    IsHalted = true;
                }
                this.Log(LogLevel.Info, "Setting initial values: PC = 0x{0:X}, SP = 0x{1:X}.", pc, sp);
                PC = pc;
                SP = sp;
            }
        }

        [Export]
        private void SetPendingIRQ(int number)
        {
            nvic.SetPendingIRQ(number);
        }

        [Export]
        private int AcknowledgeIRQ()
        {
            var result = nvic.AcknowledgeIRQ();
            return result;
        }

        [Export]
        private void CompleteIRQ(int number)
        {
            nvic.CompleteIRQ(number);
        }

        [Export]
        private void OnBASEPRIWrite(int value)
        {
            nvic.BASEPRI = (byte)value;
        }

        [Export]
        private void OnPRIMASKWrite(int value)
        {
            if (nvic != null)
            {
                nvic.PRIMASK = (value != 0);
            }
        }

        [Export]
        private int PendingMaskedIRQ()
        {
            return nvic.MaskedInterruptPresent.WaitOne(0) ? 1 : 0;
        }


        private NVIC nvic;
        private bool pcNotInitialized = true;
        private bool vtorInitialized;

        // 649:  Field '...' is never assigned to, and will always have its default value null
        #pragma warning disable 649

        [Import]
        private ActionInt32 tlibToggleFpu;

        [Import]
        private FuncUInt32 tlibGetInterruptVectorBase;

        [Import]
        private ActionUInt32 tlibSetInterruptVectorBase;

        #pragma warning restore 649
    }
}

