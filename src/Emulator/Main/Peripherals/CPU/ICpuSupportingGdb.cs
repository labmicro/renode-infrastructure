//
// Copyright (c) 2010-2018 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Collections.Generic;

namespace Antmicro.Renode.Peripherals.CPU
{
    public struct IGBDRegister
    {
        public string Name { get; }
        public uint Size { get; }
        public uint Number { get; }
        public string Type { get; }
        public string Group { get; }

        public IGBDRegister(string name, uint size, uint number, string type, string group) : this()
        {
            this.Name = name;
            this.Size = size;
            this.Number = number;
            this.Type = type;
            this.Group = group;
        }
    }

    public struct IGBDFeature
    {
        public string Name { get; }
        public List<IGBDRegister> Registers { get; }
        public IGBDFeature(string name) : this()
        {
            this.Name = name;
            this.Registers = new List<IGBDRegister>();
        }
    }

    public interface ICpuSupportingGdb : ICPUWithHooks, IControllableCPU
    {
        ulong Step(int count = 1);
        ExecutionMode ExecutionMode { get; set; }
        event Action<HaltArguments> Halted;
        void EnterSingleStepModeSafely(HaltArguments args);

        string GDBArchitecture { get; }
        List<IGBDFeature> GDBFeatures { get; }
        bool DebuggerConnected { get; set; }
        uint Id { get; }
        string Name { get; }
    }
}

