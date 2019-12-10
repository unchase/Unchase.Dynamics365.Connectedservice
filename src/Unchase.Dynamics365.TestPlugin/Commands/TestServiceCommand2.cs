using System;
using Unchase.Dynamics365.Shared;
using Unchase.Dynamics365.Shared.Models;

namespace Unchase.Dynamics365.TestPlugin.Commands
{
    public class TestServiceCommand2 : IServiceCommand
    {
        private int _commandItem;


        public TestServiceCommand2(int commandItem)
        {
            this._commandItem = commandItem;
        }


        public string CommandName
        {
            get
            {
                return this.GetType().Name;
            }
        }


        public void Execute(Context context)
        {
            this._commandItem = 2;
            //ToDo: add your command logic there
            context.TraceMessage($"Set value to {this._commandItem} in {this.CommandName}");

            throw new NotImplementedException();
        }
    }
}
