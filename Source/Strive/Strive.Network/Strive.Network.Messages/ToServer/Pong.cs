namespace Strive.Network.Messages.ToServer
{
    public class Pong
    {
        public int SequenceNumber;

        public Pong(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
    }
}
