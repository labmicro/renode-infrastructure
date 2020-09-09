//
// Copyright (c) 2010-2018 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//

namespace Antmicro.Renode.Utilities.GDB.Commands
{
    internal class ThreadInfoCommand : Command
    {
        public ThreadInfoCommand(CommandsManager manager) : base(manager)
        {
        }

        //[Execute("qfThreadInfo")]
        //public PacketData StartList()
        //{
        //    return new PacketData("m0000dead");
        //}

        //[Execute("qsThreadInfo")]
        //public PacketData ContinuetList()
        //{
        //    return new PacketData("l");
        //}

        [Execute("qL")]
        public PacketData ContinuetList()
        {
            return new PacketData();
        }
    }
}

