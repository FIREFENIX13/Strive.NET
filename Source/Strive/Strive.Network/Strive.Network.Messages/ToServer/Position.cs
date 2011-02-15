using System.Windows.Media.Media3D;


namespace Strive.Network.Messages.ToServer
{
    public class MyPosition : IMessage
    {
        public int PossessingId;
        public Vector3D Position;
        public Quaternion Rotation;

        public MyPosition(int possessingId, Vector3D position, Quaternion rotation)
        {
            PossessingId = possessingId;
            Position = position;
            Rotation = rotation;
        }
    }
}
