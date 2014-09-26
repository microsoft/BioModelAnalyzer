using MvvmFx.Common.ViewModels.Behaviors.Messaging;

namespace BioCheck.Views
{
    public class StartRelationshipMessage
     : MessageBase
    {
        private readonly RelationshipClient client;
        private readonly RelationshipEventArgs args;

        public StartRelationshipMessage(RelationshipClient client, RelationshipEventArgs args)
        {
            this.client = client;
            this.args = args;
        }

        public RelationshipEventArgs Args
        {
            get { return args; }
        }

        public RelationshipClient Client
        {
            get { return client; }
        }
    }
}