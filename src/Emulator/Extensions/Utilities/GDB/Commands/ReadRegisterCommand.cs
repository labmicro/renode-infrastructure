//
// Copyright (c) 2010-2018 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Linq;
using System.Text;

namespace Antmicro.Renode.Utilities.GDB.Commands
{
    internal class ReadRegisterCommand : Command
    {
        public ReadRegisterCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("p")]
        public PacketData Execute(
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]int registerNumber)
        {
            var content = new StringBuilder();
            if (registerNumber == 29)
            {
                //content.Append(":");
            }
            if(!manager.Cpu.GetRegisters().Any(x => x.Index == registerNumber))
            {
                foreach(var feature in manager.Cpu.GDBFeatures)
                {
                    foreach(var register in feature.Registers)
                    {
                        if (register.Number == registerNumber)
                        {
                            for(int b = 0; b < (register.Size / 4); b++)
                            {
                                content.AppendFormat("{0:x2}", b);
                            }
                            return new PacketData(content.ToString());
                        }
                    }
                }
                return PacketData.ErrorReply(1);
            }

            //var content = new StringBuilder();
            foreach(var b in manager.Cpu.GetRegisterUnsafe(registerNumber).GetBytes())
            {
                content.AppendFormat("{0:x2}", b);
            }
            return new PacketData(content.ToString());
        }
    }
}

