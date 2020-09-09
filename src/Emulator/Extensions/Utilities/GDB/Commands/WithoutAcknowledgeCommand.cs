//
// Copyright (c) 2010-2018 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//

namespace Antmicro.Renode.Utilities.GDB.Commands
{
    internal class WithoutAcknowledgeCommand : Command
    {
        public WithoutAcknowledgeCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("QStartNoAckMode")]
        public PacketData Execute()
        {
            return PacketData.Success;
        }
    }
}

