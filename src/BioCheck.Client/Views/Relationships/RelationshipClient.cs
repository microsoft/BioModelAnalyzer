using System;
using MvvmFx.Common.ViewModels.Behaviors.Messaging;

namespace BioCheck.Views
{
    /// <summary>
    /// Helper class to manage publishing the Relationship messages which the RelationshipManager/Drawer subscribes to.
    /// </summary>
    public class RelationshipClient
    {
        private readonly IRelationshipTarget target;
        private readonly MessengerService messenger;

        public RelationshipClient(IRelationshipTarget target)
        {
            this.target = target;
            this.messenger = new MessengerService();
        }

        public IRelationshipTarget Target
        {
            get { return target; }
        }

        /// <summary>
        /// Drops the specified relationship event args.
        /// </summary>
        /// <param name="relationshipEventArgs">The <see cref="BioCheck.Views.RelationshipEventArgs"/> instance containing the event data.</param>
        public void Drop(RelationshipEventArgs relationshipEventArgs)
        {
            this.messenger.Publish<StartRelationshipMessage>(new StartRelationshipMessage(this, relationshipEventArgs));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            this.messenger.Publish<CancelRelationshipMessage>(new CancelRelationshipMessage(this));
        }
    }
}