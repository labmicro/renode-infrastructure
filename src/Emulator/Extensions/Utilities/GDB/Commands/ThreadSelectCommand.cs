//
// Copyright (c) 2010-2018 Antmicro
//
//  This file is licensed under the MIT License.
//  Full license text is available in 'licenses/MIT.txt'.
//
using Antmicro.Renode.Utilities.GDB;

namespace Antmicro.Renode.Utilities.GDB.Commands
{
    public class ThreadSelectCommand : Command
    {
        public ThreadSelectCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("Hc")]
        public PacketData SelectExecute()
        {
            return PacketData.Success;
        }

        [Execute("Hg")]
        public PacketData SelectOther()
        {
            return PacketData.Success;
        }
    }
}

