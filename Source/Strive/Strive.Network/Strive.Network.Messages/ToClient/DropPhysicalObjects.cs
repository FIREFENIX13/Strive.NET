using System;
using System.Collections.Generic;
using Strive.Server.Model;

namespace Strive.Network.Messages.ToClient
{
	/// <summary>
	/// Summary description for DropPhysicalObject.
	/// </summary>
	[Serializable]
	public class DropPhysicalObjects : IMessage {
		public DropPhysicalObjects(){}
        public DropPhysicalObjects(List<PhysicalObject> physicalObjects)
        {
            instanceIDs = new int[physicalObjects.Count];
            int i = 0;
            foreach (PhysicalObject po in physicalObjects)
            {
                instanceIDs[i] = po.ObjectInstanceID;
                i++;
            }
        }

		public int [] instanceIDs;
	}
}
