using MvvmFx.Common.ViewModels.Behaviors.Messaging;

namespace BioCheck.Views
{
    public class CancelRelationshipMessage
     : MessageBase
    {
        private readonly RelationshipClient client;

        public CancelRelationshipMessage(RelationshipClient client)
        {
            this.client = client;
        }

        public RelationshipClient Client
        {
            get { return client; }
        }
    }
}